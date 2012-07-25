// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace AdvancedPatternsModel
{
    using System;

    public class MailRoom
    {
        public int id { get; set; }
        public Building Building { get; set; }
        public Guid BuildingId { get; set; }
    }
}
