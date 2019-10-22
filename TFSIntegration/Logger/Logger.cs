namespace TFSIntegration
{
    public static class Logger
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public static log4net.ILog Log => log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
