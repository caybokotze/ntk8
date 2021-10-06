using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Ntk8.Tests.ContextBuilders
{
    public class HttpContextBuilder
    {
        private IFeatureCollection _features;
        private HttpRequest _request;
        private HttpResponse _response;
        private ConnectionInfo _connection;
        private WebSocketManager _webSockets;
        private ClaimsPrincipal _user;
        private IDictionary<object, object> _items;
        private IServiceProvider _requestServices;
        private CancellationToken _requestAborted;
        private string _traceIdentifier;
        private ISession _session;

        public HttpContextBuilder WithFeatures(IFeatureCollection featureCollection)
        {
            _features = featureCollection;
            return this;
        }

        public HttpContextBuilder WithRequest(HttpRequest httpRequest)
        {
            _request = httpRequest;
            return this;
        }

        public HttpContextBuilder WithResponse(HttpResponse httpResponse)
        {
            _response = httpResponse;
            return this;
        }

        public HttpContextBuilder WithConnectionInfo(ConnectionInfo connectionInfo)
        {
            _connection = connectionInfo;
            return this;
        }

        public HttpContextBuilder WithWebSocketManager(WebSocketManager webSocketManager)
        {
            _webSockets = webSocketManager;
            return this;
        }

        public HttpContextBuilder WithClaimsPrincipal(ClaimsPrincipal claimsPrincipal)
        {
            _user = claimsPrincipal;
            return this;
        }

        public HttpContextBuilder WithItems(Dictionary<object, object> items)
        {
            _items = items;
            return this;
        }

        public HttpContextBuilder WithServiceProvider(IServiceProvider serviceProvider)
        {
            _requestServices = serviceProvider;
            return this;
        }

        public HttpContextBuilder WithCancellationToken(CancellationToken token)
        {
            _requestAborted = token;
            return this;
        }

        public HttpContextBuilder WithTraceIdentifier(string traceIdentifier)
        {
            _traceIdentifier = traceIdentifier;
            return this;
        }

        public HttpContextBuilder WithSession(ISession session)
        {
            _session = session;
            return this;
        }

        public HttpContext Build()
        {
            return new FakeHttpContext(
                _features,
                _request,
                _response,
                _connection,
                _webSockets,
                _user,
                _items,
                _requestServices,
                _requestAborted,
                _traceIdentifier,
                _session);
        }
    }
}