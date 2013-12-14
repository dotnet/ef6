namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using CqtExpression = System.Data.Entity.Core.Common.CommandTrees.DbExpression;
    using LinqExpression = System.Linq.Expressions.Expression;

    internal sealed partial class ExpressionConverter
    {
        internal static class StringTranslatorUtil
        {
            internal static IEnumerable<Expression> GetConcatArgs(Expression linq)
            {
                if (linq.IsStringAddExpression())
                {
                    foreach (var arg in GetConcatArgs((BinaryExpression)linq))
                    {
                        yield return arg;
                    }
                }
                else
                {
                    yield return linq; //leaf node
                }
            }

            internal static IEnumerable<Expression> GetConcatArgs(BinaryExpression linq)
            {
                // one could also flatten calls to String.Concat here, to avoid multi concat 
                // in "a + b + String.Concat(d, e)", just flatten it to a, b, c, d, e

                //rec traverse left node
                foreach (var arg in GetConcatArgs(linq.Left))
                {
                    yield return arg;
                }

                //rec traverse right node
                foreach (var arg in GetConcatArgs(linq.Right))
                {
                    yield return arg;
                }
            }

            internal static CqtExpression ConcatArgs(ExpressionConverter parent, BinaryExpression linq)
            {
                return ConcatArgs(parent, linq, GetConcatArgs(linq).ToArray());
            }

            private static bool UseCSharpNull(ExpressionConverter parent)
            {
                return parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior;
            }

            internal static CqtExpression ConcatArgs(ExpressionConverter parent, Expression linq, params Expression[] linqArgs)
            {
                CqtExpression[] args = null;
                if (UseCSharpNull(parent))
                {                                     
                    args = linqArgs
                        .Where(arg => !arg.IsNullConstant()) //if csharp behavior, remove null constants   
                        .Select(arg => StringTranslatorUtil.ConvertToString(parent, arg)) // Apply ToString semantics                    
                        .ToArray();

                    //if all args was null constants, optimize the entire expression to constant "" if we use csharp null semantics
                    //e.g null + null + null == ""
                    if (args.Length == 0)
                    {
                        return DbExpression.FromString(string.Empty); //concat gives "" when no args and CSharp null semantics
                    }
                }
                else
                {
                    // if any of the args are null, return null
                    // this is consistent with the old concat behavior, but reduced to a constant for this special case
                    if (linqArgs.Any(arg => arg.IsNullConstant()))
                    {
                        return DbExpression.FromString(null);
                    }

                    args = linqArgs
                        .Select(arg => StringTranslatorUtil.ConvertToString(parent, arg)) // Apply ToString semantics
                        .ToArray();
                }

                var current = args.First();
                foreach (var next in args.Skip(1)) //concat all args
                    current = parent.CreateCanonicalFunction(Concat, linq, current, next);

                return current;
            }

            internal static CqtExpression StripNull(ExpressionConverter parent,LinqExpression sourceExpression, DbExpression inputExpression, DbExpression outputExpression)
            {
                if (UseCSharpNull(parent))
                {
                    //ensure this holds for future use cases too

                    //optimize constant null expression
                    if (sourceExpression.IsNullConstant())
                    {
                        return DbExpression.FromString(string.Empty);
                    }

                    //optimize constant expression
                    if (sourceExpression.NodeType == ExpressionType.Constant)
                    {
                        return outputExpression;
                    }

                    if (sourceExpression is QueryParameterExpression)
                    {
                        //Should this be treated as a constant value or is query structure cached so it needs to do full generation?
                    }

                    // converts evaluated null values to empty string, nullable primitive properties etc.
                    var castNullToEmptyString = DbExpressionBuilder.Case(
                        new[] { inputExpression.IsNull() },
                        new[] { DbExpression.FromString(string.Empty) },
                        outputExpression);
                    return castNullToEmptyString;
                }

                return outputExpression;
            }

            internal static DbExpression ConvertToString(ExpressionConverter parent,  LinqExpression linqExpression)
            {
                if (linqExpression.Type == typeof(object))
                {
                    if (linqExpression is ConstantExpression)
                    {
                        var value = ((ConstantExpression)linqExpression).Value;
                        linqExpression = LinqExpression.Constant(value);
                    }
                    else
                    {
                        linqExpression = linqExpression.RemoveConvert();
                    }
                }

                DbExpression expression = parent.TranslateExpression(linqExpression);

                var clrType = linqExpression.Type;
                clrType = TypeSystem.GetNonNullableType(clrType);

                if (clrType.IsEnum)
                {
                    var integralType = clrType.GetEnumUnderlyingType();
                    var type = parent.GetValueLayerType(integralType);

                    //Flag enums are not supported.
                    if (clrType.IsDefined(typeof(FlagsAttribute)))
                        throw new NotSupportedException("Flag enums are not supported");

                    if (linqExpression.IsNullConstant())
                    {
                        return NullReplacement(parent);
                    }

                    //Constant expression, optimize to constant name
                    if (linqExpression.NodeType == ExpressionType.Constant)
                    {
                        var value = ((ConstantExpression)linqExpression).Value;
                        var name = Enum.GetName(clrType, value) ?? value.ToString();
                        return DbExpression.FromString(name);
                    }

                    //This currently handles standard enums only

                    var values = clrType.GetEnumValues()
                        .Cast<object>()
                        .Select(v => System.Convert.ChangeType(v, integralType)) //cast to integral type so that unmapped enum types works too
                        .Select(v => DbExpressionBuilder.Constant(v))
                        .Select(c => (DbExpression)expression.CastTo(type).Equal(c)) //cast expression to integral type before comparing to constant                        
                        .ToList();
                    values.Add((DbExpression)expression.CastTo(type).IsNull());

                    var names = clrType.GetEnumNames()
                        .Select(s => DbExpression.FromString(s))
                        .ToList();
                    names.Add(NullReplacement(parent));

                    //translate unnamed enum values for the else clause, raw linq -> as integral value -> translate to cqt -> to string
                    //e.g.  ((DayOfWeek)99) -> "99"
                    var asIntegralLinq = LinqExpression.Convert(linqExpression, integralType);
                    var asStringCqt = parent
                        .TranslateExpression(asIntegralLinq)
                        .CastTo(parent.GetValueLayerType(typeof(string)));

                    return DbExpressionBuilder.Case(values, names, asStringCqt);
                }
                else if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.String))
                {
                    return StripNull(parent,linqExpression,expression,expression);
                }
                else if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.Guid))
                {
                    return StripNull(parent, linqExpression, expression, expression.CastTo(parent.GetValueLayerType(typeof(string))).ToLower());
                }
                else if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.Boolean))
                {
                    //Booleans aren't localized with ToString(), but we get less literals this way
                    var trueString = true.ToString();
                    var falseString = false.ToString();

                    if (linqExpression.IsNullConstant())
                    {
                        return NullReplacement(parent);
                    }

                    if (linqExpression.NodeType == ExpressionType.Constant)
                    {
                        var name = ((ConstantExpression)linqExpression).Value.ToString();
                        return DbExpression.FromString(name);
                    }

                    var whenTrue = expression.Equal(DbExpression.FromBoolean(true));
                    var whenFalse = expression.Equal(DbExpression.FromBoolean(false));
                    var thenTrue = DbExpression.FromString(trueString);
                    var thenFalse = DbExpression.FromString(falseString);

                    return DbExpressionBuilder.Case(
                        new[] { whenTrue, whenFalse },
                        new[] { thenTrue, thenFalse },
                        NullReplacement(parent));
                }
                else
                {
                    if (!SupportsCastToString(expression.ResultType))
                        throw new NotSupportedException("The type " + expression.ResultType.EdmType.Name + " can not be converted to string");

                    //treat all other types as a simple cast
                    return StripNull(parent, linqExpression, expression, expression.CastTo(parent.GetValueLayerType(typeof(string))));
                }
            }

            private static CqtExpression NullReplacement(ExpressionConverter parent)
            {
                return DbExpression.FromString(UseCSharpNull(parent) ? "" : null);
            }

            internal static bool SupportsCastToString(TypeUsage typeUsage)
            {
                return (TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.String)
                    || TypeSemantics.IsNumericType(typeUsage)
                    || TypeSemantics.IsBooleanType(typeUsage)
                    || TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.DateTime)
                    || TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.DateTimeOffset)
                    || TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.Guid));
            }
        }
    }
}