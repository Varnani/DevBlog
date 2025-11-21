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
            Stopwatch stopWatch = Stopwatch.StartNew();

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

            PostData? postData = PostDatabase.GetPost(id);

            if (postData is null)
            {
                ResponseParams error = WebServer.GenerateErrorResponse(HttpStatusCode.NotFound, "Post not found.");
                return error;
            }

            string markdown = postData.Value.content;

            string content;
            lock (pipeline) content = Markdown.ToHtml(markdown, pipeline: pipeline);

            string html = RouteHelpers.GetPostTemplate();
            StringBuilder htmlBuilder = new(html);

            RouteHelpers.InsertPostContent(htmlBuilder, content);
            RouteHelpers.InsertCurrentYear(htmlBuilder);

            stopWatch.Stop();
            TimeSpan elapsed = stopWatch.Elapsed;

            RouteHelpers.InsertRenderTime(htmlBuilder, elapsed);

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
