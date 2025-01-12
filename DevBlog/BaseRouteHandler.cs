using System.Collections.Specialized;

namespace DevBlog
{
    internal abstract class BaseRouteHandler
    {
        private string route = string.Empty;

        internal string Route
        {
            get => route;
            private set => route = value;
        }

        protected BaseRouteHandler(string route)
        {
            Route = route;
        }

        internal abstract ResponseParams HandleResponse(NameValueCollection parameters);
    }
}
