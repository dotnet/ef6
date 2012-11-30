// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    internal class ErrorLog : InternalBase
    {
        internal ErrorLog()
        {
            m_log = new List<Record>();
        }

        private readonly List<Record> m_log;

        internal int Count
        {
            get { return m_log.Count; }
        }

        internal IEnumerable<EdmSchemaError> Errors
        {
            get
            {
                foreach (var record in m_log)
                {
                    yield return record.Error;
                }
            }
        }

        internal void AddEntry(Record record)
        {
            DebugCheck.NotNull(record);
            m_log.Add(record);
        }

        internal void Merge(ErrorLog log)
        {
            foreach (var record in log.m_log)
            {
                m_log.Add(record);
            }
        }

        internal void PrintTrace()
        {
            var builder = new StringBuilder();
            ToCompactString(builder);
            Helpers.StringTraceLine(builder.ToString());
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            foreach (var record in m_log)
            {
                record.ToCompactString(builder);
            }
        }

        internal string ToUserString()
        {
            var builder = new StringBuilder();
            foreach (var record in m_log)
            {
                var recordString = record.ToUserString();
                builder.AppendLine(recordString);
            }
            return builder.ToString();
        }

        internal class Record : InternalBase
        {
            // effects: Creates an error record for wrappers, a debug message
            // and an error message given by "message". Note: wrappers cannot
            // be null
            internal Record(
                ViewGenErrorCode errorCode, string message,
                IEnumerable<LeftCellWrapper> wrappers, string debugMessage)
            {
                DebugCheck.NotNull(wrappers);
                var cells = LeftCellWrapper.GetInputCellsForWrappers(wrappers);
                Init(errorCode, message, cells, debugMessage);
            }

            internal Record(ViewGenErrorCode errorCode, string message, Cell sourceCell, string debugMessage)
            {
                Init(errorCode, message, new[] { sourceCell }, debugMessage);
            }

            internal Record(
                ViewGenErrorCode errorCode, string message, IEnumerable<Cell> sourceCells,
                string debugMessage)
            {
                Init(errorCode, message, sourceCells, debugMessage);
            }

            //There are cases when we want to create a ViewGen error that is not specific to any mapping fragment
            //In this case, it is better to just create the EdmSchemaError directly and hold on to it.
            internal Record(EdmSchemaError error)
            {
                m_debugMessage = error.ToString();
                m_mappingError = error;
            }

            private void Init(
                ViewGenErrorCode errorCode, string message,
                IEnumerable<Cell> sourceCells, string debugMessage)
            {
                m_sourceCells = new List<Cell>(sourceCells);

                Debug.Assert(m_sourceCells.Count > 0, "Error record must have at least one cell");

                // For certain foreign key messages, we may need the SSDL line numbers and file names
                var label = m_sourceCells[0].CellLabel;
                var sourceLocation = label.SourceLocation;
                var lineNumber = label.StartLineNumber;
                var columnNumber = label.StartLinePosition;

                var userMessage = InternalToString(message, debugMessage, m_sourceCells, errorCode, false);
                m_debugMessage = InternalToString(message, debugMessage, m_sourceCells, errorCode, true);
                m_mappingError = new EdmSchemaError(
                    userMessage, (int)errorCode, EdmSchemaErrorSeverity.Error, sourceLocation,
                    lineNumber, columnNumber);
            }

            private EdmSchemaError m_mappingError;
            private List<Cell> m_sourceCells;
            private string m_debugMessage;

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            // referenced (indirectly) by System.Data.Entity.Design.dll
            internal EdmSchemaError Error
            {
                get { return m_mappingError; }
            }

            internal override void ToCompactString(StringBuilder builder)
            {
                builder.Append(m_debugMessage);
            }

            // effects: adds a comma-separated list of line numbers to the string builder
            private static void GetUserLinesFromCells(IEnumerable<Cell> sourceCells, StringBuilder lineBuilder, bool isInvariant)
            {
                var orderedCells = sourceCells.OrderBy(cell => cell.CellLabel.StartLineNumber, Comparer<int>.Default);

                var isFirst = true;
                // Get the line numbers
                foreach (var cell in orderedCells)
                {
                    if (isFirst == false)
                    {
                        lineBuilder.Append(isInvariant ? EntityRes.GetString(EntityRes.ViewGen_CommaBlank) : ", ");
                    }
                    isFirst = false;
                    lineBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", cell.CellLabel.StartLineNumber);
                }
                Debug.Assert(isFirst == false, "No cells");
            }

            // effects: Converts the message/debugMessage to a user-readable
            // message using resources (if isInvariant is false) or a test
            // message (if isInvariant is true)
            private static string InternalToString(
                string message, string debugMessage,
                List<Cell> sourceCells, ViewGenErrorCode errorCode, bool isInvariant)
            {
                var builder = new StringBuilder();

                if (isInvariant)
                {
                    builder.AppendLine(debugMessage);

                    builder.Append(isInvariant ? "ERROR" : Strings.ViewGen_Error);
                    StringUtil.FormatStringBuilder(builder, " ({0}): ", (int)errorCode);
                }

                var lineBuilder = new StringBuilder();
                GetUserLinesFromCells(sourceCells, lineBuilder, isInvariant);

                if (isInvariant)
                {
                    if (sourceCells.Count > 1)
                    {
                        StringUtil.FormatStringBuilder(
                            builder, "Problem in Mapping Fragments starting at lines {0}: ", lineBuilder.ToString());
                    }
                    else
                    {
                        StringUtil.FormatStringBuilder(
                            builder, "Problem in Mapping Fragment starting at line {0}: ", lineBuilder.ToString());
                    }
                }
                else
                {
                    if (sourceCells.Count > 1)
                    {
                        builder.Append(Strings.ViewGen_ErrorLog2(lineBuilder.ToString()));
                    }
                    else
                    {
                        builder.Append(Strings.ViewGen_ErrorLog(lineBuilder.ToString()));
                    }
                }
                builder.AppendLine(message);
                return builder.ToString();
            }

            internal string ToUserString()
            {
                return m_mappingError.ToString();
            }
        }
    }
}
