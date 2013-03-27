// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class LightweightModificationFunctionsConfiguration
    {
        private readonly Type _type;

        private readonly ModificationFunctionsConfiguration _configuration
            = new ModificationFunctionsConfiguration();

        internal LightweightModificationFunctionsConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            _type = type;
        }

        internal ModificationFunctionsConfiguration Configuration
        {
            get { return _configuration; }
        }

        public LightweightModificationFunctionsConfiguration Insert(
            Action<LightweightInsertModificationFunctionConfiguration> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new LightweightInsertModificationFunctionConfiguration(_type);

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Insert(modificationFunctionConfiguration.Configuration);

            return this;
        }

        public LightweightModificationFunctionsConfiguration Update(
            Action<LightweightUpdateModificationFunctionConfiguration> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new LightweightUpdateModificationFunctionConfiguration(_type);

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Update(modificationFunctionConfiguration.Configuration);

            return this;
        }

        public LightweightModificationFunctionsConfiguration Delete(
            Action<LightweightDeleteModificationFunctionConfiguration> modificationFunctionConfigurationAction)
        {
            Check.NotNull(modificationFunctionConfigurationAction, "modificationFunctionConfigurationAction");

            var modificationFunctionConfiguration
                = new LightweightDeleteModificationFunctionConfiguration(_type);

            modificationFunctionConfigurationAction(modificationFunctionConfiguration);

            _configuration.Delete(modificationFunctionConfiguration.Configuration);

            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
