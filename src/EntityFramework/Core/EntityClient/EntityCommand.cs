namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class representing a command for the conceptual layer
    /// </summary>
    public sealed class EntityCommand : DbCommand
    {
        private InternalEntityCommand _internalEntityCommand;

        /// <summary>
        /// Constructs the EntityCommand object not yet associated to a connection object
        /// </summary>
        public EntityCommand()
            : this(new InternalEntityCommand())
        {
        }

        /// <summary>
        /// Constructs the EntityCommand object with the given eSQL statement, but not yet associated to a connection object
        /// </summary>
        /// <param name="statement">The eSQL command text to execute</param>
        public EntityCommand(string statement)
            : this(new InternalEntityCommand(statement))
        {
        }

        /// <summary>
        /// Constructs the EntityCommand object with the given eSQL statement and the connection object to use
        /// </summary>
        /// <param name="statement">The eSQL command text to execute</param>
        /// <param name="connection">The connection object</param>
        public EntityCommand(string statement, EntityConnection connection)
            : this(new InternalEntityCommand(statement, connection))
        {
        }

        /// <summary>
        /// Constructs the EntityCommand object with the given eSQL statement and the connection object to use
        /// </summary>
        /// <param name="statement">The eSQL command text to execute</param>
        /// <param name="connection">The connection object</param>
        /// <param name="transaction">The transaction object this command executes in</param>
        public EntityCommand(string statement, EntityConnection connection, EntityTransaction transaction)
            : this(new InternalEntityCommand(statement, connection, transaction))
        {
        }

        /// <summary>
        /// Internal constructor used by EntityCommandDefinition
        /// </summary>
        /// <param name="commandDefinition">The prepared command definition that can be executed using this EntityCommand</param>
        internal EntityCommand(EntityCommandDefinition commandDefinition)
            : this(new InternalEntityCommand(commandDefinition))
        {
        }

        /// <summary>
        /// Constructs a new EntityCommand given a EntityConnection and an EntityCommandDefition. This 
        /// constructor is used by ObjectQueryExecution plan to execute an ObjectQuery.
        /// </summary>
        /// <param name="connection">The connection against which this EntityCommand should execute</param>
        /// <param name="commandDefinition">The prepared command definition that can be executed using this EntityCommand</param>
        internal EntityCommand(EntityConnection connection, EntityCommandDefinition entityCommandDefinition)
            : this(new InternalEntityCommand(connection, entityCommandDefinition))
        {
        }

        internal EntityCommand(InternalEntityCommand internalEntityCommand)
        {
            _internalEntityCommand = internalEntityCommand;
            _internalEntityCommand.EntityCommandWrapper = this;
        }

        /// <summary>
        /// The connection object used for executing the command
        /// </summary>
        public new EntityConnection Connection
        {
            get { return _internalEntityCommand.Connection; }
            set { _internalEntityCommand.Connection = value; }
        }

        /// <summary>
        /// The connection object used for executing the command
        /// </summary>
        protected override DbConnection DbConnection
        {
            get { return Connection; }
            set { Connection = (EntityConnection)value; }
        }

        /// <summary>
        /// The eSQL statement to execute, only one of the command tree or the command text can be set, not both
        /// </summary>
        public override string CommandText
        {
            get { return _internalEntityCommand.CommandText; }
            set { _internalEntityCommand.CommandText = value; }
        }

        /// <summary>
        /// The command tree to execute, only one of the command tree or the command text can be set, not both.
        /// </summary>
        public DbCommandTree CommandTree
        {
            get { return _internalEntityCommand.CommandTree; }
            set { _internalEntityCommand.CommandTree = value; }
        }

        /// <summary>
        /// Get or set the time in seconds to wait for the command to execute
        /// </summary>
        public override int CommandTimeout
        {
            get { return _internalEntityCommand.CommandTimeout; }
            set { _internalEntityCommand.CommandTimeout = value; }
        }

        /// <summary>
        /// The type of command being executed, only applicable when the command is using an eSQL statement and not the tree
        /// </summary>
        public override CommandType CommandType
        {
            get { return _internalEntityCommand.CommandType; }
            set { _internalEntityCommand.CommandType = value; }
        }

        /// <summary>
        /// The collection of parameters for this command
        /// </summary>
        public new EntityParameterCollection Parameters
        {
            get { return _internalEntityCommand.Parameters; }
        }

        /// <summary>
        /// The collection of parameters for this command
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get { return Parameters; }
        }

        /// <summary>
        /// The transaction object used for executing the command
        /// </summary>
        public new EntityTransaction Transaction
        {
            get { return _internalEntityCommand.Transaction; }
            set { _internalEntityCommand.Transaction = value; }
        }

        /// <summary>
        /// The transaction that this command executes in
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get { return Transaction; }
            set { Transaction = (EntityTransaction)value; }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the DataRow when used by the Update method of a DbDataAdapter
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get { return _internalEntityCommand.UpdatedRowSource; }
            set { _internalEntityCommand.UpdatedRowSource = value; }
        }

        /// <summary>
        /// Hidden property used by the designers
        /// </summary>
        public override bool DesignTimeVisible
        {
            get { return _internalEntityCommand.DesignTimeVisible; }
            set { _internalEntityCommand.DesignTimeVisible = value; }
        }

        /// <summary>
        /// Enables/Disables query plan caching for this EntityCommand
        /// </summary>
        public bool EnablePlanCaching
        {
            get { return _internalEntityCommand.EnablePlanCaching; }
            set { _internalEntityCommand.EnablePlanCaching = value; }
        }

        /// <summary>
        /// Cancel the execution of the command
        /// </summary>
        public override void Cancel()
        {
        }

        /// <summary>
        /// Create and return a new parameter object representing a parameter in the eSQL statement
        /// </summary>
        ///
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new EntityParameter CreateParameter()
        {
            return new EntityParameter();
        }

        /// <summary>
        /// Create and return a new parameter object representing a parameter in the eSQL statement
        /// </summary>
        protected override DbParameter CreateDbParameter()
        {
            return CreateParameter();
        }

        /// <summary>
        /// Executes the command and returns a data reader for reading the results
        /// </summary>
        /// <returns>An EntityDataReader object</returns>
        public new EntityDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Executes the command and returns a data reader for reading the results. May only
        /// be called on CommandType.CommandText (otherwise, use the standard Execute* methods)
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <returns>An EntityDataReader object</returns>
        /// <exception cref="InvalidOperationException">For stored procedure commands, if called
        /// for anything but an entity collection result</exception>
        public new EntityDataReader ExecuteReader(CommandBehavior behavior)
        {
            return _internalEntityCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// An asynchronous version of ExecuteReader, which
        /// executes the command and returns a data reader for reading the results. May only
        /// be called on CommandType.CommandText (otherwise, use the standard Execute* methods)
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <returns>A Task containing sn EntityDataReader object.</returns>
        /// <exception cref="InvalidOperationException">For stored procedure commands, if called
        /// for anything but an entity collection result</exception>
        public new Task<EntityDataReader> ExecuteReaderAsync()
        {
            return ExecuteReaderAsync(CommandBehavior.Default, CancellationToken.None);
        }

        /// <summary>
        /// An asynchronous version of ExecuteReader, which
        /// executes the command and returns a data reader for reading the results. May only
        /// be called on CommandType.CommandText (otherwise, use the standard Execute* methods)
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>A Task containing sn EntityDataReader object.</returns>
        /// <exception cref="InvalidOperationException">For stored procedure commands, if called
        /// for anything but an entity collection result</exception>
        public new Task<EntityDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
        {
            return ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
        }

        /// <summary>
        /// An asynchronous version of ExecuteReader, which
        /// executes the command and returns a data reader for reading the results. May only
        /// be called on CommandType.CommandText (otherwise, use the standard Execute* methods)
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <returns>A Task containing sn EntityDataReader object.</returns>
        /// <exception cref="InvalidOperationException">For stored procedure commands, if called
        /// for anything but an entity collection result</exception>
        public new Task<EntityDataReader> ExecuteReaderAsync(CommandBehavior behavior)
        {
            return ExecuteReaderAsync(behavior, CancellationToken.None);
        }

        /// <summary>
        /// An asynchronous version of ExecuteReader, which
        /// executes the command and returns a data reader for reading the results. May only
        /// be called on CommandType.CommandText (otherwise, use the standard Execute* methods)
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>A Task containing sn EntityDataReader object.</returns>
        /// <exception cref="InvalidOperationException">For stored procedure commands, if called
        /// for anything but an entity collection result</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken"),
        SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "behavior"),
        SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new Task<EntityDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the command and returns a data reader for reading the results
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <returns>A DbDataReader object</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        /// <summary>
        /// An asynchronous version of ExecuteDbDataReader, which
        /// executes the command and returns a data reader for reading the results
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return await ExecuteReaderAsync(behavior, cancellationToken);
        }

        /// <summary>
        /// Executes the command and discard any results returned from the command
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public override int ExecuteNonQuery()
        {
            return _internalEntityCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// An asynchronous version of ExecuteNonQuery, which
        /// executes the command and discard any results returned from the command
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the command and return the first column in the first row of the result, extra results are ignored
        /// </summary>
        /// <returns>The result in the first column in the first row</returns>
        public override object ExecuteScalar()
        {
            return _internalEntityCommand.ExecuteScalar();
        }

        /// <summary>
        /// Clear out any "compile" state
        /// </summary>
        internal void Unprepare()
        {
            _internalEntityCommand.Unprepare();
        }

        /// <summary>
        /// Creates a prepared version of this command
        /// </summary>
        public override void Prepare()
        {
            _internalEntityCommand.Prepare();
        }

        /// <summary>
        /// Get the command definition for the command; will construct one if there is not already
        /// one constructed, which means it will prepare the command on the client.
        /// </summary>
        /// <returns>the command definition</returns>
        internal EntityCommandDefinition GetCommandDefinition()
        {
            return _internalEntityCommand.GetCommandDefinition();
        }

        /// <summary>
        /// Returns the store command text.
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        public string ToTraceString()
        {
            return _internalEntityCommand.ToTraceString();
        }

        /// <summary>
        /// Returns a dictionary of parameter name and parameter typeusage in s-space from the entity parameter 
        /// collection given by the user.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, TypeUsage> GetParameterTypeUsage()
        {
            return _internalEntityCommand.GetParameterTypeUsage();
        }

        /// <summary>
        /// Call only when the reader associated with this command is closing. Copies parameter values where necessary.
        /// </summary>
        internal void NotifyDataReaderClosing()
        {
            _internalEntityCommand.NotifyDataReaderClosing();
        }

        /// <summary>
        /// Tells the EntityCommand about the underlying store provider command in case it needs to pull parameter values
        /// when the reader is closing.
        /// </summary>
        internal void SetStoreProviderCommand(DbCommand storeProviderCommand)
        {
            _internalEntityCommand.SetStoreProviderCommand(storeProviderCommand);
        }

        internal bool IsNotNullOnDataReaderClosingEvent()
        {
            return null != OnDataReaderClosing;
        }

        internal void InvokeOnDataReaderClosingEvent(EntityCommand sender, EventArgs e)
        {
            OnDataReaderClosing(sender, e);
        }

        /// <summary>
        /// Event raised when the reader is closing.
        /// </summary>
        internal event EventHandler OnDataReaderClosing;
    }
}
