// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to delete a function mapping from the model.
    /// </summary>
    internal class DeleteFunctionMappingCommand : DeleteEFElementCommand
    {
        /// <summary>
        ///     Delete a function mapping.  The function mapping is located using the passed in EntityType to validate
        ///     the EntityTypeMapping and then the Function to locate the mapping itself.
        /// </summary>
        /// <param name="conceptualEntityType"></param>
        /// <param name="function"></param>
        /// <param name="type"></param>
        internal DeleteFunctionMappingCommand(EntityType conceptualEntityType, Function function, ModificationFunctionType type)
            : base((EFElement)null)
        {
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);
            CommandValidation.ValidateFunction(function);

            foreach (var mf in function.GetAntiDependenciesOfType<ModificationFunction>())
            {
                if (mf.ModificationFunctionMapping.EntityTypeMapping.FirstBoundConceptualEntityType == conceptualEntityType
                    && mf.FunctionType == type)
                {
                    EFElement = mf;
                    break;
                }
            }

            // don't throw now in the c'tor
            Debug.Assert(ModificationFunction != null, "Couldn't find the ModificationFunction needed to complete this operation");
        }

        /// <summary>
        ///     Delete the passed in function mapping
        /// </summary>
        /// <param name="mf"></param>
        internal DeleteFunctionMappingCommand(ModificationFunction mf)
            : base(mf)
        {
            CommandValidation.ValidateModificationFunction(mf);
        }

        protected ModificationFunction ModificationFunction
        {
            get
            {
                var elem = EFElement as ModificationFunction;
                Debug.Assert(elem != null, "underlying element does not exist or is not a ModificationFunction");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "efobj")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (ModificationFunction.ModificationFunctionMapping.Children.Count() == 1
                && ModificationFunction.ModificationFunctionMapping.EntityTypeMapping != null)
            {
                // if we are the only child, then remove the ModificationFunctionMapping and the EntityTypeMapping
                // as well as ourselves
                DeleteInTransaction(cpc, ModificationFunction.ModificationFunctionMapping.EntityTypeMapping);
            }
            else
            {
                // just remove ourselves
                base.InvokeInternal(cpc);
            }
        }
    }
}
