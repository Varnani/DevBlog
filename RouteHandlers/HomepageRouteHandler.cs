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

            StringBuilder postListBuilder = new(1000);

            postListBuilder.Append("<div class=home-posts>");
            for (int i = 0; i < posts.Count; i++)
            {
                PostData post = posts[i];
                postListBuilder.Append("<div class=home-post-holder>");
                postListBuilder.Append($"<div class=home-post-title><a href=/post?id={i}>{post.title}</a></div>");
                postListBuilder.Append($"<div class=home-post-date>{post.date}</div>");
                postListBuilder.Append("</div>");
            }
            postListBuilder.Append("</div>");

            string html = RouteHelpers.GetHomeTemplate();
            StringBuilder htmlBuilder = new(html);

            htmlBuilder.Replace("%POST_LIST%", postListBuilder.ToString());
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
