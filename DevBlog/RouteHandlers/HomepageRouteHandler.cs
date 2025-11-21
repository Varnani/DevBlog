using DevBlog.Helpers;
using DevBlog.Server;
using Markdig;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace DevBlog.RouteHandlers
{
    internal class HomepageRouteHandler : BaseRouteHandler
    {
        private readonly MarkdownPipeline pipeline;

        internal HomepageRouteHandler(MarkdownPipeline pipeline) : base("/")
        {
            this.pipeline = pipeline;
        }

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            List<PostData> posts = PostDatabase.GetPosts();

            StringBuilder postListBuilder = new();

            for (int i = 0; i < posts.Count; i++)
            {
                PostData post = posts[i];
                postListBuilder.AppendLine($"[{post.date} - {post.title}](/post?id={i})  ");
            }

            string mdPath = Path.Combine(WebServer.SPECIAL_PATH, "home_content.md");
            string markdown = RouteHelpers.LoadTextFile(mdPath);

            markdown = markdown.Replace("%POST_LIST%", postListBuilder.ToString());

            string content;

            lock (pipeline)
            {
                content = Markdown.ToHtml(markdown, pipeline);
            }

            string html = RouteHelpers.GetHomeTemplate();
            StringBuilder htmlBuilder = new(html);

            RouteHelpers.InsertPostContent(htmlBuilder, content);
            RouteHelpers.InsertCurrentYear(htmlBuilder);

            stopWatch.Stop();
            TimeSpan elapsed = stopWatch.Elapsed;

            RouteHelpers.InsertRenderTime(htmlBuilder, elapsed);

            ResponseParams response = new()
            {
                mime = WebServer.HTML_MIME,
                data = Encoding.UTF8.GetBytes(htmlBuilder.ToString()),
                encoding = Encoding.UTF8
            };

            return response;
        }
    }
}
