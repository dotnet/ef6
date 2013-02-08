// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.Collections.Generic;

    public class ArubaOwner
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Alias { get; set; }
        public ArubaRun OwnedRun { get; set; }
        public ICollection<ArubaBug> Bugs { get; set; }
    }
}