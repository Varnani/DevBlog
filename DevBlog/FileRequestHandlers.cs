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


        };

        internal static void HTMLRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/html");
        }

        internal static void PlainTextRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/plain");
        }

        internal static void JavascriptRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/javascript");
        }

        internal static void CSSRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {
            HandleTextRequest(response, path, headOnly, "text/css");
        }

        internal static void MarkdownRequestHandler(HttpListenerResponse response, string path, bool headOnly)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HandleTextRequest(HttpListenerResponse response, string path, bool headOnly, string mime)
        {
            using StreamReader stream = File.OpenText(path);
            string text = stream.ReadToEnd();

            byte[] data = Encoding.UTF8.GetBytes(text);

            response.SendResponseAndClose(mime, data, headOnly, encoding: Encoding.UTF8);
        }
    }
}
