// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    internal static class EFModelErrorTaskFactory
    {
        internal static ErrorTask CreateErrorTask(
            string document, string errorMessage, TextSpan textSpan, TaskErrorCategory taskErrorCategory, IVsHierarchy hierarchy,
            uint itemID)
        {
            return CreateErrorTask(document, errorMessage, textSpan, taskErrorCategory, hierarchy, itemID, MARKERTYPE.MARKER_COMPILE_ERROR);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsHierarchy.GetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider@)")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static ErrorTask CreateErrorTask(
            string document, string errorMessage, TextSpan textSpan, TaskErrorCategory taskErrorCategory, IVsHierarchy hierarchy,
            uint itemID, MARKERTYPE markerType)
        {
            ErrorTask errorTask = null;

            IOleServiceProvider oleSp = null;
            hierarchy.GetSite(out oleSp);
            IServiceProvider sp = new ServiceProvider(oleSp);

            // see if Document is open
            IVsTextLines buffer = null;
            var docData = VSHelpers.GetDocData(sp, document);
            if (docData != null)
            {
                buffer = VSHelpers.GetVsTextLinesFromDocData(docData);
            }

            if (buffer != null)
            {
                errorTask = new EFModelDocumentTask(sp, buffer, markerType, textSpan, document, itemID, errorMessage, hierarchy);
                errorTask.ErrorCategory = taskErrorCategory;
            }
            else
            {
                errorTask = new EFModelErrorTask(
                    document, errorMessage, textSpan.iStartLine, textSpan.iEndLine, taskErrorCategory, hierarchy, itemID);
            }

            return errorTask;
        }

        internal static ErrorTask CreateErrorTask(ErrorInfo errorInfo, IVsHierarchy hierarchy, uint itemID)
        {
            // errorInfo.Item can be null if the error came from CodeFirst
            return CreateErrorTask(
                errorInfo.ItemPath, 
                errorInfo.Message, 
                errorInfo.Item != null ? VSXmlModel.ConvertToVSTextSpan(errorInfo.Item.GetTextSpan()) : new TextSpan(),
                GetCategory(errorInfo), 
                hierarchy, 
                itemID);
        }

        internal static ErrorTask CreateErrorTask(string document, Exception e, IVsHierarchy hierarchy, uint itemID)
        {
            var textSpan = new TextSpan();

            var xmle = e as XmlException;
            if (xmle != null)
            {
                if (xmle.LineNumber >= 0)
                {
                    textSpan.iStartLine = xmle.LineNumber;
                    textSpan.iEndLine = xmle.LineNumber;
                }
                if (xmle.LinePosition >= 0)
                {
                    textSpan.iStartIndex = xmle.LinePosition;
                    textSpan.iEndIndex = xmle.LinePosition;
                }
            }

            return CreateErrorTask(document, e.Message, textSpan, TaskErrorCategory.Error, hierarchy, itemID);
        }

        private static TaskErrorCategory GetCategory(ErrorInfo errorInfo)
        {
            if (errorInfo.IsError())
            {
                return TaskErrorCategory.Error;
            }
            else if (errorInfo.IsInfo())
            {
                return TaskErrorCategory.Message;
            }
            else if (errorInfo.IsWarning())
            {
                return TaskErrorCategory.Warning;
            }
            else
            {
                return TaskErrorCategory.Error;
            }
        }
    }
}
