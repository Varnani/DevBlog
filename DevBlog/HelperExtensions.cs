using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace DevBlog
{
    internal static class HelperExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string LeftOf(this string str, char c)
        {
            int index = str.IndexOf(c);
            if (index == -1) index = str.Length;
            return str.Substring(0, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string RightOf(this string str, char c)
        {
            int index = str.IndexOf(c);
            if (index == -1) return string.Empty;
            return str.Substring(index + 1);
        }

        internal static void SendHTMLAndClose(this HttpListenerResponse response, string html, bool headOnly)
        {
            byte[] data = Encoding.UTF8.GetBytes(html);
            response.SendResponseAndClose("text/html", data, headOnly, encoding: Encoding.UTF8);
        }

        internal static void SendResponseAndClose(this HttpListenerResponse response, string mime, byte[] data, bool headOnly, Encoding? encoding = null)
        {
            response.ContentEncoding = encoding;
            response.ContentType = mime;
            response.ContentLength64 = data.Length;

            if (!headOnly)
            {
                response.OutputStream.Write(data, 0, data.Length);
            }

            response.OutputStream.Close();
        }
    }
}
