// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;

    public class Product
    {
        public virtual int ProductID { get; set; }

        public virtual string Name { get; set; }

        public virtual string ProductNumber { get; set; }

        public virtual bool MakeFlag { get; set; }

        public virtual bool FinishedGoodsFlag { get; set; }

        public virtual short SafetyStockLevel { get; set; }

        public virtual short ReorderPoint { get; set; }

        public virtual decimal StandardCost { get; set; }

        public virtual decimal ListPrice { get; set; }

        public virtual string Size { get; set; }

        public virtual decimal? Weight { get; set; }

        public virtual int DaysToManufacture { get; set; }

        public virtual string ProductLine { get; set; }

        public virtual string Class { get; set; }

        public virtual int? ProductSubcategoryID
        {
            get { return _productSubcategoryID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_productSubcategoryID != value)
                    {
                        if (ProductSubcategory != null && ProductSubcategory.ProductSubcategoryID != value)
                        {
                            ProductSubcategory = null;
                        }
                        _productSubcategoryID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int? _productSubcategoryID;

        public virtual int? ProductModelID
        {
            get { return _productModelID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_productModelID != value)
                    {
                        if (ProductModel != null && ProductModel.ProductModelID != value)
                        {
                            ProductModel = null;
                        }
                        _productModelID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int? _productModelID;

        public virtual DateTime SellStartDate { get; set; }

        [Required]
        public virtual DateTime? SellEndDate { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<BillOfMaterials> BillOfMaterials
        {
            get
            {
                if (_billOfMaterials == null)
                {
                    var newCollection = new FixupCollection<BillOfMaterials>();
                    newCollection.CollectionChanged += FixupBillOfMaterials;
                    _billOfMaterials = newCollection;
                }
                return _billOfMaterials;
            }
            set
            {
                if (!ReferenceEquals(_billOfMaterials, value))
                {
                    var previousValue = _billOfMaterials as FixupCollection<BillOfMaterials>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupBillOfMaterials;
                    }
                    _billOfMaterials = value;
                    var newValue = value as FixupCollection<BillOfMaterials>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupBillOfMaterials;
                    }
                }
            }
        }
        private ICollection<BillOfMaterials> _billOfMaterials;

        public virtual ICollection<BillOfMaterials> BillOfMaterials1
        {
            get
            {
                if (_billOfMaterials1 == null)
                {
                    var newCollection = new FixupCollection<BillOfMaterials>();
                    newCollection.CollectionChanged += FixupBillOfMaterials1;
                    _billOfMaterials1 = newCollection;
                }
                return _billOfMaterials1;
            }
            set
            {
                if (!ReferenceEquals(_billOfMaterials1, value))
                {
                    var previousValue = _billOfMaterials1 as FixupCollection<BillOfMaterials>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupBillOfMaterials1;
                    }
                    _billOfMaterials1 = value;
                    var newValue = value as FixupCollection<BillOfMaterials>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupBillOfMaterials1;
                    }
                }
            }
        }
        private ICollection<BillOfMaterials> _billOfMaterials1;

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

        public virtual ProductSubcategory ProductSubcategory
        {
            get { return _productSubcategory; }
            set
            {
                if (!ReferenceEquals(_productSubcategory, value))
                {
                    var previousValue = _productSubcategory;
                    _productSubcategory = value;
                    FixupProductSubcategory(previousValue);
                }
            }
        }
        private ProductSubcategory _productSubcategory;

        public virtual UnitMeasure SizeUnitMeasure { get; set; }
        public virtual UnitMeasure WeightUnitMeasure { get; set; }

        public virtual ICollection<ProductCostHistory> ProductCostHistories
        {
            get
            {
                if (_productCostHistories == null)
                {
                    var newCollection = new FixupCollection<ProductCostHistory>();
                    newCollection.CollectionChanged += FixupProductCostHistories;
                    _productCostHistories = newCollection;
                }
                return _productCostHistories;
            }
            set
            {
                if (!ReferenceEquals(_productCostHistories, value))
                {
                    var previousValue = _productCostHistories as FixupCollection<ProductCostHistory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductCostHistories;
                    }
                    _productCostHistories = value;
                    var newValue = value as FixupCollection<ProductCostHistory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductCostHistories;
                    }
                }
            }
        }
        private ICollection<ProductCostHistory> _productCostHistories;

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

        public virtual ICollection<ProductInventory> ProductInventories
        {
            get
            {
                if (_productInventories == null)
                {
                    var newCollection = new FixupCollection<ProductInventory>();
                    newCollection.CollectionChanged += FixupProductInventories;
                    _productInventories = newCollection;
                }
                return _productInventories;
            }
            set
            {
                if (!ReferenceEquals(_productInventories, value))
                {
                    var previousValue = _productInventories as FixupCollection<ProductInventory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductInventories;
                    }
                    _productInventories = value;
                    var newValue = value as FixupCollection<ProductInventory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductInventories;
                    }
                }
            }
        }
        private ICollection<ProductInventory> _productInventories;

        public virtual ICollection<ProductListPriceHistory> ProductListPriceHistories
        {
            get
            {
                if (_productListPriceHistories == null)
                {
                    var newCollection = new FixupCollection<ProductListPriceHistory>();
                    newCollection.CollectionChanged += FixupProductListPriceHistories;
                    _productListPriceHistories = newCollection;
                }
                return _productListPriceHistories;
            }
            set
            {
                if (!ReferenceEquals(_productListPriceHistories, value))
                {
                    var previousValue = _productListPriceHistories as FixupCollection<ProductListPriceHistory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductListPriceHistories;
                    }
                    _productListPriceHistories = value;
                    var newValue = value as FixupCollection<ProductListPriceHistory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductListPriceHistories;
                    }
                }
            }
        }
        private ICollection<ProductListPriceHistory> _productListPriceHistories;

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

        public virtual ICollection<ProductReview> ProductReviews
        {
            get
            {
                if (_productReviews == null)
                {
                    var newCollection = new FixupCollection<ProductReview>();
                    newCollection.CollectionChanged += FixupProductReviews;
                    _productReviews = newCollection;
                }
                return _productReviews;
            }
            set
            {
                if (!ReferenceEquals(_productReviews, value))
                {
                    var previousValue = _productReviews as FixupCollection<ProductReview>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductReviews;
                    }
                    _productReviews = value;
                    var newValue = value as FixupCollection<ProductReview>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductReviews;
                    }
                }
            }
        }
        private ICollection<ProductReview> _productReviews;

        public virtual ICollection<ProductVendor> ProductVendors
        {
            get
            {
                if (_productVendors == null)
                {
                    var newCollection = new FixupCollection<ProductVendor>();
                    newCollection.CollectionChanged += FixupProductVendors;
                    _productVendors = newCollection;
                }
                return _productVendors;
            }
            set
            {
                if (!ReferenceEquals(_productVendors, value))
                {
                    var previousValue = _productVendors as FixupCollection<ProductVendor>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductVendors;
                    }
                    _productVendors = value;
                    var newValue = value as FixupCollection<ProductVendor>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductVendors;
                    }
                }
            }
        }
        private ICollection<ProductVendor> _productVendors;

        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails
        {
            get
            {
                if (_purchaseOrderDetails == null)
                {
                    var newCollection = new FixupCollection<PurchaseOrderDetail>();
                    newCollection.CollectionChanged += FixupPurchaseOrderDetails;
                    _purchaseOrderDetails = newCollection;
                }
                return _purchaseOrderDetails;
            }
            set
            {
                if (!ReferenceEquals(_purchaseOrderDetails, value))
                {
                    var previousValue = _purchaseOrderDetails as FixupCollection<PurchaseOrderDetail>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupPurchaseOrderDetails;
                    }
                    _purchaseOrderDetails = value;
                    var newValue = value as FixupCollection<PurchaseOrderDetail>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupPurchaseOrderDetails;
                    }
                }
            }
        }
        private ICollection<PurchaseOrderDetail> _purchaseOrderDetails;

        public virtual ICollection<ShoppingCartItem> ShoppingCartItems
        {
            get
            {
                if (_shoppingCartItems == null)
                {
                    var newCollection = new FixupCollection<ShoppingCartItem>();
                    newCollection.CollectionChanged += FixupShoppingCartItems;
                    _shoppingCartItems = newCollection;
                }
                return _shoppingCartItems;
            }
            set
            {
                if (!ReferenceEquals(_shoppingCartItems, value))
                {
                    var previousValue = _shoppingCartItems as FixupCollection<ShoppingCartItem>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupShoppingCartItems;
                    }
                    _shoppingCartItems = value;
                    var newValue = value as FixupCollection<ShoppingCartItem>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupShoppingCartItems;
                    }
                }
            }
        }
        private ICollection<ShoppingCartItem> _shoppingCartItems;

        public virtual ICollection<SpecialOfferProduct> SpecialOfferProducts
        {
            get
            {
                if (_specialOfferProducts == null)
                {
                    var newCollection = new FixupCollection<SpecialOfferProduct>();
                    newCollection.CollectionChanged += FixupSpecialOfferProducts;
                    _specialOfferProducts = newCollection;
                }
                return _specialOfferProducts;
            }
            set
            {
                if (!ReferenceEquals(_specialOfferProducts, value))
                {
                    var previousValue = _specialOfferProducts as FixupCollection<SpecialOfferProduct>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSpecialOfferProducts;
                    }
                    _specialOfferProducts = value;
                    var newValue = value as FixupCollection<SpecialOfferProduct>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSpecialOfferProducts;
                    }
                }
            }
        }
        private ICollection<SpecialOfferProduct> _specialOfferProducts;

        public virtual ICollection<TransactionHistory> TransactionHistories
        {
            get
            {
                if (_transactionHistories == null)
                {
                    var newCollection = new FixupCollection<TransactionHistory>();
                    newCollection.CollectionChanged += FixupTransactionHistories;
                    _transactionHistories = newCollection;
                }
                return _transactionHistories;
            }
            set
            {
                if (!ReferenceEquals(_transactionHistories, value))
                {
                    var previousValue = _transactionHistories as FixupCollection<TransactionHistory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupTransactionHistories;
                    }
                    _transactionHistories = value;
                    var newValue = value as FixupCollection<TransactionHistory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupTransactionHistories;
                    }
                }
            }
        }
        private ICollection<TransactionHistory> _transactionHistories;

        public virtual ICollection<WorkOrder> WorkOrders
        {
            get
            {
                if (_workOrders == null)
                {
                    var newCollection = new FixupCollection<WorkOrder>();
                    newCollection.CollectionChanged += FixupWorkOrders;
                    _workOrders = newCollection;
                }
                return _workOrders;
            }
            set
            {
                if (!ReferenceEquals(_workOrders, value))
                {
                    var previousValue = _workOrders as FixupCollection<WorkOrder>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupWorkOrders;
                    }
                    _workOrders = value;
                    var newValue = value as FixupCollection<WorkOrder>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupWorkOrders;
                    }
                }
            }
        }
        private ICollection<WorkOrder> _workOrders;

        private bool _settingFK;

        private void FixupProductModel(ProductModel previousValue)
        {
            if (previousValue != null && previousValue.Products.Contains(this))
            {
                previousValue.Products.Remove(this);
            }

            if (ProductModel != null)
            {
                if (!ProductModel.Products.Contains(this))
                {
                    ProductModel.Products.Add(this);
                }
                if (ProductModelID != ProductModel.ProductModelID)
                {
                    ProductModelID = ProductModel.ProductModelID;
                }
            }
            else if (!_settingFK)
            {
                ProductModelID = null;
            }
        }

        private void FixupProductSubcategory(ProductSubcategory previousValue)
        {
            if (previousValue != null && previousValue.Products.Contains(this))
            {
                previousValue.Products.Remove(this);
            }

            if (ProductSubcategory != null)
            {
                if (!ProductSubcategory.Products.Contains(this))
                {
                    ProductSubcategory.Products.Add(this);
                }
                if (ProductSubcategoryID != ProductSubcategory.ProductSubcategoryID)
                {
                    ProductSubcategoryID = ProductSubcategory.ProductSubcategoryID;
                }
            }
            else if (!_settingFK)
            {
                ProductSubcategoryID = null;
            }
        }

        private void FixupBillOfMaterials(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (BillOfMaterials item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (BillOfMaterials item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupBillOfMaterials1(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (BillOfMaterials item in e.NewItems)
                {
                    item.Product1 = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (BillOfMaterials item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product1, this))
                    {
                        item.Product1 = null;
                    }
                }
            }
        }

        private void FixupProductCostHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductCostHistory item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductCostHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupProductDocuments(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductDocument item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductDocument item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupProductInventories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductInventory item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductInventory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupProductListPriceHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductListPriceHistory item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductListPriceHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupProductProductPhotoes(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductProductPhoto item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductProductPhoto item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupProductReviews(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductReview item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductReview item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupProductVendors(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductVendor item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductVendor item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupPurchaseOrderDetails(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PurchaseOrderDetail item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (PurchaseOrderDetail item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupShoppingCartItems(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ShoppingCartItem item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ShoppingCartItem item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupSpecialOfferProducts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SpecialOfferProduct item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SpecialOfferProduct item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupTransactionHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (TransactionHistory item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (TransactionHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }

        private void FixupWorkOrders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (WorkOrder item in e.NewItems)
                {
                    item.Product = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (WorkOrder item in e.OldItems)
                {
                    if (ReferenceEquals(item.Product, this))
                    {
                        item.Product = null;
                    }
                }
            }
        }
    }
}