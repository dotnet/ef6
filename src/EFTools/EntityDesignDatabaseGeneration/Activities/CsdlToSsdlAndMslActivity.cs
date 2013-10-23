// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     A Windows Workflow activity that generates a storage model and mapping information based on a conceptual model.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Csdl")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssdl")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msl")]
    public sealed class CsdlToSsdlAndMslActivity : OutputGeneratorActivity
    {
        /// <summary>
        ///     A Windows Workflow <see cref="InArgument{T}" /> that specifies the conceptual schema definition language (CSDL) from
        ///     which store schema definition language (SSDL) and mapping specification language (MSL) are generated.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Csdl")]
        public InArgument<EdmItemCollection> CsdlInput { get; set; }

        /// <summary>
        ///     The assembly-qualified name of the type used to generate mapping specification language (MSL)
        ///     from the conceptual schema definition language (CSDL) in the CsdlInput property.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msl")]
        public InArgument<string> MslOutputGeneratorType { get; set; }

        /// <summary>
        ///     A Windows Workflow <see cref="OutArgument{T}" /> that specifies the store schema language definition (SSDL)
        ///     generated from conceptual schema definition language (CSDL) in the CsdlInput property.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssdl")]
        public OutArgument<string> SsdlOutput { get; set; }

        /// <summary>
        ///     A Windows Workflow <see cref="OutArgument{T}" /> that specifies the mapping specification language (MSL)
        ///     generated from conceptual schema definition language (CSDL) in the CsdlInput property.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msl")]
        public OutArgument<string> MslOutput { get; set; }

        /// <summary>
        ///     Generates output that is supplied to the specified NativeActivityContext based on input specified in the NativeActivityContext.
        /// </summary>
        /// <param name="context"> The state of the current activity. </param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        protected override void Execute(NativeActivityContext context)
        {
            var edmItemCollection = CsdlInput.Get(context);
            if (edmItemCollection == null)
            {
                throw new InvalidOperationException(Resources.ErrorCouldNotFindCSDL);
            }

            var symbolResolver = context.GetExtension<SymbolResolver>();
            var edmParameterBag = symbolResolver[typeof(EdmParameterBag).Name] as EdmParameterBag;
            if (edmParameterBag == null)
            {
                throw new InvalidOperationException(Resources.ErrorNoEdmParameterBag);
            }

            // Find the TargetVersion parameter
            var targetFrameworkVersion = edmParameterBag.GetParameter<Version>(EdmParameterBag.ParameterName.TargetVersion);
            if (targetFrameworkVersion == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.ErrorNoParameterDefined,
                        EdmParameterBag.ParameterName.TargetVersion.ToString()));
            }

            // Validate the TargetVersion parameter
            if (false == EntityFrameworkVersion.IsValidVersion(targetFrameworkVersion))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidTargetVersion, targetFrameworkVersion));
            }

            // Construct the Code View inputs in a dictionary 
            var inputs = new Dictionary<string, object>
                {
                    { EdmConstants.csdlInputName, edmItemCollection }
                };

            // Process the SSDL and MSL code views, feeding in the CSDL
            var ssdl = ProcessOutputGenerator<string>(OutputGeneratorType.Get(context), context, inputs);
            var msl = ProcessOutputGenerator<string>(MslOutputGeneratorType.Get(context), context, inputs);

            // Validate the SSDL, but catch any naming errors and throw a friendlier one
            var ssdlCollection = EdmExtension.CreateAndValidateStoreItemCollection(
                ssdl, targetFrameworkVersion, DependencyResolver.Instance, true);

#if DEBUG
            // Validate the MSL in Debug mode
            IList<EdmSchemaError> mslErrors;
            EdmExtension.CreateStorageMappingItemCollection(
                edmItemCollection, ssdlCollection, msl, out mslErrors);
            if (mslErrors != null
                && mslErrors.Count > 0)
            {
                var errorSb = new StringBuilder();
                errorSb.AppendLine("Encountered the following errors while validating the MSL:");
                foreach (var error in mslErrors)
                {
                    errorSb.AppendLine(error.Message);
                }

                Debug.Fail(errorSb.ToString());
            }
#endif

            // We are done processing, save off all the outputs for the next stage
            SsdlOutput.Set(context, ssdl);
            MslOutput.Set(context, msl);
        }
    }
}
