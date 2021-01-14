// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Xunit;

    public class AttributeBasedOCLoading : FunctionalTestBase
    {
#if NET452
        [Fact]
        public void O_space_types_are_discovered_when_using_attribute_based_mapping()
        {
            var edmItemCollection = new EdmItemCollection(
                new[]
                    {
                        XDocument.Load(
                            typeof(AttributeBasedOCLoading).Assembly.GetManifestResourceStream(
                                "System.Data.Entity.TestModels.TemplateModels.Schemas.MonsterModel.csdl")).CreateReader()
                    });

            var storeItemCollection = new StoreItemCollection(
                new[]
                    {
                        XDocument.Load(
                            typeof(AttributeBasedOCLoading).Assembly.GetManifestResourceStream(
                                "System.Data.Entity.TestModels.TemplateModels.Schemas.MonsterModel.ssdl")).CreateReader()
                    });
            var storageMappingItemCollection = LoadMsl(
                edmItemCollection, storeItemCollection, XDocument.Load(
                    typeof(AttributeBasedOCLoading).Assembly.GetManifestResourceStream(
                        "System.Data.Entity.TestModels.TemplateModels.Schemas.MonsterModel.msl")));

            var workspace = new MetadataWorkspace(
                () => edmItemCollection,
                () => storeItemCollection,
                () => storageMappingItemCollection);

            var assembly = BuildEntitiesAssembly(ObjectLayer);
            workspace.LoadFromAssembly(assembly);

            var oSpaceItems = (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace);

            // Sanity checks that types/relationships were actually found
            // Entity types
            var entityTypes = oSpaceItems
                .OfType<EdmType>()
                .Where(i => i.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                .ToList();

            Assert.Equal(
                new[]
                    {
                        "BackOrderLine2Mm", "BackOrderLineMm", "BarcodeDetailMm", "BarcodeMm", "ComplaintMm", "ComputerDetailMm",
                        "ComputerMm", "CustomerInfoMm", "CustomerMm", "DiscontinuedProductMm", "DriverMm", "IncorrectScanMm",
                        "LastLoginMm", "LicenseMm", "LoginMm", "MessageMm", "OrderLineMm", "OrderMm", "OrderNoteMm",
                        "OrderQualityCheckMm", "PageViewMm", "PasswordResetMm", "ProductDetailMm", "ProductMm", "ProductPageViewMm",
                        "ProductPhotoMm", "ProductReviewMm", "ProductWebFeatureMm", "ResolutionMm", "RSATokenMm", "SmartCardMm",
                        "SupplierInfoMm", "SupplierLogoMm", "SupplierMm", "SuspiciousActivityMm"
                    },
                entityTypes.Select(i => i.Name).OrderBy(n => n));

            Assert.True(entityTypes.All(e => e.NamespaceName == "BuildMonsterModel"));
            Assert.True(entityTypes.All(e => oSpaceItems.GetClrType((StructuralType)e).Assembly == assembly));

            // Complex types
            var complexTypes = oSpaceItems
                .OfType<EdmType>()
                .Where(i => i.BuiltInTypeKind == BuiltInTypeKind.ComplexType)
                .ToList();

            Assert.Equal(
                new[] { "AuditInfoMm", "ConcurrencyInfoMm", "ContactDetailsMm", "DimensionsMm", "PhoneMm" },
                complexTypes.Select(i => i.Name).OrderBy(n => n));

            Assert.True(complexTypes.All(e => e.NamespaceName == "BuildMonsterModel"));
            Assert.True(complexTypes.All(e => oSpaceItems.GetClrType((StructuralType)e).Assembly == assembly));

            // Enum types
            var enumTypes = oSpaceItems
                .OfType<EdmType>()
                .Where(i => i.BuiltInTypeKind == BuiltInTypeKind.EnumType)
                .ToList();

            Assert.Equal(
                new[] { "LicenseStateMm", "PhoneTypeMm" },
                enumTypes.Select(i => i.Name).OrderBy(n => n));

            Assert.True(enumTypes.All(e => e.NamespaceName == "BuildMonsterModel"));
            Assert.True(enumTypes.All(e => oSpaceItems.GetClrType((EnumType)e).Assembly == assembly));

            // Associations
            var associations = oSpaceItems
                .OfType<AssociationType>()
                .Where(i => i.BuiltInTypeKind == BuiltInTypeKind.AssociationType)
                .ToList();

            Assert.Equal(
                new[]
                    {
                        "Barcode_BarcodeDetail", "Barcode_IncorrectScanActual", "Barcode_IncorrectScanExpected", "Complaint_Resolution",
                        "Computer_ComputerDetail", "Customer_Complaints", "Customer_CustomerInfo", "Customer_Logins", "Customer_Orders",
                        "DiscontinuedProduct_Replacement", "Driver_License", "Husband_Wife", "LastLogin_SmartCard", "Login_LastLogin",
                        "Login_Orders", "Login_PageViews", "Login_PasswordResets", "Login_ReceivedMessages", "Login_RSAToken",
                        "Login_SentMessages", "Login_SmartCard", "Login_SuspiciousActivity", "Order_OrderLines", "Order_OrderNotes",
                        "Order_QualityCheck", "Product_Barcodes", "Product_OrderLines", "Product_ProductDetail", "Product_ProductPageViews",
                        "Product_ProductPhoto", "Product_ProductReview", "Products_Suppliers", "ProductWebFeature_ProductPhoto",
                        "ProductWebFeature_ProductReview", "Supplier_BackOrderLines", "Supplier_SupplierInfo", "Supplier_SupplierLogo"
                    },
                associations.Select(i => i.Name).OrderBy(n => n));

            Assert.True(associations.All(e => e.NamespaceName == "MonsterNamespace"));
        }
#endif

        private static StorageMappingItemCollection LoadMsl(
            EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection, XDocument msl)
        {
            IList<EdmSchemaError> errors;
            return StorageMappingItemCollection.Create(
                edmItemCollection,
                storeItemCollection,
                new[] { msl.CreateReader() },
                null,
                out errors);
        }

        public Assembly BuildEntitiesAssembly(string source)
        {
            var options = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true
                };

            options.ReferencedAssemblies.Add(typeof(string).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(DbContext).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(DbConnection).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(Component).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(DataContractSerializer).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(XmlSerializer).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(IOrderedQueryable).Assembly.Location);

            return CodeDomProvider.CreateProvider("cs").CompileAssemblyFromSource(options, source).CompiledAssembly;
        }

        private const string ObjectLayer = @"
//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

[assembly: EdmSchemaAttribute()]
#region EDM Relationship Metadata

[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Customer_Complaints"", ""Customer"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.CustomerMm), ""Complaints"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.ComplaintMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_SentMessages"", ""Sender"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LoginMm), ""Message"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.MessageMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_ReceivedMessages"", ""Recipient"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LoginMm), ""Message"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.MessageMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Customer_CustomerInfo"", ""Customer"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.CustomerMm), ""Info"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.CustomerInfoMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Supplier_SupplierInfo"", ""Supplier"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.SupplierMm), ""Info"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.SupplierInfoMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_Orders"", ""Login"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.LoginMm), ""Orders"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.OrderMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Order_OrderNotes"", ""Order"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.OrderMm), ""Notes"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.OrderNoteMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Order_QualityCheck"", ""Order"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.OrderMm), ""QualityCheck"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.OrderQualityCheckMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Supplier_SupplierLogo"", ""Supplier"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.SupplierMm), ""Logo"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.SupplierLogoMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Customer_Orders"", ""Customer"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.CustomerMm), ""Order"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.OrderMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Customer_Logins"", ""Customer"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.CustomerMm), ""Logins"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.LoginMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_LastLogin"", ""Login"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LoginMm), ""LastLogin"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.LastLoginMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""LastLogin_SmartCard"", ""LastLogin"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.LastLoginMm), ""SmartCard"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.SmartCardMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Order_OrderLines"", ""Order"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.OrderMm), ""OrderLines"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.OrderLineMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Product_OrderLines"", ""Product"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ProductMm), ""OrderLines"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.OrderLineMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Products_Suppliers"", ""Products"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.ProductMm), ""Suppliers"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.SupplierMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Supplier_BackOrderLines"", ""Supplier"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.SupplierMm), ""BackOrderLines"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.BackOrderLineMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""DiscontinuedProduct_Replacement"", ""Replacement"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.ProductMm), ""DiscontinuedProduct"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.DiscontinuedProductMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Product_ProductDetail"", ""Product"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ProductMm), ""ProductDetail"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.ProductDetailMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Product_ProductReview"", ""Product"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ProductMm), ""ProductReview"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.ProductReviewMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Product_ProductPhoto"", ""Product"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ProductMm), ""ProductPhoto"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.ProductPhotoMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""ProductWebFeature_ProductPhoto"", ""ProductWebFeature"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.ProductWebFeatureMm), ""ProductPhoto"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.ProductPhotoMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""ProductWebFeature_ProductReview"", ""ProductWebFeature"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.ProductWebFeatureMm), ""ProductReview"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.ProductReviewMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Complaint_Resolution"", ""Complaint"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ComplaintMm), ""Resolution"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.ResolutionMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Barcode_IncorrectScanExpected"", ""Barcode"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.BarcodeMm), ""Expected"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.IncorrectScanMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Husband_Wife"", ""Husband"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.CustomerMm), ""Wife"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.CustomerMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Barcode_IncorrectScanActual"", ""Barcode"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.BarcodeMm), ""Actual"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.IncorrectScanMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Product_Barcodes"", ""Product"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ProductMm), ""Barcodes"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.BarcodeMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Barcode_BarcodeDetail"", ""Barcode"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.BarcodeMm), ""BarcodeDetail"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.BarcodeDetailMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_SuspiciousActivity"", ""Login"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.LoginMm), ""Activity"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.SuspiciousActivityMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_RSAToken"", ""Login"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LoginMm), ""RSAToken"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.RSATokenMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_SmartCard"", ""Login"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LoginMm), ""SmartCard"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.ZeroOrOne, typeof(BuildMonsterModel.SmartCardMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_PasswordResets"", ""Login"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LoginMm), ""PasswordResets"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.PasswordResetMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Product_ProductPageViews"", ""Product"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ProductMm), ""ProductPageViews"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.ProductPageViewMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Login_PageViews"", ""Login"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LoginMm), ""PageViews"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many, typeof(BuildMonsterModel.PageViewMm), true)]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Computer_ComputerDetail"", ""Computer"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ComputerMm), ""ComputerDetail"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.ComputerDetailMm))]
[assembly: EdmRelationshipAttribute(""MonsterNamespace"", ""Driver_License"", ""Driver"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.DriverMm), ""License"", System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.One, typeof(BuildMonsterModel.LicenseMm), true)]

#endregion

namespace BuildMonsterModel
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class MonsterModel : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new MonsterModel object using the connection string found in the 'MonsterModel' section of the application configuration file.
        /// </summary>
        public MonsterModel() : base(""name=MonsterModel"", ""MonsterModel"")
        {
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new MonsterModel object.
        /// </summary>
        public MonsterModel(string connectionString) : base(connectionString, ""MonsterModel"")
        {
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new MonsterModel object.
        /// </summary>
        public MonsterModel(EntityConnection connection) : base(connection, ""MonsterModel"")
        {
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<CustomerMm> Customer
        {
            get
            {
                if ((_Customer == null))
                {
                    _Customer = base.CreateObjectSet<CustomerMm>(""Customer"");
                }
                return _Customer;
            }
        }
        private ObjectSet<CustomerMm> _Customer;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<BarcodeMm> Barcode
        {
            get
            {
                if ((_Barcode == null))
                {
                    _Barcode = base.CreateObjectSet<BarcodeMm>(""Barcode"");
                }
                return _Barcode;
            }
        }
        private ObjectSet<BarcodeMm> _Barcode;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<IncorrectScanMm> IncorrectScan
        {
            get
            {
                if ((_IncorrectScan == null))
                {
                    _IncorrectScan = base.CreateObjectSet<IncorrectScanMm>(""IncorrectScan"");
                }
                return _IncorrectScan;
            }
        }
        private ObjectSet<IncorrectScanMm> _IncorrectScan;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<BarcodeDetailMm> BarcodeDetail
        {
            get
            {
                if ((_BarcodeDetail == null))
                {
                    _BarcodeDetail = base.CreateObjectSet<BarcodeDetailMm>(""BarcodeDetail"");
                }
                return _BarcodeDetail;
            }
        }
        private ObjectSet<BarcodeDetailMm> _BarcodeDetail;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ComplaintMm> Complaint
        {
            get
            {
                if ((_Complaint == null))
                {
                    _Complaint = base.CreateObjectSet<ComplaintMm>(""Complaint"");
                }
                return _Complaint;
            }
        }
        private ObjectSet<ComplaintMm> _Complaint;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ResolutionMm> Resolution
        {
            get
            {
                if ((_Resolution == null))
                {
                    _Resolution = base.CreateObjectSet<ResolutionMm>(""Resolution"");
                }
                return _Resolution;
            }
        }
        private ObjectSet<ResolutionMm> _Resolution;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<LoginMm> Login
        {
            get
            {
                if ((_Login == null))
                {
                    _Login = base.CreateObjectSet<LoginMm>(""Login"");
                }
                return _Login;
            }
        }
        private ObjectSet<LoginMm> _Login;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<SuspiciousActivityMm> SuspiciousActivity
        {
            get
            {
                if ((_SuspiciousActivity == null))
                {
                    _SuspiciousActivity = base.CreateObjectSet<SuspiciousActivityMm>(""SuspiciousActivity"");
                }
                return _SuspiciousActivity;
            }
        }
        private ObjectSet<SuspiciousActivityMm> _SuspiciousActivity;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<SmartCardMm> SmartCard
        {
            get
            {
                if ((_SmartCard == null))
                {
                    _SmartCard = base.CreateObjectSet<SmartCardMm>(""SmartCard"");
                }
                return _SmartCard;
            }
        }
        private ObjectSet<SmartCardMm> _SmartCard;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<RSATokenMm> RSAToken
        {
            get
            {
                if ((_RSAToken == null))
                {
                    _RSAToken = base.CreateObjectSet<RSATokenMm>(""RSAToken"");
                }
                return _RSAToken;
            }
        }
        private ObjectSet<RSATokenMm> _RSAToken;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<PasswordResetMm> PasswordReset
        {
            get
            {
                if ((_PasswordReset == null))
                {
                    _PasswordReset = base.CreateObjectSet<PasswordResetMm>(""PasswordReset"");
                }
                return _PasswordReset;
            }
        }
        private ObjectSet<PasswordResetMm> _PasswordReset;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<PageViewMm> PageView
        {
            get
            {
                if ((_PageView == null))
                {
                    _PageView = base.CreateObjectSet<PageViewMm>(""PageView"");
                }
                return _PageView;
            }
        }
        private ObjectSet<PageViewMm> _PageView;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<LastLoginMm> LastLogin
        {
            get
            {
                if ((_LastLogin == null))
                {
                    _LastLogin = base.CreateObjectSet<LastLoginMm>(""LastLogin"");
                }
                return _LastLogin;
            }
        }
        private ObjectSet<LastLoginMm> _LastLogin;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<MessageMm> Message
        {
            get
            {
                if ((_Message == null))
                {
                    _Message = base.CreateObjectSet<MessageMm>(""Message"");
                }
                return _Message;
            }
        }
        private ObjectSet<MessageMm> _Message;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<OrderMm> Order
        {
            get
            {
                if ((_Order == null))
                {
                    _Order = base.CreateObjectSet<OrderMm>(""Order"");
                }
                return _Order;
            }
        }
        private ObjectSet<OrderMm> _Order;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<OrderNoteMm> OrderNote
        {
            get
            {
                if ((_OrderNote == null))
                {
                    _OrderNote = base.CreateObjectSet<OrderNoteMm>(""OrderNote"");
                }
                return _OrderNote;
            }
        }
        private ObjectSet<OrderNoteMm> _OrderNote;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<OrderQualityCheckMm> OrderQualityCheck
        {
            get
            {
                if ((_OrderQualityCheck == null))
                {
                    _OrderQualityCheck = base.CreateObjectSet<OrderQualityCheckMm>(""OrderQualityCheck"");
                }
                return _OrderQualityCheck;
            }
        }
        private ObjectSet<OrderQualityCheckMm> _OrderQualityCheck;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<OrderLineMm> OrderLine
        {
            get
            {
                if ((_OrderLine == null))
                {
                    _OrderLine = base.CreateObjectSet<OrderLineMm>(""OrderLine"");
                }
                return _OrderLine;
            }
        }
        private ObjectSet<OrderLineMm> _OrderLine;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ProductMm> Product
        {
            get
            {
                if ((_Product == null))
                {
                    _Product = base.CreateObjectSet<ProductMm>(""Product"");
                }
                return _Product;
            }
        }
        private ObjectSet<ProductMm> _Product;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ProductDetailMm> ProductDetail
        {
            get
            {
                if ((_ProductDetail == null))
                {
                    _ProductDetail = base.CreateObjectSet<ProductDetailMm>(""ProductDetail"");
                }
                return _ProductDetail;
            }
        }
        private ObjectSet<ProductDetailMm> _ProductDetail;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ProductReviewMm> ProductReview
        {
            get
            {
                if ((_ProductReview == null))
                {
                    _ProductReview = base.CreateObjectSet<ProductReviewMm>(""ProductReview"");
                }
                return _ProductReview;
            }
        }
        private ObjectSet<ProductReviewMm> _ProductReview;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ProductPhotoMm> ProductPhoto
        {
            get
            {
                if ((_ProductPhoto == null))
                {
                    _ProductPhoto = base.CreateObjectSet<ProductPhotoMm>(""ProductPhoto"");
                }
                return _ProductPhoto;
            }
        }
        private ObjectSet<ProductPhotoMm> _ProductPhoto;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ProductWebFeatureMm> ProductWebFeature
        {
            get
            {
                if ((_ProductWebFeature == null))
                {
                    _ProductWebFeature = base.CreateObjectSet<ProductWebFeatureMm>(""ProductWebFeature"");
                }
                return _ProductWebFeature;
            }
        }
        private ObjectSet<ProductWebFeatureMm> _ProductWebFeature;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<SupplierMm> Supplier
        {
            get
            {
                if ((_Supplier == null))
                {
                    _Supplier = base.CreateObjectSet<SupplierMm>(""Supplier"");
                }
                return _Supplier;
            }
        }
        private ObjectSet<SupplierMm> _Supplier;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<SupplierLogoMm> SupplierLogo
        {
            get
            {
                if ((_SupplierLogo == null))
                {
                    _SupplierLogo = base.CreateObjectSet<SupplierLogoMm>(""SupplierLogo"");
                }
                return _SupplierLogo;
            }
        }
        private ObjectSet<SupplierLogoMm> _SupplierLogo;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<SupplierInfoMm> SupplierInfo
        {
            get
            {
                if ((_SupplierInfo == null))
                {
                    _SupplierInfo = base.CreateObjectSet<SupplierInfoMm>(""SupplierInfo"");
                }
                return _SupplierInfo;
            }
        }
        private ObjectSet<SupplierInfoMm> _SupplierInfo;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<CustomerInfoMm> CustomerInfo
        {
            get
            {
                if ((_CustomerInfo == null))
                {
                    _CustomerInfo = base.CreateObjectSet<CustomerInfoMm>(""CustomerInfo"");
                }
                return _CustomerInfo;
            }
        }
        private ObjectSet<CustomerInfoMm> _CustomerInfo;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ComputerMm> Computer
        {
            get
            {
                if ((_Computer == null))
                {
                    _Computer = base.CreateObjectSet<ComputerMm>(""Computer"");
                }
                return _Computer;
            }
        }
        private ObjectSet<ComputerMm> _Computer;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<ComputerDetailMm> ComputerDetail
        {
            get
            {
                if ((_ComputerDetail == null))
                {
                    _ComputerDetail = base.CreateObjectSet<ComputerDetailMm>(""ComputerDetail"");
                }
                return _ComputerDetail;
            }
        }
        private ObjectSet<ComputerDetailMm> _ComputerDetail;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<DriverMm> Driver
        {
            get
            {
                if ((_Driver == null))
                {
                    _Driver = base.CreateObjectSet<DriverMm>(""Driver"");
                }
                return _Driver;
            }
        }
        private ObjectSet<DriverMm> _Driver;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<LicenseMm> License
        {
            get
            {
                if ((_License == null))
                {
                    _License = base.CreateObjectSet<LicenseMm>(""License"");
                }
                return _License;
            }
        }
        private ObjectSet<LicenseMm> _License;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Customer EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToCustomer(CustomerMm customerMm)
        {
            base.AddObject(""Customer"", customerMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Barcode EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToBarcode(BarcodeMm barcodeMm)
        {
            base.AddObject(""Barcode"", barcodeMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the IncorrectScan EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToIncorrectScan(IncorrectScanMm incorrectScanMm)
        {
            base.AddObject(""IncorrectScan"", incorrectScanMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the BarcodeDetail EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToBarcodeDetail(BarcodeDetailMm barcodeDetailMm)
        {
            base.AddObject(""BarcodeDetail"", barcodeDetailMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Complaint EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToComplaint(ComplaintMm complaintMm)
        {
            base.AddObject(""Complaint"", complaintMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Resolution EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToResolution(ResolutionMm resolutionMm)
        {
            base.AddObject(""Resolution"", resolutionMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Login EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToLogin(LoginMm loginMm)
        {
            base.AddObject(""Login"", loginMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the SuspiciousActivity EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToSuspiciousActivity(SuspiciousActivityMm suspiciousActivityMm)
        {
            base.AddObject(""SuspiciousActivity"", suspiciousActivityMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the SmartCard EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToSmartCard(SmartCardMm smartCardMm)
        {
            base.AddObject(""SmartCard"", smartCardMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the RSAToken EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToRSAToken(RSATokenMm rSATokenMm)
        {
            base.AddObject(""RSAToken"", rSATokenMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the PasswordReset EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToPasswordReset(PasswordResetMm passwordResetMm)
        {
            base.AddObject(""PasswordReset"", passwordResetMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the PageView EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToPageView(PageViewMm pageViewMm)
        {
            base.AddObject(""PageView"", pageViewMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the LastLogin EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToLastLogin(LastLoginMm lastLoginMm)
        {
            base.AddObject(""LastLogin"", lastLoginMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Message EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToMessage(MessageMm messageMm)
        {
            base.AddObject(""Message"", messageMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Order EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToOrder(OrderMm orderMm)
        {
            base.AddObject(""Order"", orderMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the OrderNote EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToOrderNote(OrderNoteMm orderNoteMm)
        {
            base.AddObject(""OrderNote"", orderNoteMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the OrderQualityCheck EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToOrderQualityCheck(OrderQualityCheckMm orderQualityCheckMm)
        {
            base.AddObject(""OrderQualityCheck"", orderQualityCheckMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the OrderLine EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToOrderLine(OrderLineMm orderLineMm)
        {
            base.AddObject(""OrderLine"", orderLineMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Product EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToProduct(ProductMm productMm)
        {
            base.AddObject(""Product"", productMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the ProductDetail EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToProductDetail(ProductDetailMm productDetailMm)
        {
            base.AddObject(""ProductDetail"", productDetailMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the ProductReview EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToProductReview(ProductReviewMm productReviewMm)
        {
            base.AddObject(""ProductReview"", productReviewMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the ProductPhoto EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToProductPhoto(ProductPhotoMm productPhotoMm)
        {
            base.AddObject(""ProductPhoto"", productPhotoMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the ProductWebFeature EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToProductWebFeature(ProductWebFeatureMm productWebFeatureMm)
        {
            base.AddObject(""ProductWebFeature"", productWebFeatureMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Supplier EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToSupplier(SupplierMm supplierMm)
        {
            base.AddObject(""Supplier"", supplierMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the SupplierLogo EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToSupplierLogo(SupplierLogoMm supplierLogoMm)
        {
            base.AddObject(""SupplierLogo"", supplierLogoMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the SupplierInfo EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToSupplierInfo(SupplierInfoMm supplierInfoMm)
        {
            base.AddObject(""SupplierInfo"", supplierInfoMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the CustomerInfo EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToCustomerInfo(CustomerInfoMm customerInfoMm)
        {
            base.AddObject(""CustomerInfo"", customerInfoMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Computer EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToComputer(ComputerMm computerMm)
        {
            base.AddObject(""Computer"", computerMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the ComputerDetail EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToComputerDetail(ComputerDetailMm computerDetailMm)
        {
            base.AddObject(""ComputerDetail"", computerDetailMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Driver EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToDriver(DriverMm driverMm)
        {
            base.AddObject(""Driver"", driverMm);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the License EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToLicense(LicenseMm licenseMm)
        {
            base.AddObject(""License"", licenseMm);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name=""modifiedDate"">No Metadata Documentation available.</param>
        private ObjectResult<AuditInfoMm> FunctionImport1(ObjectParameter modifiedDate)
        {
            return base.ExecuteFunction<AuditInfoMm>(""FunctionImport1"", modifiedDate);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        internal ObjectResult<CustomerMm> FunctionImport2()
        {
            return base.ExecuteFunction<CustomerMm>(""FunctionImport2"");
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name=""mergeOption""></param>
        internal ObjectResult<CustomerMm> FunctionImport2(MergeOption mergeOption)
        {
            return base.ExecuteFunction<CustomerMm>(""FunctionImport2"", mergeOption);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name=""binary"">No Metadata Documentation available.</param>
        /// <param name=""bool"">No Metadata Documentation available.</param>
        /// <param name=""dateTime"">No Metadata Documentation available.</param>
        /// <param name=""decimal"">No Metadata Documentation available.</param>
        /// <param name=""float"">No Metadata Documentation available.</param>
        /// <param name=""guid"">No Metadata Documentation available.</param>
        /// <param name=""int"">No Metadata Documentation available.</param>
        /// <param name=""string"">No Metadata Documentation available.</param>
        public int ParameterTest(global::System.Byte[] binary, Nullable<global::System.Boolean> @bool, Nullable<global::System.DateTime> dateTime, Nullable<global::System.Decimal> @decimal, Nullable<global::System.Double> @float, Nullable<global::System.Guid> guid, ObjectParameter @int, ObjectParameter @string)
        {
            ObjectParameter binaryParameter;
            if (binary != null)
            {
                binaryParameter = new ObjectParameter(""binary"", binary);
            }
            else
            {
                binaryParameter = new ObjectParameter(""binary"", typeof(global::System.Byte[]));
            }
    
            ObjectParameter boolParameter;
            if (@bool.HasValue)
            {
                boolParameter = new ObjectParameter(""bool"", @bool);
            }
            else
            {
                boolParameter = new ObjectParameter(""bool"", typeof(global::System.Boolean));
            }
    
            ObjectParameter dateTimeParameter;
            if (dateTime.HasValue)
            {
                dateTimeParameter = new ObjectParameter(""dateTime"", dateTime);
            }
            else
            {
                dateTimeParameter = new ObjectParameter(""dateTime"", typeof(global::System.DateTime));
            }
    
            ObjectParameter decimalParameter;
            if (@decimal.HasValue)
            {
                decimalParameter = new ObjectParameter(""decimal"", @decimal);
            }
            else
            {
                decimalParameter = new ObjectParameter(""decimal"", typeof(global::System.Decimal));
            }
    
            ObjectParameter floatParameter;
            if (@float.HasValue)
            {
                floatParameter = new ObjectParameter(""float"", @float);
            }
            else
            {
                floatParameter = new ObjectParameter(""float"", typeof(global::System.Double));
            }
    
            ObjectParameter guidParameter;
            if (guid.HasValue)
            {
                guidParameter = new ObjectParameter(""guid"", guid);
            }
            else
            {
                guidParameter = new ObjectParameter(""guid"", typeof(global::System.Guid));
            }
    
            return base.ExecuteFunction(""ParameterTest"", binaryParameter, boolParameter, dateTimeParameter, decimalParameter, floatParameter, guidParameter, @int, @string);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectResult<ComputerMm> EntityTypeTest()
        {
            return base.ExecuteFunction<ComputerMm>(""EntityTypeTest"");
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name=""mergeOption""></param>
        public ObjectResult<ComputerMm> EntityTypeTest(MergeOption mergeOption)
        {
            return base.ExecuteFunction<ComputerMm>(""EntityTypeTest"", mergeOption);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""BackOrderLine2Mm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class BackOrderLine2Mm : BackOrderLineMm
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new BackOrderLine2Mm object.
        /// </summary>
        /// <param name=""orderId"">Initial value of the OrderId property.</param>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""concurrencyToken"">Initial value of the ConcurrencyToken property.</param>
        /// <param name=""eTA"">Initial value of the ETA property.</param>
        public static BackOrderLine2Mm CreateBackOrderLine2Mm(global::System.Int32 orderId, global::System.Int32 productId, global::System.String concurrencyToken, global::System.DateTime eTA)
        {
            BackOrderLine2Mm backOrderLine2Mm = new BackOrderLine2Mm();
            backOrderLine2Mm.OrderId = orderId;
            backOrderLine2Mm.ProductId = productId;
            backOrderLine2Mm.ConcurrencyToken = concurrencyToken;
            backOrderLine2Mm.ETA = eTA;
            return backOrderLine2Mm;
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""BackOrderLineMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [KnownTypeAttribute(typeof(BackOrderLine2Mm))]
    public partial class BackOrderLineMm : OrderLineMm
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new BackOrderLineMm object.
        /// </summary>
        /// <param name=""orderId"">Initial value of the OrderId property.</param>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""concurrencyToken"">Initial value of the ConcurrencyToken property.</param>
        /// <param name=""eTA"">Initial value of the ETA property.</param>
        public static BackOrderLineMm CreateBackOrderLineMm(global::System.Int32 orderId, global::System.Int32 productId, global::System.String concurrencyToken, global::System.DateTime eTA)
        {
            BackOrderLineMm backOrderLineMm = new BackOrderLineMm();
            backOrderLineMm.OrderId = orderId;
            backOrderLineMm.ProductId = productId;
            backOrderLineMm.ConcurrencyToken = concurrencyToken;
            backOrderLineMm.ETA = eTA;
            return backOrderLineMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime ETA
        {
            get
            {
                return _ETA;
            }
            set
            {
                OnETAChanging(value);
                ReportPropertyChanging(""ETA"");
                _ETA = StructuralObject.SetValidValue(value, ""ETA"");
                ReportPropertyChanged(""ETA"");
                OnETAChanged();
            }
        }
        private global::System.DateTime _ETA;
        partial void OnETAChanging(global::System.DateTime value);
        partial void OnETAChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Supplier_BackOrderLines"", ""Supplier"")]
        public SupplierMm Supplier
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_BackOrderLines"", ""Supplier"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_BackOrderLines"", ""Supplier"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<SupplierMm> SupplierReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_BackOrderLines"", ""Supplier"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_BackOrderLines"", ""Supplier"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""BarcodeDetailMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class BarcodeDetailMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new BarcodeDetailMm object.
        /// </summary>
        /// <param name=""code"">Initial value of the Code property.</param>
        /// <param name=""registeredTo"">Initial value of the RegisteredTo property.</param>
        public static BarcodeDetailMm CreateBarcodeDetailMm(global::System.Byte[] code, global::System.String registeredTo)
        {
            BarcodeDetailMm barcodeDetailMm = new BarcodeDetailMm();
            barcodeDetailMm.Code = code;
            barcodeDetailMm.RegisteredTo = registeredTo;
            return barcodeDetailMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Byte[] Code
        {
            get
            {
                return StructuralObject.GetValidValue(_Code);
            }
            set
            {
                if (!StructuralObject.BinaryEquals(_Code, value))
                {
                    OnCodeChanging(value);
                    ReportPropertyChanging(""Code"");
                    _Code = StructuralObject.SetValidValue(value, false, ""Code"");
                    ReportPropertyChanged(""Code"");
                    OnCodeChanged();
                }
            }
        }
        private global::System.Byte[] _Code;
        partial void OnCodeChanging(global::System.Byte[] value);
        partial void OnCodeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String RegisteredTo
        {
            get
            {
                return _RegisteredTo;
            }
            set
            {
                OnRegisteredToChanging(value);
                ReportPropertyChanging(""RegisteredTo"");
                _RegisteredTo = StructuralObject.SetValidValue(value, false, ""RegisteredTo"");
                ReportPropertyChanged(""RegisteredTo"");
                OnRegisteredToChanged();
            }
        }
        private global::System.String _RegisteredTo;
        partial void OnRegisteredToChanging(global::System.String value);
        partial void OnRegisteredToChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""BarcodeMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class BarcodeMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new BarcodeMm object.
        /// </summary>
        /// <param name=""code"">Initial value of the Code property.</param>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""text"">Initial value of the Text property.</param>
        public static BarcodeMm CreateBarcodeMm(global::System.Byte[] code, global::System.Int32 productId, global::System.String text)
        {
            BarcodeMm barcodeMm = new BarcodeMm();
            barcodeMm.Code = code;
            barcodeMm.ProductId = productId;
            barcodeMm.Text = text;
            return barcodeMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Byte[] Code
        {
            get
            {
                return StructuralObject.GetValidValue(_Code);
            }
            set
            {
                if (!StructuralObject.BinaryEquals(_Code, value))
                {
                    OnCodeChanging(value);
                    ReportPropertyChanging(""Code"");
                    _Code = StructuralObject.SetValidValue(value, false, ""Code"");
                    ReportPropertyChanged(""Code"");
                    OnCodeChanged();
                }
            }
        }
        private global::System.Byte[] _Code;
        partial void OnCodeChanging(global::System.Byte[] value);
        partial void OnCodeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                OnProductIdChanging(value);
                ReportPropertyChanging(""ProductId"");
                _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                ReportPropertyChanged(""ProductId"");
                OnProductIdChanged();
            }
        }
        private global::System.Int32 _ProductId;
        partial void OnProductIdChanging(global::System.Int32 value);
        partial void OnProductIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Text
        {
            get
            {
                return _Text;
            }
            set
            {
                OnTextChanging(value);
                ReportPropertyChanging(""Text"");
                _Text = StructuralObject.SetValidValue(value, false, ""Text"");
                ReportPropertyChanged(""Text"");
                OnTextChanged();
            }
        }
        private global::System.String _Text;
        partial void OnTextChanging(global::System.String value);
        partial void OnTextChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_Barcodes"", ""Product"")]
        public ProductMm Product
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_Barcodes"", ""Product"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_Barcodes"", ""Product"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductMm> ProductReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_Barcodes"", ""Product"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductMm>(""MonsterNamespace.Product_Barcodes"", ""Product"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Barcode_IncorrectScanExpected"", ""Expected"")]
        public EntityCollection<IncorrectScanMm> BadScans
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<IncorrectScanMm>(""MonsterNamespace.Barcode_IncorrectScanExpected"", ""Expected"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<IncorrectScanMm>(""MonsterNamespace.Barcode_IncorrectScanExpected"", ""Expected"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Barcode_BarcodeDetail"", ""BarcodeDetail"")]
        public BarcodeDetailMm Detail
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeDetailMm>(""MonsterNamespace.Barcode_BarcodeDetail"", ""BarcodeDetail"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeDetailMm>(""MonsterNamespace.Barcode_BarcodeDetail"", ""BarcodeDetail"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<BarcodeDetailMm> DetailReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeDetailMm>(""MonsterNamespace.Barcode_BarcodeDetail"", ""BarcodeDetail"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<BarcodeDetailMm>(""MonsterNamespace.Barcode_BarcodeDetail"", ""BarcodeDetail"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ComplaintMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ComplaintMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ComplaintMm object.
        /// </summary>
        /// <param name=""complaintId"">Initial value of the ComplaintId property.</param>
        /// <param name=""logged"">Initial value of the Logged property.</param>
        /// <param name=""details"">Initial value of the Details property.</param>
        public static ComplaintMm CreateComplaintMm(global::System.Int32 complaintId, global::System.DateTime logged, global::System.String details)
        {
            ComplaintMm complaintMm = new ComplaintMm();
            complaintMm.ComplaintId = complaintId;
            complaintMm.Logged = logged;
            complaintMm.Details = details;
            return complaintMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ComplaintId
        {
            get
            {
                return _ComplaintId;
            }
            set
            {
                if (_ComplaintId != value)
                {
                    OnComplaintIdChanging(value);
                    ReportPropertyChanging(""ComplaintId"");
                    _ComplaintId = StructuralObject.SetValidValue(value, ""ComplaintId"");
                    ReportPropertyChanged(""ComplaintId"");
                    OnComplaintIdChanged();
                }
            }
        }
        private global::System.Int32 _ComplaintId;
        partial void OnComplaintIdChanging(global::System.Int32 value);
        partial void OnComplaintIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> CustomerId
        {
            get
            {
                return _CustomerId;
            }
            set
            {
                OnCustomerIdChanging(value);
                ReportPropertyChanging(""CustomerId"");
                _CustomerId = StructuralObject.SetValidValue(value, ""CustomerId"");
                ReportPropertyChanged(""CustomerId"");
                OnCustomerIdChanged();
            }
        }
        private Nullable<global::System.Int32> _CustomerId;
        partial void OnCustomerIdChanging(Nullable<global::System.Int32> value);
        partial void OnCustomerIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Logged
        {
            get
            {
                return _Logged;
            }
            set
            {
                OnLoggedChanging(value);
                ReportPropertyChanging(""Logged"");
                _Logged = StructuralObject.SetValidValue(value, ""Logged"");
                ReportPropertyChanged(""Logged"");
                OnLoggedChanged();
            }
        }
        private global::System.DateTime _Logged;
        partial void OnLoggedChanging(global::System.DateTime value);
        partial void OnLoggedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Details
        {
            get
            {
                return _Details;
            }
            set
            {
                OnDetailsChanging(value);
                ReportPropertyChanging(""Details"");
                _Details = StructuralObject.SetValidValue(value, false, ""Details"");
                ReportPropertyChanged(""Details"");
                OnDetailsChanged();
            }
        }
        private global::System.String _Details;
        partial void OnDetailsChanging(global::System.String value);
        partial void OnDetailsChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Customer_Complaints"", ""Customer"")]
        public CustomerMm Customer
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Complaints"", ""Customer"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Complaints"", ""Customer"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<CustomerMm> CustomerReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Complaints"", ""Customer"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Complaints"", ""Customer"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Complaint_Resolution"", ""Resolution"")]
        public ResolutionMm Resolution
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ResolutionMm>(""MonsterNamespace.Complaint_Resolution"", ""Resolution"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ResolutionMm>(""MonsterNamespace.Complaint_Resolution"", ""Resolution"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ResolutionMm> ResolutionReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ResolutionMm>(""MonsterNamespace.Complaint_Resolution"", ""Resolution"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ResolutionMm>(""MonsterNamespace.Complaint_Resolution"", ""Resolution"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ComputerDetailMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ComputerDetailMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ComputerDetailMm object.
        /// </summary>
        /// <param name=""computerDetailId"">Initial value of the ComputerDetailId property.</param>
        /// <param name=""manufacturer"">Initial value of the Manufacturer property.</param>
        /// <param name=""model"">Initial value of the Model property.</param>
        /// <param name=""serial"">Initial value of the Serial property.</param>
        /// <param name=""specifications"">Initial value of the Specifications property.</param>
        /// <param name=""purchaseDate"">Initial value of the PurchaseDate property.</param>
        /// <param name=""dimensions"">Initial value of the Dimensions property.</param>
        public static ComputerDetailMm CreateComputerDetailMm(global::System.Int32 computerDetailId, global::System.String manufacturer, global::System.String model, global::System.String serial, global::System.String specifications, global::System.DateTime purchaseDate, DimensionsMm dimensions)
        {
            ComputerDetailMm computerDetailMm = new ComputerDetailMm();
            computerDetailMm.ComputerDetailId = computerDetailId;
            computerDetailMm.Manufacturer = manufacturer;
            computerDetailMm.Model = model;
            computerDetailMm.Serial = serial;
            computerDetailMm.Specifications = specifications;
            computerDetailMm.PurchaseDate = purchaseDate;
            computerDetailMm.Dimensions = StructuralObject.VerifyComplexObjectIsNotNull(dimensions, ""Dimensions"");
            return computerDetailMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ComputerDetailId
        {
            get
            {
                return _ComputerDetailId;
            }
            set
            {
                if (_ComputerDetailId != value)
                {
                    OnComputerDetailIdChanging(value);
                    ReportPropertyChanging(""ComputerDetailId"");
                    _ComputerDetailId = StructuralObject.SetValidValue(value, ""ComputerDetailId"");
                    ReportPropertyChanged(""ComputerDetailId"");
                    OnComputerDetailIdChanged();
                }
            }
        }
        private global::System.Int32 _ComputerDetailId;
        partial void OnComputerDetailIdChanging(global::System.Int32 value);
        partial void OnComputerDetailIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Manufacturer
        {
            get
            {
                return _Manufacturer;
            }
            set
            {
                OnManufacturerChanging(value);
                ReportPropertyChanging(""Manufacturer"");
                _Manufacturer = StructuralObject.SetValidValue(value, false, ""Manufacturer"");
                ReportPropertyChanged(""Manufacturer"");
                OnManufacturerChanged();
            }
        }
        private global::System.String _Manufacturer;
        partial void OnManufacturerChanging(global::System.String value);
        partial void OnManufacturerChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Model
        {
            get
            {
                return _Model;
            }
            set
            {
                OnModelChanging(value);
                ReportPropertyChanging(""Model"");
                _Model = StructuralObject.SetValidValue(value, false, ""Model"");
                ReportPropertyChanged(""Model"");
                OnModelChanged();
            }
        }
        private global::System.String _Model;
        partial void OnModelChanging(global::System.String value);
        partial void OnModelChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Serial
        {
            get
            {
                return _Serial;
            }
            set
            {
                OnSerialChanging(value);
                ReportPropertyChanging(""Serial"");
                _Serial = StructuralObject.SetValidValue(value, false, ""Serial"");
                ReportPropertyChanged(""Serial"");
                OnSerialChanged();
            }
        }
        private global::System.String _Serial;
        partial void OnSerialChanging(global::System.String value);
        partial void OnSerialChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Specifications
        {
            get
            {
                return _Specifications;
            }
            set
            {
                OnSpecificationsChanging(value);
                ReportPropertyChanging(""Specifications"");
                _Specifications = StructuralObject.SetValidValue(value, false, ""Specifications"");
                ReportPropertyChanged(""Specifications"");
                OnSpecificationsChanged();
            }
        }
        private global::System.String _Specifications;
        partial void OnSpecificationsChanging(global::System.String value);
        partial void OnSpecificationsChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime PurchaseDate
        {
            get
            {
                return _PurchaseDate;
            }
            set
            {
                OnPurchaseDateChanging(value);
                ReportPropertyChanging(""PurchaseDate"");
                _PurchaseDate = StructuralObject.SetValidValue(value, ""PurchaseDate"");
                ReportPropertyChanged(""PurchaseDate"");
                OnPurchaseDateChanged();
            }
        }
        private global::System.DateTime _PurchaseDate;
        partial void OnPurchaseDateChanging(global::System.DateTime value);
        partial void OnPurchaseDateChanged();

        #endregion

        #region Complex Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public DimensionsMm Dimensions
        {
            get
            {
                _Dimensions = GetValidValue(_Dimensions, ""Dimensions"", false, _DimensionsInitialized);
                _DimensionsInitialized = true;
                return _Dimensions;
            }
            set
            {
                OnDimensionsChanging(value);
                ReportPropertyChanging(""Dimensions"");
                _Dimensions = SetValidValue(_Dimensions, value, ""Dimensions"");
                _DimensionsInitialized = true;
                ReportPropertyChanged(""Dimensions"");
                OnDimensionsChanged();
            }
        }
        private DimensionsMm _Dimensions;
        private bool _DimensionsInitialized;
        partial void OnDimensionsChanging(DimensionsMm value);
        partial void OnDimensionsChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Computer_ComputerDetail"", ""Computer"")]
        public ComputerMm Computer
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComputerMm>(""MonsterNamespace.Computer_ComputerDetail"", ""Computer"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComputerMm>(""MonsterNamespace.Computer_ComputerDetail"", ""Computer"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ComputerMm> ComputerReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComputerMm>(""MonsterNamespace.Computer_ComputerDetail"", ""Computer"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ComputerMm>(""MonsterNamespace.Computer_ComputerDetail"", ""Computer"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ComputerMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ComputerMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ComputerMm object.
        /// </summary>
        /// <param name=""computerId"">Initial value of the ComputerId property.</param>
        /// <param name=""name"">Initial value of the Name property.</param>
        public static ComputerMm CreateComputerMm(global::System.Int32 computerId, global::System.String name)
        {
            ComputerMm computerMm = new ComputerMm();
            computerMm.ComputerId = computerId;
            computerMm.Name = name;
            return computerMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ComputerId
        {
            get
            {
                return _ComputerId;
            }
            set
            {
                if (_ComputerId != value)
                {
                    OnComputerIdChanging(value);
                    ReportPropertyChanging(""ComputerId"");
                    _ComputerId = StructuralObject.SetValidValue(value, ""ComputerId"");
                    ReportPropertyChanged(""ComputerId"");
                    OnComputerIdChanged();
                }
            }
        }
        private global::System.Int32 _ComputerId;
        partial void OnComputerIdChanging(global::System.Int32 value);
        partial void OnComputerIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging(""Name"");
                _Name = StructuralObject.SetValidValue(value, false, ""Name"");
                ReportPropertyChanged(""Name"");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Computer_ComputerDetail"", ""ComputerDetail"")]
        public ComputerDetailMm ComputerDetail
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComputerDetailMm>(""MonsterNamespace.Computer_ComputerDetail"", ""ComputerDetail"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComputerDetailMm>(""MonsterNamespace.Computer_ComputerDetail"", ""ComputerDetail"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ComputerDetailMm> ComputerDetailReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComputerDetailMm>(""MonsterNamespace.Computer_ComputerDetail"", ""ComputerDetail"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ComputerDetailMm>(""MonsterNamespace.Computer_ComputerDetail"", ""ComputerDetail"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""CustomerInfoMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class CustomerInfoMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new CustomerInfoMm object.
        /// </summary>
        /// <param name=""customerInfoId"">Initial value of the CustomerInfoId property.</param>
        /// <param name=""information"">Initial value of the Information property.</param>
        public static CustomerInfoMm CreateCustomerInfoMm(global::System.Int32 customerInfoId, global::System.String information)
        {
            CustomerInfoMm customerInfoMm = new CustomerInfoMm();
            customerInfoMm.CustomerInfoId = customerInfoId;
            customerInfoMm.Information = information;
            return customerInfoMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 CustomerInfoId
        {
            get
            {
                return _CustomerInfoId;
            }
            set
            {
                if (_CustomerInfoId != value)
                {
                    OnCustomerInfoIdChanging(value);
                    ReportPropertyChanging(""CustomerInfoId"");
                    _CustomerInfoId = StructuralObject.SetValidValue(value, ""CustomerInfoId"");
                    ReportPropertyChanged(""CustomerInfoId"");
                    OnCustomerInfoIdChanged();
                }
            }
        }
        private global::System.Int32 _CustomerInfoId;
        partial void OnCustomerInfoIdChanging(global::System.Int32 value);
        partial void OnCustomerInfoIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Information
        {
            get
            {
                return _Information;
            }
            set
            {
                OnInformationChanging(value);
                ReportPropertyChanging(""Information"");
                _Information = StructuralObject.SetValidValue(value, false, ""Information"");
                ReportPropertyChanged(""Information"");
                OnInformationChanged();
            }
        }
        private global::System.String _Information;
        partial void OnInformationChanging(global::System.String value);
        partial void OnInformationChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""CustomerMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class CustomerMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new CustomerMm object.
        /// </summary>
        /// <param name=""customerId"">Initial value of the CustomerId property.</param>
        /// <param name=""name"">Initial value of the Name property.</param>
        /// <param name=""contactInfo"">Initial value of the ContactInfo property.</param>
        /// <param name=""auditing"">Initial value of the Auditing property.</param>
        public static CustomerMm CreateCustomerMm(global::System.Int32 customerId, global::System.String name, ContactDetailsMm contactInfo, AuditInfoMm auditing)
        {
            CustomerMm customerMm = new CustomerMm();
            customerMm.CustomerId = customerId;
            customerMm.Name = name;
            customerMm.ContactInfo = StructuralObject.VerifyComplexObjectIsNotNull(contactInfo, ""ContactInfo"");
            customerMm.Auditing = StructuralObject.VerifyComplexObjectIsNotNull(auditing, ""Auditing"");
            return customerMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 CustomerId
        {
            get
            {
                return _CustomerId;
            }
            set
            {
                if (_CustomerId != value)
                {
                    OnCustomerIdChanging(value);
                    ReportPropertyChanging(""CustomerId"");
                    _CustomerId = StructuralObject.SetValidValue(value, ""CustomerId"");
                    ReportPropertyChanged(""CustomerId"");
                    OnCustomerIdChanged();
                }
            }
        }
        private global::System.Int32 _CustomerId;
        partial void OnCustomerIdChanging(global::System.Int32 value);
        partial void OnCustomerIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging(""Name"");
                _Name = StructuralObject.SetValidValue(value, false, ""Name"");
                ReportPropertyChanged(""Name"");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();

        #endregion

        #region Complex Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public ContactDetailsMm ContactInfo
        {
            get
            {
                _ContactInfo = GetValidValue(_ContactInfo, ""ContactInfo"", false, _ContactInfoInitialized);
                _ContactInfoInitialized = true;
                return _ContactInfo;
            }
            set
            {
                OnContactInfoChanging(value);
                ReportPropertyChanging(""ContactInfo"");
                _ContactInfo = SetValidValue(_ContactInfo, value, ""ContactInfo"");
                _ContactInfoInitialized = true;
                ReportPropertyChanged(""ContactInfo"");
                OnContactInfoChanged();
            }
        }
        private ContactDetailsMm _ContactInfo;
        private bool _ContactInfoInitialized;
        partial void OnContactInfoChanging(ContactDetailsMm value);
        partial void OnContactInfoChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public AuditInfoMm Auditing
        {
            get
            {
                _Auditing = GetValidValue(_Auditing, ""Auditing"", false, _AuditingInitialized);
                _AuditingInitialized = true;
                return _Auditing;
            }
            set
            {
                OnAuditingChanging(value);
                ReportPropertyChanging(""Auditing"");
                _Auditing = SetValidValue(_Auditing, value, ""Auditing"");
                _AuditingInitialized = true;
                ReportPropertyChanged(""Auditing"");
                OnAuditingChanged();
            }
        }
        private AuditInfoMm _Auditing;
        private bool _AuditingInitialized;
        partial void OnAuditingChanging(AuditInfoMm value);
        partial void OnAuditingChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Customer_Orders"", ""Order"")]
        public EntityCollection<OrderMm> Orders
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<OrderMm>(""MonsterNamespace.Customer_Orders"", ""Order"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<OrderMm>(""MonsterNamespace.Customer_Orders"", ""Order"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Customer_Logins"", ""Logins"")]
        public EntityCollection<LoginMm> Logins
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<LoginMm>(""MonsterNamespace.Customer_Logins"", ""Logins"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<LoginMm>(""MonsterNamespace.Customer_Logins"", ""Logins"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Husband_Wife"", ""Husband"")]
        public CustomerMm Husband
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Husband"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Husband"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<CustomerMm> HusbandReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Husband"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Husband"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Husband_Wife"", ""Wife"")]
        public CustomerMm Wife
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Wife"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Wife"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<CustomerMm> WifeReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Wife"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<CustomerMm>(""MonsterNamespace.Husband_Wife"", ""Wife"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Customer_CustomerInfo"", ""Info"")]
        public CustomerInfoMm Info
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerInfoMm>(""MonsterNamespace.Customer_CustomerInfo"", ""Info"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerInfoMm>(""MonsterNamespace.Customer_CustomerInfo"", ""Info"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<CustomerInfoMm> InfoReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerInfoMm>(""MonsterNamespace.Customer_CustomerInfo"", ""Info"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<CustomerInfoMm>(""MonsterNamespace.Customer_CustomerInfo"", ""Info"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""DiscontinuedProductMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class DiscontinuedProductMm : ProductMm
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new DiscontinuedProductMm object.
        /// </summary>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""dimensions"">Initial value of the Dimensions property.</param>
        /// <param name=""baseConcurrency"">Initial value of the BaseConcurrency property.</param>
        /// <param name=""complexConcurrency"">Initial value of the ComplexConcurrency property.</param>
        /// <param name=""nestedComplexConcurrency"">Initial value of the NestedComplexConcurrency property.</param>
        /// <param name=""discontinued"">Initial value of the Discontinued property.</param>
        public static DiscontinuedProductMm CreateDiscontinuedProductMm(global::System.Int32 productId, DimensionsMm dimensions, global::System.String baseConcurrency, ConcurrencyInfoMm complexConcurrency, AuditInfoMm nestedComplexConcurrency, global::System.DateTime discontinued)
        {
            DiscontinuedProductMm discontinuedProductMm = new DiscontinuedProductMm();
            discontinuedProductMm.ProductId = productId;
            discontinuedProductMm.Dimensions = StructuralObject.VerifyComplexObjectIsNotNull(dimensions, ""Dimensions"");
            discontinuedProductMm.BaseConcurrency = baseConcurrency;
            discontinuedProductMm.ComplexConcurrency = StructuralObject.VerifyComplexObjectIsNotNull(complexConcurrency, ""ComplexConcurrency"");
            discontinuedProductMm.NestedComplexConcurrency = StructuralObject.VerifyComplexObjectIsNotNull(nestedComplexConcurrency, ""NestedComplexConcurrency"");
            discontinuedProductMm.Discontinued = discontinued;
            return discontinuedProductMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Discontinued
        {
            get
            {
                return _Discontinued;
            }
            set
            {
                OnDiscontinuedChanging(value);
                ReportPropertyChanging(""Discontinued"");
                _Discontinued = StructuralObject.SetValidValue(value, ""Discontinued"");
                ReportPropertyChanged(""Discontinued"");
                OnDiscontinuedChanged();
            }
        }
        private global::System.DateTime _Discontinued;
        partial void OnDiscontinuedChanging(global::System.DateTime value);
        partial void OnDiscontinuedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> ReplacementProductId
        {
            get
            {
                return _ReplacementProductId;
            }
            set
            {
                OnReplacementProductIdChanging(value);
                ReportPropertyChanging(""ReplacementProductId"");
                _ReplacementProductId = StructuralObject.SetValidValue(value, ""ReplacementProductId"");
                ReportPropertyChanged(""ReplacementProductId"");
                OnReplacementProductIdChanged();
            }
        }
        private Nullable<global::System.Int32> _ReplacementProductId;
        partial void OnReplacementProductIdChanging(Nullable<global::System.Int32> value);
        partial void OnReplacementProductIdChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""DiscontinuedProduct_Replacement"", ""Replacement"")]
        public ProductMm ReplacedBy
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.DiscontinuedProduct_Replacement"", ""Replacement"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.DiscontinuedProduct_Replacement"", ""Replacement"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductMm> ReplacedByReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.DiscontinuedProduct_Replacement"", ""Replacement"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductMm>(""MonsterNamespace.DiscontinuedProduct_Replacement"", ""Replacement"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""DriverMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class DriverMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new DriverMm object.
        /// </summary>
        /// <param name=""name"">Initial value of the Name property.</param>
        /// <param name=""birthDate"">Initial value of the BirthDate property.</param>
        public static DriverMm CreateDriverMm(global::System.String name, global::System.DateTime birthDate)
        {
            DriverMm driverMm = new DriverMm();
            driverMm.Name = name;
            driverMm.BirthDate = birthDate;
            return driverMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    OnNameChanging(value);
                    ReportPropertyChanging(""Name"");
                    _Name = StructuralObject.SetValidValue(value, false, ""Name"");
                    ReportPropertyChanged(""Name"");
                    OnNameChanged();
                }
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime BirthDate
        {
            get
            {
                return _BirthDate;
            }
            set
            {
                OnBirthDateChanging(value);
                ReportPropertyChanging(""BirthDate"");
                _BirthDate = StructuralObject.SetValidValue(value, ""BirthDate"");
                ReportPropertyChanged(""BirthDate"");
                OnBirthDateChanged();
            }
        }
        private global::System.DateTime _BirthDate;
        partial void OnBirthDateChanging(global::System.DateTime value);
        partial void OnBirthDateChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Driver_License"", ""License"")]
        public LicenseMm License
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LicenseMm>(""MonsterNamespace.Driver_License"", ""License"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LicenseMm>(""MonsterNamespace.Driver_License"", ""License"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LicenseMm> LicenseReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LicenseMm>(""MonsterNamespace.Driver_License"", ""License"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LicenseMm>(""MonsterNamespace.Driver_License"", ""License"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""IncorrectScanMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class IncorrectScanMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new IncorrectScanMm object.
        /// </summary>
        /// <param name=""incorrectScanId"">Initial value of the IncorrectScanId property.</param>
        /// <param name=""expectedCode"">Initial value of the ExpectedCode property.</param>
        /// <param name=""scanDate"">Initial value of the ScanDate property.</param>
        /// <param name=""details"">Initial value of the Details property.</param>
        public static IncorrectScanMm CreateIncorrectScanMm(global::System.Int32 incorrectScanId, global::System.Byte[] expectedCode, global::System.DateTime scanDate, global::System.String details)
        {
            IncorrectScanMm incorrectScanMm = new IncorrectScanMm();
            incorrectScanMm.IncorrectScanId = incorrectScanId;
            incorrectScanMm.ExpectedCode = expectedCode;
            incorrectScanMm.ScanDate = scanDate;
            incorrectScanMm.Details = details;
            return incorrectScanMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 IncorrectScanId
        {
            get
            {
                return _IncorrectScanId;
            }
            set
            {
                if (_IncorrectScanId != value)
                {
                    OnIncorrectScanIdChanging(value);
                    ReportPropertyChanging(""IncorrectScanId"");
                    _IncorrectScanId = StructuralObject.SetValidValue(value, ""IncorrectScanId"");
                    ReportPropertyChanged(""IncorrectScanId"");
                    OnIncorrectScanIdChanged();
                }
            }
        }
        private global::System.Int32 _IncorrectScanId;
        partial void OnIncorrectScanIdChanging(global::System.Int32 value);
        partial void OnIncorrectScanIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Byte[] ExpectedCode
        {
            get
            {
                return StructuralObject.GetValidValue(_ExpectedCode);
            }
            set
            {
                OnExpectedCodeChanging(value);
                ReportPropertyChanging(""ExpectedCode"");
                _ExpectedCode = StructuralObject.SetValidValue(value, false, ""ExpectedCode"");
                ReportPropertyChanged(""ExpectedCode"");
                OnExpectedCodeChanged();
            }
        }
        private global::System.Byte[] _ExpectedCode;
        partial void OnExpectedCodeChanging(global::System.Byte[] value);
        partial void OnExpectedCodeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.Byte[] ActualCode
        {
            get
            {
                return StructuralObject.GetValidValue(_ActualCode);
            }
            set
            {
                OnActualCodeChanging(value);
                ReportPropertyChanging(""ActualCode"");
                _ActualCode = StructuralObject.SetValidValue(value, true, ""ActualCode"");
                ReportPropertyChanged(""ActualCode"");
                OnActualCodeChanged();
            }
        }
        private global::System.Byte[] _ActualCode;
        partial void OnActualCodeChanging(global::System.Byte[] value);
        partial void OnActualCodeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime ScanDate
        {
            get
            {
                return _ScanDate;
            }
            set
            {
                OnScanDateChanging(value);
                ReportPropertyChanging(""ScanDate"");
                _ScanDate = StructuralObject.SetValidValue(value, ""ScanDate"");
                ReportPropertyChanged(""ScanDate"");
                OnScanDateChanged();
            }
        }
        private global::System.DateTime _ScanDate;
        partial void OnScanDateChanging(global::System.DateTime value);
        partial void OnScanDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Details
        {
            get
            {
                return _Details;
            }
            set
            {
                OnDetailsChanging(value);
                ReportPropertyChanging(""Details"");
                _Details = StructuralObject.SetValidValue(value, false, ""Details"");
                ReportPropertyChanged(""Details"");
                OnDetailsChanged();
            }
        }
        private global::System.String _Details;
        partial void OnDetailsChanging(global::System.String value);
        partial void OnDetailsChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Barcode_IncorrectScanExpected"", ""Barcode"")]
        public BarcodeMm ExpectedBarcode
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanExpected"", ""Barcode"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanExpected"", ""Barcode"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<BarcodeMm> ExpectedBarcodeReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanExpected"", ""Barcode"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanExpected"", ""Barcode"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Barcode_IncorrectScanActual"", ""Barcode"")]
        public BarcodeMm ActualBarcode
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanActual"", ""Barcode"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanActual"", ""Barcode"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<BarcodeMm> ActualBarcodeReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanActual"", ""Barcode"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<BarcodeMm>(""MonsterNamespace.Barcode_IncorrectScanActual"", ""Barcode"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""LastLoginMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class LastLoginMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new LastLoginMm object.
        /// </summary>
        /// <param name=""username"">Initial value of the Username property.</param>
        /// <param name=""loggedIn"">Initial value of the LoggedIn property.</param>
        public static LastLoginMm CreateLastLoginMm(global::System.String username, global::System.DateTime loggedIn)
        {
            LastLoginMm lastLoginMm = new LastLoginMm();
            lastLoginMm.Username = username;
            lastLoginMm.LoggedIn = loggedIn;
            return lastLoginMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Username
        {
            get
            {
                return _Username;
            }
            set
            {
                if (_Username != value)
                {
                    OnUsernameChanging(value);
                    ReportPropertyChanging(""Username"");
                    _Username = StructuralObject.SetValidValue(value, false, ""Username"");
                    ReportPropertyChanged(""Username"");
                    OnUsernameChanged();
                }
            }
        }
        private global::System.String _Username;
        partial void OnUsernameChanging(global::System.String value);
        partial void OnUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime LoggedIn
        {
            get
            {
                return _LoggedIn;
            }
            set
            {
                OnLoggedInChanging(value);
                ReportPropertyChanging(""LoggedIn"");
                _LoggedIn = StructuralObject.SetValidValue(value, ""LoggedIn"");
                ReportPropertyChanged(""LoggedIn"");
                OnLoggedInChanged();
            }
        }
        private global::System.DateTime _LoggedIn;
        partial void OnLoggedInChanging(global::System.DateTime value);
        partial void OnLoggedInChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> LoggedOut
        {
            get
            {
                return _LoggedOut;
            }
            set
            {
                OnLoggedOutChanging(value);
                ReportPropertyChanging(""LoggedOut"");
                _LoggedOut = StructuralObject.SetValidValue(value, ""LoggedOut"");
                ReportPropertyChanged(""LoggedOut"");
                OnLoggedOutChanged();
            }
        }
        private Nullable<global::System.DateTime> _LoggedOut;
        partial void OnLoggedOutChanging(Nullable<global::System.DateTime> value);
        partial void OnLoggedOutChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_LastLogin"", ""Login"")]
        public LoginMm Login
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_LastLogin"", ""Login"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_LastLogin"", ""Login"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> LoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_LastLogin"", ""Login"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_LastLogin"", ""Login"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""LicenseMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class LicenseMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new LicenseMm object.
        /// </summary>
        /// <param name=""name"">Initial value of the Name property.</param>
        /// <param name=""licenseNumber"">Initial value of the LicenseNumber property.</param>
        /// <param name=""restrictions"">Initial value of the Restrictions property.</param>
        /// <param name=""expirationDate"">Initial value of the ExpirationDate property.</param>
        public static LicenseMm CreateLicenseMm(global::System.String name, global::System.String licenseNumber, global::System.String restrictions, global::System.DateTime expirationDate)
        {
            LicenseMm licenseMm = new LicenseMm();
            licenseMm.Name = name;
            licenseMm.LicenseNumber = licenseNumber;
            licenseMm.Restrictions = restrictions;
            licenseMm.ExpirationDate = expirationDate;
            return licenseMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    OnNameChanging(value);
                    ReportPropertyChanging(""Name"");
                    _Name = StructuralObject.SetValidValue(value, false, ""Name"");
                    ReportPropertyChanged(""Name"");
                    OnNameChanged();
                }
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String LicenseNumber
        {
            get
            {
                return _LicenseNumber;
            }
            set
            {
                OnLicenseNumberChanging(value);
                ReportPropertyChanging(""LicenseNumber"");
                _LicenseNumber = StructuralObject.SetValidValue(value, false, ""LicenseNumber"");
                ReportPropertyChanged(""LicenseNumber"");
                OnLicenseNumberChanged();
            }
        }
        private global::System.String _LicenseNumber;
        partial void OnLicenseNumberChanging(global::System.String value);
        partial void OnLicenseNumberChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String LicenseClass
        {
            get
            {
                return _LicenseClass;
            }
            set
            {
                OnLicenseClassChanging(value);
                ReportPropertyChanging(""LicenseClass"");
                _LicenseClass = StructuralObject.SetValidValue(value, false, ""LicenseClass"");
                ReportPropertyChanged(""LicenseClass"");
                OnLicenseClassChanged();
            }
        }
        private global::System.String _LicenseClass = ""C"";
        partial void OnLicenseClassChanging(global::System.String value);
        partial void OnLicenseClassChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Restrictions
        {
            get
            {
                return _Restrictions;
            }
            set
            {
                OnRestrictionsChanging(value);
                ReportPropertyChanging(""Restrictions"");
                _Restrictions = StructuralObject.SetValidValue(value, false, ""Restrictions"");
                ReportPropertyChanged(""Restrictions"");
                OnRestrictionsChanged();
            }
        }
        private global::System.String _Restrictions;
        partial void OnRestrictionsChanging(global::System.String value);
        partial void OnRestrictionsChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime ExpirationDate
        {
            get
            {
                return _ExpirationDate;
            }
            set
            {
                OnExpirationDateChanging(value);
                ReportPropertyChanging(""ExpirationDate"");
                _ExpirationDate = StructuralObject.SetValidValue(value, ""ExpirationDate"");
                ReportPropertyChanged(""ExpirationDate"");
                OnExpirationDateChanged();
            }
        }
        private global::System.DateTime _ExpirationDate;
        partial void OnExpirationDateChanging(global::System.DateTime value);
        partial void OnExpirationDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<LicenseStateMm> State
        {
            get
            {
                return _State;
            }
            set
            {
                OnStateChanging(value);
                ReportPropertyChanging(""State"");
                _State = (Nullable<LicenseStateMm>)StructuralObject.SetValidValue((Nullable<int>)value, ""State"");
                ReportPropertyChanged(""State"");
                OnStateChanged();
            }
        }
        private Nullable<LicenseStateMm> _State;
        partial void OnStateChanging(Nullable<LicenseStateMm> value);
        partial void OnStateChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Driver_License"", ""Driver"")]
        public DriverMm Driver
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<DriverMm>(""MonsterNamespace.Driver_License"", ""Driver"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<DriverMm>(""MonsterNamespace.Driver_License"", ""Driver"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<DriverMm> DriverReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<DriverMm>(""MonsterNamespace.Driver_License"", ""Driver"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<DriverMm>(""MonsterNamespace.Driver_License"", ""Driver"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""LoginMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class LoginMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new LoginMm object.
        /// </summary>
        /// <param name=""username"">Initial value of the Username property.</param>
        /// <param name=""customerId"">Initial value of the CustomerId property.</param>
        public static LoginMm CreateLoginMm(global::System.String username, global::System.Int32 customerId)
        {
            LoginMm loginMm = new LoginMm();
            loginMm.Username = username;
            loginMm.CustomerId = customerId;
            return loginMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Username
        {
            get
            {
                return _Username;
            }
            set
            {
                if (_Username != value)
                {
                    OnUsernameChanging(value);
                    ReportPropertyChanging(""Username"");
                    _Username = StructuralObject.SetValidValue(value, false, ""Username"");
                    ReportPropertyChanged(""Username"");
                    OnUsernameChanged();
                }
            }
        }
        private global::System.String _Username;
        partial void OnUsernameChanging(global::System.String value);
        partial void OnUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 CustomerId
        {
            get
            {
                return _CustomerId;
            }
            set
            {
                OnCustomerIdChanging(value);
                ReportPropertyChanging(""CustomerId"");
                _CustomerId = StructuralObject.SetValidValue(value, ""CustomerId"");
                ReportPropertyChanged(""CustomerId"");
                OnCustomerIdChanged();
            }
        }
        private global::System.Int32 _CustomerId;
        partial void OnCustomerIdChanging(global::System.Int32 value);
        partial void OnCustomerIdChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Customer_Logins"", ""Customer"")]
        public CustomerMm Customer
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Logins"", ""Customer"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Logins"", ""Customer"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<CustomerMm> CustomerReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Logins"", ""Customer"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Logins"", ""Customer"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_LastLogin"", ""LastLogin"")]
        public LastLoginMm LastLogin
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LastLoginMm>(""MonsterNamespace.Login_LastLogin"", ""LastLogin"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LastLoginMm>(""MonsterNamespace.Login_LastLogin"", ""LastLogin"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LastLoginMm> LastLoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LastLoginMm>(""MonsterNamespace.Login_LastLogin"", ""LastLogin"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LastLoginMm>(""MonsterNamespace.Login_LastLogin"", ""LastLogin"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_SentMessages"", ""Message"")]
        public EntityCollection<MessageMm> SentMessages
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<MessageMm>(""MonsterNamespace.Login_SentMessages"", ""Message"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<MessageMm>(""MonsterNamespace.Login_SentMessages"", ""Message"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_ReceivedMessages"", ""Message"")]
        public EntityCollection<MessageMm> ReceivedMessages
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<MessageMm>(""MonsterNamespace.Login_ReceivedMessages"", ""Message"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<MessageMm>(""MonsterNamespace.Login_ReceivedMessages"", ""Message"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_Orders"", ""Orders"")]
        public EntityCollection<OrderMm> Orders
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<OrderMm>(""MonsterNamespace.Login_Orders"", ""Orders"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<OrderMm>(""MonsterNamespace.Login_Orders"", ""Orders"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""MessageMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class MessageMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new MessageMm object.
        /// </summary>
        /// <param name=""messageId"">Initial value of the MessageId property.</param>
        /// <param name=""fromUsername"">Initial value of the FromUsername property.</param>
        /// <param name=""toUsername"">Initial value of the ToUsername property.</param>
        /// <param name=""sent"">Initial value of the Sent property.</param>
        /// <param name=""subject"">Initial value of the Subject property.</param>
        /// <param name=""isRead"">Initial value of the IsRead property.</param>
        public static MessageMm CreateMessageMm(global::System.Int32 messageId, global::System.String fromUsername, global::System.String toUsername, global::System.DateTime sent, global::System.String subject, global::System.Boolean isRead)
        {
            MessageMm messageMm = new MessageMm();
            messageMm.MessageId = messageId;
            messageMm.FromUsername = fromUsername;
            messageMm.ToUsername = toUsername;
            messageMm.Sent = sent;
            messageMm.Subject = subject;
            messageMm.IsRead = isRead;
            return messageMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 MessageId
        {
            get
            {
                return _MessageId;
            }
            set
            {
                if (_MessageId != value)
                {
                    OnMessageIdChanging(value);
                    ReportPropertyChanging(""MessageId"");
                    _MessageId = StructuralObject.SetValidValue(value, ""MessageId"");
                    ReportPropertyChanged(""MessageId"");
                    OnMessageIdChanged();
                }
            }
        }
        private global::System.Int32 _MessageId;
        partial void OnMessageIdChanging(global::System.Int32 value);
        partial void OnMessageIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String FromUsername
        {
            get
            {
                return _FromUsername;
            }
            set
            {
                if (_FromUsername != value)
                {
                    OnFromUsernameChanging(value);
                    ReportPropertyChanging(""FromUsername"");
                    _FromUsername = StructuralObject.SetValidValue(value, false, ""FromUsername"");
                    ReportPropertyChanged(""FromUsername"");
                    OnFromUsernameChanged();
                }
            }
        }
        private global::System.String _FromUsername;
        partial void OnFromUsernameChanging(global::System.String value);
        partial void OnFromUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ToUsername
        {
            get
            {
                return _ToUsername;
            }
            set
            {
                OnToUsernameChanging(value);
                ReportPropertyChanging(""ToUsername"");
                _ToUsername = StructuralObject.SetValidValue(value, false, ""ToUsername"");
                ReportPropertyChanged(""ToUsername"");
                OnToUsernameChanged();
            }
        }
        private global::System.String _ToUsername;
        partial void OnToUsernameChanging(global::System.String value);
        partial void OnToUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Sent
        {
            get
            {
                return _Sent;
            }
            set
            {
                OnSentChanging(value);
                ReportPropertyChanging(""Sent"");
                _Sent = StructuralObject.SetValidValue(value, ""Sent"");
                ReportPropertyChanged(""Sent"");
                OnSentChanged();
            }
        }
        private global::System.DateTime _Sent;
        partial void OnSentChanging(global::System.DateTime value);
        partial void OnSentChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Subject
        {
            get
            {
                return _Subject;
            }
            set
            {
                OnSubjectChanging(value);
                ReportPropertyChanging(""Subject"");
                _Subject = StructuralObject.SetValidValue(value, false, ""Subject"");
                ReportPropertyChanged(""Subject"");
                OnSubjectChanged();
            }
        }
        private global::System.String _Subject;
        partial void OnSubjectChanging(global::System.String value);
        partial void OnSubjectChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Body
        {
            get
            {
                return _Body;
            }
            set
            {
                OnBodyChanging(value);
                ReportPropertyChanging(""Body"");
                _Body = StructuralObject.SetValidValue(value, true, ""Body"");
                ReportPropertyChanged(""Body"");
                OnBodyChanged();
            }
        }
        private global::System.String _Body;
        partial void OnBodyChanging(global::System.String value);
        partial void OnBodyChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Boolean IsRead
        {
            get
            {
                return _IsRead;
            }
            set
            {
                OnIsReadChanging(value);
                ReportPropertyChanging(""IsRead"");
                _IsRead = StructuralObject.SetValidValue(value, ""IsRead"");
                ReportPropertyChanged(""IsRead"");
                OnIsReadChanged();
            }
        }
        private global::System.Boolean _IsRead;
        partial void OnIsReadChanging(global::System.Boolean value);
        partial void OnIsReadChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_SentMessages"", ""Sender"")]
        public LoginMm Sender
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_SentMessages"", ""Sender"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_SentMessages"", ""Sender"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> SenderReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_SentMessages"", ""Sender"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_SentMessages"", ""Sender"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_ReceivedMessages"", ""Recipient"")]
        public LoginMm Recipient
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_ReceivedMessages"", ""Recipient"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_ReceivedMessages"", ""Recipient"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> RecipientReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_ReceivedMessages"", ""Recipient"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_ReceivedMessages"", ""Recipient"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""OrderLineMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [KnownTypeAttribute(typeof(BackOrderLineMm))]
    public partial class OrderLineMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OrderLineMm object.
        /// </summary>
        /// <param name=""orderId"">Initial value of the OrderId property.</param>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""concurrencyToken"">Initial value of the ConcurrencyToken property.</param>
        public static OrderLineMm CreateOrderLineMm(global::System.Int32 orderId, global::System.Int32 productId, global::System.String concurrencyToken)
        {
            OrderLineMm orderLineMm = new OrderLineMm();
            orderLineMm.OrderId = orderId;
            orderLineMm.ProductId = productId;
            orderLineMm.ConcurrencyToken = concurrencyToken;
            return orderLineMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 OrderId
        {
            get
            {
                return _OrderId;
            }
            set
            {
                if (_OrderId != value)
                {
                    OnOrderIdChanging(value);
                    ReportPropertyChanging(""OrderId"");
                    _OrderId = StructuralObject.SetValidValue(value, ""OrderId"");
                    ReportPropertyChanged(""OrderId"");
                    OnOrderIdChanged();
                }
            }
        }
        private global::System.Int32 _OrderId;
        partial void OnOrderIdChanging(global::System.Int32 value);
        partial void OnOrderIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                if (_ProductId != value)
                {
                    OnProductIdChanging(value);
                    ReportPropertyChanging(""ProductId"");
                    _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                    ReportPropertyChanged(""ProductId"");
                    OnProductIdChanged();
                }
            }
        }
        private global::System.Int32 _ProductId;
        partial void OnProductIdChanging(global::System.Int32 value);
        partial void OnProductIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 Quantity
        {
            get
            {
                return _Quantity;
            }
            set
            {
                OnQuantityChanging(value);
                ReportPropertyChanging(""Quantity"");
                _Quantity = StructuralObject.SetValidValue(value, ""Quantity"");
                ReportPropertyChanged(""Quantity"");
                OnQuantityChanged();
            }
        }
        private global::System.Int32 _Quantity = 1;
        partial void OnQuantityChanging(global::System.Int32 value);
        partial void OnQuantityChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ConcurrencyToken
        {
            get
            {
                return _ConcurrencyToken;
            }
            set
            {
                OnConcurrencyTokenChanging(value);
                ReportPropertyChanging(""ConcurrencyToken"");
                _ConcurrencyToken = StructuralObject.SetValidValue(value, false, ""ConcurrencyToken"");
                ReportPropertyChanged(""ConcurrencyToken"");
                OnConcurrencyTokenChanged();
            }
        }
        private global::System.String _ConcurrencyToken;
        partial void OnConcurrencyTokenChanging(global::System.String value);
        partial void OnConcurrencyTokenChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Order_OrderLines"", ""Order"")]
        public OrderMm Order
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderLines"", ""Order"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderLines"", ""Order"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<OrderMm> OrderReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderLines"", ""Order"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderLines"", ""Order"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_OrderLines"", ""Product"")]
        public ProductMm Product
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_OrderLines"", ""Product"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_OrderLines"", ""Product"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductMm> ProductReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_OrderLines"", ""Product"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductMm>(""MonsterNamespace.Product_OrderLines"", ""Product"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""OrderMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class OrderMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OrderMm object.
        /// </summary>
        /// <param name=""orderId"">Initial value of the OrderId property.</param>
        /// <param name=""concurrency"">Initial value of the Concurrency property.</param>
        public static OrderMm CreateOrderMm(global::System.Int32 orderId, ConcurrencyInfoMm concurrency)
        {
            OrderMm orderMm = new OrderMm();
            orderMm.OrderId = orderId;
            orderMm.Concurrency = StructuralObject.VerifyComplexObjectIsNotNull(concurrency, ""Concurrency"");
            return orderMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 OrderId
        {
            get
            {
                return _OrderId;
            }
            set
            {
                if (_OrderId != value)
                {
                    OnOrderIdChanging(value);
                    ReportPropertyChanging(""OrderId"");
                    _OrderId = StructuralObject.SetValidValue(value, ""OrderId"");
                    ReportPropertyChanged(""OrderId"");
                    OnOrderIdChanged();
                }
            }
        }
        private global::System.Int32 _OrderId;
        partial void OnOrderIdChanging(global::System.Int32 value);
        partial void OnOrderIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> CustomerId
        {
            get
            {
                return _CustomerId;
            }
            set
            {
                OnCustomerIdChanging(value);
                ReportPropertyChanging(""CustomerId"");
                _CustomerId = StructuralObject.SetValidValue(value, ""CustomerId"");
                ReportPropertyChanged(""CustomerId"");
                OnCustomerIdChanged();
            }
        }
        private Nullable<global::System.Int32> _CustomerId;
        partial void OnCustomerIdChanging(Nullable<global::System.Int32> value);
        partial void OnCustomerIdChanged();

        #endregion

        #region Complex Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public ConcurrencyInfoMm Concurrency
        {
            get
            {
                _Concurrency = GetValidValue(_Concurrency, ""Concurrency"", false, _ConcurrencyInitialized);
                _ConcurrencyInitialized = true;
                return _Concurrency;
            }
            set
            {
                OnConcurrencyChanging(value);
                ReportPropertyChanging(""Concurrency"");
                _Concurrency = SetValidValue(_Concurrency, value, ""Concurrency"");
                _ConcurrencyInitialized = true;
                ReportPropertyChanged(""Concurrency"");
                OnConcurrencyChanged();
            }
        }
        private ConcurrencyInfoMm _Concurrency;
        private bool _ConcurrencyInitialized;
        partial void OnConcurrencyChanging(ConcurrencyInfoMm value);
        partial void OnConcurrencyChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Customer_Orders"", ""Customer"")]
        public CustomerMm Customer
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Orders"", ""Customer"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Orders"", ""Customer"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<CustomerMm> CustomerReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Orders"", ""Customer"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<CustomerMm>(""MonsterNamespace.Customer_Orders"", ""Customer"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Order_OrderLines"", ""OrderLines"")]
        public EntityCollection<OrderLineMm> OrderLines
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<OrderLineMm>(""MonsterNamespace.Order_OrderLines"", ""OrderLines"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<OrderLineMm>(""MonsterNamespace.Order_OrderLines"", ""OrderLines"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Order_OrderNotes"", ""Notes"")]
        public EntityCollection<OrderNoteMm> Notes
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<OrderNoteMm>(""MonsterNamespace.Order_OrderNotes"", ""Notes"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<OrderNoteMm>(""MonsterNamespace.Order_OrderNotes"", ""Notes"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_Orders"", ""Login"")]
        public LoginMm Login
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_Orders"", ""Login"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_Orders"", ""Login"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> LoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_Orders"", ""Login"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_Orders"", ""Login"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""OrderNoteMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class OrderNoteMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OrderNoteMm object.
        /// </summary>
        /// <param name=""noteId"">Initial value of the NoteId property.</param>
        /// <param name=""note"">Initial value of the Note property.</param>
        public static OrderNoteMm CreateOrderNoteMm(global::System.Int32 noteId, global::System.String note)
        {
            OrderNoteMm orderNoteMm = new OrderNoteMm();
            orderNoteMm.NoteId = noteId;
            orderNoteMm.Note = note;
            return orderNoteMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 NoteId
        {
            get
            {
                return _NoteId;
            }
            set
            {
                if (_NoteId != value)
                {
                    OnNoteIdChanging(value);
                    ReportPropertyChanging(""NoteId"");
                    _NoteId = StructuralObject.SetValidValue(value, ""NoteId"");
                    ReportPropertyChanged(""NoteId"");
                    OnNoteIdChanged();
                }
            }
        }
        private global::System.Int32 _NoteId;
        partial void OnNoteIdChanging(global::System.Int32 value);
        partial void OnNoteIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Note
        {
            get
            {
                return _Note;
            }
            set
            {
                OnNoteChanging(value);
                ReportPropertyChanging(""Note"");
                _Note = StructuralObject.SetValidValue(value, false, ""Note"");
                ReportPropertyChanged(""Note"");
                OnNoteChanged();
            }
        }
        private global::System.String _Note;
        partial void OnNoteChanging(global::System.String value);
        partial void OnNoteChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Order_OrderNotes"", ""Order"")]
        public OrderMm Order
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderNotes"", ""Order"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderNotes"", ""Order"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<OrderMm> OrderReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderNotes"", ""Order"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<OrderMm>(""MonsterNamespace.Order_OrderNotes"", ""Order"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""OrderQualityCheckMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class OrderQualityCheckMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OrderQualityCheckMm object.
        /// </summary>
        /// <param name=""orderId"">Initial value of the OrderId property.</param>
        /// <param name=""checkedBy"">Initial value of the CheckedBy property.</param>
        /// <param name=""checkedDateTime"">Initial value of the CheckedDateTime property.</param>
        public static OrderQualityCheckMm CreateOrderQualityCheckMm(global::System.Int32 orderId, global::System.String checkedBy, global::System.DateTime checkedDateTime)
        {
            OrderQualityCheckMm orderQualityCheckMm = new OrderQualityCheckMm();
            orderQualityCheckMm.OrderId = orderId;
            orderQualityCheckMm.CheckedBy = checkedBy;
            orderQualityCheckMm.CheckedDateTime = checkedDateTime;
            return orderQualityCheckMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 OrderId
        {
            get
            {
                return _OrderId;
            }
            set
            {
                if (_OrderId != value)
                {
                    OnOrderIdChanging(value);
                    ReportPropertyChanging(""OrderId"");
                    _OrderId = StructuralObject.SetValidValue(value, ""OrderId"");
                    ReportPropertyChanged(""OrderId"");
                    OnOrderIdChanged();
                }
            }
        }
        private global::System.Int32 _OrderId;
        partial void OnOrderIdChanging(global::System.Int32 value);
        partial void OnOrderIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String CheckedBy
        {
            get
            {
                return _CheckedBy;
            }
            set
            {
                OnCheckedByChanging(value);
                ReportPropertyChanging(""CheckedBy"");
                _CheckedBy = StructuralObject.SetValidValue(value, false, ""CheckedBy"");
                ReportPropertyChanged(""CheckedBy"");
                OnCheckedByChanged();
            }
        }
        private global::System.String _CheckedBy;
        partial void OnCheckedByChanging(global::System.String value);
        partial void OnCheckedByChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime CheckedDateTime
        {
            get
            {
                return _CheckedDateTime;
            }
            set
            {
                OnCheckedDateTimeChanging(value);
                ReportPropertyChanging(""CheckedDateTime"");
                _CheckedDateTime = StructuralObject.SetValidValue(value, ""CheckedDateTime"");
                ReportPropertyChanged(""CheckedDateTime"");
                OnCheckedDateTimeChanged();
            }
        }
        private global::System.DateTime _CheckedDateTime;
        partial void OnCheckedDateTimeChanging(global::System.DateTime value);
        partial void OnCheckedDateTimeChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Order_QualityCheck"", ""Order"")]
        public OrderMm Order
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_QualityCheck"", ""Order"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_QualityCheck"", ""Order"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<OrderMm> OrderReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<OrderMm>(""MonsterNamespace.Order_QualityCheck"", ""Order"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<OrderMm>(""MonsterNamespace.Order_QualityCheck"", ""Order"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""PageViewMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [KnownTypeAttribute(typeof(ProductPageViewMm))]
    public partial class PageViewMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new PageViewMm object.
        /// </summary>
        /// <param name=""pageViewId"">Initial value of the PageViewId property.</param>
        /// <param name=""username"">Initial value of the Username property.</param>
        /// <param name=""viewed"">Initial value of the Viewed property.</param>
        /// <param name=""pageUrl"">Initial value of the PageUrl property.</param>
        public static PageViewMm CreatePageViewMm(global::System.Int32 pageViewId, global::System.String username, global::System.DateTime viewed, global::System.String pageUrl)
        {
            PageViewMm pageViewMm = new PageViewMm();
            pageViewMm.PageViewId = pageViewId;
            pageViewMm.Username = username;
            pageViewMm.Viewed = viewed;
            pageViewMm.PageUrl = pageUrl;
            return pageViewMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 PageViewId
        {
            get
            {
                return _PageViewId;
            }
            set
            {
                if (_PageViewId != value)
                {
                    OnPageViewIdChanging(value);
                    ReportPropertyChanging(""PageViewId"");
                    _PageViewId = StructuralObject.SetValidValue(value, ""PageViewId"");
                    ReportPropertyChanged(""PageViewId"");
                    OnPageViewIdChanged();
                }
            }
        }
        private global::System.Int32 _PageViewId;
        partial void OnPageViewIdChanging(global::System.Int32 value);
        partial void OnPageViewIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Username
        {
            get
            {
                return _Username;
            }
            set
            {
                OnUsernameChanging(value);
                ReportPropertyChanging(""Username"");
                _Username = StructuralObject.SetValidValue(value, false, ""Username"");
                ReportPropertyChanged(""Username"");
                OnUsernameChanged();
            }
        }
        private global::System.String _Username;
        partial void OnUsernameChanging(global::System.String value);
        partial void OnUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Viewed
        {
            get
            {
                return _Viewed;
            }
            set
            {
                OnViewedChanging(value);
                ReportPropertyChanging(""Viewed"");
                _Viewed = StructuralObject.SetValidValue(value, ""Viewed"");
                ReportPropertyChanged(""Viewed"");
                OnViewedChanged();
            }
        }
        private global::System.DateTime _Viewed;
        partial void OnViewedChanging(global::System.DateTime value);
        partial void OnViewedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String PageUrl
        {
            get
            {
                return _PageUrl;
            }
            set
            {
                OnPageUrlChanging(value);
                ReportPropertyChanging(""PageUrl"");
                _PageUrl = StructuralObject.SetValidValue(value, false, ""PageUrl"");
                ReportPropertyChanged(""PageUrl"");
                OnPageUrlChanged();
            }
        }
        private global::System.String _PageUrl;
        partial void OnPageUrlChanging(global::System.String value);
        partial void OnPageUrlChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_PageViews"", ""Login"")]
        public LoginMm Login
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_PageViews"", ""Login"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_PageViews"", ""Login"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> LoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_PageViews"", ""Login"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_PageViews"", ""Login"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""PasswordResetMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class PasswordResetMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new PasswordResetMm object.
        /// </summary>
        /// <param name=""resetNo"">Initial value of the ResetNo property.</param>
        /// <param name=""username"">Initial value of the Username property.</param>
        /// <param name=""tempPassword"">Initial value of the TempPassword property.</param>
        /// <param name=""emailedTo"">Initial value of the EmailedTo property.</param>
        public static PasswordResetMm CreatePasswordResetMm(global::System.Int32 resetNo, global::System.String username, global::System.String tempPassword, global::System.String emailedTo)
        {
            PasswordResetMm passwordResetMm = new PasswordResetMm();
            passwordResetMm.ResetNo = resetNo;
            passwordResetMm.Username = username;
            passwordResetMm.TempPassword = tempPassword;
            passwordResetMm.EmailedTo = emailedTo;
            return passwordResetMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ResetNo
        {
            get
            {
                return _ResetNo;
            }
            set
            {
                if (_ResetNo != value)
                {
                    OnResetNoChanging(value);
                    ReportPropertyChanging(""ResetNo"");
                    _ResetNo = StructuralObject.SetValidValue(value, ""ResetNo"");
                    ReportPropertyChanged(""ResetNo"");
                    OnResetNoChanged();
                }
            }
        }
        private global::System.Int32 _ResetNo;
        partial void OnResetNoChanging(global::System.Int32 value);
        partial void OnResetNoChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Username
        {
            get
            {
                return _Username;
            }
            set
            {
                if (_Username != value)
                {
                    OnUsernameChanging(value);
                    ReportPropertyChanging(""Username"");
                    _Username = StructuralObject.SetValidValue(value, false, ""Username"");
                    ReportPropertyChanged(""Username"");
                    OnUsernameChanged();
                }
            }
        }
        private global::System.String _Username;
        partial void OnUsernameChanging(global::System.String value);
        partial void OnUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String TempPassword
        {
            get
            {
                return _TempPassword;
            }
            set
            {
                OnTempPasswordChanging(value);
                ReportPropertyChanging(""TempPassword"");
                _TempPassword = StructuralObject.SetValidValue(value, false, ""TempPassword"");
                ReportPropertyChanged(""TempPassword"");
                OnTempPasswordChanged();
            }
        }
        private global::System.String _TempPassword;
        partial void OnTempPasswordChanging(global::System.String value);
        partial void OnTempPasswordChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String EmailedTo
        {
            get
            {
                return _EmailedTo;
            }
            set
            {
                OnEmailedToChanging(value);
                ReportPropertyChanging(""EmailedTo"");
                _EmailedTo = StructuralObject.SetValidValue(value, false, ""EmailedTo"");
                ReportPropertyChanged(""EmailedTo"");
                OnEmailedToChanged();
            }
        }
        private global::System.String _EmailedTo;
        partial void OnEmailedToChanging(global::System.String value);
        partial void OnEmailedToChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_PasswordResets"", ""Login"")]
        public LoginMm Login
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_PasswordResets"", ""Login"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_PasswordResets"", ""Login"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> LoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_PasswordResets"", ""Login"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_PasswordResets"", ""Login"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ProductDetailMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ProductDetailMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ProductDetailMm object.
        /// </summary>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""details"">Initial value of the Details property.</param>
        public static ProductDetailMm CreateProductDetailMm(global::System.Int32 productId, global::System.String details)
        {
            ProductDetailMm productDetailMm = new ProductDetailMm();
            productDetailMm.ProductId = productId;
            productDetailMm.Details = details;
            return productDetailMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                if (_ProductId != value)
                {
                    OnProductIdChanging(value);
                    ReportPropertyChanging(""ProductId"");
                    _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                    ReportPropertyChanged(""ProductId"");
                    OnProductIdChanged();
                }
            }
        }
        private global::System.Int32 _ProductId;
        partial void OnProductIdChanging(global::System.Int32 value);
        partial void OnProductIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Details
        {
            get
            {
                return _Details;
            }
            set
            {
                OnDetailsChanging(value);
                ReportPropertyChanging(""Details"");
                _Details = StructuralObject.SetValidValue(value, false, ""Details"");
                ReportPropertyChanged(""Details"");
                OnDetailsChanged();
            }
        }
        private global::System.String _Details;
        partial void OnDetailsChanging(global::System.String value);
        partial void OnDetailsChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_ProductDetail"", ""Product"")]
        public ProductMm Product
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductDetail"", ""Product"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductDetail"", ""Product"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductMm> ProductReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductDetail"", ""Product"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductDetail"", ""Product"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ProductMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    [KnownTypeAttribute(typeof(DiscontinuedProductMm))]
    public partial class ProductMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ProductMm object.
        /// </summary>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""dimensions"">Initial value of the Dimensions property.</param>
        /// <param name=""baseConcurrency"">Initial value of the BaseConcurrency property.</param>
        /// <param name=""complexConcurrency"">Initial value of the ComplexConcurrency property.</param>
        /// <param name=""nestedComplexConcurrency"">Initial value of the NestedComplexConcurrency property.</param>
        public static ProductMm CreateProductMm(global::System.Int32 productId, DimensionsMm dimensions, global::System.String baseConcurrency, ConcurrencyInfoMm complexConcurrency, AuditInfoMm nestedComplexConcurrency)
        {
            ProductMm productMm = new ProductMm();
            productMm.ProductId = productId;
            productMm.Dimensions = StructuralObject.VerifyComplexObjectIsNotNull(dimensions, ""Dimensions"");
            productMm.BaseConcurrency = baseConcurrency;
            productMm.ComplexConcurrency = StructuralObject.VerifyComplexObjectIsNotNull(complexConcurrency, ""ComplexConcurrency"");
            productMm.NestedComplexConcurrency = StructuralObject.VerifyComplexObjectIsNotNull(nestedComplexConcurrency, ""NestedComplexConcurrency"");
            return productMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                if (_ProductId != value)
                {
                    OnProductIdChanging(value);
                    ReportPropertyChanging(""ProductId"");
                    _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                    ReportPropertyChanged(""ProductId"");
                    OnProductIdChanged();
                }
            }
        }
        private global::System.Int32 _ProductId;
        partial void OnProductIdChanging(global::System.Int32 value);
        partial void OnProductIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Description
        {
            get
            {
                return _Description;
            }
            set
            {
                OnDescriptionChanging(value);
                ReportPropertyChanging(""Description"");
                _Description = StructuralObject.SetValidValue(value, true, ""Description"");
                ReportPropertyChanged(""Description"");
                OnDescriptionChanged();
            }
        }
        private global::System.String _Description;
        partial void OnDescriptionChanging(global::System.String value);
        partial void OnDescriptionChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String BaseConcurrency
        {
            get
            {
                return _BaseConcurrency;
            }
            set
            {
                OnBaseConcurrencyChanging(value);
                ReportPropertyChanging(""BaseConcurrency"");
                _BaseConcurrency = StructuralObject.SetValidValue(value, false, ""BaseConcurrency"");
                ReportPropertyChanged(""BaseConcurrency"");
                OnBaseConcurrencyChanged();
            }
        }
        private global::System.String _BaseConcurrency;
        partial void OnBaseConcurrencyChanging(global::System.String value);
        partial void OnBaseConcurrencyChanged();

        #endregion

        #region Complex Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public DimensionsMm Dimensions
        {
            get
            {
                _Dimensions = GetValidValue(_Dimensions, ""Dimensions"", false, _DimensionsInitialized);
                _DimensionsInitialized = true;
                return _Dimensions;
            }
            set
            {
                OnDimensionsChanging(value);
                ReportPropertyChanging(""Dimensions"");
                _Dimensions = SetValidValue(_Dimensions, value, ""Dimensions"");
                _DimensionsInitialized = true;
                ReportPropertyChanged(""Dimensions"");
                OnDimensionsChanged();
            }
        }
        private DimensionsMm _Dimensions;
        private bool _DimensionsInitialized;
        partial void OnDimensionsChanging(DimensionsMm value);
        partial void OnDimensionsChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public ConcurrencyInfoMm ComplexConcurrency
        {
            get
            {
                _ComplexConcurrency = GetValidValue(_ComplexConcurrency, ""ComplexConcurrency"", false, _ComplexConcurrencyInitialized);
                _ComplexConcurrencyInitialized = true;
                return _ComplexConcurrency;
            }
            set
            {
                OnComplexConcurrencyChanging(value);
                ReportPropertyChanging(""ComplexConcurrency"");
                _ComplexConcurrency = SetValidValue(_ComplexConcurrency, value, ""ComplexConcurrency"");
                _ComplexConcurrencyInitialized = true;
                ReportPropertyChanged(""ComplexConcurrency"");
                OnComplexConcurrencyChanged();
            }
        }
        private ConcurrencyInfoMm _ComplexConcurrency;
        private bool _ComplexConcurrencyInitialized;
        partial void OnComplexConcurrencyChanging(ConcurrencyInfoMm value);
        partial void OnComplexConcurrencyChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public AuditInfoMm NestedComplexConcurrency
        {
            get
            {
                _NestedComplexConcurrency = GetValidValue(_NestedComplexConcurrency, ""NestedComplexConcurrency"", false, _NestedComplexConcurrencyInitialized);
                _NestedComplexConcurrencyInitialized = true;
                return _NestedComplexConcurrency;
            }
            set
            {
                OnNestedComplexConcurrencyChanging(value);
                ReportPropertyChanging(""NestedComplexConcurrency"");
                _NestedComplexConcurrency = SetValidValue(_NestedComplexConcurrency, value, ""NestedComplexConcurrency"");
                _NestedComplexConcurrencyInitialized = true;
                ReportPropertyChanged(""NestedComplexConcurrency"");
                OnNestedComplexConcurrencyChanged();
            }
        }
        private AuditInfoMm _NestedComplexConcurrency;
        private bool _NestedComplexConcurrencyInitialized;
        partial void OnNestedComplexConcurrencyChanging(AuditInfoMm value);
        partial void OnNestedComplexConcurrencyChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Products_Suppliers"", ""Suppliers"")]
        public EntityCollection<SupplierMm> Suppliers
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<SupplierMm>(""MonsterNamespace.Products_Suppliers"", ""Suppliers"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<SupplierMm>(""MonsterNamespace.Products_Suppliers"", ""Suppliers"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""DiscontinuedProduct_Replacement"", ""DiscontinuedProduct"")]
        public EntityCollection<DiscontinuedProductMm> Replaces
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<DiscontinuedProductMm>(""MonsterNamespace.DiscontinuedProduct_Replacement"", ""DiscontinuedProduct"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<DiscontinuedProductMm>(""MonsterNamespace.DiscontinuedProduct_Replacement"", ""DiscontinuedProduct"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_ProductDetail"", ""ProductDetail"")]
        public ProductDetailMm Detail
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductDetailMm>(""MonsterNamespace.Product_ProductDetail"", ""ProductDetail"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductDetailMm>(""MonsterNamespace.Product_ProductDetail"", ""ProductDetail"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductDetailMm> DetailReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductDetailMm>(""MonsterNamespace.Product_ProductDetail"", ""ProductDetail"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductDetailMm>(""MonsterNamespace.Product_ProductDetail"", ""ProductDetail"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_ProductReview"", ""ProductReview"")]
        public EntityCollection<ProductReviewMm> Reviews
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<ProductReviewMm>(""MonsterNamespace.Product_ProductReview"", ""ProductReview"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<ProductReviewMm>(""MonsterNamespace.Product_ProductReview"", ""ProductReview"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_ProductPhoto"", ""ProductPhoto"")]
        public EntityCollection<ProductPhotoMm> Photos
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<ProductPhotoMm>(""MonsterNamespace.Product_ProductPhoto"", ""ProductPhoto"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<ProductPhotoMm>(""MonsterNamespace.Product_ProductPhoto"", ""ProductPhoto"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_Barcodes"", ""Barcodes"")]
        public EntityCollection<BarcodeMm> Barcodes
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<BarcodeMm>(""MonsterNamespace.Product_Barcodes"", ""Barcodes"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<BarcodeMm>(""MonsterNamespace.Product_Barcodes"", ""Barcodes"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ProductPageViewMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ProductPageViewMm : PageViewMm
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ProductPageViewMm object.
        /// </summary>
        /// <param name=""pageViewId"">Initial value of the PageViewId property.</param>
        /// <param name=""username"">Initial value of the Username property.</param>
        /// <param name=""viewed"">Initial value of the Viewed property.</param>
        /// <param name=""pageUrl"">Initial value of the PageUrl property.</param>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        public static ProductPageViewMm CreateProductPageViewMm(global::System.Int32 pageViewId, global::System.String username, global::System.DateTime viewed, global::System.String pageUrl, global::System.Int32 productId)
        {
            ProductPageViewMm productPageViewMm = new ProductPageViewMm();
            productPageViewMm.PageViewId = pageViewId;
            productPageViewMm.Username = username;
            productPageViewMm.Viewed = viewed;
            productPageViewMm.PageUrl = pageUrl;
            productPageViewMm.ProductId = productId;
            return productPageViewMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                OnProductIdChanging(value);
                ReportPropertyChanging(""ProductId"");
                _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                ReportPropertyChanged(""ProductId"");
                OnProductIdChanged();
            }
        }
        private global::System.Int32 _ProductId;
        partial void OnProductIdChanging(global::System.Int32 value);
        partial void OnProductIdChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_ProductPageViews"", ""Product"")]
        public ProductMm Product
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductPageViews"", ""Product"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductPageViews"", ""Product"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductMm> ProductReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductPageViews"", ""Product"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductPageViews"", ""Product"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ProductPhotoMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ProductPhotoMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ProductPhotoMm object.
        /// </summary>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""photoId"">Initial value of the PhotoId property.</param>
        /// <param name=""photo"">Initial value of the Photo property.</param>
        public static ProductPhotoMm CreateProductPhotoMm(global::System.Int32 productId, global::System.Int32 photoId, global::System.Byte[] photo)
        {
            ProductPhotoMm productPhotoMm = new ProductPhotoMm();
            productPhotoMm.ProductId = productId;
            productPhotoMm.PhotoId = photoId;
            productPhotoMm.Photo = photo;
            return productPhotoMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                if (_ProductId != value)
                {
                    OnProductIdChanging(value);
                    ReportPropertyChanging(""ProductId"");
                    _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                    ReportPropertyChanged(""ProductId"");
                    OnProductIdChanged();
                }
            }
        }
        private global::System.Int32 _ProductId;
        partial void OnProductIdChanging(global::System.Int32 value);
        partial void OnProductIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 PhotoId
        {
            get
            {
                return _PhotoId;
            }
            set
            {
                if (_PhotoId != value)
                {
                    OnPhotoIdChanging(value);
                    ReportPropertyChanging(""PhotoId"");
                    _PhotoId = StructuralObject.SetValidValue(value, ""PhotoId"");
                    ReportPropertyChanged(""PhotoId"");
                    OnPhotoIdChanged();
                }
            }
        }
        private global::System.Int32 _PhotoId;
        partial void OnPhotoIdChanging(global::System.Int32 value);
        partial void OnPhotoIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Byte[] Photo
        {
            get
            {
                return StructuralObject.GetValidValue(_Photo);
            }
            set
            {
                OnPhotoChanging(value);
                ReportPropertyChanging(""Photo"");
                _Photo = StructuralObject.SetValidValue(value, false, ""Photo"");
                ReportPropertyChanged(""Photo"");
                OnPhotoChanged();
            }
        }
        private global::System.Byte[] _Photo;
        partial void OnPhotoChanging(global::System.Byte[] value);
        partial void OnPhotoChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""ProductWebFeature_ProductPhoto"", ""ProductWebFeature"")]
        public EntityCollection<ProductWebFeatureMm> Features
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<ProductWebFeatureMm>(""MonsterNamespace.ProductWebFeature_ProductPhoto"", ""ProductWebFeature"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<ProductWebFeatureMm>(""MonsterNamespace.ProductWebFeature_ProductPhoto"", ""ProductWebFeature"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ProductReviewMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ProductReviewMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ProductReviewMm object.
        /// </summary>
        /// <param name=""productId"">Initial value of the ProductId property.</param>
        /// <param name=""reviewId"">Initial value of the ReviewId property.</param>
        /// <param name=""review"">Initial value of the Review property.</param>
        public static ProductReviewMm CreateProductReviewMm(global::System.Int32 productId, global::System.Int32 reviewId, global::System.String review)
        {
            ProductReviewMm productReviewMm = new ProductReviewMm();
            productReviewMm.ProductId = productId;
            productReviewMm.ReviewId = reviewId;
            productReviewMm.Review = review;
            return productReviewMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                if (_ProductId != value)
                {
                    OnProductIdChanging(value);
                    ReportPropertyChanging(""ProductId"");
                    _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                    ReportPropertyChanged(""ProductId"");
                    OnProductIdChanged();
                }
            }
        }
        private global::System.Int32 _ProductId;
        partial void OnProductIdChanging(global::System.Int32 value);
        partial void OnProductIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ReviewId
        {
            get
            {
                return _ReviewId;
            }
            set
            {
                if (_ReviewId != value)
                {
                    OnReviewIdChanging(value);
                    ReportPropertyChanging(""ReviewId"");
                    _ReviewId = StructuralObject.SetValidValue(value, ""ReviewId"");
                    ReportPropertyChanged(""ReviewId"");
                    OnReviewIdChanged();
                }
            }
        }
        private global::System.Int32 _ReviewId;
        partial void OnReviewIdChanging(global::System.Int32 value);
        partial void OnReviewIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Review
        {
            get
            {
                return _Review;
            }
            set
            {
                OnReviewChanging(value);
                ReportPropertyChanging(""Review"");
                _Review = StructuralObject.SetValidValue(value, false, ""Review"");
                ReportPropertyChanged(""Review"");
                OnReviewChanged();
            }
        }
        private global::System.String _Review;
        partial void OnReviewChanging(global::System.String value);
        partial void OnReviewChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Product_ProductReview"", ""Product"")]
        public ProductMm Product
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductReview"", ""Product"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductReview"", ""Product"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductMm> ProductReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductReview"", ""Product"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductMm>(""MonsterNamespace.Product_ProductReview"", ""Product"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""ProductWebFeature_ProductReview"", ""ProductWebFeature"")]
        public EntityCollection<ProductWebFeatureMm> Features
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<ProductWebFeatureMm>(""MonsterNamespace.ProductWebFeature_ProductReview"", ""ProductWebFeature"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<ProductWebFeatureMm>(""MonsterNamespace.ProductWebFeature_ProductReview"", ""ProductWebFeature"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ProductWebFeatureMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ProductWebFeatureMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ProductWebFeatureMm object.
        /// </summary>
        /// <param name=""featureId"">Initial value of the FeatureId property.</param>
        /// <param name=""reviewId"">Initial value of the ReviewId property.</param>
        /// <param name=""heading"">Initial value of the Heading property.</param>
        public static ProductWebFeatureMm CreateProductWebFeatureMm(global::System.Int32 featureId, global::System.Int32 reviewId, global::System.String heading)
        {
            ProductWebFeatureMm productWebFeatureMm = new ProductWebFeatureMm();
            productWebFeatureMm.FeatureId = featureId;
            productWebFeatureMm.ReviewId = reviewId;
            productWebFeatureMm.Heading = heading;
            return productWebFeatureMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 FeatureId
        {
            get
            {
                return _FeatureId;
            }
            set
            {
                if (_FeatureId != value)
                {
                    OnFeatureIdChanging(value);
                    ReportPropertyChanging(""FeatureId"");
                    _FeatureId = StructuralObject.SetValidValue(value, ""FeatureId"");
                    ReportPropertyChanged(""FeatureId"");
                    OnFeatureIdChanged();
                }
            }
        }
        private global::System.Int32 _FeatureId;
        partial void OnFeatureIdChanging(global::System.Int32 value);
        partial void OnFeatureIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> ProductId
        {
            get
            {
                return _ProductId;
            }
            set
            {
                OnProductIdChanging(value);
                ReportPropertyChanging(""ProductId"");
                _ProductId = StructuralObject.SetValidValue(value, ""ProductId"");
                ReportPropertyChanged(""ProductId"");
                OnProductIdChanged();
            }
        }
        private Nullable<global::System.Int32> _ProductId;
        partial void OnProductIdChanging(Nullable<global::System.Int32> value);
        partial void OnProductIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> PhotoId
        {
            get
            {
                return _PhotoId;
            }
            set
            {
                OnPhotoIdChanging(value);
                ReportPropertyChanging(""PhotoId"");
                _PhotoId = StructuralObject.SetValidValue(value, ""PhotoId"");
                ReportPropertyChanged(""PhotoId"");
                OnPhotoIdChanged();
            }
        }
        private Nullable<global::System.Int32> _PhotoId;
        partial void OnPhotoIdChanging(Nullable<global::System.Int32> value);
        partial void OnPhotoIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ReviewId
        {
            get
            {
                return _ReviewId;
            }
            set
            {
                OnReviewIdChanging(value);
                ReportPropertyChanging(""ReviewId"");
                _ReviewId = StructuralObject.SetValidValue(value, ""ReviewId"");
                ReportPropertyChanged(""ReviewId"");
                OnReviewIdChanged();
            }
        }
        private global::System.Int32 _ReviewId;
        partial void OnReviewIdChanging(global::System.Int32 value);
        partial void OnReviewIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Heading
        {
            get
            {
                return _Heading;
            }
            set
            {
                OnHeadingChanging(value);
                ReportPropertyChanging(""Heading"");
                _Heading = StructuralObject.SetValidValue(value, false, ""Heading"");
                ReportPropertyChanged(""Heading"");
                OnHeadingChanged();
            }
        }
        private global::System.String _Heading;
        partial void OnHeadingChanging(global::System.String value);
        partial void OnHeadingChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""ProductWebFeature_ProductReview"", ""ProductReview"")]
        public ProductReviewMm Review
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductReviewMm>(""MonsterNamespace.ProductWebFeature_ProductReview"", ""ProductReview"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductReviewMm>(""MonsterNamespace.ProductWebFeature_ProductReview"", ""ProductReview"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductReviewMm> ReviewReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductReviewMm>(""MonsterNamespace.ProductWebFeature_ProductReview"", ""ProductReview"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductReviewMm>(""MonsterNamespace.ProductWebFeature_ProductReview"", ""ProductReview"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""ProductWebFeature_ProductPhoto"", ""ProductPhoto"")]
        public ProductPhotoMm Photo
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductPhotoMm>(""MonsterNamespace.ProductWebFeature_ProductPhoto"", ""ProductPhoto"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductPhotoMm>(""MonsterNamespace.ProductWebFeature_ProductPhoto"", ""ProductPhoto"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ProductPhotoMm> PhotoReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ProductPhotoMm>(""MonsterNamespace.ProductWebFeature_ProductPhoto"", ""ProductPhoto"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ProductPhotoMm>(""MonsterNamespace.ProductWebFeature_ProductPhoto"", ""ProductPhoto"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ResolutionMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class ResolutionMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ResolutionMm object.
        /// </summary>
        /// <param name=""resolutionId"">Initial value of the ResolutionId property.</param>
        /// <param name=""details"">Initial value of the Details property.</param>
        public static ResolutionMm CreateResolutionMm(global::System.Int32 resolutionId, global::System.String details)
        {
            ResolutionMm resolutionMm = new ResolutionMm();
            resolutionMm.ResolutionId = resolutionId;
            resolutionMm.Details = details;
            return resolutionMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ResolutionId
        {
            get
            {
                return _ResolutionId;
            }
            set
            {
                if (_ResolutionId != value)
                {
                    OnResolutionIdChanging(value);
                    ReportPropertyChanging(""ResolutionId"");
                    _ResolutionId = StructuralObject.SetValidValue(value, ""ResolutionId"");
                    ReportPropertyChanged(""ResolutionId"");
                    OnResolutionIdChanged();
                }
            }
        }
        private global::System.Int32 _ResolutionId;
        partial void OnResolutionIdChanging(global::System.Int32 value);
        partial void OnResolutionIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Details
        {
            get
            {
                return _Details;
            }
            set
            {
                OnDetailsChanging(value);
                ReportPropertyChanging(""Details"");
                _Details = StructuralObject.SetValidValue(value, false, ""Details"");
                ReportPropertyChanged(""Details"");
                OnDetailsChanged();
            }
        }
        private global::System.String _Details;
        partial void OnDetailsChanging(global::System.String value);
        partial void OnDetailsChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Complaint_Resolution"", ""Complaint"")]
        public ComplaintMm Complaint
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComplaintMm>(""MonsterNamespace.Complaint_Resolution"", ""Complaint"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComplaintMm>(""MonsterNamespace.Complaint_Resolution"", ""Complaint"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<ComplaintMm> ComplaintReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<ComplaintMm>(""MonsterNamespace.Complaint_Resolution"", ""Complaint"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<ComplaintMm>(""MonsterNamespace.Complaint_Resolution"", ""Complaint"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""RSATokenMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class RSATokenMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new RSATokenMm object.
        /// </summary>
        /// <param name=""serial"">Initial value of the Serial property.</param>
        /// <param name=""issued"">Initial value of the Issued property.</param>
        public static RSATokenMm CreateRSATokenMm(global::System.String serial, global::System.DateTime issued)
        {
            RSATokenMm rSATokenMm = new RSATokenMm();
            rSATokenMm.Serial = serial;
            rSATokenMm.Issued = issued;
            return rSATokenMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Serial
        {
            get
            {
                return _Serial;
            }
            set
            {
                if (_Serial != value)
                {
                    OnSerialChanging(value);
                    ReportPropertyChanging(""Serial"");
                    _Serial = StructuralObject.SetValidValue(value, false, ""Serial"");
                    ReportPropertyChanged(""Serial"");
                    OnSerialChanged();
                }
            }
        }
        private global::System.String _Serial;
        partial void OnSerialChanging(global::System.String value);
        partial void OnSerialChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Issued
        {
            get
            {
                return _Issued;
            }
            set
            {
                OnIssuedChanging(value);
                ReportPropertyChanging(""Issued"");
                _Issued = StructuralObject.SetValidValue(value, ""Issued"");
                ReportPropertyChanged(""Issued"");
                OnIssuedChanged();
            }
        }
        private global::System.DateTime _Issued;
        partial void OnIssuedChanging(global::System.DateTime value);
        partial void OnIssuedChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_RSAToken"", ""Login"")]
        public LoginMm Login
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_RSAToken"", ""Login"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_RSAToken"", ""Login"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> LoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_RSAToken"", ""Login"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_RSAToken"", ""Login"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""SmartCardMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class SmartCardMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SmartCardMm object.
        /// </summary>
        /// <param name=""username"">Initial value of the Username property.</param>
        /// <param name=""cardSerial"">Initial value of the CardSerial property.</param>
        /// <param name=""issued"">Initial value of the Issued property.</param>
        public static SmartCardMm CreateSmartCardMm(global::System.String username, global::System.String cardSerial, global::System.DateTime issued)
        {
            SmartCardMm smartCardMm = new SmartCardMm();
            smartCardMm.Username = username;
            smartCardMm.CardSerial = cardSerial;
            smartCardMm.Issued = issued;
            return smartCardMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Username
        {
            get
            {
                return _Username;
            }
            set
            {
                if (_Username != value)
                {
                    OnUsernameChanging(value);
                    ReportPropertyChanging(""Username"");
                    _Username = StructuralObject.SetValidValue(value, false, ""Username"");
                    ReportPropertyChanged(""Username"");
                    OnUsernameChanged();
                }
            }
        }
        private global::System.String _Username;
        partial void OnUsernameChanging(global::System.String value);
        partial void OnUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String CardSerial
        {
            get
            {
                return _CardSerial;
            }
            set
            {
                OnCardSerialChanging(value);
                ReportPropertyChanging(""CardSerial"");
                _CardSerial = StructuralObject.SetValidValue(value, false, ""CardSerial"");
                ReportPropertyChanged(""CardSerial"");
                OnCardSerialChanged();
            }
        }
        private global::System.String _CardSerial;
        partial void OnCardSerialChanging(global::System.String value);
        partial void OnCardSerialChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Issued
        {
            get
            {
                return _Issued;
            }
            set
            {
                OnIssuedChanging(value);
                ReportPropertyChanging(""Issued"");
                _Issued = StructuralObject.SetValidValue(value, ""Issued"");
                ReportPropertyChanged(""Issued"");
                OnIssuedChanged();
            }
        }
        private global::System.DateTime _Issued;
        partial void OnIssuedChanging(global::System.DateTime value);
        partial void OnIssuedChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Login_SmartCard"", ""Login"")]
        public LoginMm Login
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_SmartCard"", ""Login"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_SmartCard"", ""Login"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LoginMm> LoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LoginMm>(""MonsterNamespace.Login_SmartCard"", ""Login"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LoginMm>(""MonsterNamespace.Login_SmartCard"", ""Login"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""LastLogin_SmartCard"", ""LastLogin"")]
        public LastLoginMm LastLogin
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LastLoginMm>(""MonsterNamespace.LastLogin_SmartCard"", ""LastLogin"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LastLoginMm>(""MonsterNamespace.LastLogin_SmartCard"", ""LastLogin"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<LastLoginMm> LastLoginReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<LastLoginMm>(""MonsterNamespace.LastLogin_SmartCard"", ""LastLogin"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<LastLoginMm>(""MonsterNamespace.LastLogin_SmartCard"", ""LastLogin"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""SupplierInfoMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class SupplierInfoMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SupplierInfoMm object.
        /// </summary>
        /// <param name=""supplierInfoId"">Initial value of the SupplierInfoId property.</param>
        /// <param name=""information"">Initial value of the Information property.</param>
        public static SupplierInfoMm CreateSupplierInfoMm(global::System.Int32 supplierInfoId, global::System.String information)
        {
            SupplierInfoMm supplierInfoMm = new SupplierInfoMm();
            supplierInfoMm.SupplierInfoId = supplierInfoId;
            supplierInfoMm.Information = information;
            return supplierInfoMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 SupplierInfoId
        {
            get
            {
                return _SupplierInfoId;
            }
            set
            {
                if (_SupplierInfoId != value)
                {
                    OnSupplierInfoIdChanging(value);
                    ReportPropertyChanging(""SupplierInfoId"");
                    _SupplierInfoId = StructuralObject.SetValidValue(value, ""SupplierInfoId"");
                    ReportPropertyChanged(""SupplierInfoId"");
                    OnSupplierInfoIdChanged();
                }
            }
        }
        private global::System.Int32 _SupplierInfoId;
        partial void OnSupplierInfoIdChanging(global::System.Int32 value);
        partial void OnSupplierInfoIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Information
        {
            get
            {
                return _Information;
            }
            set
            {
                OnInformationChanging(value);
                ReportPropertyChanging(""Information"");
                _Information = StructuralObject.SetValidValue(value, false, ""Information"");
                ReportPropertyChanged(""Information"");
                OnInformationChanged();
            }
        }
        private global::System.String _Information;
        partial void OnInformationChanging(global::System.String value);
        partial void OnInformationChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Supplier_SupplierInfo"", ""Supplier"")]
        public SupplierMm Supplier
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_SupplierInfo"", ""Supplier"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_SupplierInfo"", ""Supplier"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<SupplierMm> SupplierReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_SupplierInfo"", ""Supplier"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<SupplierMm>(""MonsterNamespace.Supplier_SupplierInfo"", ""Supplier"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""SupplierLogoMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class SupplierLogoMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SupplierLogoMm object.
        /// </summary>
        /// <param name=""supplierId"">Initial value of the SupplierId property.</param>
        /// <param name=""logo"">Initial value of the Logo property.</param>
        public static SupplierLogoMm CreateSupplierLogoMm(global::System.Int32 supplierId, global::System.Byte[] logo)
        {
            SupplierLogoMm supplierLogoMm = new SupplierLogoMm();
            supplierLogoMm.SupplierId = supplierId;
            supplierLogoMm.Logo = logo;
            return supplierLogoMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 SupplierId
        {
            get
            {
                return _SupplierId;
            }
            set
            {
                if (_SupplierId != value)
                {
                    OnSupplierIdChanging(value);
                    ReportPropertyChanging(""SupplierId"");
                    _SupplierId = StructuralObject.SetValidValue(value, ""SupplierId"");
                    ReportPropertyChanged(""SupplierId"");
                    OnSupplierIdChanged();
                }
            }
        }
        private global::System.Int32 _SupplierId;
        partial void OnSupplierIdChanging(global::System.Int32 value);
        partial void OnSupplierIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Byte[] Logo
        {
            get
            {
                return StructuralObject.GetValidValue(_Logo);
            }
            set
            {
                OnLogoChanging(value);
                ReportPropertyChanging(""Logo"");
                _Logo = StructuralObject.SetValidValue(value, false, ""Logo"");
                ReportPropertyChanged(""Logo"");
                OnLogoChanged();
            }
        }
        private global::System.Byte[] _Logo;
        partial void OnLogoChanging(global::System.Byte[] value);
        partial void OnLogoChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""SupplierMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class SupplierMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SupplierMm object.
        /// </summary>
        /// <param name=""supplierId"">Initial value of the SupplierId property.</param>
        /// <param name=""name"">Initial value of the Name property.</param>
        public static SupplierMm CreateSupplierMm(global::System.Int32 supplierId, global::System.String name)
        {
            SupplierMm supplierMm = new SupplierMm();
            supplierMm.SupplierId = supplierId;
            supplierMm.Name = name;
            return supplierMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 SupplierId
        {
            get
            {
                return _SupplierId;
            }
            set
            {
                if (_SupplierId != value)
                {
                    OnSupplierIdChanging(value);
                    ReportPropertyChanging(""SupplierId"");
                    _SupplierId = StructuralObject.SetValidValue(value, ""SupplierId"");
                    ReportPropertyChanged(""SupplierId"");
                    OnSupplierIdChanged();
                }
            }
        }
        private global::System.Int32 _SupplierId;
        partial void OnSupplierIdChanging(global::System.Int32 value);
        partial void OnSupplierIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging(""Name"");
                _Name = StructuralObject.SetValidValue(value, false, ""Name"");
                ReportPropertyChanged(""Name"");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();

        #endregion

        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Products_Suppliers"", ""Products"")]
        public EntityCollection<ProductMm> Products
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<ProductMm>(""MonsterNamespace.Products_Suppliers"", ""Products"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<ProductMm>(""MonsterNamespace.Products_Suppliers"", ""Products"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Supplier_BackOrderLines"", ""BackOrderLines"")]
        public EntityCollection<BackOrderLineMm> BackOrderLines
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<BackOrderLineMm>(""MonsterNamespace.Supplier_BackOrderLines"", ""BackOrderLines"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<BackOrderLineMm>(""MonsterNamespace.Supplier_BackOrderLines"", ""BackOrderLines"", value);
                }
            }
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute(""MonsterNamespace"", ""Supplier_SupplierLogo"", ""Logo"")]
        public SupplierLogoMm Logo
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierLogoMm>(""MonsterNamespace.Supplier_SupplierLogo"", ""Logo"").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierLogoMm>(""MonsterNamespace.Supplier_SupplierLogo"", ""Logo"").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<SupplierLogoMm> LogoReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<SupplierLogoMm>(""MonsterNamespace.Supplier_SupplierLogo"", ""Logo"");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<SupplierLogoMm>(""MonsterNamespace.Supplier_SupplierLogo"", ""Logo"", value);
                }
            }
        }

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""SuspiciousActivityMm"")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class SuspiciousActivityMm : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SuspiciousActivityMm object.
        /// </summary>
        /// <param name=""suspiciousActivityId"">Initial value of the SuspiciousActivityId property.</param>
        /// <param name=""activity"">Initial value of the Activity property.</param>
        public static SuspiciousActivityMm CreateSuspiciousActivityMm(global::System.Int32 suspiciousActivityId, global::System.String activity)
        {
            SuspiciousActivityMm suspiciousActivityMm = new SuspiciousActivityMm();
            suspiciousActivityMm.SuspiciousActivityId = suspiciousActivityId;
            suspiciousActivityMm.Activity = activity;
            return suspiciousActivityMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 SuspiciousActivityId
        {
            get
            {
                return _SuspiciousActivityId;
            }
            set
            {
                if (_SuspiciousActivityId != value)
                {
                    OnSuspiciousActivityIdChanging(value);
                    ReportPropertyChanging(""SuspiciousActivityId"");
                    _SuspiciousActivityId = StructuralObject.SetValidValue(value, ""SuspiciousActivityId"");
                    ReportPropertyChanged(""SuspiciousActivityId"");
                    OnSuspiciousActivityIdChanged();
                }
            }
        }
        private global::System.Int32 _SuspiciousActivityId;
        partial void OnSuspiciousActivityIdChanging(global::System.Int32 value);
        partial void OnSuspiciousActivityIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Activity
        {
            get
            {
                return _Activity;
            }
            set
            {
                OnActivityChanging(value);
                ReportPropertyChanging(""Activity"");
                _Activity = StructuralObject.SetValidValue(value, false, ""Activity"");
                ReportPropertyChanged(""Activity"");
                OnActivityChanged();
            }
        }
        private global::System.String _Activity;
        partial void OnActivityChanging(global::System.String value);
        partial void OnActivityChanged();

        #endregion

    }

    #endregion

    #region ComplexTypes
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""AuditInfoMm"")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class AuditInfoMm : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new AuditInfoMm object.
        /// </summary>
        /// <param name=""modifiedDate"">Initial value of the ModifiedDate property.</param>
        /// <param name=""modifiedBy"">Initial value of the ModifiedBy property.</param>
        /// <param name=""concurrency"">Initial value of the Concurrency property.</param>
        public static AuditInfoMm CreateAuditInfoMm(global::System.DateTime modifiedDate, global::System.String modifiedBy, ConcurrencyInfoMm concurrency)
        {
            AuditInfoMm auditInfoMm = new AuditInfoMm();
            auditInfoMm.ModifiedDate = modifiedDate;
            auditInfoMm.ModifiedBy = modifiedBy;
            auditInfoMm.Concurrency = StructuralObject.VerifyComplexObjectIsNotNull(concurrency, ""Concurrency"");
            return auditInfoMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime ModifiedDate
        {
            get
            {
                return _ModifiedDate;
            }
            set
            {
                OnModifiedDateChanging(value);
                ReportPropertyChanging(""ModifiedDate"");
                _ModifiedDate = StructuralObject.SetValidValue(value, ""ModifiedDate"");
                ReportPropertyChanged(""ModifiedDate"");
                OnModifiedDateChanged();
            }
        }
        private global::System.DateTime _ModifiedDate;
        partial void OnModifiedDateChanging(global::System.DateTime value);
        partial void OnModifiedDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ModifiedBy
        {
            get
            {
                return _ModifiedBy;
            }
            set
            {
                OnModifiedByChanging(value);
                ReportPropertyChanging(""ModifiedBy"");
                _ModifiedBy = StructuralObject.SetValidValue(value, false, ""ModifiedBy"");
                ReportPropertyChanged(""ModifiedBy"");
                OnModifiedByChanged();
            }
        }
        private global::System.String _ModifiedBy;
        partial void OnModifiedByChanging(global::System.String value);
        partial void OnModifiedByChanged();

        #endregion

        #region Complex Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public ConcurrencyInfoMm Concurrency
        {
            get
            {
                _Concurrency = GetValidValue(_Concurrency, ""Concurrency"", false, _ConcurrencyInitialized);
                _ConcurrencyInitialized = true;
                return _Concurrency;
            }
            set
            {
                OnConcurrencyChanging(value);
                ReportPropertyChanging(""Concurrency"");
                _Concurrency = SetValidValue(_Concurrency, value, ""Concurrency"");
                _ConcurrencyInitialized = true;
                ReportPropertyChanged(""Concurrency"");
                OnConcurrencyChanged();
            }
        }
        private ConcurrencyInfoMm _Concurrency;
        private bool _ConcurrencyInitialized;
        partial void OnConcurrencyChanging(ConcurrencyInfoMm value);
        partial void OnConcurrencyChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ConcurrencyInfoMm"")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class ConcurrencyInfoMm : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ConcurrencyInfoMm object.
        /// </summary>
        /// <param name=""token"">Initial value of the Token property.</param>
        public static ConcurrencyInfoMm CreateConcurrencyInfoMm(global::System.String token)
        {
            ConcurrencyInfoMm concurrencyInfoMm = new ConcurrencyInfoMm();
            concurrencyInfoMm.Token = token;
            return concurrencyInfoMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Token
        {
            get
            {
                return _Token;
            }
            set
            {
                OnTokenChanging(value);
                ReportPropertyChanging(""Token"");
                _Token = StructuralObject.SetValidValue(value, false, ""Token"");
                ReportPropertyChanged(""Token"");
                OnTokenChanged();
            }
        }
        private global::System.String _Token;
        partial void OnTokenChanging(global::System.String value);
        partial void OnTokenChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> QueriedDateTime
        {
            get
            {
                return _QueriedDateTime;
            }
            set
            {
                OnQueriedDateTimeChanging(value);
                ReportPropertyChanging(""QueriedDateTime"");
                _QueriedDateTime = StructuralObject.SetValidValue(value, ""QueriedDateTime"");
                ReportPropertyChanged(""QueriedDateTime"");
                OnQueriedDateTimeChanged();
            }
        }
        private Nullable<global::System.DateTime> _QueriedDateTime;
        partial void OnQueriedDateTimeChanging(Nullable<global::System.DateTime> value);
        partial void OnQueriedDateTimeChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""ContactDetailsMm"")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class ContactDetailsMm : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new ContactDetailsMm object.
        /// </summary>
        /// <param name=""email"">Initial value of the Email property.</param>
        /// <param name=""homePhone"">Initial value of the HomePhone property.</param>
        /// <param name=""workPhone"">Initial value of the WorkPhone property.</param>
        /// <param name=""mobilePhone"">Initial value of the MobilePhone property.</param>
        public static ContactDetailsMm CreateContactDetailsMm(global::System.String email, PhoneMm homePhone, PhoneMm workPhone, PhoneMm mobilePhone)
        {
            ContactDetailsMm contactDetailsMm = new ContactDetailsMm();
            contactDetailsMm.Email = email;
            contactDetailsMm.HomePhone = StructuralObject.VerifyComplexObjectIsNotNull(homePhone, ""HomePhone"");
            contactDetailsMm.WorkPhone = StructuralObject.VerifyComplexObjectIsNotNull(workPhone, ""WorkPhone"");
            contactDetailsMm.MobilePhone = StructuralObject.VerifyComplexObjectIsNotNull(mobilePhone, ""MobilePhone"");
            return contactDetailsMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Email
        {
            get
            {
                return _Email;
            }
            set
            {
                OnEmailChanging(value);
                ReportPropertyChanging(""Email"");
                _Email = StructuralObject.SetValidValue(value, false, ""Email"");
                ReportPropertyChanged(""Email"");
                OnEmailChanged();
            }
        }
        private global::System.String _Email;
        partial void OnEmailChanging(global::System.String value);
        partial void OnEmailChanged();

        #endregion

        #region Complex Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public PhoneMm HomePhone
        {
            get
            {
                _HomePhone = GetValidValue(_HomePhone, ""HomePhone"", false, _HomePhoneInitialized);
                _HomePhoneInitialized = true;
                return _HomePhone;
            }
            set
            {
                OnHomePhoneChanging(value);
                ReportPropertyChanging(""HomePhone"");
                _HomePhone = SetValidValue(_HomePhone, value, ""HomePhone"");
                _HomePhoneInitialized = true;
                ReportPropertyChanged(""HomePhone"");
                OnHomePhoneChanged();
            }
        }
        private PhoneMm _HomePhone;
        private bool _HomePhoneInitialized;
        partial void OnHomePhoneChanging(PhoneMm value);
        partial void OnHomePhoneChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public PhoneMm WorkPhone
        {
            get
            {
                _WorkPhone = GetValidValue(_WorkPhone, ""WorkPhone"", false, _WorkPhoneInitialized);
                _WorkPhoneInitialized = true;
                return _WorkPhone;
            }
            set
            {
                OnWorkPhoneChanging(value);
                ReportPropertyChanging(""WorkPhone"");
                _WorkPhone = SetValidValue(_WorkPhone, value, ""WorkPhone"");
                _WorkPhoneInitialized = true;
                ReportPropertyChanged(""WorkPhone"");
                OnWorkPhoneChanged();
            }
        }
        private PhoneMm _WorkPhone;
        private bool _WorkPhoneInitialized;
        partial void OnWorkPhoneChanging(PhoneMm value);
        partial void OnWorkPhoneChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmComplexPropertyAttribute()]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [XmlElement(IsNullable=true)]
        [SoapElement(IsNullable=true)]
        [DataMemberAttribute()]
        public PhoneMm MobilePhone
        {
            get
            {
                _MobilePhone = GetValidValue(_MobilePhone, ""MobilePhone"", false, _MobilePhoneInitialized);
                _MobilePhoneInitialized = true;
                return _MobilePhone;
            }
            set
            {
                OnMobilePhoneChanging(value);
                ReportPropertyChanging(""MobilePhone"");
                _MobilePhone = SetValidValue(_MobilePhone, value, ""MobilePhone"");
                _MobilePhoneInitialized = true;
                ReportPropertyChanged(""MobilePhone"");
                OnMobilePhoneChanged();
            }
        }
        private PhoneMm _MobilePhone;
        private bool _MobilePhoneInitialized;
        partial void OnMobilePhoneChanging(PhoneMm value);
        partial void OnMobilePhoneChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""DimensionsMm"")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class DimensionsMm : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new DimensionsMm object.
        /// </summary>
        /// <param name=""width"">Initial value of the Width property.</param>
        /// <param name=""height"">Initial value of the Height property.</param>
        /// <param name=""depth"">Initial value of the Depth property.</param>
        public static DimensionsMm CreateDimensionsMm(global::System.Decimal width, global::System.Decimal height, global::System.Decimal depth)
        {
            DimensionsMm dimensionsMm = new DimensionsMm();
            dimensionsMm.Width = width;
            dimensionsMm.Height = height;
            dimensionsMm.Depth = depth;
            return dimensionsMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Decimal Width
        {
            get
            {
                return _Width;
            }
            set
            {
                OnWidthChanging(value);
                ReportPropertyChanging(""Width"");
                _Width = StructuralObject.SetValidValue(value, ""Width"");
                ReportPropertyChanged(""Width"");
                OnWidthChanged();
            }
        }
        private global::System.Decimal _Width;
        partial void OnWidthChanging(global::System.Decimal value);
        partial void OnWidthChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Decimal Height
        {
            get
            {
                return _Height;
            }
            set
            {
                OnHeightChanging(value);
                ReportPropertyChanging(""Height"");
                _Height = StructuralObject.SetValidValue(value, ""Height"");
                ReportPropertyChanged(""Height"");
                OnHeightChanged();
            }
        }
        private global::System.Decimal _Height;
        partial void OnHeightChanging(global::System.Decimal value);
        partial void OnHeightChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Decimal Depth
        {
            get
            {
                return _Depth;
            }
            set
            {
                OnDepthChanging(value);
                ReportPropertyChanging(""Depth"");
                _Depth = StructuralObject.SetValidValue(value, ""Depth"");
                ReportPropertyChanged(""Depth"");
                OnDepthChanged();
            }
        }
        private global::System.Decimal _Depth;
        partial void OnDepthChanging(global::System.Decimal value);
        partial void OnDepthChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""PhoneMm"")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class PhoneMm : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new PhoneMm object.
        /// </summary>
        /// <param name=""phoneNumber"">Initial value of the PhoneNumber property.</param>
        /// <param name=""phoneType"">Initial value of the PhoneType property.</param>
        public static PhoneMm CreatePhoneMm(global::System.String phoneNumber, PhoneTypeMm phoneType)
        {
            PhoneMm phoneMm = new PhoneMm();
            phoneMm.PhoneNumber = phoneNumber;
            phoneMm.PhoneType = phoneType;
            return phoneMm;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String PhoneNumber
        {
            get
            {
                return _PhoneNumber;
            }
            set
            {
                OnPhoneNumberChanging(value);
                ReportPropertyChanging(""PhoneNumber"");
                _PhoneNumber = StructuralObject.SetValidValue(value, false, ""PhoneNumber"");
                ReportPropertyChanged(""PhoneNumber"");
                OnPhoneNumberChanged();
            }
        }
        private global::System.String _PhoneNumber;
        partial void OnPhoneNumberChanging(global::System.String value);
        partial void OnPhoneNumberChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Extension
        {
            get
            {
                return _Extension;
            }
            set
            {
                OnExtensionChanging(value);
                ReportPropertyChanging(""Extension"");
                _Extension = StructuralObject.SetValidValue(value, true, ""Extension"");
                ReportPropertyChanged(""Extension"");
                OnExtensionChanged();
            }
        }
        private global::System.String _Extension = ""None"";
        partial void OnExtensionChanging(global::System.String value);
        partial void OnExtensionChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public PhoneTypeMm PhoneType
        {
            get
            {
                return _PhoneType;
            }
            set
            {
                OnPhoneTypeChanging(value);
                ReportPropertyChanging(""PhoneType"");
                _PhoneType = (PhoneTypeMm)StructuralObject.SetValidValue((int)value, ""PhoneType"");
                ReportPropertyChanged(""PhoneType"");
                OnPhoneTypeChanged();
            }
        }
        private PhoneTypeMm _PhoneType;
        partial void OnPhoneTypeChanging(PhoneTypeMm value);
        partial void OnPhoneTypeChanged();

        #endregion

    }

    #endregion

    #region Enums
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEnumTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""LicenseStateMm"")]
    [DataContractAttribute()]
    public enum LicenseStateMm : int
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EnumMemberAttribute()]
        Active = 1,
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EnumMemberAttribute()]
        Suspended = 2,
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EnumMemberAttribute()]
        Revoked = 3
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEnumTypeAttribute(NamespaceName=""MonsterNamespace"", Name=""PhoneTypeMm"")]
    [DataContractAttribute()]
    public enum PhoneTypeMm : int
    {
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EnumMemberAttribute()]
        Cell = 1,
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EnumMemberAttribute()]
        Land = 2,
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EnumMemberAttribute()]
        Satellite = 3
    }

    #endregion

}";
    }
}
