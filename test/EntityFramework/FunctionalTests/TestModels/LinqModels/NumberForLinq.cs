// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace SimpleModel
{
    using System;
    using System.Globalization;

    public class NumberForLinq : BaseTypeForLinq
    {
        public NumberForLinq() { }
        public NumberForLinq(int value, string name) { Value = value; Name = name; }
        public int Value { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "ID: {0}, Value: {1}, Name: {2}", Id, Value, Name);
        }
    }
}
