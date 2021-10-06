using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Tests.ContextBuilders
{
    public class ConnectionBuilder
    {
        private string _id;
        private IPAddress _remoteIpAddress;
        private int _remotePort;
        private IPAddress _localIpAddress;
        private int _localPort;
        private X509Certificate2 _clientCertificate;

        public ConnectionBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ConnectionBuilder WithRemoteIpAddress(IPAddress ipAddress)
        {
            _remoteIpAddress = ipAddress;
            return this;
        }

        public ConnectionBuilder WithLocalIpAddress(IPAddress ipAddress)
        {
            _localIpAddress = ipAddress;
            return this;
        }

        public ConnectionBuilder WithRemotePort(int port)
        {
            _remotePort = port;
            return this;
        }

        public ConnectionBuilder WithLocalPort(int port)
        {
            _localPort = port;
            return this;
        }

        public ConnectionBuilder WithClientCertificate(X509Certificate2 clientCertificate)
        {
            _clientCertificate = clientCertificate;
            return this;
        }

        public ConnectionInfo Build()
        {
            return new FakeConnection()
            {
                Id = _id,
                ClientCertificate = _clientCertificate,
                LocalIpAddress = _localIpAddress,
                LocalPort = _localPort,
                RemoteIpAddress = _remoteIpAddress,
                RemotePort = _remotePort
            };
        }
        
    }
}