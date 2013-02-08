// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.Collections.Generic;

    public class ArubaFailure
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestCase { get; set; }
        public int Variation { get; set; }
        public DateTime Changed { get; set; }
        public string Log { get; set; }
        public ICollection<ArubaConfig> Configs { get; set; }
        public ICollection<ArubaBug> Bugs { get; set; }
    }
}