using Markdig;
using System.Collections.Specialized;
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

            string result = Markdown.ToHtml($"If this is in *italics*, that means markdig is working. Yay! Also, the post id is {id}.",
                pipeline: pipeline);

            ResponseParams response = new()
            {
                mime = Server.HTML_MIME,
                data = Encoding.UTF8.GetBytes(result),
                encoding = Encoding.UTF8
            };

            return response;
        }
    }
}
