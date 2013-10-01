// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{
    public enum ArubaBugResolution
    {
        ByDesign = 1,
        Fixed = 2,
        NoRepro = 3,
        WontFix = 4,
        None = 0,
    }

    public class Bug
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Comment { get; set; }
        public Failure Failure { get; set; }
        public ArubaBugResolution? Resolution { get; set; }
    }
}

