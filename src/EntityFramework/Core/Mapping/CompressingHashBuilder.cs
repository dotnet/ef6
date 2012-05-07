namespace System.Data.Entity.Core.Mapping
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography;

    /// <summary>
    /// This class keeps recomputing the hash and adding it to the front of the 
    /// builder when the length of the string gets too long
    /// </summary>
    internal class CompressingHashBuilder : StringHashBuilder
    {
        // this max comes from the value that Md5Hasher uses for a buffer size when it is reading
        // from a stream
        private const int HashCharacterCompressionThreshold = 0x1000 / 2; // num bytes / 2 to convert to typical unicode char size
        private const int SpacesPerIndent = 4;

        private int _indent;

        // we are starting the buffer at 1.5 times the number of bytes
        // for the threshold
        internal CompressingHashBuilder(HashAlgorithm hashAlgorithm)
            : base(hashAlgorithm, (HashCharacterCompressionThreshold + (HashCharacterCompressionThreshold / 2)) * 2)
        {
        }

        internal override void Append(string content)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.Append(content);
            CompressHash();
        }

        internal override void AppendLine(string content)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.AppendLine(content);
            CompressHash();
        }

        /// <summary>
        /// add string like "typename Instance#1"
        /// </summary>
        /// <param name="objectIndex"></param>
        internal void AppendObjectStartDump(object o, int objectIndex)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.Append(o.GetType().ToString());
            base.Append(" Instance#");
            base.AppendLine(objectIndex.ToString(CultureInfo.InvariantCulture));
            CompressHash();

            _indent++;
        }

        internal void AppendObjectEndDump()
        {
            Debug.Assert(_indent > 0, "Indent and unindent should be paired");
            _indent--;
        }

        private void CompressHash()
        {
            if (base.CharCount >= HashCharacterCompressionThreshold)
            {
                var hash = ComputeHash();
                Clear();
                base.Append(hash);
            }
        }
    }
}