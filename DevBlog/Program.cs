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

            bool shouldExit = false;

            {
                Console.CancelKeyPress += (sender, args) =>
                {
                    if (shouldExit) return;

                    args.Cancel = true;
                    shouldExit = true;
                    Logger.Log("Received cancellation.", Logger.Level.Info);
                };

                AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
                {
                    if (shouldExit) return;

                    shouldExit = true;
                    Logger.Log("Received exit signal.", Logger.Level.Info);
                };
            }

            while (!shouldExit)
            {
                Thread.Sleep(100);
            }

            server.Stop();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}