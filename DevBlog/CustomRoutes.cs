using System.Collections.Specialized;
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
        public PostRouteHandler() : base("/post") { }

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            string? value = parameters["id"];

            if (value == null)
            {
                ResponseParams response = Server.GenerateErrorResponse(System.Net.HttpStatusCode.BadRequest, "Post id is missing.");
                return response;
            }

            else
            {
                if (int.TryParse(value, out int id))
                {
                    ResponseParams response = new()
                    {
                        data = Encoding.UTF8.GetBytes($"post page for id {id}"),
                        encoding = Encoding.UTF8
                    };

                    return response;
                }

                else
                {
                    ResponseParams response = Server.GenerateErrorResponse(System.Net.HttpStatusCode.BadRequest, "Post id is malformed.");
                    return response;
                }
            }
        }
    }
}
