// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.Collections.Generic;

    public class ArubaConfig
    {
        public int Id { get; set; }
        public string OS { get; set; }
        public string Lang { get; set; }
        public string Arch { get; set; }
        public ICollection<ArubaFailure> Failures { get; set; }
    }
}