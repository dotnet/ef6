// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;

    public class BillOfMaterials
    {
        public virtual int BillOfMaterialsID { get; set; }

        public virtual int? ProductAssemblyID
        {
            get { return _productAssemblyID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_productAssemblyID != value)
                    {
                        if (Product1 != null && Product1.ProductID != value)
                        {
                            Product1 = null;
                        }
                        _productAssemblyID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int? _productAssemblyID;

        public virtual int ComponentID
        {
            get { return _componentID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_componentID != value)
                    {
                        if (Product != null && Product.ProductID != value)
                        {
                            Product = null;
                        }
                        _componentID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _componentID;

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

        public virtual string UnitMeasureCode
        {
            get { return _unitMeasureCode; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_unitMeasureCode != value)
                    {
                        if (UnitMeasure != null && UnitMeasure.UnitMeasureCode != value)
                        {
                            UnitMeasure = null;
                        }
                        _unitMeasureCode = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private string _unitMeasureCode;

        public virtual short BOMLevel { get; set; }

        public virtual decimal PerAssemblyQty { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

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

        public virtual Product Product1
        {
            get { return _product1; }
            set
            {
                if (!ReferenceEquals(_product1, value))
                {
                    var previousValue = _product1;
                    _product1 = value;
                    FixupProduct1(previousValue);
                }
            }
        }
        private Product _product1;

        public virtual UnitMeasure UnitMeasure { get; set; }

        private bool _settingFK;

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null && previousValue.BillOfMaterials.Contains(this))
            {
                previousValue.BillOfMaterials.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.BillOfMaterials.Contains(this))
                {
                    Product.BillOfMaterials.Add(this);
                }
                if (ComponentID != Product.ProductID)
                {
                    ComponentID = Product.ProductID;
                }
            }
        }

        private void FixupProduct1(Product previousValue)
        {
            if (previousValue != null && previousValue.BillOfMaterials1.Contains(this))
            {
                previousValue.BillOfMaterials1.Remove(this);
            }

            if (Product1 != null)
            {
                if (!Product1.BillOfMaterials1.Contains(this))
                {
                    Product1.BillOfMaterials1.Add(this);
                }
                if (ProductAssemblyID != Product1.ProductID)
                {
                    ProductAssemblyID = Product1.ProductID;
                }
            }
            else if (!_settingFK)
            {
                ProductAssemblyID = null;
            }
        }
    }
}