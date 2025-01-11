using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace DevBlog
{
    internal class Server
    {
        private const string LISTEN_ADDR = $"http://127.0.0.1:2525/";
        private const int MAX_HANDLERS = 32;

        private const string ROOT_PATH = "Root";
        private const string ERROR_PAGE = "error_template.html";

        private const string ERROR_TYPE_TOKEN = "%ERROR_TYPE%";
        private const string ERROR_MESSAGE_TOKEN = "%ERROR_MSG%";

        private bool started = false;
        private bool cancelToken = false;
        private Lock lockObject = new();
        private Task? serverLoopTask = null;

        private Dictionary<string, BaseRouteHandler> routerDict = new();

        public bool Started
        {
            get => started;
            private set => started = value;
        }

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

        internal static void SendErrorResponse(HttpListenerResponse response, ErrorCode code, string message, bool headOnly)
        {
            response.StatusCode = ((int)code);

            string errorPagePath = Path.Combine(ROOT_PATH, ERROR_PAGE);
            using StreamReader stream = File.OpenText(errorPagePath);
            string page = stream.ReadToEnd();

            page = page.Replace(ERROR_TYPE_TOKEN, $"{((int)code)} - {code}");
            page = page.Replace(ERROR_MESSAGE_TOKEN, message);

            response.SendHTMLAndClose(page, headOnly);
        }

        private void ServerLoop()
        {
            Console.WriteLine("Configuring listener...");
            HttpListener listener = new();
            listener.Prefixes.Add(LISTEN_ADDR);

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

        private async Task HandleHttpRequestAsync(Task<HttpListenerContext> task)
        {
            HttpListenerContext ctx = await task;

            HttpListenerRequest request = ctx.Request;
            HttpListenerResponse response = ctx.Response;

            Console.WriteLine("------------------------");
            Console.WriteLine($"Incoming {request.HttpMethod} request from {request.Headers.Get("X-Real-IP")} to {request.RawUrl}");
            Console.WriteLine(request.Headers);
            Console.WriteLine("------------------------");

            HttpMethod method = new(request.HttpMethod);

            if (method != HttpMethod.Get && method != HttpMethod.Head)
            {
                SendErrorResponse(response, ErrorCode.NotImplemented, $"HTTP Method {request.HttpMethod} is not supported.", false);
            }

            bool isHeadOnly = method == HttpMethod.Head;

            if (request.RawUrl == null)
            {
                SendErrorResponse(response, ErrorCode.Internal, "Internal server error.", false);
            }

            else
            {
                string route = request.RawUrl.LeftOf('?');
                string query = request.RawUrl.RightOf('?');

                NameValueCollection parameters = HttpUtility.ParseQueryString(query);

                if (routerDict.TryGetValue(route, out BaseRouteHandler? handler))
                {
                    handler!.HandleResponse(response, parameters, isHeadOnly);
                }

                else
                {
                    // there aren't any handlers for requested route. 
                    // we'll try to serve a file instead.

                    string file = route.LeftOf('.');
                    string extension = route.RightOf('.');

                    if (string.IsNullOrWhiteSpace(extension)) extension = "html";
                    if (file == "/" || string.IsNullOrWhiteSpace(file)) file = "/index";

                    if (file[^1] == '/')
                    {
                        file += "index";
                    }

                    string path = $"{ROOT_PATH}{file}.{extension}";

                    if (!File.Exists(path))
                    {
                        SendErrorResponse(response, ErrorCode.NotFound, "Content not found.", isHeadOnly);
                        return;
                    }

                    if (FileRequestHandlers.Handlers.TryGetValue(extension, out FileRequestHandlers.HandlerDelegate? serveHandler))
                    {
                        try
                        {
                            serveHandler!.Invoke(response, path, isHeadOnly);
                        }

                        catch (Exception)
                        {
                            SendErrorResponse(response, ErrorCode.Internal, "Internal server error.", isHeadOnly);
                        }
                    }

                    else
                    {
                        SendErrorResponse(response, ErrorCode.Unavailable, "Content unavailable.", isHeadOnly);
                    }
                }
            }
        }
    }
}
