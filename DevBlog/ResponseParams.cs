using System.Net;
using System.Text;

namespace DevBlog
{
    internal struct ResponseParams
    {
        internal HttpStatusCode code = HttpStatusCode.OK;
        internal string? mime;
        internal Encoding? encoding;
        internal byte[]? data;

        public ResponseParams() { }
    }
}
