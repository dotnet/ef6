namespace System.Data.Entity.Core.EntityClient
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Class representing a parameter used in EntityCommand
    /// </summary>
    public sealed partial class EntityParameter : DbParameter, IDbDataParameter
    {
        InternalEntityParameter _internalEntityParameter;

        /// <summary>
        /// Constructs the EntityParameter object
        /// </summary>
        public EntityParameter()
            : this(new InternalEntityParameter())
        {
        }

        /// <summary>
        /// Constructs the EntityParameter object with the given parameter name and the type of the parameter
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dbType">The type of the parameter</param>
        public EntityParameter(string parameterName, DbType dbType)
            : this(new InternalEntityParameter(parameterName, dbType))
        {
        }

        /// <summary>
        /// Constructs the EntityParameter object with the given parameter name, the type of the parameter, and the size of the
        /// parameter
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dbType">The type of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        public EntityParameter(string parameterName, DbType dbType, int size)
            : this(new InternalEntityParameter(parameterName, dbType, size))
        {
        }

        /// <summary>
        /// Constructs the EntityParameter object with the given parameter name, the type of the parameter, the size of the
        /// parameter, and the name of the source column
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dbType">The type of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="sourceColumn">The name of the source column mapped to the data set, used for loading the parameter value</param>
        public EntityParameter(string parameterName, DbType dbType, int size, string sourceColumn)
            : this(new InternalEntityParameter(parameterName, dbType, size, sourceColumn)) 
        {
        }

        /// <summary>
        /// Constructs the EntityParameter object with the given parameter name, the type of the parameter, the size of the
        /// parameter, and the name of the source column
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dbType">The type of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="direction">The direction of the parameter, whether it's input/output/both/return value</param>
        /// <param name="isNullable">If the parameter is nullable</param>
        /// <param name="precision">The floating point precision of the parameter, valid only if the parameter type is a floating point type</param>
        /// <param name="scale">The scale of the parameter, valid only if the parameter type is a floating point type</param>
        /// <param name="sourceColumn">The name of the source column mapped to the data set, used for loading the parameter value</param>
        /// <param name="sourceVersion">The data row version to use when loading the parameter value</param>
        /// <param name="value">The value of the parameter</param>
        public EntityParameter(
            string parameterName,
            DbType dbType,
            int size,
            ParameterDirection direction,
            bool isNullable,
            byte precision,
            byte scale,
            string sourceColumn,
            DataRowVersion sourceVersion,
            object value)
            : this(new InternalEntityParameter(
                parameterName, 
                dbType, 
                size, 
                direction, 
                isNullable, 
                precision, 
                scale, 
                sourceColumn, 
                sourceVersion, 
                value))
        {
        }

        internal EntityParameter(InternalEntityParameter internalEntityParameter)
        {
            _internalEntityParameter = internalEntityParameter;
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public override string ParameterName
        {
            get { return _internalEntityParameter.ParameterName; }
            set { _internalEntityParameter.ParameterName = value; }
        }

        /// <summary>
        /// The type of the parameter, EdmType may also be set, and may provide more detailed information.
        /// </summary>
        public override DbType DbType
        {
            get { return _internalEntityParameter.DbType; }
            set { _internalEntityParameter.DbType = value; }
        }

        /// <summary>
        /// The type of the parameter, expressed as an EdmType.
        /// May be null (which is what it will be if unset).  This means
        /// that the DbType contains all the type information.
        /// Non-null values must not contradict DbType (only restate or specialize).
        /// </summary>
        public EdmType EdmType
        {
            get { return _internalEntityParameter.EdmType; }
            set { _internalEntityParameter.EdmType = value; }
        }

        /// <summary>
        /// The precision of the parameter if the parameter is a floating point type
        /// </summary>
        public byte Precision
        {
            get { return _internalEntityParameter.Precision; }
            set { _internalEntityParameter.Precision = value; }
        }

        /// <summary>
        /// The scale of the parameter if the parameter is a floating point type
        /// </summary>
        public byte Scale
        {
            get { return _internalEntityParameter.Scale; }
            set { _internalEntityParameter.Scale = value; }
        }

        /// <summary>
        /// The value of the parameter
        /// </summary>
        public override object Value
        {
            get { return _internalEntityParameter.Value; }
            set { _internalEntityParameter.Value = value; }
        }

        /// <summary>
        /// Gets whether this collection has been changes since the last reset
        /// </summary>
        internal bool IsDirty
        {
            get { return _internalEntityParameter.IsDirty; }
        }

        /// <summary>
        /// Indicates whether the DbType property has been set by the user;
        /// </summary>
        internal bool IsDbTypeSpecified
        {
            get { return _internalEntityParameter.IsDbTypeSpecified; }
        }

        /// <summary>
        /// Indicates whether the Direction property has been set by the user;
        /// </summary>
        internal bool IsDirectionSpecified
        {
            get { return _internalEntityParameter.IsDirectionSpecified; }
        }

        /// <summary>
        /// Indicates whether the IsNullable property has been set by the user;
        /// </summary>
        internal bool IsIsNullableSpecified
        {
            get { return _internalEntityParameter.IsIsNullableSpecified; }
        }

        /// <summary>
        /// Indicates whether the Precision property has been set by the user;
        /// </summary>
        internal bool IsPrecisionSpecified
        {
            get { return _internalEntityParameter.IsPrecisionSpecified; }
        }

        /// <summary>
        /// Indicates whether the Scale property has been set by the user;
        /// </summary>
        internal bool IsScaleSpecified
        {
            get { return _internalEntityParameter.IsScaleSpecified; }
        }

        /// <summary>
        /// Indicates whether the Size property has been set by the user;
        /// </summary>
        internal bool IsSizeSpecified
        {
            get { return _internalEntityParameter.IsSizeSpecified; }
        }

        [RefreshProperties(RefreshProperties.All)]
        [EntityResCategory(EntityRes.DataCategory_Data)]
        [EntityResDescription(EntityRes.DbParameter_Direction)]
        public override ParameterDirection Direction
        {
            get { return _internalEntityParameter.Direction; }
            set { _internalEntityParameter.Direction = value; }
        }

        public override bool IsNullable
        {
            get { return _internalEntityParameter.IsNullable; }
            set { _internalEntityParameter.IsNullable = value; }
        }

        [EntityResCategory(EntityRes.DataCategory_Data)]
        [EntityResDescription(EntityRes.DbParameter_Size)]
        public override int Size
        {
            get { return _internalEntityParameter.Size; }
            set { _internalEntityParameter.Size = value; }
        }

        [EntityResCategory(EntityRes.DataCategory_Update)]
        [EntityResDescription(EntityRes.DbParameter_SourceColumn)]
        public override string SourceColumn
        {
            get { return _internalEntityParameter.SourceColumn; }
            set { _internalEntityParameter.SourceColumn = value; }
        }

        public override bool SourceColumnNullMapping
        {
            get { return _internalEntityParameter.SourceColumnNullMapping; }
            set { _internalEntityParameter.SourceColumnNullMapping = value; }
        }

        [EntityResCategory(EntityRes.DataCategory_Update)]
        [EntityResDescription(EntityRes.DbParameter_SourceVersion)]
        public override DataRowVersion SourceVersion
        {
            get { return _internalEntityParameter.SourceVersion; }
            set { _internalEntityParameter.SourceVersion = value; }
        }

        /// <summary>
        /// Resets the DbType property to its original settings
        /// </summary>
        public override void ResetDbType()
        {
            _internalEntityParameter.ResetDbType();
        }

        /// <summary>
        /// Clones this parameter object
        /// </summary>
        /// <returns>The new cloned object</returns>
        internal EntityParameter Clone()
        {
            var clonedEntityParameter = new EntityParameter();
            clonedEntityParameter._internalEntityParameter = this._internalEntityParameter.Clone();

            return clonedEntityParameter;
        }

        /// <summary>
        /// Get the type usage for this parameter in model terms.
        /// </summary>
        /// <returns>The type usage for this parameter</returns>
        /// <remarks>Because GetTypeUsage throws CommandValidationExceptions, it should only be called from EntityCommand during command execution</remarks>
        internal TypeUsage GetTypeUsage()
        {
            return _internalEntityParameter.GetTypeUsage();
        }

        /// <summary>
        /// Reset the dirty flag on the collection
        /// </summary>
        internal void ResetIsDirty()
        {
            _internalEntityParameter.ResetIsDirty();
        }

        internal void CopyTo(DbParameter destination)
        {
            Contract.Requires(destination != null);
            ((EntityParameter)destination)._internalEntityParameter.CopyTo(this._internalEntityParameter);
        }

        internal object CompareExchangeParent(object value, object comparand)
        {
            return _internalEntityParameter.CompareExchangeParent(value, comparand);
        }

        internal void ResetParent()
        {
            _internalEntityParameter.ResetParent();
        }

        public override string ToString()
        {
            return _internalEntityParameter.ToString();
        }
    }
}