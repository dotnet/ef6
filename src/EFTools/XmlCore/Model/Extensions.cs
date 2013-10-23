// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class TextRange
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int OpenStartLine { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int OpenStartColumn { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int CloseEndLine { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int CloseEndColumn { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("OpenStartLine=");
            buffer.Append(OpenStartLine);
            buffer.Append(",OpenStartColumn=");
            buffer.Append(OpenStartColumn);
            buffer.Append(",CloseEndLine=");
            buffer.Append(CloseEndLine);
            buffer.Append(",CloseEndColumn=");
            buffer.Append(CloseEndColumn);
            return buffer.ToString();
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class ElementTextRange
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int OpenStartLine { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int OpenStartColumn { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int OpenEndLine { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int OpenEndColumn { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int CloseEndLine { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public int CloseEndColumn { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("OpenStartLine=");
            buffer.Append(OpenStartLine);
            buffer.Append(",OpenStartColumn=");
            buffer.Append(OpenStartColumn);
            buffer.Append(",OpenEndLine=");
            buffer.Append(OpenEndLine);
            buffer.Append(",OpenEndColumn=");
            buffer.Append(OpenEndColumn);
            buffer.Append(",CloseEndLine=");
            buffer.Append(CloseEndLine);
            buffer.Append(",CloseEndColumn=");
            buffer.Append(CloseEndColumn);
            return buffer.ToString();
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class XmlAttributeAnnotation
    {
        internal TextRange TextRange;
    }

    internal static class Extensions
    {
        public static TextRange GetTextRange(this XObject attribute)
        {
            var saa = attribute.Annotation<XmlAttributeAnnotation>();
            if (saa == null)
            {
                return null;
            }
            return saa.TextRange;
        }

        public static void SetTextRange(this XObject attribute, TextRange textRange)
        {
            var saa = attribute.Annotation<XmlAttributeAnnotation>();
            if (saa == null)
            {
                saa = new XmlAttributeAnnotation();
                attribute.AddAnnotation(saa);
            }
            saa.TextRange = textRange;
        }

        private class XmlFileAnnotation
        {
            internal ElementTextRange TextRange;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an extension method.")]
        public static ElementTextRange GetTextRange(this XElement element)
        {
            var sfa = element.Annotation<XmlFileAnnotation>();
            if (sfa == null)
            {
                return null;
            }
            return sfa.TextRange;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an extension method")]
        public static void SetTextRange(this XElement element, ElementTextRange textRange)
        {
            var sfa = element.Annotation<XmlFileAnnotation>();
            if (sfa == null)
            {
                sfa = new XmlFileAnnotation();
                element.AddAnnotation(sfa);
            }
            sfa.TextRange = textRange;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is an extension method")]
        public static void EnsureAnnotation(this XElement element)
        {
            var sfa = element.Annotation<XmlFileAnnotation>();
            if (sfa == null)
            {
                sfa = new XmlFileAnnotation();
                element.AddAnnotation(sfa);
            }
        }
    }
}
