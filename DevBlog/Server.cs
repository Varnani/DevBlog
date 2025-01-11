using System.Collections.Specialized;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace DevBlog
{
    internal class Server
    {
        // TODO: implement a config system
        private const bool SEND_GZIP = true;

        private const string LISTEN_ADDR = "http://127.0.0.1:2525/";
        private const int MAX_HANDLERS = 32;

        private const string ROOT_PATH = "Root";
        private const string SPECIAL_PATH = "SpecialPages";

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

        internal static void SendBody(HttpListenerResponse response, string mime, byte[] data, bool headOnly, Encoding? encoding = null)
        {
            if (SEND_GZIP)
            {
                using MemoryStream compressed = new();
                using GZipStream zip = new(compressed, CompressionMode.Compress);
                zip.Write(data, 0, data.Length);
                zip.Flush();

                data = compressed.ToArray();

                response.AddHeader("Content-Encoding", "gzip");
            }

            response.ContentEncoding = encoding;
            response.ContentType = mime;
            response.ContentLength64 = data.Length;

            if (!headOnly)
            {
                response.OutputStream.Write(data, 0, data.Length);
            }

            response.OutputStream.Close();
        }

        internal static void SendHTML(HttpListenerResponse response, string html, bool headOnly)
        {
            byte[] data = Encoding.UTF8.GetBytes(html);
            SendBody(response, "text/html; charset=utf-8", data, headOnly, encoding: Encoding.UTF8);
        }

        internal static void SendError(HttpListenerResponse response, ErrorCode code, string message, bool headOnly)
        {
            response.StatusCode = ((int)code);

            string errorPagePath = Path.Combine(SPECIAL_PATH, ERROR_PAGE);
            using StreamReader stream = File.OpenText(errorPagePath);
            string page = stream.ReadToEnd();

            page = page.Replace(ERROR_TYPE_TOKEN, $"{((int)code)} - {code}");
            page = page.Replace(ERROR_MESSAGE_TOKEN, message);

            SendHTML(response, page, headOnly);
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
                SendError(response, ErrorCode.NotImplemented, $"HTTP Method {request.HttpMethod} is not supported.", false);
            }

            bool isHeadOnly = method == HttpMethod.Head;

            if (request.RawUrl == null)
            {
                SendError(response, ErrorCode.Internal, "Internal server error.", false);
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
                        SendError(response, ErrorCode.NotFound, "Content not found.", isHeadOnly);
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
                            SendError(response, ErrorCode.Internal, "Internal server error.", isHeadOnly);
                        }
                    }

                    else
                    {
                        SendError(response, ErrorCode.Unavailable, "Content unavailable.", isHeadOnly);
                    }
                }
            }
        }
    }
}
