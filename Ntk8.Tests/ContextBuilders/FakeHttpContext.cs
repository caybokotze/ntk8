using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Ntk8.Tests.ContextBuilders
{
    public class FakeHttpContext : HttpContext
    {
        public FakeHttpContext(
            IFeatureCollection featureCollection,
            HttpRequest httpRequest, 
            HttpResponse httpResponse,
            ConnectionInfo connectionInfo,
            WebSocketManager webSocketManager,
            ClaimsPrincipal user,
            IDictionary<object, object> items,
            IServiceProvider requestServices,
            CancellationToken requestAborted,
            string traceIdentifier,
            ISession session
            )
        {
            Features = featureCollection;
            Request = httpRequest;
            Response = httpResponse;
            Connection = connectionInfo;
            WebSockets = webSocketManager;
            User = user;
            Items = items;
            RequestServices = requestServices;
            RequestAborted = requestAborted;
            TraceIdentifier = traceIdentifier;
            Session = session;
        }
        
        public override void Abort()
        {
            throw new Exception("fake http context has been aborted...");
        }

        public override IFeatureCollection Features { get; }
        public override HttpRequest Request { get; }
        public override HttpResponse Response { get; }
        public override ConnectionInfo Connection { get; }
        public override WebSocketManager WebSockets { get; }
        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; }
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; }
        public override ISession Session { get; set; }
    }
}