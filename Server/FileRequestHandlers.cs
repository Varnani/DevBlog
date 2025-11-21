using System.Runtime.CompilerServices;
using System.Text;

namespace DevBlog.Server
{
    internal static class FileRequestHandlers
    {
        internal delegate ResponseParams HandlerDelegate(string path);

        internal static Dictionary<string, HandlerDelegate> Handlers = new()
        {
            { "html", HTMLRequestHandler },
            { "txt", PlainTextRequestHandler },
            { "js", JavascriptRequestHandler },
            { "css", CSSRequestHandler },
            { "md", MarkdownRequestHandler },

            { "jpg", JPGRequestHandler},
            { "jpeg", JPGRequestHandler},
            { "gif", GIFRequestHandler},
            { "png", PNGRequestHandler},
            { "ico", ICORequestHandler }
        };

        internal static ResponseParams ICORequestHandler(string path)
        {
            return HandleBinaryRequest(path, "image/vnd.microsoft.icon");
        }

        internal static ResponseParams JPGRequestHandler(string path)
        {
            return HandleBinaryRequest(path, "image/jpeg");
        }

        internal static ResponseParams PNGRequestHandler(string path)
        {
            return HandleBinaryRequest(path, "image/png");
        }

        internal static ResponseParams GIFRequestHandler(string path)
        {
            return HandleBinaryRequest(path, "image/gif");
        }

        internal static ResponseParams HTMLRequestHandler(string path)
        {
            return HandleTextRequest(path, WebServer.HTML_MIME);
        }

        internal static ResponseParams PlainTextRequestHandler(string path)
        {
            return HandleTextRequest(path, "text/plain; charset=utf-8");
        }

        internal static ResponseParams JavascriptRequestHandler(string path)
        {
            return HandleTextRequest(path, "text/javascript; charset=utf-8");
        }

        internal static ResponseParams CSSRequestHandler(string path)
        {
            return HandleTextRequest(path, "text/css; charset=utf-8");
        }

        internal static ResponseParams MarkdownRequestHandler(string path)
        {
            return HandleTextRequest(path, "text/markdown; charset=utf-8");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ResponseParams HandleBinaryRequest(string path, string mime)
        {
            byte[] data = File.ReadAllBytes(path);

            ResponseParams response = new()
            {
                mime = mime,
                data = data
            };

            return response;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ResponseParams HandleTextRequest(string path, string mime)
        {
            using StreamReader stream = File.OpenText(path);
            string text = stream.ReadToEnd();

            byte[] data = Encoding.UTF8.GetBytes(text);

            ResponseParams response = new()
            {
                encoding = Encoding.UTF8,
                mime = mime,
                data = data
            };

            return response;
        }
    }
}
