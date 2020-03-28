// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ProductModelIllustration
    {
        public virtual int ProductModelID
        {
            get { return _productModelID; }
            set
            {
                if (_productModelID != value)
                {
                    if (ProductModel != null
                        && ProductModel.ProductModelID != value)
                    {
                        ProductModel = null;
                    }
                    _productModelID = value;
                }
            }
        }

        private int _productModelID;

        public virtual int IllustrationID
        {
            get { return _illustrationID; }
            set
            {
                if (_illustrationID != value)
                {
                    if (Illustration != null
                        && Illustration.IllustrationID != value)
                    {
                        Illustration = null;
                    }
                    _illustrationID = value;
                }
            }
        }

        private int _illustrationID;

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Illustration Illustration
        {
            get { return _illustration; }
            set
            {
                if (!ReferenceEquals(_illustration, value))
                {
                    var previousValue = _illustration;
                    _illustration = value;
                    FixupIllustration(previousValue);
                }
            }
        }

        private Illustration _illustration;

        public virtual ProductModel ProductModel
        {
            get { return _productModel; }
            set
            {
                if (!ReferenceEquals(_productModel, value))
                {
                    var previousValue = _productModel;
                    _productModel = value;
                    FixupProductModel(previousValue);
                }
            }
        }

        private ProductModel _productModel;

        private void FixupIllustration(Illustration previousValue)
        {
            if (previousValue != null
                && previousValue.ProductModelIllustrations.Contains(this))
            {
                previousValue.ProductModelIllustrations.Remove(this);
            }

            if (Illustration != null)
            {
                if (!Illustration.ProductModelIllustrations.Contains(this))
                {
                    Illustration.ProductModelIllustrations.Add(this);
                }
                if (IllustrationID != Illustration.IllustrationID)
                {
                    IllustrationID = Illustration.IllustrationID;
                }
            }
        }

        private void FixupProductModel(ProductModel previousValue)
        {
            if (previousValue != null
                && previousValue.ProductModelIllustrations.Contains(this))
            {
                previousValue.ProductModelIllustrations.Remove(this);
            }

            if (ProductModel != null)
            {
                if (!ProductModel.ProductModelIllustrations.Contains(this))
                {
                    ProductModel.ProductModelIllustrations.Add(this);
                }
                if (ProductModelID != ProductModel.ProductModelID)
                {
                    ProductModelID = ProductModel.ProductModelID;
                }
            }
        }
    }
}
