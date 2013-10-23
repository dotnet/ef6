// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.ComponentModel;

    /// <summary>
    ///     Used to site the tree control in the window.  Provides access to shell services.
    /// </summary>
    internal sealed class TreeGridDesignerSite : ISite
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     Create an TreeGridDesignerSite.
        /// </summary>
        /// <param name="serviceProvider"></param>
        internal TreeGridDesignerSite(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            _serviceProvider = serviceProvider;
        }

        #region ISite Members

        /// <summary>
        /// </summary>
        /// <value></value>
        public IComponent /* ISite */ Component
        {
            get
            {
                // only support IServiceProvider
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        public IContainer /* ISite */ Container
        {
            get
            {
                // only support IServiceProvider
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        public bool /* ISite */ DesignMode
        {
            get { return false; }
        }

        /// <summary>
        /// </summary>
        /// <value></value>
        public string /* ISite */ Name
        {
            get
            {
                // only support IServiceProvider
                return String.Empty;
            }
            set
            {
                // only support IServiceProvider
            }
        }

        #endregion

        #region IServiceProvider Members

        /// <summary>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object /* IServiceProvider */ GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        #endregion
    }
}
