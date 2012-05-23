namespace System.Data.Entity.Core.Mapping
{
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class LineInfo : IXmlLineInfo
    {
        private readonly bool m_hasLineInfo;
        private readonly int m_lineNumber;
        private readonly int m_linePosition;

        internal LineInfo(XPathNavigator nav)
            : this((IXmlLineInfo)nav)
        {
        }

        internal LineInfo(IXmlLineInfo lineInfo)
        {
            m_hasLineInfo = lineInfo.HasLineInfo();
            m_lineNumber = lineInfo.LineNumber;
            m_linePosition = lineInfo.LinePosition;
        }

        internal static readonly LineInfo Empty = new LineInfo();

        private LineInfo()
        {
            m_hasLineInfo = false;
            m_lineNumber = default(int);
            m_linePosition = default(int);
        }

        public int LineNumber
        {
            get { return m_lineNumber; }
        }

        public int LinePosition
        {
            get { return m_linePosition; }
        }

        public bool HasLineInfo()
        {
            return m_hasLineInfo;
        }
    }
}
