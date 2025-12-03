using DevBlog.Server;
using Markdig;
using System.Collections.Specialized;

namespace DevBlog.RouteHandlers
{
    internal abstract class BaseRouteHandler(string route, MarkdownPipeline pipeline)
    {
        protected string route = route;
        protected readonly MarkdownPipeline pipeline = pipeline;

        internal string Route
        {
            get => route;
            private set => route = value;
        }

        internal abstract ResponseParams HandleResponse(NameValueCollection parameters);
    }
}
