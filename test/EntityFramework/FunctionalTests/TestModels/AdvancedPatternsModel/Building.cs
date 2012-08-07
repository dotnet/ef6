// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace AdvancedPatternsModel
{
    using System;
    using System.Collections.Generic;

    public class Building
    {
        public Building()
        {
            Offices = new List<Office>();
            MailRooms = new List<MailRoom>();
        }

        public Guid BuildingId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public virtual ICollection<Office> Offices { get; set; }
        public virtual IList<MailRoom> MailRooms { get; set; }
        public Address Address { get; set; }

        public int? PrincipalMailRoomId { get; set; }
        public MailRoom PrincipalMailRoom { get; set; }

        public string NotInModel { get; set; }

        private string _noGetter = "NoGetter";

        public string NoGetter
        {
            set { _noGetter = value; }
        }

        public string GetNoGetterValue()
        {
            return _noGetter;
        }

        public string NoSetter
        {
            get { return "NoSetter"; }
        }
    }
}
