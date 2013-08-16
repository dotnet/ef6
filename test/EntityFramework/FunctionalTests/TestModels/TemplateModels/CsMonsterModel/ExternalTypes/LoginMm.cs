// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Another.Place
{
    using System.Collections.Generic;
    using FunctionalTests.ProductivityApi.TemplateModels.CsMonsterModel;

    public class LoginMm
    {
        public LoginMm()
        {
            SentMessages = new HashSet<MessageMm>();
            ReceivedMessages = new HashSet<MessageMm>();
            Orders = new HashSet<OrderMm>();
        }

        public string Username { get; set; }
        public int CustomerId { get; set; }

        public virtual CustomerMm Customer { get; set; }
        public virtual LastLoginMm LastLogin { get; set; }
        public virtual ICollection<MessageMm> SentMessages { get; set; }
        public virtual ICollection<MessageMm> ReceivedMessages { get; set; }
        public virtual ICollection<OrderMm> Orders { get; set; }
    }
}
