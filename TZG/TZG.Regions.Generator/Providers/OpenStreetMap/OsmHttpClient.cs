using PinkSystem;
using PinkSystem.IO.Data;
using PinkSystem.Net;
using PinkSystem.Net.Http;
using PinkSystem.Net.Http.Handlers;

namespace TZG.Regions.Generator.Providers.OpenStreetMap
{
    public sealed class OsmHttpRequest
    {
        public required HttpRequest Request { get; init; }
        public bool UseProxy { get; init; } = false;
    }

    public sealed class OsmHttpClient
    {
        private readonly IHttpRequestHandlerFactory _httpRequestHandlerFactory;
        private readonly IDataReader<Proxy> _proxiesDataReader;
        private readonly IHttpRequestHandler _defaultHttpRequestHandler;

        public OsmHttpClient(
            IHttpRequestHandlerFactory httpRequestHandlerFactory,
            IDataReader<Proxy> proxiesDataReader
        )
        {
            _httpRequestHandlerFactory = httpRequestHandlerFactory;
            _proxiesDataReader = proxiesDataReader.Repeat();

            _defaultHttpRequestHandler = CreateHttpRequestHandler();
        }

        public async Task<HttpResponse> Send(OsmHttpRequest osmRequest, CancellationToken cancellationToken)
        {
            IHttpRequestHandler? httpRequestHandler = null;

            try
            {
                if (osmRequest.UseProxy)
                {
                    httpRequestHandler = CreateHttpRequestHandler(new HttpRequestHandlerOptions()
                    {
                        Proxy = _proxiesDataReader.Read()
                    });
                }
                else
                {
                    httpRequestHandler = _defaultHttpRequestHandler;
                }

                return await httpRequestHandler.SendAsync(osmRequest.Request, cancellationToken);
            }
            finally
            {
                httpRequestHandler?.Dispose();
            }
        }

        private IHttpRequestHandler CreateHttpRequestHandler(IHttpRequestHandlerOptions? options = null)
        {
            return _httpRequestHandlerFactory
                .Create(options)
                .WithCompression();
        }
    }
}
