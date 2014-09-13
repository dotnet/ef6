namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
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

            internal static CqtExpression ConcatArgs(ExpressionConverter parent, Expression linq, Expression[] linqArgs)
            {
                var args = linqArgs
                        .Where(arg => !arg.IsNullConstant()) // remove null constants   
                        .Select(arg => ConvertToString(parent, arg)) // Apply ToString semantics                    
                        .ToArray();

                //if all args was null constants, optimize the entire expression to constant "" 
                // e.g null + null + null == ""
                if (args.Length == 0)
                {
                    return DbExpressionBuilder.Constant(string.Empty);
                }

                var current = args.First();
                foreach (var next in args.Skip(1)) //concat all args
                {
                    current = parent.CreateCanonicalFunction(Concat, linq, current, next);
                }

                return current;
            }

            internal static CqtExpression StripNull(LinqExpression sourceExpression, 
                DbExpression inputExpression, DbExpression outputExpression, bool useDatabaseNullSemantics)
            {
                if (sourceExpression.IsNullConstant())
                {
                    return DbExpressionBuilder.Constant(string.Empty);
                }

                if (sourceExpression.NodeType == ExpressionType.Constant)
                {
                    return outputExpression;
                }

                if (useDatabaseNullSemantics)
                {
                    return inputExpression;
                }

                // converts evaluated null values to empty string, nullable primitive properties etc.
                var castNullToEmptyString = DbExpressionBuilder.Case(
                    new[] { inputExpression.IsNull() },
                    new[] { DbExpressionBuilder.Constant(string.Empty) },
                    outputExpression);
                return castNullToEmptyString;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", 
                Justification = "the same linqExpression value is never cast to ConstantExpression twice")]
            internal static DbExpression ConvertToString(ExpressionConverter parent,  LinqExpression linqExpression)
            {
                if (linqExpression.Type == typeof(object))
                {
                    var constantExpression = linqExpression as ConstantExpression;
                    linqExpression =
                        constantExpression != null ?
                            Expression.Constant(constantExpression.Value) :
                            linqExpression.RemoveConvert();
                }

                var expression = parent.TranslateExpression(linqExpression);
                var clrType = TypeSystem.GetNonNullableType(linqExpression.Type);
                var useDatabaseNullSemantics = !parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior;

                if (clrType.IsEnum)
                {
                    //Flag enums are not supported.
                    if (Attribute.IsDefined(clrType, typeof(FlagsAttribute)))
                    {
                        throw new NotSupportedException(Strings.Elinq_ToStringNotSupportedForEnumsWithFlags);
                    }

                    if (linqExpression.IsNullConstant())
                    {
                        return DbExpressionBuilder.Constant(string.Empty);
                    }

                    //Constant expression, optimize to constant name
                    if (linqExpression.NodeType == ExpressionType.Constant)
                    {
                        var value = ((ConstantExpression)linqExpression).Value;
                        var name = Enum.GetName(clrType, value) ?? value.ToString();
                        return DbExpressionBuilder.Constant(name);
                    }

                    var integralType = clrType.GetEnumUnderlyingType();
                    var type = parent.GetValueLayerType(integralType);

                    var values = clrType.GetEnumValues()
                        .Cast<object>()
                        .Select(v => System.Convert.ChangeType(v, integralType, CultureInfo.InvariantCulture)) //cast to integral type so that unmapped enum types works too
                        .Select(v => DbExpressionBuilder.Constant(v))
                        .Select(c => (DbExpression)expression.CastTo(type).Equal(c)) //cast expression to integral type before comparing to constant
                        .Concat(new[] { expression.CastTo(type).IsNull() }); // default case
                        
                    var names = clrType.GetEnumNames()
                        .Select(s => DbExpressionBuilder.Constant(s))
                        .Concat(new[] { DbExpressionBuilder.Constant(string.Empty) }); // default case

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
                    return StripNull(linqExpression, expression, expression, useDatabaseNullSemantics);
                }
                else if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.Guid))
                {
                    return StripNull(linqExpression, expression, expression.CastTo(parent.GetValueLayerType(typeof(string))).ToLower(), useDatabaseNullSemantics);
                }
                else if (TypeSemantics.IsPrimitiveType(expression.ResultType, PrimitiveTypeKind.Boolean))
                {
                    if (linqExpression.IsNullConstant())
                    {
                        return DbExpressionBuilder.Constant(string.Empty);
                    }

                    if (linqExpression.NodeType == ExpressionType.Constant)
                    {
                        var name = ((ConstantExpression)linqExpression).Value.ToString();
                        return DbExpressionBuilder.Constant(name);
                    }

                    var whenTrue = expression.Equal(DbExpressionBuilder.True);
                    var whenFalse = expression.Equal(DbExpressionBuilder.False);
                    var thenTrue = DbExpressionBuilder.Constant(true.ToString());
                    var thenFalse = DbExpressionBuilder.Constant(false.ToString());

                    return DbExpressionBuilder.Case(
                        new[] { whenTrue, whenFalse },
                        new[] { thenTrue, thenFalse },
                        DbExpressionBuilder.Constant(string.Empty));
                }
                else
                {
                    if (!SupportsCastToString(expression.ResultType))
                    {
                        throw new NotSupportedException(
                            Strings.Elinq_ToStringNotSupportedForType(expression.ResultType.EdmType.Name));
                    }

                    //treat all other types as a simple cast
                    return StripNull(linqExpression, expression, expression.CastTo(parent.GetValueLayerType(typeof(string))), useDatabaseNullSemantics);
                }
            }

            internal static bool SupportsCastToString(TypeUsage typeUsage)
            {
                return (TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.String)
                    || TypeSemantics.IsNumericType(typeUsage)
                    || TypeSemantics.IsBooleanType(typeUsage)
                    || TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.DateTime)
                    || TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.DateTimeOffset)
                    || TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.Time)
                    || TypeSemantics.IsPrimitiveType(typeUsage, PrimitiveTypeKind.Guid));
            }
        }
    }
}