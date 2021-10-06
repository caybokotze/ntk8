using System.IO;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Tests.ContextBuilders
{
    public class HttpRequestBuilder
    {
        private HttpContext _httpContext;
        private string _method;
        private string _scheme;
        private bool _isHttps;
        private HostString _host;
        private PathString _pathBase;
        private PathString _path;
        private QueryString _queryString;
        private IQueryCollection _query;
        private string _protocol;
        private IHeaderDictionary _headers;
        private IRequestCookieCollection _cookies;
        private long? _contentLength;
        private string _contentType;
        private Stream _body;
        private bool _hasFormContentType;
        private IFormCollection _form;

        public HttpRequestBuilder WithHttpContext(HttpContext httpContext)
        {
            _httpContext = httpContext;
            return this;
        }

        public HttpRequestBuilder WithMethod(string method)
        {
            _method = method;
            return this;
        }

        public HttpRequestBuilder WithScheme(string sheme)
        {
            _scheme = sheme;
            return this;
        }

        public HttpRequestBuilder IsHttps(bool isHttps)
        {
            _isHttps = isHttps;
            return this;
        }

        public HttpRequestBuilder WithHost(HostString host)
        {
            _host = host;
            return this;
        }

        public HttpRequestBuilder WithPathBase(PathString pathBase)
        {
            _pathBase = pathBase;
            return this;
        }

        public HttpRequestBuilder WithPath(PathString path)
        {
            _path = path;
            return this;
        }

        public HttpRequestBuilder WithQueryString(QueryString queryString)
        {
            _queryString = queryString;
            return this;
        }

        public HttpRequestBuilder WithQuery(QueryCollection query)
        {
            _query = query;
            return this;
        }

        public HttpRequestBuilder WithProtocol(string protocol)
        {
            _protocol = protocol;
            return this;
        }

        public HttpRequestBuilder WithHeaders(IHeaderDictionary headers)
        {
            _headers = headers;
            return this;
        }

        public HttpRequestBuilder WithCookies(IRequestCookieCollection cookies)
        {
            _cookies = cookies;
            return this;
        }

        public HttpRequestBuilder WithContentLength(long contentLength)
        {
            _contentLength = contentLength;
            return this;
        }

        public HttpRequestBuilder WithContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public HttpRequestBuilder WithBody(Stream body)
        {
            _body = body;
            return this;
        }

        public HttpRequestBuilder HasFormContentType(bool hasFormContentType)
        {
            _hasFormContentType = hasFormContentType;
            return this;
        }

        public HttpRequestBuilder WithFormCollection(IFormCollection form)
        {
            _form = form;
            return this;
        }
        
        public HttpRequest Build()
        {
            return new FakeHttpRequest(
                _httpContext, 
                _method, 
                _scheme, 
                _isHttps, 
                _host, 
                _pathBase, 
                _path, 
                _queryString,
                _query, 
                _protocol, 
                _headers, 
                _cookies, 
                _contentType, 
                _body, 
                _contentLength, 
                _hasFormContentType, 
                _form);
        }
    }
}