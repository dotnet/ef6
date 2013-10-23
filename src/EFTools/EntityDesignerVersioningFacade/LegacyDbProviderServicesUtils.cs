// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;

    internal class LegacyDbProviderServicesUtils
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool CanGetDbProviderServices(IServiceProvider serviceProvider)
        {
            try
            {
                return serviceProvider.GetService(typeof(DbProviderServices)) != null;
            }
            catch (Exception)
            {
                // just swallow the exception.  Something failed with the call above.
                // this could be caused by having an out-of-date provider installed on the machine.
                // Not swallowing this exception will cause the wizard to crash.
            }

            return false;
        }
    }
}
