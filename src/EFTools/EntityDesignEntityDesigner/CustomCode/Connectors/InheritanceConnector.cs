// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using System.Globalization;
    using Microsoft.Data.Entity.Design.EntityDesigner.Properties;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.VisualStudio.Modeling.Diagrams.GraphObject;
    using Microsoft.VisualStudio.Modeling.Immutability;

    partial class InheritanceConnector
    {
        public new Inheritance ModelElement
        {
            get { return base.ModelElement as Inheritance; }
        }

        public override string AccessibleName
        {
            get
            {
                var o = ModelElement;
                if (o == null)
                {
                    return Resources.Acc_Unnamed;
                }

                if (o.SourceEntityType == null
                    ||
                    string.IsNullOrEmpty(o.SourceEntityType.Name))
                {
                    return Resources.Acc_Unnamed;
                }

                if (o.TargetEntityType == null
                    ||
                    string.IsNullOrEmpty(o.TargetEntityType.Name))
                {
                    return o.SourceEntityType.Name + "." + Resources.Acc_Unnamed;
                }

                return o.SourceEntityType.Name + "." + o.TargetEntityType.Name;
            }
        }

        public override string AccessibleDescription
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AccDesc_Inheritance,
                    Resources.CompClassName_Inheritance,
                    ModelElement.SourceEntityType.Name,
                    ModelElement.TargetEntityType.Name);
            }
        }

        protected override VGRoutingStyle DefaultRoutingStyle
        {
            get { return VGRoutingStyle.VGRouteOrgChartNS; }
        }

        public override bool CanMove
        {
            get { return (Partition.GetLocks() & Locks.Properties) != Locks.Properties; }
        }

        public override bool CanManuallyRoute
        {
            get { return (Partition.GetLocks() & Locks.Properties) != Locks.Properties; }
        }
    }
}
