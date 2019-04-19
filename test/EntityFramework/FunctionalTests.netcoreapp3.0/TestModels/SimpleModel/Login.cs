// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System;

    public class Login
    {
        public Login()
        {
        }

        public Login(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
        public string Username { get; set; }
    }
}
