using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace DevBlog
{
    internal static class FileRequestHandlers
    {
        internal delegate void HandlerDelegate(HttpListenerResponse response, string path, bool headOnly);

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

        internal static void ICORequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleBinaryRequest(response, path, headOnly, "image/vnd.microsoft.icon");
        }

        internal static void JPGRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleBinaryRequest(response, path, headOnly, "image/jpeg");
        }

        internal static void PNGRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleBinaryRequest(response, path, headOnly, "image/png");
        }

        internal static void GIFRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleBinaryRequest(response, path, headOnly, "image/gif");
        }

        internal static void HTMLRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/html; charset=utf-8");
        }

        internal static void PlainTextRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/plain; charset=utf-8");
        }

        internal static void JavascriptRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/javascript; charset=utf-8");
        }

        internal static void CSSRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/css; charset=utf-8");
        }

        internal static void MarkdownRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/markdown; charset=utf-8");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void HandleBinaryRequest(HttpListenerResponse response, string path, bool headOnly, string mime)
        {
            byte[] data = File.ReadAllBytes(path);
            Server.SendBody(response, mime, data, headOnly);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HandleTextRequest(HttpListenerResponse response, string path, bool headOnly, string mime)
        {
            using StreamReader stream = File.OpenText(path);
            string text = stream.ReadToEnd();

            byte[] data = Encoding.UTF8.GetBytes(text);

            Server.SendBody(response, mime, data, headOnly, encoding: Encoding.UTF8);
        }
    }
}
