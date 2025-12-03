using DevBlog.Helpers;
using DevBlog.RouteHandlers;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace DevBlog.Server
{
    internal class WebServer
    {
        // TODO: implement a config system
        // TOOD: implement a rate limiting system
        // TODO: implement a log system

        public const string HTML_MIME = "text/html; charset=utf-8";

        public const string SPECIAL_PATH = "www/SpecialPages";
        public const string ROOT_PATH = "www/Root";

        private const bool SEND_GZIP = true;

        private const string LISTEN_ADDR = "http://127.0.0.1:2525/";
        private const int MAX_HANDLERS = 32;

        private const string ERROR_PAGE = "error_template.html";

        private const string ERROR_TYPE_TOKEN = "%ERROR_TYPE%";
        private const string ERROR_MESSAGE_TOKEN = "%ERROR_MSG%";

        private bool started = false;
        private bool cancelToken = false;
        private Lock lockObject = new();
        private Task? serverLoopTask = null;
        private Dictionary<string, BaseRouteHandler> routes = new();

        internal bool Started
        {
            get => started;
            private set => started = value;
        }

        internal void Start()
        {
            if (Started) return;
            Started = true;

            cancelToken = false;

            Logger.Log("Starting server...", Logger.Level.Info);

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

            Logger.Log("Server closed. Bye!", Logger.Level.Info);
        }

        internal void AddRoute(BaseRouteHandler handler)
        {
            routes.Add(handler.Route, handler);
        }

        private void ServerLoop()
        {
            Logger.Log("Configuring listener...", Logger.Level.Info);
            HttpListener listener = new();
            listener.Prefixes.Add(LISTEN_ADDR);

            Logger.Log($"Starting listener on {LISTEN_ADDR}...", Logger.Level.Info);
            listener.Start();

            List<Task> handlers = new(MAX_HANDLERS);

            Task StartListenerTask()
            {
                Task<HttpListenerContext> ctx = listener.GetContextAsync();
                Task handler = HandleHttpRequestAsync(ctx);

                return handler;
            }

            for (int i = 0; i < MAX_HANDLERS; i++)
            {
                Task handler = StartListenerTask();
                handlers.Add(handler);
            }

            while (!cancelToken)
            {
                for (int i = 0; i < handlers.Count;)
                {
                    Task task = handlers[i];

                    if (task.IsCompleted)
                    {
                        if (task.Status == TaskStatus.Faulted)
                        {
                            Logger.Log("A handler was faulted.", Logger.Level.Error);
                            Logger.Log(task.Exception!.ToString(), Logger.Level.Error);
                        }

                        handlers[i] = StartListenerTask();
                    }

                    else
                    {
                        i++;
                    }
                }

                Thread.Sleep(100);
            }

            Logger.Log("Stopping server...", Logger.Level.Info);
            listener.Stop();
        }

        private async Task HandleHttpRequestAsync(Task<HttpListenerContext> task)
        {
            HttpListenerContext ctx = await task;

            HttpListenerRequest request = ctx.Request;
            HttpListenerResponse response = ctx.Response;

            Logger.Log($"{request.Headers.Get("X-Real-IP")}: Incoming {request.HttpMethod} request to {request.RawUrl}", Logger.Level.Info);

            HttpMethod method = new(request.HttpMethod);
            bool headOnly = method == HttpMethod.Head;

            try
            {
                ResponseParams responseParams;

                if (method != HttpMethod.Get && method != HttpMethod.Head)
                {
                    responseParams = GenerateErrorResponse(HttpStatusCode.NotImplemented, $"HTTP Method {request.HttpMethod} is not supported.");
                }

                else
                {
                    string route;
                    string query;

                    if (request.RawUrl == null)
                    {
                        route = "/";
                        query = string.Empty;
                    }

                    else
                    {
                        route = request.RawUrl.LeftOf('?');
                        query = request.RawUrl.RightOf('?');
                    }

                    NameValueCollection parameters = HttpUtility.ParseQueryString(query);

                    if (routes.TryGetValue(route, out BaseRouteHandler? handler))
                    {
                        responseParams = handler!.HandleResponse(parameters);
                    }

                    else
                    {
                        // there aren't any handlers for the requested route. 
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
                            responseParams = GenerateErrorResponse(HttpStatusCode.NotFound, "Content not found.");
                        }

                        else
                        {
                            if (FileRequestHandlers.Handlers.TryGetValue(extension, out FileRequestHandlers.HandlerDelegate? serveHandler))
                            {
                                responseParams = serveHandler!.Invoke(path);
                            }

                            else
                            {
                                responseParams = GenerateErrorResponse(HttpStatusCode.ServiceUnavailable, "Content unavailable.");
                            }
                        }
                    }
                }

                SendResponse(responseParams, request, response, headOnly);
            }

            catch (Exception)
            {
                ResponseParams responseParams = GenerateErrorResponse(HttpStatusCode.InternalServerError, "An internal server error occured.");
                SendResponse(responseParams, request, response, headOnly);

                throw;
            }
        }

        internal static ResponseParams GenerateErrorResponse(HttpStatusCode error, string message)
        {
            string errorPagePath = Path.Combine(SPECIAL_PATH, ERROR_PAGE);
            using StreamReader stream = File.OpenText(errorPagePath);
            string page = stream.ReadToEnd();

            page = page.Replace(ERROR_TYPE_TOKEN, $"{(int)error} - {error}");
            page = page.Replace(ERROR_MESSAGE_TOKEN, message);

            byte[] data = Encoding.UTF8.GetBytes(page);

            ResponseParams response = new()
            {
                code = error,
                mime = HTML_MIME,
                data = data,
                encoding = Encoding.UTF8
            };

            Logger.Log($"Generated error response: {error} - {message}", Logger.Level.Error);

            return response;
        }

        private static void SendResponse(ResponseParams responseParams, HttpListenerRequest request, HttpListenerResponse response, bool headOnly)
        {
            if (responseParams.data == null) responseParams.data = [];

            if (SEND_GZIP)
            {
                string? acceptedEncodings = request.Headers["Accept-Encoding"];

                if (acceptedEncodings != null)
                {
                    if (acceptedEncodings.Contains("gzip"))
                    {
                        using MemoryStream compressed = new();
                        using GZipStream zip = new(compressed, CompressionMode.Compress);

                        zip.Write(responseParams.data, 0, responseParams.data.Length);
                        zip.Flush();

                        responseParams.data = compressed.ToArray();

                        response.AddHeader("Content-Encoding", "gzip");
                    }
                }
            }

            response.StatusCode = (int)responseParams.code;
            response.ContentEncoding = responseParams.encoding;
            response.ContentType = responseParams.mime;
            response.ContentLength64 = responseParams.data.Length;

            if (!headOnly)
            {
                response.OutputStream.Write(responseParams.data, 0, responseParams.data.Length);
            }

            response.OutputStream.Close();
        }
    }
}
