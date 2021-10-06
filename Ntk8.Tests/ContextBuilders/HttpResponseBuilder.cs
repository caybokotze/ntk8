using System.IO;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Tests.ContextBuilders
{
    public class HttpResponseBuilder
    {
        private HttpContext _httpContext;
        private int _statusCode;
        private IHeaderDictionary _headers;
        private Stream _body;
        private long? _contentLength;
        private string _contentType;
        private IResponseCookies _cookies;
        private bool _hasStarted;

        public HttpResponseBuilder WithHttpContext(HttpContext httpContext)
        {
            _httpContext = httpContext;
            return this;
        }

        public HttpResponseBuilder WithHttpStatusCode(int statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

        public HttpResponseBuilder WithHeaders(IHeaderDictionary headers)
        {
            _headers = headers;
            return this;
        }

        public HttpResponseBuilder WithBody(Stream body)
        {
            _body = body;
            return this;
        }

        public HttpResponseBuilder WithContentLength(long contentLength)
        {
            _contentLength = contentLength;
            return this;
        }

        public HttpResponseBuilder WithContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public HttpResponseBuilder WithResponseCookies(IResponseCookies responseCookies)
        {
            _cookies = responseCookies;
            return this;
        }

        public HttpResponseBuilder HasStarted(bool hasStarted)
        {
            _hasStarted = hasStarted;
            return this;
        }

        public HttpResponse Build()
        {
            return new FakeHttpResponse(
                _httpContext,
                _statusCode,
                _headers,
                _body,
                _contentLength,
                _contentType,
                _cookies,
                _hasStarted);
        }
    }
}