using CommandLine;

namespace fastdee
{
    class Args
    {
        [Value(0, MetaName = "pool", MetaValue = "STRATUM_IP_OR_URI:PORT", Required = true, HelpText = "Pool server endpoint.")]
        public string Pool { get; set; } = "";

        [Option('f', "fakeVersion", Required = false, HelpText = "Want to mess with the server? Specify your preferred program version to the server!")]
        public string? SubscribeAs { get; set; }
    }
}
