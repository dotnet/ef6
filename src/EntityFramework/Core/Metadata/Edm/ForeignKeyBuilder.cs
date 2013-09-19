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
        private readonly AssociationType _associationType;
        private readonly AssociationSet _associationSet;

        internal ForeignKeyBuilder()
        {
            // testing only
        }

        public ForeignKeyBuilder(EdmModel database, string name)
        {
            Check.NotNull(database, "database");

            _database = database;

            _associationType
                = new AssociationType(
                    name,
                    EdmModelExtensions.DefaultStoreNamespace,
                    true,
                    DataSpace.SSpace);

            _associationSet
                = new AssociationSet(_associationType.Name, _associationType);
        }

        public string Name
        {
            get { return _associationType.Name; }
        }

        public virtual EntityType PrincipalTable
        {
            get { return _associationType.SourceEnd.GetEntityType(); }
            set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                _associationType.SourceEnd
                    = new AssociationEndMember(value.Name, value);

                _associationSet.SourceSet
                    = _database.GetEntitySet(value);

                if ((_associationType.TargetEnd != null)
                    && (value.Name == _associationType.TargetEnd.Name))
                {
                    _associationType.TargetEnd.Name = value.Name + SelfRefSuffix;
                }
            }
        }

        public virtual void SetOwner(EntityType owner)
        {
            Util.ThrowIfReadOnly(this);

            if (owner == null)
            {
                _database.RemoveAssociationType(_associationType);
            }
            else
            {
                _associationType.TargetEnd
                    = new AssociationEndMember(
                        owner != PrincipalTable ? owner.Name : owner.Name + SelfRefSuffix,
                        owner);

                _associationSet.TargetSet
                    = _database.GetEntitySet(owner);

                if (!_database.AssociationTypes.Contains(_associationType))
                {
                    _database.AddAssociationType(_associationType);
                    _database.AddAssociationSet(_associationSet);
                }
            }
        }

        public virtual IEnumerable<EdmProperty> DependentColumns
        {
            get
            {
                return _associationType.Constraint != null
                           ? _associationType.Constraint.ToProperties
                           : Enumerable.Empty<EdmProperty>();
            }
            set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                _associationType.Constraint
                    = new ReferentialConstraint(
                        _associationType.SourceEnd,
                        _associationType.TargetEnd,
                        PrincipalTable.KeyProperties,
                        value);

                SetMultiplicities();
            }
        }

        public OperationAction DeleteAction
        {
            get
            {
                return _associationType.SourceEnd != null
                           ? _associationType.SourceEnd.DeleteBehavior
                           : default(OperationAction);
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                _associationType.SourceEnd.DeleteBehavior = value;
            }
        }

        private void SetMultiplicities()
        {
            _associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
            _associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            var dependentTable = _associationType.TargetEnd.GetEntityType();

            var dependentKeyProperties = dependentTable.KeyProperties.Where(key => dependentTable.DeclaredMembers.Contains(key)).ToList();
            if (dependentKeyProperties.Count == DependentColumns.Count()
                && dependentKeyProperties.All(DependentColumns.Contains))
            {
                _associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
                _associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
            }
            else if (!DependentColumns.Any(p => p.Nullable))
            {
                _associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            }
        }

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { throw new NotImplementedException(); }
        }

        string INamedDataModelItem.Identity
        {
            get { return Identity; }
        }

        internal override string Identity
        {
            get { throw new NotImplementedException(); }
        }
    }
}
