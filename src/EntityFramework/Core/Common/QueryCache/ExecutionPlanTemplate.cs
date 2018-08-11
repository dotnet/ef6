namespace System.Data.Entity.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// This class saves the execution plan template data.
    /// </summary>
    public class ExecutionPlanTemplate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExecutionPlanTemplate(string query, IList<string> resultColumns)
        {
            this.ExecutionPlanQuery = query;
            this.ResultColumns = resultColumns;
        }

        /// <summary>
        /// Execution plan string.
        /// </summary>
        public string ExecutionPlanQuery { get; private set; }

        /// <summary>
        /// Execution plan result columns.
        /// </summary>
        public IList<string> ResultColumns { get; private set; } 
    }
}
