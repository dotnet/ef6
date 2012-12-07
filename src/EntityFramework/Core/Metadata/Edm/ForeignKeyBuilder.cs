// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class ForeignKeyBuilder : MetadataItem, INamedDataModelItem
    {
        private const string SelfRefSuffix = "Self";

        private readonly EdmModel _database;
        private readonly AssociationType _assocationType;
        private readonly AssociationSet _associationSet;

        internal ForeignKeyBuilder()
        {
            // testing only
        }

        public ForeignKeyBuilder(EdmModel database, string name)
        {
            Check.NotNull(database, "database");

            _database = database;

            _assocationType
                = new AssociationType(
                    name,
                    XmlConstants.GetSsdlNamespace(database.Version),
                    true,
                    DataSpace.SSpace);

            _associationSet
                = new AssociationSet(_assocationType.Name, _assocationType);
        }

        public string Name
        {
            get { return _assocationType.Name; }
        }

        public virtual EntityType PrincipalTable
        {
            get { return _assocationType.SourceEnd.GetEntityType(); }
            set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                _assocationType.SourceEnd
                    = new AssociationEndMember(value.Name, value);

                if ((_assocationType.TargetEnd != null)
                    && (value.Name == _assocationType.TargetEnd.Name))
                {
                    _assocationType.TargetEnd.Name = value.Name + SelfRefSuffix;
                }
            }
        }

        public virtual void SetOwner(EntityType owner)
        {
            Util.ThrowIfReadOnly(this);

            if (owner == null)
            {
                _database.RemoveAssociationType(_assocationType);
            }
            else
            {
                _assocationType.TargetEnd
                    = new AssociationEndMember(
                        owner != PrincipalTable ? owner.Name : owner.Name + SelfRefSuffix,
                        owner);

                if (!_database.AssociationTypes.Contains(_assocationType))
                {
                    _database.AddAssociationType(_assocationType);
                    _database.AddAssociationSet(_associationSet);
                }
            }
        }

        public virtual IEnumerable<EdmProperty> DependentColumns
        {
            get
            {
                return _assocationType.Constraint != null
                           ? _assocationType.Constraint.ToProperties
                           : Enumerable.Empty<EdmProperty>();
            }
            set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                _assocationType.Constraint
                    = new ReferentialConstraint(
                        _assocationType.SourceEnd,
                        _assocationType.TargetEnd,
                        PrincipalTable.DeclaredKeyProperties,
                        value);

                SetMultiplicities();
            }
        }

        public OperationAction DeleteAction
        {
            get
            {
                return _assocationType.SourceEnd != null
                           ? _assocationType.SourceEnd.DeleteBehavior
                           : default(OperationAction);
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                _assocationType.SourceEnd.DeleteBehavior = value;
            }
        }

        private void SetMultiplicities()
        {
            _assocationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
            _assocationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            var dependentTable = _assocationType.TargetEnd.GetEntityType();

            if (dependentTable.DeclaredKeyProperties.Count() == DependentColumns.Count()
                && dependentTable.DeclaredKeyProperties.All(DependentColumns.Contains))
            {
                _assocationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
                _assocationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
            }
            else if (!DependentColumns.Any(p => p.Nullable))
            {
                _assocationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            }
        }

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { throw new NotImplementedException(); }
        }

        internal override string Identity
        {
            get { throw new NotImplementedException(); }
        }
    }
}
