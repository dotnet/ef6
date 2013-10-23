// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using XmlDesignerBaseTextSpan = Microsoft.Data.Tools.XmlDesignerBase.Model.TextSpan;
using VSTextSpan = Microsoft.VisualStudio.TextManager.Interop.TextSpan;

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Threading;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class VSXmlModel : XmlModel
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public event EventHandler BufferReload;

        private Microsoft.VisualStudio.XmlEditor.XmlModel _xmlModel;
        private Dispatcher _dispatcher;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        internal VSXmlModel(IServiceProvider serviceProvider, Microsoft.VisualStudio.XmlEditor.XmlModel model)
        {
            _xmlModel = model;
            _xmlModel.BufferReloaded += OnBufferReload;

            _dispatcher = Dispatcher.CurrentDispatcher;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="disposing">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_xmlModel != null)
                    {
                        try
                        {
                            _xmlModel.Dispose();
                        }
                        finally
                        {
                            _xmlModel = null;
                        }
                    }
                }
                _dispatcher = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal Microsoft.VisualStudio.XmlEditor.XmlModel XmlModel
        {
            get { return _xmlModel; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override bool CanEditXmlModel()
        {
            string moniker = null;

            // first attempt to extract the file path of this XML Model
            try
            {
                if (Uri != null)
                {
                    moniker = Uri.LocalPath;
                }
            }
            catch (Exception e)
            {
                Debug.Fail("Could not parse the URI of this XML model because of the exception: " + e.Message);
                // note we get out of here quickly if this happens as we don't want to allow any cases where somehow documents.Count > 0 
                return false;
            }

            if (!String.IsNullOrWhiteSpace(moniker))
            {
                return VSHelpers.CheckOutFilesIfEditable(_serviceProvider, new[] { moniker });
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override XDocument Document
        {
            get { return _xmlModel.Document; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override string Name
        {
            get { return _xmlModel.Name; }
        }

        private void OnBufferReload(object sender, EventArgs e)
        {
            if (Dispatcher.CurrentDispatcher != _dispatcher)
            {
                // make sure the event is handled in the appropriate thread for the VsXmlModel
                _dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new EventHandler<EventArgs>(OnBufferReload), sender, e);
                return;
            }
            if (BufferReload != null)
            {
                BufferReload(this, e);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="xobject">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override XmlDesignerBaseTextSpan GetTextSpan(XObject xobject)
        {
            return ConvertFromVSTextSpan(_xmlModel.GetTextSpan(xobject));
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="textSpan">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public static VSTextSpan ConvertToVSTextSpan(XmlDesignerBaseTextSpan textSpan)
        {
            var ret = new VSTextSpan();
            ret.iStartIndex = textSpan.iStartIndex;
            ret.iStartLine = textSpan.iStartLine;
            ret.iEndIndex = textSpan.iEndIndex;
            ret.iEndLine = textSpan.iEndLine;

            return ret;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="textSpan">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public static XmlDesignerBaseTextSpan ConvertFromVSTextSpan(VSTextSpan textSpan)
        {
            var ret = new XmlDesignerBaseTextSpan();
            ret.iStartIndex = textSpan.iStartIndex;
            ret.iStartLine = textSpan.iStartLine;
            ret.iEndIndex = textSpan.iEndIndex;
            ret.iEndLine = textSpan.iEndLine;

            return ret;
        }
    }
}
