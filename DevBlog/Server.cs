using System.Net;

namespace DevBlog
{
    internal class Server
    {
        private const string PROTOCOL = "http";
        private const string URI_PREFIX = $"{PROTOCOL}://127.0.0.1:2525/";

        private const int MAX_HANDLERS = 20;

        private bool started = false;
        private bool cancelToken = false;
        private Lock lockObject = new();
        private Task? serverLoopTask = null;

        public bool Started { get => started; private set => started = value; }

        internal void Start()
        {
            if (Started) return;
            Started = true;

            Console.WriteLine("Starting server...");

            cancelToken = false;
            serverLoopTask = Task.Run(() =>
            {
                ServerLoop();
            });
        }

        internal void Stop()
        {
            if (!Started) return;
            Started = false;

            lock (lockObject) cancelToken = true;

            serverLoopTask?.Wait();

            Console.WriteLine("Server closed. Bye!");
        }

        private void ServerLoop()
        {
            Console.WriteLine("Configuring listener...");
            HttpListener listener = new();
            listener.Prefixes.Add(URI_PREFIX);

            Console.WriteLine("Starting listener...");
            listener.Start();

            List<Task> handlers = new(MAX_HANDLERS);

            while (!cancelToken)
            {
                while (handlers.Count < MAX_HANDLERS)
                {
                    Task<HttpListenerContext> ctx = listener.GetContextAsync();
                    Task handler = HandleHttpRequestAsync(ctx);
                    handlers.Add(handler);
                }

                for (int i = 0; i < handlers.Count;)
                {
                    Task task = handlers[i];

                    if (task.IsCompleted)
                    {
                        handlers.Remove(task);

                        if (task.Status == TaskStatus.Faulted)
                        {
                            Console.WriteLine("A handler was faulted.");
                            Console.WriteLine(task.Exception);
                        }
                    }

                    else
                    {
                        i++;
                    }
                }

                Thread.Sleep(250);
            }

            Console.WriteLine("Stopping server...");
            listener.Stop();
        }

        private static async Task HandleHttpRequestAsync(Task<HttpListenerContext> task)
        {
            HttpListenerContext ctx = await task;

            HttpListenerRequest request = ctx.Request;
            HttpListenerResponse response = ctx.Response;

            string method = request.HttpMethod;

            Console.WriteLine("------------------------");

            Console.WriteLine(
                $"Received {method} request.\n" +
                $" - User: {request.RemoteEndPoint.Address}\n" +
                $" - Raw: {request.RawUrl}\n" +
                $" - Headers:\n{request.Headers}\n");

            if (request.RawUrl == null)
            {
                Console.WriteLine("RawUrl is null.");
                string text = "An error is encountered.";
                response.SendResponseAndClose(text);
            }

            else
            {
                ReadOnlySpan<char> rawUrlSpan = request.RawUrl.AsSpan();

                ReadOnlySpan<char> route = rawUrlSpan.LeftOf('?');
                ReadOnlySpan<char> parameters = rawUrlSpan.RightOf('?');
                ReadOnlySpan<char> fileName = route.LeftOf('.');
                ReadOnlySpan<char> extension = route.RightOf('.');

                if (extension.IsEmpty()) extension = "html".AsSpan();
                if (fileName.IsSame("/") || fileName.IsEmpty()) fileName = "/index";

                Console.Write(" - Route: ");
                Console.Out.WriteLine(route);

                Console.Write(" - Parameters: ");
                Console.Out.WriteLine(parameters);

                Console.Write(" - File Name: ");
                Console.Out.WriteLine(fileName);

                Console.Write(" - Extension: ");
                Console.Out.WriteLine(extension);

                using StreamReader file = File.OpenText("Pages/test.html");
                string text = file.ReadToEnd();
                response.SendResponseAndClose(text);
            }

            Console.WriteLine("------------------------");
        }
    }
}
