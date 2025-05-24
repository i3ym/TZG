using Microsoft.Extensions.Logging;
using PinkSystem;
using PinkSystem.IO.Data;
using PinkSystem.Net;
using PinkSystem.Net.Http;
using PinkSystem.Net.Http.Handlers;
using PinkSystem.Net.Http.Handlers.Factories;

namespace TZG.Regions.Generator.Http
{
    internal sealed class HttpReqeustHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly IDataReader<Proxy> _proxiesReader;
        private readonly IHttpRequestHandlerFactory _httpRequestHandlerFactory;
        private readonly ILoggerFactory _loggerFactory;

        public HttpReqeustHandlerFactory(
            IDataReader<Proxy> proxiesReader,
            IHttpRequestHandlerFactory httpRequestHandlerFactory,
            ILoggerFactory loggerFactory
        )
        {
            _proxiesReader = proxiesReader.AsRepeatable();
            _httpRequestHandlerFactory = httpRequestHandlerFactory;
            _loggerFactory = loggerFactory;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            options.Proxy = _proxiesReader.Read();

            IHttpRequestHandler httpRequestHandler = _httpRequestHandlerFactory.Create(options);

            httpRequestHandler = new RepeatHttpRequestHandler(
                httpRequestHandler,
                5,
                TimeSpan.FromSeconds(10),
                _loggerFactory.CreateLogger<RepeatHttpRequestHandler>()
            );

            return httpRequestHandler;
        }
    }
}
