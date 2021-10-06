using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Tests.ContextBuilders
{
    public class FakeHttpResponse : HttpResponse
    {
        public FakeHttpResponse(HttpContext httpContext,
            int statusCode,
            IHeaderDictionary headers,
            Stream body,
            long? contentLength,
            string contentType,
            IResponseCookies cookies,
            bool hasStarted)
        {
            HttpContext = httpContext;
            StatusCode = statusCode;
            Headers = headers;
            Body = body;
            ContentLength = contentLength;
            ContentType = contentType;
            Cookies = cookies;
            HasStarted = hasStarted;
        }
        
        public override void OnStarting(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void Redirect(string location, bool permanent)
        {
            throw new NotImplementedException();
        }

        public override HttpContext HttpContext { get; }
        public override int StatusCode { get; set; }
        public override IHeaderDictionary Headers { get; }
        public override Stream Body { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override IResponseCookies Cookies { get; }
        public override bool HasStarted { get; }
    }
}