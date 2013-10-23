// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;

    [Flags]
    internal enum ErrorClass : uint
    {
        None = 0,
        ParseError = 1,
        ResolveError = 2,
        Runtime_CSDL = 4,
        Runtime_SSDL = 8,
        Runtime_MSL = 16,
        Runtime_ViewGen = 32,
        Escher_CSDL = 64,
        Escher_SSDL = 128,
        Escher_MSL = 256,
        Escher_UpdateModelFromDB = 512,

        Runtime_All = Runtime_CSDL | Runtime_SSDL | Runtime_MSL | Runtime_ViewGen,
        Escher_All = Escher_CSDL | Escher_SSDL | Escher_MSL | Escher_UpdateModelFromDB,

        All = ParseError | ResolveError | Runtime_All | Escher_All
    }
}
