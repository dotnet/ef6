// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// this class collects several strings together, and allows you to (
    /// </summary>
    internal class StringHashBuilder
    {
        private readonly HashAlgorithm _hashAlgorithm;
        private const string NewLine = "\n";
        private readonly List<string> _strings = new List<string>();
        private int _totalLength;

        private byte[] _cachedBuffer;

        internal StringHashBuilder(HashAlgorithm hashAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
        }

        internal StringHashBuilder(HashAlgorithm hashAlgorithm, int startingBufferSize)
            : this(hashAlgorithm)
        {
            Debug.Assert(startingBufferSize > 0, "should be a non zero positive integer");
            _cachedBuffer = new byte[startingBufferSize];
        }

        internal int CharCount
        {
            get { return _totalLength; }
        }

        internal virtual void Append(string s)
        {
            InternalAppend(s);
        }

        internal virtual void AppendLine(string s)
        {
            InternalAppend(s);
            InternalAppend(NewLine);
        }

        private void InternalAppend(string s)
        {
            if (s.Length == 0)
            {
                return;
            }

            _strings.Add(s);
            _totalLength += s.Length;
        }

        internal string ComputeHash()
        {
            var byteCount = GetByteCount();
            if (_cachedBuffer == null)
            {
                // assume it is a one time use, and 
                // it will grow later if needed
                _cachedBuffer = new byte[byteCount];
            }
            else if (_cachedBuffer.Length < byteCount)
            {
                // grow it by what is needed at a minimum, or 1.5 times bigger
                // if that is bigger than what is needed this time.  We
                // make it 1.5 times bigger in hopes to reduce the number of allocations (consider the
                // case where the next one it 1 bigger)
                var bufferSize = Math.Max(_cachedBuffer.Length + (_cachedBuffer.Length / 2), byteCount);
                _cachedBuffer = new byte[bufferSize];
            }

            var start = 0;
            foreach (var s in _strings)
            {
                start += Encoding.Unicode.GetBytes(s, 0, s.Length, _cachedBuffer, start);
            }
            Debug.Assert(start == byteCount, "Did we use a different calculation for these?");

            var hash = _hashAlgorithm.ComputeHash(_cachedBuffer, 0, byteCount);
            return ConvertHashToString(hash);
        }

        internal void Clear()
        {
            _strings.Clear();
            _totalLength = 0;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            _strings.ForEach(s => builder.Append(s));
            return builder.ToString();
        }

        private int GetByteCount()
        {
            var count = 0;
            foreach (var s in _strings)
            {
                count += Encoding.Unicode.GetByteCount(s);
            }

            return count;
        }

        private static string ConvertHashToString(byte[] hash)
        {
            var stringData = new StringBuilder(hash.Length * 2);
            // Loop through each byte of the data and format each one as a 
            // hexadecimal string
            for (var i = 0; i < hash.Length; i++)
            {
                stringData.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return stringData.ToString();
        }

        public static string ComputeHash(HashAlgorithm hashAlgorithm, string source)
        {
            var builder = new StringHashBuilder(hashAlgorithm);
            builder.Append(source);
            return builder.ComputeHash();
        }
    }
}
