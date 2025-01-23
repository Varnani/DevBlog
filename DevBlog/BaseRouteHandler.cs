using System.Collections.Specialized;

namespace DevBlog
{
    internal abstract class BaseRouteHandler(string route)
    {
        private string route = route;

        internal string Route
        {
            get => route;
            private set => route = value;
        }

        internal abstract ResponseParams HandleResponse(NameValueCollection parameters);
    }
}
