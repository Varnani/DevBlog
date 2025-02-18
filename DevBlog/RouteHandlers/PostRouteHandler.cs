using DevBlog.Helpers;
using DevBlog.Server;
using Markdig;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DevBlog.RouteHandlers
{
    internal class PostRouteHandler : BaseRouteHandler
    {
        private readonly MarkdownPipeline pipeline;

        internal PostRouteHandler(MarkdownPipeline pipeline) : base("/post")
        {
            this.pipeline = pipeline;
        }

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            string? value = parameters["id"];

            if (value == null)
            {
                ResponseParams error = WebServer.GenerateErrorResponse(HttpStatusCode.BadRequest, "Post id parameter is missing.");
                return error;
            }

            if (!int.TryParse(value, out int id))
            {
                ResponseParams error = WebServer.GenerateErrorResponse(HttpStatusCode.BadRequest, "Post id parameter is malformed.");
                return error;
            }

            if (id < -1)
            {
                ResponseParams error = WebServer.GenerateErrorResponse(HttpStatusCode.BadRequest, "Post id parameter is malformed.");
                return error;
            }

            string? mdPath = null;

            if (id == -1)
            {
                mdPath = Path.Combine(WebServer.SPECIAL_PATH, "test_markdown.md");
            }

            else
            {
                FileInfo[] files = RouteHelpers.GetPostFiles();

                if (id < files.Length)
                {
                    Array.Sort(files, (file1, file2) =>
                    {
                        return file1.CreationTime.CompareTo(file2.CreationTime);
                    });

                    mdPath = files[id].FullName;
                }
            }

            if (mdPath is null)
            {
                ResponseParams error = WebServer.GenerateErrorResponse(HttpStatusCode.NotFound, "Post not found.");
                return error;
            }

            else
            {
                string markdown = RouteHelpers.LoadTextFile(mdPath);

                string content;

                lock (pipeline)
                {
                    if (id == -1)
                    {
                        Stopwatch stopWatch = Stopwatch.StartNew();

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

                string html = RouteHelpers.GetPostTemplate();
                StringBuilder htmlBuilder = new(html);

                RouteHelpers.InsertPostContent(htmlBuilder, content);
                RouteHelpers.InsertCurrentYear(htmlBuilder);

                ResponseParams response = new()
                {
                    mime = WebServer.HTML_MIME,
                    data = Encoding.UTF8.GetBytes(htmlBuilder.ToString()),
                    encoding = Encoding.UTF8
                };

                return response;
            }
        }
    }
}
