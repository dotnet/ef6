// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Class representing a function parameter
    /// </summary>
    public sealed class FunctionParameter : MetadataItem, INamedDataModelItem
    {
        internal static Func<FunctionParameter, SafeLink<EdmFunction>> DeclaringFunctionLinker = fp => fp._declaringFunction;

        internal FunctionParameter()
        {
            // testing
        }

        /// <summary>
        ///     The constructor for FunctionParameter taking in a name and a TypeUsage object
        /// </summary>
        /// <param name="name"> The name of this FunctionParameter </param>
        /// <param name="typeUsage"> The TypeUsage describing the type of this FunctionParameter </param>
        /// <param name="parameterMode"> Mode of the parameter </param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or typeUsage arguments are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal FunctionParameter(string name, TypeUsage typeUsage, ParameterMode parameterMode)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(typeUsage, "typeUsage");
            _name = name;
            _typeUsage = typeUsage;
            SetParameterMode(parameterMode);
        }

        private readonly TypeUsage _typeUsage;
        private string _name;
        private readonly SafeLink<EdmFunction> _declaringFunction = new SafeLink<EdmFunction>();

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.FunctionParameter; }
        }

        /// <summary>
        ///     Gets/Sets the mode of this parameter
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if value passed into setter is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the FunctionParameter instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.ParameterMode, false)]
        public ParameterMode Mode
        {
            get { return GetParameterMode(); }
        }

        /// <summary>
        ///     Returns the identity of the member
        /// </summary>
        internal override string Identity
        {
            get { return _name; }
        }

        /// <summary>
        ///     Returns the name of the member
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public String Name
        {
            get { return _name; }
            set
            {
                Check.NotEmpty(value, "value");

                _name = value;
            }
        }

        /// <summary>
        ///     Returns the TypeUsage object containing the type information and facets
        ///     about the type
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
        public TypeUsage TypeUsage
        {
            get { return _typeUsage; }
        }

        public string TypeName
        {
            get { return TypeUsage.EdmType.Name; }
        }

        /// <summary>
        ///     Returns the declaring function of this parameter
        /// </summary>
        public EdmFunction DeclaringFunction
        {
            get { return _declaringFunction.Value; }
        }

        /// <summary>
        ///     Overriding System.Object.ToString to provide better String representation
        ///     for this type.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     Sets the member to read only mode. Once this is done, there are no changes
        ///     that can be done to this class
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                // TypeUsage is always readonly, no reason to set it
            }
        }

        /// <summary>
        ///     The factory method for constructing the <see cref="FunctionParameter" /> object.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="edmType">The EdmType of the parameter.</param>
        /// <param name="parameterMode">
        ///     The <see cref="ParameterMode" /> of the parameter.
        /// </param>
        /// <returns>
        ///     A new, read-only instance of the <see cref="EdmFunction" /> type.
        /// </returns>
        public static FunctionParameter Create(string name, EdmType edmType, ParameterMode parameterMode)
        {
            Check.NotNull(name, "name");
            Check.NotNull(edmType, "edmType");

            var functionParameter =
                new FunctionParameter(name, TypeUsage.Create(edmType, FacetValues.NullFacetValues), parameterMode);

            functionParameter.SetReadOnly();

            return functionParameter;
        }
    }
}
