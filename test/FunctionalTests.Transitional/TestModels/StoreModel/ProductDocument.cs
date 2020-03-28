// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ProductDocument
    {
        public virtual int ProductID
        {
            get { return _productID; }
            set
            {
                if (_productID != value)
                {
                    if (Product != null
                        && Product.ProductID != value)
                    {
                        Product = null;
                    }
                    _productID = value;
                }
            }
        }

        private int _productID;

        public virtual int DocumentID
        {
            get { return _documentID; }
            set
            {
                if (_documentID != value)
                {
                    if (Document != null
                        && Document.DocumentID != value)
                    {
                        Document = null;
                    }
                    _documentID = value;
                }
            }
        }

        private int _documentID;

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Document Document
        {
            get { return _document; }
            set
            {
                if (!ReferenceEquals(_document, value))
                {
                    var previousValue = _document;
                    _document = value;
                    FixupDocument(previousValue);
                }
            }
        }

        private Document _document;

        public virtual Product Product
        {
            get { return _product; }
            set
            {
                if (!ReferenceEquals(_product, value))
                {
                    var previousValue = _product;
                    _product = value;
                    FixupProduct(previousValue);
                }
            }
        }

        private Product _product;

        private void FixupDocument(Document previousValue)
        {
            if (previousValue != null
                && previousValue.ProductDocuments.Contains(this))
            {
                previousValue.ProductDocuments.Remove(this);
            }

            if (Document != null)
            {
                if (!Document.ProductDocuments.Contains(this))
                {
                    Document.ProductDocuments.Add(this);
                }
                if (DocumentID != Document.DocumentID)
                {
                    DocumentID = Document.DocumentID;
                }
            }
        }

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null
                && previousValue.ProductDocuments.Contains(this))
            {
                previousValue.ProductDocuments.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.ProductDocuments.Contains(this))
                {
                    Product.ProductDocuments.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }
    }
}
