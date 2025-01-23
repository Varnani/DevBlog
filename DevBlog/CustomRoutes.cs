using Markdig;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DevBlog
{
    internal class HomepageRouteHandler : BaseRouteHandler
    {
        public HomepageRouteHandler() : base("/") { }

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            ResponseParams response = new()
            {
                data = Encoding.UTF8.GetBytes("home page"),
                encoding = Encoding.UTF8
            };

            return response;
        }
    }

    internal class PostRouteHandler : BaseRouteHandler
    {
        private readonly MarkdownPipeline pipeline;

        public PostRouteHandler() : base("/post")
        {
            pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
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
                DirectoryInfo info = new(Path.Combine(Server.ROOT_PATH, "Posts/"));
                FileInfo[] files = info.GetFiles();

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

                string result;

                lock (pipeline)
                {
                    if (id == -1)
                    {
                        Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();

                        result = Markdown.ToHtml(markdown, pipeline: pipeline);

                        stopWatch.Stop();
                        TimeSpan elapsed = stopWatch.Elapsed;

                        result = result.Replace("%MD_RENDER_TIME%", elapsed.TotalSeconds.ToString());
                    }

                    else
                    {
                        result = Markdown.ToHtml(markdown, pipeline: pipeline);
                    }
                }

                html = html.Replace("%POST_CONTENT%", result);

                ResponseParams response = new()
                {
                    mime = Server.HTML_MIME,
                    data = Encoding.UTF8.GetBytes(html),
                    encoding = Encoding.UTF8
                };

                return response;
            }
        }
    }
}
