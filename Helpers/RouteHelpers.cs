using System.Text;

namespace DevBlog.Helpers
{
    internal static class RouteHelpers
    {
        internal static string LoadTextFile(string path)
        {
            try
            {
                using StreamReader stream = File.OpenText(path);
                string content = stream.ReadToEnd();
                return content;
            }
            catch (Exception ex)
            {
                Logger.Log($"An exception occured while loading file at path {path}.", Logger.Level.Error);
                Logger.Log(ex.Message, Logger.Level.Error);
                return "n/a";
            }
        }

        internal static string GetPostTemplate()
        {
            string path = Path.Combine(Server.WebServer.SPECIAL_PATH, "post_template.html");
            return LoadTextFile(path);
        }

        internal static string GetHomeTemplate()
        {
            string path = Path.Combine(Server.WebServer.SPECIAL_PATH, "home_template.html");
            return LoadTextFile(path);
        }

        internal static string GetHomeContent()
        {
            string path = Path.Combine(Server.WebServer.SPECIAL_PATH, "home_content.md");
            return LoadTextFile(path);
        }

        internal static void InsertCurrentYear(StringBuilder sb)
        {
            sb.Replace("%CURRENT_YEAR%", DateTime.Now.Year.ToString());
        }

        internal static void InsertRenderTime(StringBuilder sb, TimeSpan elapsed)
        {
            sb.Replace("%RENDER_TIME%", $"{elapsed.TotalMilliseconds}ms");
        }
    }
}
