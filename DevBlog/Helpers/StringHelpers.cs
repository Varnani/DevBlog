using System.Runtime.CompilerServices;

namespace DevBlog.Helpers
{
    internal static class StringHelpers
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Capitalize(string str)
        {
            return str; //TODO: implement this
        }
    }
}
