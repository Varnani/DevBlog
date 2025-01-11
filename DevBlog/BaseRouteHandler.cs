using System.Collections.Specialized;
using System.Net;

namespace DevBlog
{
    internal abstract class BaseRouteHandler
    {
        private string route = string.Empty;

        public string Route
        {
            get => route;
            private set => route = value;
        }

        protected BaseRouteHandler(string route)
        {
            this.Route = route;
        }

        internal abstract void HandleResponse(HttpListenerResponse response, NameValueCollection parameters, bool headOnly);
    }
}
