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

            postListBuilder.AppendLine();
            postListBuilder.AppendLine();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                string title = file.Name;
                title = title.Replace('-', ' ');

                DateTime date = file.CreationTimeUtc;

                string entry = $"{date} - {title}";
                postListBuilder.AppendLine(entry);

                postListBuilder.AppendLine("  ");
            }

            string html = RouteHelpers.GetPostTemplate();
            StringBuilder htmlBuilder = new(html);

            string content;

            lock (pipeline)
            {
                content = Markdown.ToHtml(postListBuilder.ToString(), pipeline);
            }

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
