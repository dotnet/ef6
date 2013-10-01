// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{
    using System.Collections.Generic;

    public class Failure
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestCase { get; set; }
        public int Variation { get; set; }
        public DateTime Changed { get; set; }
        public string Log { get; set; }
        public ICollection<Config> Configs { get; set; }
        public ICollection<Bug> Bugs { get; set; }
    }
}