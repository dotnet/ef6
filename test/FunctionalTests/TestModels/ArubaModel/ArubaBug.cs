// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    public enum ArubaBugResolution
    {
        ByDesign = 1,
        Fixed = 2,
        NoRepro = 3,
        WontFix = 4,
        None = 0,
    }

    public class ArubaBug
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Comment { get; set; }
        public ArubaFailure Failure { get; set; }
        public ArubaBugResolution? Resolution { get; set; }
    }
}

