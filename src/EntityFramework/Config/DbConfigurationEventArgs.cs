// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Event arguments passed to <see cref="DbConfiguration.OnLockingConfiguration"/> event handlers.
    /// </summary>
    [Serializable]
    public class DbConfigurationEventArgs : EventArgs
    {
        internal DbConfigurationEventArgs(IDbConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            Configuration = configuration;
        }

        /// <summary>
        /// The <see cref="DbConfiguration"/> that is about to be locked.
        /// </summary>
        public IDbConfiguration Configuration { get; private set; }
    }
}
