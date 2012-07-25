// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TransactionHistoryArchive
    {
        [Key]
        public virtual int TransactionID { get; set; }
        public virtual int ProductID { get; set; }
        public virtual int ReferenceOrderID { get; set; }
        public virtual int ReferenceOrderLineID { get; set; }
        public virtual DateTime TransactionDate { get; set; }
        public virtual string TransactionType { get; set; }
        public virtual int Quantity { get; set; }
        public virtual decimal ActualCost { get; set; }
        public virtual DateTime ModifiedDate { get; set; }
    }
}