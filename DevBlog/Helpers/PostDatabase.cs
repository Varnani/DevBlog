namespace DevBlog.Helpers
{
    public struct PostData
    {
        public string title;
        public string content;
        public DateTime date;
    }

    internal static class PostDatabase
    {
        private static readonly List<PostData> postCache = new(100);

        static PostDatabase()
        {
            UpdatePostCache();
        }

        public static List<PostData> GetPosts()
        {
            return postCache;
        }

        public static PostData? GetPost(int id)
        {
            if (id < postCache.Count && id >= 0) return postCache[id];
            return null;
        }

        private static void UpdatePostCache()
        {
            postCache.Clear();

            FileInfo[] files;

            try
            {
                DirectoryInfo info = new(Path.Combine(Server.WebServer.ROOT_PATH, "Posts/"));
                files = info.GetFiles();
            }

            catch (Exception ex)
            {
                Logger.Log($"An exception occured: {ex}", Logger.Level.Error);
                return;
            }

            foreach (FileInfo file in files)
            {
                PostData data = new();

                if (file.Directory is null)
                {
                    Logger.Log($"file.Directory is null for {file.Name}, skipping.", Logger.Level.Warning);
                    continue;
                }

                string text = RouteHelpers.LoadTextFile(file.FullName);
                int firstLineEnd = text.IndexOf('\n');

                if (firstLineEnd == -1)
                {
                    Logger.Log($"Can't find title line for {file.Name}, skipping.", Logger.Level.Warning);
                    continue;
                }

                data.title = text.Substring(0, firstLineEnd);

                int secondLineEnd = text.IndexOf('\n', firstLineEnd + 1);

                if (secondLineEnd == -1)
                {
                    Logger.Log($"Can't find timestamp line for {file.Name}, skipping.", Logger.Level.Warning);
                    continue;
                }

                ReadOnlySpan<char> timestampSpan = text.AsSpan(firstLineEnd, secondLineEnd - firstLineEnd);
                if (!int.TryParse(timestampSpan, out int timestamp))
                {
                    Logger.Log($"Can't parse timestamp for {file.Name}, skipping.", Logger.Level.Warning);
                    continue;
                }

                data.date = DateTime.UnixEpoch + TimeSpan.FromSeconds(timestamp);

                data.content = text.Substring(secondLineEnd);

                postCache.Add(data);
            }

        }
    }

    //mdPath = Path.Combine(WebServer.SPECIAL_PATH, "test_markdown.md");

    //    internal static FileInfo[] GetPostFiles()
    //{
    //    static int FileDateComparer(FileInfo left, FileInfo right)
    //    {
    //        left.Refresh();
    //        right.Refresh();
    //        return right.CreationTimeUtc.CompareTo(left.CreationTimeUtc);
    //    }

    //    DirectoryInfo info = new(Path.Combine(Server.WebServer.ROOT_PATH, "Posts/"));
    //    FileInfo[] files = info.GetFiles();

    //    Array.Sort(files, FileDateComparer);

    //    return files;
    //}
}
