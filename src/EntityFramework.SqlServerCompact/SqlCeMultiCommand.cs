// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServerCompact
{
    using System.Data.Common;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Data.SqlServerCe;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    internal class SqlCeMultiCommand : DbCommand, ICloneable
    {
        private String[] commandTexts; // commandTexts for the individual commands
        private readonly SqlCeCommand command; // command to execute the individual commands

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DesignTimeVisible")]
        public override string CommandText
        {
            get { return String.Join("; ", commandTexts); }
            set
            {
                var exceptionMessage = String.Format(CultureInfo.InvariantCulture, "DesignTimeVisible");
                throw new NotSupportedException(exceptionMessage);
            }
        }

        public override int CommandTimeout
        {
            get { return 0; }

            set
            {
                if (0 != value)
                {
                    throw new ArgumentException(EntityRes.GetString(EntityRes.ADP_InvalidCommandTimeOut));
                }
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Text; }

            set
            {
                if (CommandType.Text != value)
                {
                    throw new ArgumentException(
                        EntityRes.GetString(
                            EntityRes.ADP_InvalidCommandType,
                            ((int)value).ToString(CultureInfo.CurrentCulture)));
                }
            }
        }

        protected override DbConnection DbConnection
        {
            get { return Connection; }
            set { Connection = (SqlCeConnection)value; }
        }

        public new SqlCeConnection Connection
        {
            get { return command.Connection; }
            set { command.Connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return Parameters; }
        }

        public new SqlCeParameterCollection Parameters
        {
            get { return command.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return Transaction; }
            set { Transaction = (SqlCeTransaction)value; }
        }

        public new SqlCeTransaction Transaction
        {
            get { return command.Transaction; }
            set { command.Transaction = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DesignTimeVisible")]
        public override bool DesignTimeVisible
        {
            get { return false; }
            set
            {
                var exceptionMessage = String.Format(CultureInfo.InvariantCulture, "DesignTimeVisible");
                throw new NotSupportedException(exceptionMessage);
            }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return command.UpdatedRowSource; }
            set { command.UpdatedRowSource = value; }
        }

        public String[] CommandTexts
        {
            get { return commandTexts; }
            set
            {
                Debug.Assert(value == null || value.Length <= 2, "Atmost 2 queries are expected at any point of time!!!");
                commandTexts = value ?? new string[1];
            }
        }

        public SqlCeMultiCommand()
        {
            command = new SqlCeCommand();
        }

        public override void Cancel()
        {
            var exceptionMessage = String.Format(CultureInfo.InvariantCulture, "Cancel");
            throw new NotSupportedException(exceptionMessage);
        }

        protected override DbParameter CreateDbParameter()
        {
            return CreateParameter();
        }

        public new static SqlCeParameter CreateParameter()
        {
            return new SqlCeParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        public new SqlCeDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        // Executes the specified commandTexts in the default transaction
        // of command object, and returns any exception to the caller.
        // Assumption: Atmost 2 commandTexts are supported as of now.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public new SqlCeDataReader ExecuteReader(CommandBehavior behavior)
        {
            var index = 0;
            var cAffectedRecords = 0;
            Debug.Assert(CommandTexts.Length == 1 || CommandTexts.Length == 2);
            if (commandTexts.Length > 1)
            {
                command.CommandText = commandTexts[index++];
                cAffectedRecords = command.ExecuteNonQuery();

                // If first command doesn't affect any records, then second query should not return any rows
                //
                if (cAffectedRecords == 0)
                {
                    command.CommandText = "select * from (" + CommandTexts[index] + ") as temp where 1=2;";
                }
                else
                {
                    command.CommandText = commandTexts[index];
                }
            }
            else
            {
                command.CommandText = commandTexts[index];
            }
            try
            {
                return command.ExecuteReader(behavior);
            }
            catch (SqlCeException e)
            {
                // index == 1, means there are multiple commands in this SqlCeMultiCommand. Which indicates Server generated keys scenario.
                // This check will only work under the assumptions that:
                //      1. SqlCeMultiCommand is used only for Server generated keys scenarios.
                //      2. Server generated keys query generate exactly 2 commands.
                //
                if (index == 1)
                {
                    // Wrap in inner exception and let  user know that DML has succeeded.
                    throw new SystemException(EntityRes.GetString(EntityRes.ADP_CanNotRetrieveServerGeneratedKey), e);
                }
                else
                {
                    throw;
                }
            }
        }

        public SqlCeResultSet ExecuteResultSet(ResultSetOptions options)
        {
            return ExecuteResultSet(options, null /* resultSetType */);
        }

        // Executes the specified commandTexts in the default transaction
        // of command object, and returns any exception to the caller.
        // Assumption: Atmost 2 commandTexts are supported as of now.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public SqlCeResultSet ExecuteResultSet(ResultSetOptions options, SqlCeResultSet resultSet)
        {
            var index = 0;
            var cAffectedRecords = 0;
            Debug.Assert(CommandTexts.Length == 1 || CommandTexts.Length == 2);
            if (commandTexts.Length > 1)
            {
                command.CommandText = commandTexts[index++];
                cAffectedRecords = command.ExecuteNonQuery();

                // If first command doesn't affect any records, then second query should not return any rows
                //
                if (cAffectedRecords == 0)
                {
                    command.CommandText = "select * from (" + CommandTexts[index] + ") as temp where 1=2;";
                }
                else
                {
                    command.CommandText = commandTexts[index];
                }
            }
            else
            {
                command.CommandText = commandTexts[index];
            }
            try
            {
                return command.ExecuteResultSet(options, resultSet);
            }
            catch (SqlCeException e)
            {
                // index == 1, means there are multiple commands in this SqlCeMultiCommand. Which indicates Server generated keys scenario.
                // This check will only work under the assumptions that:
                //      1. SqlCeMultiCommand is used only for Server generated keys scenarios.
                //      2. Server generated keys query generate exactly 2 commands.
                //
                if (index == 1)
                {
                    // Wrap in inner exception and let  user know that DML has succeeded.
                    throw new SystemException(EntityRes.GetString(EntityRes.ADP_CanNotRetrieveServerGeneratedKey), e);
                }
                else
                {
                    throw;
                }
            }
        }

        // Executes the specified commandTexts in the default transaction
        // of command object, and returns any exception to the caller.
        // Assumption: Atmost 2 commandTexts are supported as of now.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override int ExecuteNonQuery()
        {
            var index = 0;
            var cAffectedRecords = 0;
            Debug.Assert(CommandTexts.Length == 1 || CommandTexts.Length == 2);
            if (commandTexts.Length > 1)
            {
                command.CommandText = commandTexts[index++];
                cAffectedRecords = command.ExecuteNonQuery();

                // If first command doesn't affect any records, then second query should not return any rows
                //
                if (cAffectedRecords == 0)
                {
                    command.CommandText = "select * from (" + CommandTexts[index] + ") as temp where 1=2;";
                }
                else
                {
                    command.CommandText = commandTexts[index];
                }
            }
            else
            {
                command.CommandText = commandTexts[index];
            }
            try
            {
                return command.ExecuteNonQuery();
            }
            catch (SqlCeException e)
            {
                // index == 1, means there are multiple commands in this SqlCeMultiCommand. Which indicates Server generated keys scenario.
                // This check will only work under the assumptions that:
                //      1. SqlCeMultiCommand is used only for Server generated keys scenarios.
                //      2. Server generated keys query generate exactly 2 commands.
                //
                if (index == 1)
                {
                    // Wrap in inner exception and let  user know that DML has succeeded.
                    throw new SystemException(EntityRes.GetString(EntityRes.ADP_CanNotRetrieveServerGeneratedKey), e);
                }
                else
                {
                    throw;
                }
            }
        }

        // Executes the specified commandTexts in the default transaction
        // of command object, and returns any exception to the caller.
        // Assumption: Atmost 2 commandTexts are supported as of now.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override object ExecuteScalar()
        {
            var index = 0;
            var cAffectedRecords = 0;
            Debug.Assert(CommandTexts.Length == 1 || CommandTexts.Length == 2);
            if (commandTexts.Length > 1)
            {
                command.CommandText = commandTexts[index++];
                cAffectedRecords = command.ExecuteNonQuery();

                // If first command doesn't affect any records, then second query should not return any rows
                //
                if (cAffectedRecords == 0)
                {
                    command.CommandText = "select * from (" + CommandTexts[index] + ") as temp where 1=2;";
                }
                else
                {
                    command.CommandText = commandTexts[index];
                }
            }
            else
            {
                command.CommandText = commandTexts[index];
            }
            try
            {
                return command.ExecuteScalar();
            }
            catch (SqlCeException e)
            {
                // index == 1, means there are multiple commands in this SqlCeMultiCommand. Which indicates Server generated keys scenario.
                // This check will only work under the assumptions that:
                //      1. SqlCeMultiCommand is used only for Server generated keys scenarios.
                //      2. Server generated keys query generate exactly 2 commands.
                //
                if (index == 1)
                {
                    // Wrap in inner exception and let  user know that DML has succeeded.
                    throw new ApplicationException(EntityRes.GetString(EntityRes.ADP_CanNotRetrieveServerGeneratedKey), e);
                }
                else
                {
                    throw;
                }
            }
        }

        public override void Prepare()
        {
            throw new NotSupportedException();
        }

        Object ICloneable.Clone()
        {
            // Create a new instance of this type
            //
            var clone = (SqlCeMultiCommand)Activator.CreateInstance(
                GetType());

            // Copy all its internal properties
            //
            clone.CommandTexts = CommandTexts;
            clone.Connection = Connection;
            clone.CommandTimeout = CommandTimeout;
            clone.CommandType = CommandType;
            clone.Transaction = Transaction;

            // Copy all command parameters
            //
            if (0 < Parameters.Count)
            {
                var parameters = clone.Parameters;
                foreach (ICloneable parameter in Parameters)
                {
                    parameters.Add(parameter.Clone());
                }
            }

            // Done.
            //
            return clone;
        }
    }
}
