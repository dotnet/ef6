// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ErrorLog
    {
        public virtual int ErrorLogID { get; set; }

        public virtual DateTime ErrorTime { get; set; }

        public virtual string UserName { get; set; }

        public virtual int ErrorNumber { get; set; }

        public virtual int? ErrorSeverity { get; set; }

        public virtual int? ErrorState { get; set; }

        public virtual string ErrorProcedure { get; set; }

        public virtual int? ErrorLine { get; set; }

        public virtual string ErrorMessage { get; set; }
    }
}
