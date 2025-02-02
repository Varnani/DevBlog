using Markdig;

namespace DevBlog
{
    internal static class Program
    {
        static void Main(string[] _)
        {
            Server server = new();

            RegisterRoutes(server);

            server.Start();

            RunCommandLoop(server);
        }

        private static void RegisterRoutes(Server server)
        {
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            HomepageRouteHandler homepageHandler = new(pipeline);
            PostRouteHandler postHandler = new(pipeline);

            server.AddRoute(homepageHandler);
            server.AddRoute(postHandler);
        }

        private static void RunCommandLoop(Server server)
        {
            while (true)
            {
                string? line = Console.ReadLine();

                if (line == null) continue;

                if (line == "stop")
                {
                    if (!server.Started) Console.WriteLine("Server is not running.");
                    else server.Stop();
                }

                else if (line == "start")
                {
                    if (server.Started) Console.WriteLine("Server is already running.");
                    else server.Start();
                }

                else if (line == "restart")
                {
                    if (server.Started) server.Stop();
                    server.Start();
                }

                else if (line == "exit")
                {
                    if (server.Started) server.Stop();
                    break;
                }

                else
                {
                    Console.WriteLine("Unknown command.");
                }
            }
        }
    }
}