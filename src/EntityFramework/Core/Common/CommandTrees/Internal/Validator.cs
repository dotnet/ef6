namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class DbExpressionValidator : DbExpressionRebinder
    {
        private readonly DataSpace requiredSpace;
        private readonly DataSpace[] allowedMetadataSpaces;
        private readonly DataSpace[] allowedFunctionSpaces;

        private readonly Dictionary<string, DbParameterReferenceExpression> paramMappings =
            new Dictionary<string, DbParameterReferenceExpression>();

        private readonly Stack<Dictionary<string, TypeUsage>> variableScopes = new Stack<Dictionary<string, TypeUsage>>();

        private string expressionArgumentName;

        internal DbExpressionValidator(MetadataWorkspace metadata, DataSpace expectedDataSpace)
            : base(metadata)
        {
            requiredSpace = expectedDataSpace;
            allowedFunctionSpaces = new[] { DataSpace.CSpace, DataSpace.SSpace };
            if (expectedDataSpace == DataSpace.SSpace)
            {
                allowedMetadataSpaces = new[] { DataSpace.SSpace, DataSpace.CSpace };
            }
            else
            {
                allowedMetadataSpaces = new[] { DataSpace.CSpace };
            }
        }

        internal Dictionary<string, DbParameterReferenceExpression> Parameters
        {
            get { return paramMappings; }
        }

        internal void ValidateExpression(DbExpression expression, string argumentName)
        {
            Debug.Assert(expression != null, "Ensure expression is non-null before calling ValidateExpression");
            expressionArgumentName = argumentName;
            VisitExpression(expression);
            expressionArgumentName = null;
            Debug.Assert(variableScopes.Count == 0, "Variable scope stack left in inconsistent state");
        }

        protected override EntitySetBase VisitEntitySet(EntitySetBase entitySet)
        {
            return ValidateMetadata(entitySet, base.VisitEntitySet, es => es.EntityContainer.DataSpace, allowedMetadataSpaces);
        }

        protected override EdmFunction VisitFunction(EdmFunction function)
        {
            // Functions from the current space and S-Space are allowed
            return ValidateMetadata(function, base.VisitFunction, func => func.DataSpace, allowedFunctionSpaces);
        }

        protected override EdmType VisitType(EdmType type)
        {
            return ValidateMetadata(type, base.VisitType, et => et.DataSpace, allowedMetadataSpaces);
        }

        protected override TypeUsage VisitTypeUsage(TypeUsage type)
        {
            return ValidateMetadata(type, base.VisitTypeUsage, tu => tu.EdmType.DataSpace, allowedMetadataSpaces);
        }

        protected override void OnEnterScope(IEnumerable<DbVariableReferenceExpression> scopeVariables)
        {
            var newScope = scopeVariables.ToDictionary(var => var.VariableName, var => var.ResultType, StringComparer.Ordinal);
            variableScopes.Push(newScope);
        }

        protected override void OnExitScope()
        {
            variableScopes.Pop();
        }

        public override DbExpression Visit(DbVariableReferenceExpression expression)
        {
            var result = base.Visit(expression);
            if (result.ExpressionKind
                == DbExpressionKind.VariableReference)
            {
                var varRef = (DbVariableReferenceExpression)result;
                TypeUsage foundType = null;
                foreach (var scope in variableScopes)
                {
                    if (scope.TryGetValue(varRef.VariableName, out foundType))
                    {
                        break;
                    }
                }

                if (foundType == null)
                {
                    ThrowInvalid(Strings.Cqt_Validator_VarRefInvalid(varRef.VariableName));
                }

                // SQLBUDT#545720: Equivalence is not a sufficient check (consider row types) - equality is required.
                if (!TypeSemantics.IsEqual(varRef.ResultType, foundType))
                {
                    ThrowInvalid(Strings.Cqt_Validator_VarRefTypeMismatch(varRef.VariableName));
                }
            }

            return result;
        }

        public override DbExpression Visit(DbParameterReferenceExpression expression)
        {
            var result = base.Visit(expression);
            if (result.ExpressionKind
                == DbExpressionKind.ParameterReference)
            {
                var paramRef = result as DbParameterReferenceExpression;

                DbParameterReferenceExpression foundParam;
                if (paramMappings.TryGetValue(paramRef.ParameterName, out foundParam))
                {
                    // SQLBUDT#545720: Equivalence is not a sufficient check (consider row types for TVPs) - equality is required.
                    if (!TypeSemantics.IsEqual(paramRef.ResultType, foundParam.ResultType))
                    {
                        ThrowInvalid(Strings.Cqt_Validator_InvalidIncompatibleParameterReferences(paramRef.ParameterName));
                    }
                }
                else
                {
                    paramMappings.Add(paramRef.ParameterName, paramRef);
                }
            }
            return result;
        }

        private TMetadata ValidateMetadata<TMetadata>(
            TMetadata metadata, Func<TMetadata, TMetadata> map, Func<TMetadata, DataSpace> getDataSpace, DataSpace[] allowedSpaces)
        {
            var result = map(metadata);
            if (!ReferenceEquals(metadata, result))
            {
                ThrowInvalidMetadata<TMetadata>();
            }

            var resultSpace = getDataSpace(result);
            if (!allowedSpaces.Any(ds => ds == resultSpace))
            {
                ThrowInvalidSpace<TMetadata>();
            }
            return result;
        }

        private void ThrowInvalidMetadata<TMetadata>()
        {
            ThrowInvalid(Strings.Cqt_Validator_InvalidOtherWorkspaceMetadata(typeof(TMetadata).Name));
        }

        private void ThrowInvalidSpace<TMetadata>()
        {
            ThrowInvalid(
                Strings.Cqt_Validator_InvalidIncorrectDataSpaceMetadata(
                    typeof(TMetadata).Name, Enum.GetName(typeof(DataSpace), requiredSpace)));
        }

        private void ThrowInvalid(string message)
        {
            throw new ArgumentException(message, expressionArgumentName);
        }
    }
}
