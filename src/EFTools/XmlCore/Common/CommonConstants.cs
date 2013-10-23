// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Common
{
    /// <summary>
    ///     Constants for Common
    /// </summary>
    internal static class CommonConstants
    {
        #region VS Constants

        // See env\inc\OMGlyphs.h
        public const int OM_GLYPH_ACC_TYPE_COUNT = 6;
        public const int OM_GLYPH_ERROR = 31;
        public const int OM_GLYPH_CLASS = OM_GLYPH_ACC_TYPE_COUNT * 0;
        public const int OM_GLYPH_CSHARPFILE = OM_GLYPH_ACC_TYPE_COUNT * OM_GLYPH_ERROR + 18;
        public const int OM_GLYPH_REFERENCE = OM_GLYPH_ACC_TYPE_COUNT * OM_GLYPH_ERROR + 22;
        public const int OM_GLYPH_VBPROJECT = OM_GLYPH_ACC_TYPE_COUNT * OM_GLYPH_ERROR + 8;

        #endregion
    }
}
