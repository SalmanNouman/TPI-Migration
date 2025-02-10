namespace VARLab.DLX
{
    /// <summary>
    ///     Represents a log entry for recording player activity with primary and secondary log classification.
    /// </summary>
    public class Log
    {
        #region Properties

        /// <summary>
        ///     Indicates whether this is a primary (true) or secondary (false) log entry.
        /// </summary>
        public bool IsPrimary { get; private set; }

        /// <summary>
        ///     The recorded log message.
        /// </summary>
        public string Message { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="isPrimary">Whether this is a primary (true) or secondary (false) log.</param>
        /// <param name="message">The message to record.</param>
        public Log(bool isPrimary, string message)
        {
            IsPrimary = isPrimary;
            Message = message;
        }

        #endregion
    }
}