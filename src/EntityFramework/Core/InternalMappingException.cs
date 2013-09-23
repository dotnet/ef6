// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    // <summary>
    // Mapping exception class. Note that this class has state - so if you change even
    // its internals, it can be a breaking change
    // </summary>
    [Serializable]
    internal class InternalMappingException : EntityException
    {
        // effects: constructor with default message

        #region Constructors

        // <summary>
        // default constructor
        // </summary>
        internal InternalMappingException() // required ctor
        {
        }

        // <summary>
        // default constructor
        // </summary>
        // <param name="message"> localized error message </param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // required CTOR for exceptions.
        internal InternalMappingException(string message) // required ctor
            : base(message)
        {
        }

        // <summary>
        // constructor
        // </summary>
        // <param name="message"> localized error message </param>
        // <param name="innerException"> inner exception </param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // required CTOR for exceptions.
        internal InternalMappingException(string message, Exception innerException) // required ctor
            : base(message, innerException)
        {
        }

        // <summary>
        // constructor
        // </summary>
        protected InternalMappingException(SerializationInfo info, StreamingContext context)
            :
                base(info, context)
        {
        }

        // effects: constructor that allows a log
        internal InternalMappingException(string message, ErrorLog errorLog)
            : base(message)
        {
            DebugCheck.NotNull(errorLog);

            m_errorLog = errorLog;
        }

        // effects:  constructor that allows single mapping error
        internal InternalMappingException(string message, ErrorLog.Record record)
            : base(message)
        {
            DebugCheck.NotNull(record);

            m_errorLog = new ErrorLog();
            m_errorLog.AddEntry(record);
        }

        #endregion

        #region Fields

        // Keep track of mapping errors that we want to give to the
        // user in one shot
        private readonly ErrorLog m_errorLog;

        #endregion

        #region Properties

        // <summary>
        // Returns the inner exceptions stored in this
        // </summary>
        internal ErrorLog ErrorLog
        {
            get { return m_errorLog; }
        }

        #endregion
    }
}
