// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Config
    {
        public int Id { get; set; }
        public string OS { get; set; }
        public string Lang { get; set; }
        public string Arch { get; set; }
        public ICollection<Failure> Failures { get; set; }
    }
}
