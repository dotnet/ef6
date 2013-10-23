// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public abstract class XmlModel : IDisposable
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        protected XmlModel()
        {
            IsDisposed = false;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="disposing">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <remarks>
        ///     The name of this xml model.  This is usually the
        ///     file name, but you should not perform your own file
        ///     operations on the resulting name.
        /// </remarks>
        public abstract string Name { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract XDocument Document { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public abstract bool CanEditXmlModel();

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public Uri Uri
        {
            get
            {
                return
                    new Uri(
                        Name.StartsWith("file://", StringComparison.OrdinalIgnoreCase)
                            ? Name.Replace("#", Uri.HexEscape('#'))
                            : Name);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="xobject">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "This is a desirable name")]
        public abstract TextSpan GetTextSpan(XObject xobject);

        /// <summary>
        ///     -1: (lin,col) is before TextSpan /
        ///     0: (lin,col) is within TextSpan /
        ///     1: (lin,col) is after TextSpan /
        /// </summary>
        private static int HitTest(TextSpan ts, int line, int col)
        {
            if (ts.iStartLine < line)
            {
                if (ts.iEndLine > line)
                {
                    return 0;
                }
                else
                {
                    if (ts.iEndLine == line)
                    {
                        if (ts.iEndIndex > col)
                        {
                            return 0;
                        }
                        else
                        {
                            // ts.iEndIndex <= col
                            return 1;
                        }
                    }
                    else
                    {
                        // ts.iEndLine < line
                        return 1;
                    }
                }
            }
            else if (ts.iStartLine == line)
            {
                if (ts.iEndLine == line)
                {
                    if (ts.iStartIndex <= col)
                    {
                        if (ts.iEndIndex > col)
                        {
                            return 0;
                        }
                        else
                        {
                            // ts.iEndIndex <= col
                            return 1;
                        }
                    }
                    else
                    {
                        // ts.iStartIndex > col
                        return -1;
                    }
                }
                else
                {
                    Debug.Assert(ts.iEndLine > line);
                    if (ts.iStartIndex <= col)
                    {
                        return 0;
                    }
                    else
                    {
                        // ts.iStartIndex > col
                        return -1;
                    }
                }
            }
            else
            {
                // ts.iStartLine > line
                return -1;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <remarks>
        ///     returns an XElement corresponding to the innermost
        ///     TextSpan that contains the specified (lin, col)
        /// </remarks>
        public XObject GetXObject(int line, int col)
        {
            XObject currentNode = Document;

            var lookDeeper = currentNode != null;

            while (lookDeeper)
            {
                lookDeeper = false;
                var container = currentNode as XContainer;
                if (container != null)
                {
                    // look for a child whose textSpan contains (lin, col)
                    var children = new List<XElement>(container.Elements());
                    var start = 0;
                    var end = children.Count - 1;
                    while (start <= end)
                    {
                        // since the child elements in the list are in document order,
                        // use binary search to minimize # of calls to GetTextSpan
                        var index = (start + end) / 2;
                        XObject child = children[index];

                        TextSpan textSpan;
                        textSpan = GetTextSpan(child);

                        if (textSpan.iStartLine == -1
                            ||
                            (textSpan.iStartLine == 0 &&
                             textSpan.iStartIndex == 0 &&
                             textSpan.iEndLine == 0 &&
                             textSpan.iEndIndex == 0))
                        {
                            // textSpan information is not valid
                            Debug.Fail("Empty span on node");
                            return null;
                        }
                        else
                        {
                            var hitTest = HitTest(textSpan, line, col);

                            if (hitTest == 0)
                            {
                                // (lin,col) is within the textSpan of current child 

                                // go deeper in the hierarchy to see if 
                                // we find a narrower match containing (lin,col)
                                currentNode = child;
                                lookDeeper = true;
                                break;
                            }
                            else if (hitTest < 0)
                            {
                                // (lin,col) is before textSpan of current child 
                                end = index - 1;
                            }
                            else
                            {
                                // (lin,col) is after textSpan of current child 
                                start = index + 1;
                            }
                        }
                    }
                }
            }

            return currentNode;
        }
    }
}
