// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Designer;

    internal class CreateDiagramCommand : Command
    {
        private readonly string _name;
        private readonly Diagrams _diagrams;
        private Diagram _created;

        internal CreateDiagramCommand(string name, Diagrams diagrams)
        {
            ValidateString(name);
            Debug.Assert(diagrams != null, "diagrams is null");

            _name = name;
            _diagrams = diagrams;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // check to see if this name is unique
            string msg = null;
            if (!ModelHelper.IsUniqueName(typeof(Diagram), _diagrams, _name, true, out msg))
            {
                throw new InvalidOperationException(msg);
            }

            var diagram = new Diagram(_diagrams, null);
            diagram.Id.Value = Guid.NewGuid().ToString("N");
            diagram.LocalName.Value = _name;
            _diagrams.AddDiagram(diagram);

            XmlModelHelper.NormalizeAndResolve(diagram);

            _created = diagram;
        }

        internal Diagram Diagram
        {
            get { return _created; }
        }

        /// <summary>
        ///     This helper function will create a Diagram using default name.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes commited
        ///     otherwise the diagram will never be created.
        /// </summary>
        /// <param name="cpc"></param>
        /// <returns>The new ComplexType</returns>
        internal static Diagram CreateDiagramWithDefaultName(CommandProcessorContext cpc)
        {
            Debug.Assert(cpc != null, "The passed in CommandProcessorContext is null.");
            if (cpc != null)
            {
                var service = cpc.EditingContext.GetEFArtifactService();
                var entityDesignArtifact = service.Artifact as EntityDesignArtifact;

                if (entityDesignArtifact == null
                    || entityDesignArtifact.DesignerInfo == null
                    || entityDesignArtifact.DesignerInfo.Diagrams == null)
                {
                    throw new CannotLocateParentItemException();
                }

                var diagramName = ModelHelper.GetUniqueNameWithNumber(
                    typeof(Diagram), entityDesignArtifact.DesignerInfo.Diagrams, Resources.Model_DefaultDiagramName);

                // go create it
                var cp = new CommandProcessor(cpc);
                var cmd = new CreateDiagramCommand(diagramName, entityDesignArtifact.DesignerInfo.Diagrams);
                cp.EnqueueCommand(cmd);
                cp.Invoke();
                return cmd.Diagram;
            }
            return null;
        }
    }
}
