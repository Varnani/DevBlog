using System.Text;

namespace DevBlog.Helpers
{
    internal static class RouteHelpers
    {
        internal static FileInfo[] GetPostFiles()
        {
            DirectoryInfo info = new(Path.Combine(Server.WebServer.ROOT_PATH, "Posts/"));
            FileInfo[] files = info.GetFiles();

            return files;
        }

        internal static string GetPostTemplate()
        {
            string htmlPath = Path.Combine(Server.WebServer.SPECIAL_PATH, "post_template.html");
            using StreamReader htmlStream = File.OpenText(htmlPath);
            string html = htmlStream.ReadToEnd();

            return html;
        }

        internal static void InsertPostContent(StringBuilder sb, string result)
        {
            sb.Replace("%POST_CONTENT%", result);
        }

        internal static void InsertCurrentYear(StringBuilder sb)
        {
            sb.Replace("%CURRENT_YEAR%", DateTime.Now.Year.ToString());
        }
    }
}
