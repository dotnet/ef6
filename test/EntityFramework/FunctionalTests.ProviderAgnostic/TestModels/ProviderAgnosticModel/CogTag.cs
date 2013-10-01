// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{

    public class CogTag
    {
        public Guid Id { get; set; }
        public string Note { get; set; }

        public virtual Gear Gear { get; set; }
    }
}
