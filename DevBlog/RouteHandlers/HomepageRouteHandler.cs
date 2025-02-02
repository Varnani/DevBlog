using DevBlog.Helpers;
using DevBlog.Server;
using Markdig;
using System.Collections.Specialized;
using System.Text;

namespace DevBlog.RouteHandlers
{
    internal class HomepageRouteHandler : BaseRouteHandler
    {
        private readonly MarkdownPipeline pipeline;

        public HomepageRouteHandler(MarkdownPipeline pipeline) : base("/")
        {
            this.pipeline = pipeline;
        }

        internal override ResponseParams HandleResponse(NameValueCollection parameters)
        {
            FileInfo[] files = RouteHelpers.GetPostFiles();

            StringBuilder postListBuilder = new();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                string title = file.Name;
                title = title.LeftOf('.');
                title = title.Replace('-', ' ');
                title = title.Capitalize();

                DateTime date = file.CreationTimeUtc;

                postListBuilder.AppendLine($"[{date} - {title}](/post?id={i})  ");
            }

            string mdPath = Path.Combine(WebServer.SPECIAL_PATH, "home_content.md");
            string markdown = RouteHelpers.LoadTextFile(mdPath);

            markdown = markdown.Replace("%POST_LIST%", postListBuilder.ToString());

            string content;

            lock (pipeline)
            {
                content = Markdown.ToHtml(markdown, pipeline);
            }

            string html = RouteHelpers.GetPostTemplate();
            StringBuilder htmlBuilder = new(html);

            RouteHelpers.InsertPostContent(htmlBuilder, content);
            RouteHelpers.InsertCurrentYear(htmlBuilder);

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
