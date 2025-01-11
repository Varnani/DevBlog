namespace DevBlog
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Server server = new();

            server.Start();

            while (true)
            {
                string? line = Console.ReadLine();

                if (line == null) continue;

                if (line == "stop")
                {
                    if (!server.Started) Console.WriteLine("Server is not running.");
                    server.Stop();
                }

                else if (line == "start")
                {
                    if (server.Started) Console.WriteLine("Server is already running.");
                    server.Start();
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