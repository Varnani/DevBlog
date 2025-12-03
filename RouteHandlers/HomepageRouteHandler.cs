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
        internal HomepageRouteHandler(MarkdownPipeline pipeline) : base("/", pipeline) { }

        private readonly StringBuilder postListBuilder = new(10000);
        private readonly StringBuilder htmlBuilder = new(10000);

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            IReadOnlyList<PostData> posts = PostDatabase.GetPosts();

            postListBuilder.Clear();

            postListBuilder.Append("<div class=home-posts>");
            for (int i = 0; i < posts.Count; i++)
            {
                PostData post = posts[i];

                string formattedDate = post.date.ToString(StringHelpers.DATE_FORMAT);

                postListBuilder.Append("<div class=home-post-holder>");
                postListBuilder.Append($"<div class=home-post-title><a href=/post?id={i}>{post.title}</a></div>");
                postListBuilder.Append($"<div class=home-post-date>{formattedDate}</div>");
                postListBuilder.Append("</div>");
            }
            postListBuilder.Append("</div>");

            string html = RouteHelpers.GetHomeTemplate();
            string content = RouteHelpers.GetHomeContent();
            lock (pipeline) content = Markdown.ToHtml(content, pipeline: pipeline);

            htmlBuilder.Clear();
            htmlBuilder.Append(html);
            htmlBuilder.Replace("%POST_LIST%", postListBuilder.ToString());
            htmlBuilder.Replace("%HOME_CONTENT%", content);

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
