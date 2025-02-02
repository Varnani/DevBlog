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

        internal static string LoadTextFile(string path)
        {
            using StreamReader stream = File.OpenText(path);
            string content = stream.ReadToEnd();

            return content;
        }

        internal static string GetPostTemplate()
        {
            string htmlPath = Path.Combine(Server.WebServer.SPECIAL_PATH, "post_template.html");
            string html = LoadTextFile(htmlPath);

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
