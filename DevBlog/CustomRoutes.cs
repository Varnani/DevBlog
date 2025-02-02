using Markdig;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DevBlog
{
    internal class HomepageRouteHandler : BaseRouteHandler
    {
        private readonly MarkdownPipeline pipeline;

        public HomepageRouteHandler(MarkdownPipeline pipeline) : base("/")
        {
            this.pipeline = pipeline;
        }

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            FileInfo[] files = RouteHelpers.GetPostFiles();

            StringBuilder postListBuilder = new();

            postListBuilder.AppendLine();
            postListBuilder.AppendLine();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                string title = file.Name;
                title = title.Replace('-', ' ');

                DateTime date = file.CreationTimeUtc;

                string entry = $"{date} - {title}";
                postListBuilder.AppendLine(entry);

                postListBuilder.AppendLine("  ");
            }

            string htmlPath = Path.Combine(Server.SPECIAL_PATH, "post_template.html");
            using StreamReader htmlStream = File.OpenText(htmlPath);
            string html = htmlStream.ReadToEnd();

            StringBuilder htmlBuilder = new(html);

            string content;

            lock (pipeline)
            {
                content = Markdown.ToHtml(postListBuilder.ToString(), pipeline);
            }

            RouteHelpers.InsertPostContent(htmlBuilder, content);
            RouteHelpers.InsertCurrentYear(htmlBuilder);

            ResponseParams response = new()
            {
                mime = Server.HTML_MIME,
                data = Encoding.UTF8.GetBytes(htmlBuilder.ToString()),
                encoding = Encoding.UTF8
            };

            return response;
        }
    }

    internal class PostRouteHandler : BaseRouteHandler
    {
        private readonly MarkdownPipeline pipeline;

        public PostRouteHandler(MarkdownPipeline pipeline) : base("/post")
        {
            this.pipeline = pipeline;
        }

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            string? value = parameters["id"];

            if (value == null)
            {
                ResponseParams error = Server.GenerateErrorResponse(HttpStatusCode.BadRequest, "Post id parameter is missing.");
                return error;
            }

            if (!int.TryParse(value, out int id))
            {
                ResponseParams error = Server.GenerateErrorResponse(HttpStatusCode.BadRequest, "Post id parameter is malformed.");
                return error;
            }

            if (id < -1)
            {
                ResponseParams error = Server.GenerateErrorResponse(HttpStatusCode.BadRequest, "Post id parameter is malformed.");
                return error;
            }

            string? mdPath = null;

            if (id == -1)
            {
                mdPath = Path.Combine(Server.SPECIAL_PATH, "test_markdown.md");
            }

            else
            {
                FileInfo[] files = RouteHelpers.GetPostFiles();

                if (id < files.Length)
                {
                    Array.Sort(files, (FileInfo file1, FileInfo file2) =>
                    {
                        return file1.CreationTime.CompareTo(file2.CreationTime);
                    });

                    mdPath = files[id].FullName;
                }
            }

            if (mdPath is null)
            {
                ResponseParams error = Server.GenerateErrorResponse(HttpStatusCode.NotFound, "Post not found.");
                return error;
            }

            else
            {
                string htmlPath = Path.Combine(Server.SPECIAL_PATH, "post_template.html");

                using StreamReader htmlStream = File.OpenText(htmlPath);
                string html = htmlStream.ReadToEnd();

                using StreamReader mdStream = File.OpenText(mdPath);
                string markdown = mdStream.ReadToEnd();

                string content;

                lock (pipeline)
                {
                    if (id == -1)
                    {
                        Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();

                        content = Markdown.ToHtml(markdown, pipeline: pipeline);

                        stopWatch.Stop();
                        TimeSpan elapsed = stopWatch.Elapsed;

                        content = content.Replace("%MD_RENDER_TIME%", elapsed.TotalSeconds.ToString());
                    }

                    else
                    {
                        content = Markdown.ToHtml(markdown, pipeline: pipeline);
                    }
                }

                StringBuilder htmlBuilder = new(html);
                RouteHelpers.InsertPostContent(htmlBuilder, content);
                RouteHelpers.InsertCurrentYear(htmlBuilder);

                ResponseParams response = new()
                {
                    mime = Server.HTML_MIME,
                    data = Encoding.UTF8.GetBytes(htmlBuilder.ToString()),
                    encoding = Encoding.UTF8
                };

                return response;
            }
        }
    }

    internal static class RouteHelpers
    {
        internal static FileInfo[] GetPostFiles()
        {
            DirectoryInfo info = new(Path.Combine(Server.ROOT_PATH, "Posts/"));
            FileInfo[] files = info.GetFiles();

            return files;
        }

        internal static string GetPostTemplate()
        {
            string htmlPath = Path.Combine(Server.SPECIAL_PATH, "post_template.html");
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
