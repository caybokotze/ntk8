﻿using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Tests.ContextBuilders
{
    public class FakeConnection : ConnectionInfo
    {
        public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = new())
        {
            throw new System.NotImplementedException();
        }

        public override string Id { get; set; }
        public override IPAddress RemoteIpAddress { get; set; }
        public override int RemotePort { get; set; }
        public override IPAddress LocalIpAddress { get; set; }
        public override int LocalPort { get; set; }
        public override X509Certificate2 ClientCertificate { get; set; }
    }
}