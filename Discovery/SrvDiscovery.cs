using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DnsClient;

namespace Axon.Dns
{
    public class SrvDiscoverer<TEndpoint> : ADiscoverer<TEndpoint> where TEndpoint : IEncodableEndpoint
    {
        private LookupClient dnsClient;
        private Random discoveryRand;

        public SrvDiscoverer(string identifier, IEndpointDecoder<TEndpoint> endpointDecoder)
            : base(identifier, endpointDecoder)
        {
            this.dnsClient = new LookupClient();
            this.discoveryRand = new Random();
        }

        public override async Task<TEndpoint> Discover(int timeout = 0)
        {
            var results = await this.dnsClient.QueryAsync(this.Identifier, QueryType.SRV);
            var srvRecords = results.Answers.SrvRecords().ToArray();
            var srvRecord = srvRecords[this.discoveryRand.Next(srvRecords.Length)];

            return this.EndpointDecoder.Create(srvRecord.Target.Value.TrimEnd('.'), srvRecord.Port); //TODO
        }
        public override async Task<TEndpoint[]> DiscoverAll(int timeout = 0)
        {
            var results = await this.dnsClient.QueryAsync(this.Identifier, QueryType.SRV);
            var srvRecords = results.Answers.SrvRecords().ToArray();

            return srvRecords.Select(r => this.EndpointDecoder.Create(r.Target.Value.TrimEnd('.'), r.Port)).ToArray(); // TODO
        }
        public override Task Blacklist(TEndpoint endpoint)
        {
            Console.WriteLine("BLACKLISTING " + Encoding.UTF8.GetString(endpoint.Encode()));

            return Task.FromResult(true);
        }
    }
}
