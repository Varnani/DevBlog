using DevBlog.Helpers;
using DevBlog.RouteHandlers;
using DevBlog.Server;
using Markdig;

namespace DevBlog
{
    internal static class Program
    {
        static void Main(string[] _)
        {
#if DEBUG
            Logger.LogLevel = Logger.Level.Debug;
#endif
            WebServer server = new();

            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            HomepageRouteHandler homepageHandler = new(pipeline);
            PostRouteHandler postHandler = new(pipeline);

            server.AddRoute(homepageHandler);
            server.AddRoute(postHandler);

            server.Start();

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}