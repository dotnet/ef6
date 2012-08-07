// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace AdvancedPatternsModel
{
    using System;
    using System.Collections.Generic;

    public class Office : UnMappedOfficeBase
    {
        public Office()
        {
            WhiteBoards = new List<Whiteboard>();
        }

        public Guid BuildingId { get; set; }
        public Building Building { get; set; }
        public IList<Whiteboard> WhiteBoards { get; set; }
    }
}
