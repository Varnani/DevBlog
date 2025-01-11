using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace DevBlog
{
    internal static class HelperExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<char> LeftOf(this ReadOnlySpan<char> str, char c)
        {
            int index = str.IndexOf(c);
            if (index == -1) index = str.Length;
            return str.Slice(0, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<char> RightOf(this ReadOnlySpan<char> span, char c)
        {
            int index = span.IndexOf(c);
            if (index == -1) return ReadOnlySpan<char>.Empty;
            return span.Slice(index + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsSame(this ReadOnlySpan<char> span, string str)
        {
            if (span.Length != str.Length) return false;

            for (int i = 0; i < span.Length; i++)
            {
                char c = span[i];
                char s = str[i];

                if (s != c) return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsEmpty(this ReadOnlySpan<char> span)
        {
            return span.Length == 0;
        }

        internal static void SendHTMLAndClose(this HttpListenerResponse response, string str, bool headOnly)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
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
