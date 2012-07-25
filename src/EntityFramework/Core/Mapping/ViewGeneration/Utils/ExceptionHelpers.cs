// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.Utils
{
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

    // Miscellaneous helper routines for generating mapping exceptions
    internal static class ExceptionHelpers
    {
        internal static void ThrowMappingException(ErrorLog.Record errorRecord, ConfigViewGenerator config)
        {
            var exception = new InternalMappingException(errorRecord.ToUserString(), errorRecord);
            if (config.IsNormalTracing)
            {
                exception.ErrorLog.PrintTrace();
            }
            throw exception;
        }

        internal static void ThrowMappingException(ErrorLog errorLog, ConfigViewGenerator config)
        {
            var exception = new InternalMappingException(errorLog.ToUserString(), errorLog);
            if (config.IsNormalTracing)
            {
                exception.ErrorLog.PrintTrace();
            }
            throw exception;
        }
    }
}
