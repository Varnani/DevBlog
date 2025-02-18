namespace DevBlog.Helpers
{
    internal static class Logger
    {
        public enum Level
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }

        public static Level LogLevel = Level.Info;

        public static void Log(string message, Level level)
        {
            if (level < LogLevel) return;

            string prepend = level.ToString().ToUpper();
            string msg = $"{DateTime.Now} - {GetPrepend(level)}: {message}";

            if (level == Level.Error || level == Level.Critical)
            {
                Console.Error.WriteLine(msg);
            }

            else
            {
                Console.WriteLine(msg);
            }
        }

        private static string GetPrepend(Level level)
        {
            return level switch
            {
                Level.Debug => "DBG",
                Level.Info => "INF",
                Level.Warning => "WRN",
                Level.Error => "ERR",
                Level.Critical => "CRI",
                _ => string.Empty,
            };
        }
    }
}
