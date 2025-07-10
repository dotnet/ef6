using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using static System.String;
using System.Text.RegularExpressions;
using Xugu.Data.EntityFramework;

namespace XuguClient
{
    /// <summary>
    /// Provides a class capable of executing a SQL script containing
    /// multiple SQL statements including CREATE PROCEDURE statements
    /// that require changing the delimiter
    /// </summary>
    public class XGScript
    {
        /// <summary>
        /// Handles the event raised whenever a statement is executed.
        /// </summary>
        public event MySqlStatementExecutedEventHandler StatementExecuted;

        /// <summary>
        /// Handles the event raised whenever an error is raised by the execution of a script.
        /// </summary>
        public event MySqlScriptErrorEventHandler Error;

        /// <summary>
        /// Handles the event raised whenever a script execution is finished.
        /// </summary>
        public event EventHandler ScriptCompleted;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="XGScript"/> class.
        /// </summary>
        public XGScript()
        {
            Delimiter = ";";
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="XGScript"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public XGScript(XGConnection connection)
          : this()
        {
            Connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="XGScript"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public XGScript(string query)
          : this()
        {
            Query = query;
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="XGScript"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="query">The query.</param>
        public XGScript(XGConnection connection, string query)
          : this()
        {
            Connection = connection;
            Query = query;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>The connection.</value>
        public XGConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>The query.</value>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the delimiter.
        /// </summary>
        /// <value>The delimiter.</value>
        public string Delimiter { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>The number of statements executed as part of the script.</returns>
        public int Execute()
        {
            bool openedConnection = false;

            if (Connection == null)
                throw new InvalidOperationException(Resources.ConnectionNotSet);
            if (IsNullOrEmpty(Query))
                return 0;

            // next we open up the connetion if it is not already open
            if (Connection.State != ConnectionState.Open)
            {
                openedConnection = true;
                Connection.Open();
            }

            try
            {
                // first we break the query up into smaller queries
                List<ScriptStatement> statements = BreakIntoStatements();

                int count = 0;
                XGCommand cmd = new XGCommand(null, Connection);
                foreach (ScriptStatement statement in statements.Where(statement => !IsNullOrEmpty(statement.text)))
                {
                    cmd.CommandText = statement.text;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        count++;
                        OnQueryExecuted(statement);
                    }
                    catch (Exception ex)
                    {
                        if (Error == null)
                            throw;
                        if (!OnScriptError(ex))
                            break;
                    }
                }
                OnScriptCompleted();
                return count;
            }
            finally
            {
                if (openedConnection)
                {
                    Connection.Close();
                }
            }
        }

        #endregion

        private void OnQueryExecuted(ScriptStatement statement)
        {
            if (StatementExecuted == null) return;

            MySqlScriptEventArgs args = new MySqlScriptEventArgs { Statement = statement };
            StatementExecuted(this, args);
        }

        private void OnScriptCompleted()
        {
            ScriptCompleted?.Invoke(this, EventArgs.Empty);
        }

        private bool OnScriptError(Exception ex)
        {
            if (Error == null) return false;

            MySqlScriptErrorEventArgs args = new MySqlScriptErrorEventArgs(ex);
            Error(this, args);
            return args.Ignore;
        }

        private List<int> BreakScriptIntoLines()
        {
            List<int> lineNumbers = new List<int>();

            StringReader sr = new StringReader(Query);
            string line = sr.ReadLine();
            int pos = 0;
            while (line != null)
            {
                lineNumbers.Add(pos);
                pos += line.Length;
                line = sr.ReadLine();
            }
            return lineNumbers;
        }

        private static int FindLineNumber(int position, List<int> lineNumbers)
        {
            int i = 0;
            while (i < lineNumbers.Count && position < lineNumbers[i])
                i++;
            return i;
        }

        private List<ScriptStatement> BreakIntoStatements()
        {
            string currentDelimiter = "GO";
            int startPos = 0;
            List<ScriptStatement> statements = new List<ScriptStatement>();
            List<int> lineNumbers = BreakScriptIntoLines();
            XGTokenizer tokenizer = new XGTokenizer(Query);

            tokenizer.AnsiQuotes = true;
            //tokenizer.BackslashEscapes = !noBackslashEscapes;

            string token = tokenizer.NextToken();
            while (token != null)
            {
                if (!tokenizer.Quoted)
                {
                    if (token.ToLower(CultureInfo.InvariantCulture) == "delimiter")
                    {
                        tokenizer.NextToken();
                        AdjustDelimiterEnd(tokenizer);
                        currentDelimiter = Query.Substring(tokenizer.StartIndex,
                          tokenizer.StopIndex - tokenizer.StartIndex).Trim();
                        startPos = tokenizer.StopIndex;
                    }
                    else
                    {
                        // this handles the case where our tokenizer reads part of the
                        // delimiter

                        string pattern = $@"\b{currentDelimiter}\b";
                        Match match = Regex.Match(token, pattern, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            if ((tokenizer.StartIndex + currentDelimiter.Length) <= Query.Length)
                            {
                                if (Query.Substring(tokenizer.StartIndex, currentDelimiter.Length) == currentDelimiter)
                                {
                                    token = currentDelimiter;
                                    tokenizer.Position = tokenizer.StartIndex + currentDelimiter.Length;
                                    tokenizer.StopIndex = tokenizer.Position;
                                }
                            }
                        }

                        int delimiterPos = match.Success ? match.Index : -1;
                        if (delimiterPos != -1)
                        {
                            int endPos = tokenizer.StopIndex - token.Length + delimiterPos;
                            if (tokenizer.StopIndex == Query.Length - 1)
                                endPos++;
                            string currentQuery = Query.Substring(startPos, endPos - startPos);
                            ScriptStatement statement = new ScriptStatement();
                            statement.text = currentQuery.Trim();
                            statement.line = FindLineNumber(startPos, lineNumbers);
                            statement.position = startPos - lineNumbers[statement.line];
                            statements.Add(statement);
                            startPos = endPos + currentDelimiter.Length;
                        }
                    }
                }
                token = tokenizer.NextToken();
            }

            // now clean up the last statement
            if (startPos < Query.Length - 1)
            {
                string sqlLeftOver = Query.Substring(startPos).Trim();
                if (IsNullOrEmpty(sqlLeftOver)) return statements;
                ScriptStatement statement = new ScriptStatement
                {
                    text = sqlLeftOver,
                    line = FindLineNumber(startPos, lineNumbers)
                };
                statement.position = startPos - lineNumbers[statement.line];
                statements.Add(statement);
            }
            return statements;
        }

        private void AdjustDelimiterEnd(XGTokenizer tokenizer)
        {
            if (tokenizer.StopIndex >= Query.Length) return;

            int pos = tokenizer.StopIndex;
            char c = Query[pos];

            while (!Char.IsWhiteSpace(c) && pos < (Query.Length - 1))
            {
                c = Query[++pos];
            }
            tokenizer.StopIndex = pos;
            tokenizer.Position = pos;
        }

        #region Async
        /// <summary>
        /// Initiates the asynchronous execution of SQL statements.
        /// </summary>
        /// <returns>The number of statements executed as part of the script inside.</returns>
        public Task<int> ExecuteAsync()
        {
            return ExecuteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Initiates the asynchronous execution of SQL statements.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of statements executed as part of the script inside.</returns>
        public Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            var result = new TaskCompletionSource<int>();
            if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var executeResult = Execute();
                    result.SetResult(executeResult);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            }
            else
            {
                result.SetCanceled();
            }
            return result.Task;
        }
        #endregion
    }

    /// <summary>
    /// Represents the method that will handle errors when executing MySQL statements.
    /// </summary>
    public delegate void MySqlStatementExecutedEventHandler(object sender, MySqlScriptEventArgs args);
    /// <summary>
    /// Represents the method that will handle errors when executing MySQL scripts.
    /// </summary>
    public delegate void MySqlScriptErrorEventHandler(object sender, MySqlScriptErrorEventArgs args);

    /// <summary>
    /// Sets the arguments associated to MySQL scripts.
    /// </summary>
    public class MySqlScriptEventArgs : EventArgs
    {
        internal ScriptStatement Statement { get; set; }

        /// <summary>
        /// Gets the statement text.
        /// </summary>
        /// <value>The statement text.</value>
        public string StatementText => Statement.text;

        /// <summary>
        /// Gets the line.
        /// </summary>
        /// <value>The line.</value>
        public int Line => Statement.line;

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position => Statement.position;
    }

    /// <summary>
    /// Sets the arguments associated to MySQL script errors.
    /// </summary>
    public class MySqlScriptErrorEventArgs : MySqlScriptEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlScriptErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public MySqlScriptErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MySqlScriptErrorEventArgs"/> is ignored.
        /// </summary>
        /// <value><c>true</c> if ignore; otherwise, <c>false</c>.</value>
        public bool Ignore { get; set; }
    }

    struct ScriptStatement
    {
        public string text;
        public int line;
        public int position;
    }
}
