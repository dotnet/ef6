namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Document
    {
        public virtual int DocumentID { get; set; }

        public virtual string Title { get; set; }

        public virtual string FileName { get; set; }

        public virtual string FileExtension { get; set; }

        public virtual string Revision { get; set; }

        public virtual int ChangeNumber { get; set; }

        public virtual byte Status { get; set; }

        public virtual string DocumentSummary { get; set; }

        public virtual byte[] Document1 { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductDocument> ProductDocuments
        {
            get
            {
                if (_productDocuments == null)
                {
                    var newCollection = new FixupCollection<ProductDocument>();
                    newCollection.CollectionChanged += FixupProductDocuments;
                    _productDocuments = newCollection;
                }
                return _productDocuments;
            }
            set
            {
                if (!ReferenceEquals(_productDocuments, value))
                {
                    var previousValue = _productDocuments as FixupCollection<ProductDocument>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductDocuments;
                    }
                    _productDocuments = value;
                    var newValue = value as FixupCollection<ProductDocument>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductDocuments;
                    }
                }
            }
        }
        private ICollection<ProductDocument> _productDocuments;

        private void FixupProductDocuments(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductDocument item in e.NewItems)
                {
                    item.Document = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductDocument item in e.OldItems)
                {
                    if (ReferenceEquals(item.Document, this))
                    {
                        item.Document = null;
                    }
                }
            }
        }
    }
}