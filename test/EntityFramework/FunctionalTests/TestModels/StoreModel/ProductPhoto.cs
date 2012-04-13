namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class LargePhoto
    {
        public virtual byte[] Photo { get; set; }
    }

    public class ProductPhoto
    {
        public virtual int ProductPhotoID { get; set; }

        public virtual byte[] ThumbNailPhoto { get; set; }

        public virtual string ThumbnailPhotoFileName { get; set; }

        public virtual LargePhoto LargePhoto { get; set; }

        public virtual string LargePhotoFileName { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductProductPhoto> ProductProductPhotoes
        {
            get
            {
                if (_productProductPhotoes == null)
                {
                    var newCollection = new FixupCollection<ProductProductPhoto>();
                    newCollection.CollectionChanged += FixupProductProductPhotoes;
                    _productProductPhotoes = newCollection;
                }
                return _productProductPhotoes;
            }
            set
            {
                if (!ReferenceEquals(_productProductPhotoes, value))
                {
                    var previousValue = _productProductPhotoes as FixupCollection<ProductProductPhoto>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductProductPhotoes;
                    }
                    _productProductPhotoes = value;
                    var newValue = value as FixupCollection<ProductProductPhoto>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductProductPhotoes;
                    }
                }
            }
        }
        private ICollection<ProductProductPhoto> _productProductPhotoes;

        private void FixupProductProductPhotoes(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductProductPhoto item in e.NewItems)
                {
                    item.ProductPhoto = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductProductPhoto item in e.OldItems)
                {
                    if (ReferenceEquals(item.ProductPhoto, this))
                    {
                        item.ProductPhoto = null;
                    }
                }
            }
        }
    }
}