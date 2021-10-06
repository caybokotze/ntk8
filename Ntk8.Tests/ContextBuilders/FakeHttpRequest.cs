using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Tests.ContextBuilders
{
    public class FakeHttpRequest : HttpRequest
    {
        public FakeHttpRequest(
                HttpContext httpContext,
                string method,
                string scheme,
                bool isHttps,
                HostString host,
                PathString pathBase,
                PathString path,
                QueryString queryString,
                IQueryCollection query,
                string protocol,
                IHeaderDictionary headers,
                IRequestCookieCollection cookies,
                string contentType,
                Stream body,
                long? contentLength,
                bool hasFormContentType,
                IFormCollection form)
        {
            HttpContext = httpContext;
            Method = method;
            Scheme = scheme;
            IsHttps = isHttps;
            Host = host;
            PathBase = pathBase;
            Path = path;
            QueryString = queryString;
            Query = query;
            Protocol = protocol;
            Headers = headers;
            Cookies = cookies;
            ContentLength = contentLength;
            ContentType = contentType;
            Body = body;
            HasFormContentType = hasFormContentType;
            Form = form;
        }
        
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override HttpContext HttpContext { get; }
        public override string Method { get; set; }
        public override string Scheme { get; set; }
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; }
        public override string Protocol { get; set; }
        public override IHeaderDictionary Headers { get; }
        public override IRequestCookieCollection Cookies { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override Stream Body { get; set; }
        public override bool HasFormContentType { get; }
        public override IFormCollection Form { get; set; }
    }
}