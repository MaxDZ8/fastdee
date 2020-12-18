using CommandLine;

namespace fastdee
{
    class Args
    {
        [Value(0, MetaName = "pool", MetaValue = "STRATUM_IP_OR_URI:PORT", Required = true, HelpText = "Pool server endpoint.")]
        public string Pool { get; set; } = "";

        [Value(1, MetaName = "user", MetaValue = "<your_login>", Required = true, HelpText = "Username to pool.")]
        public string UserName { get; set; } = "";

        [Value(2, MetaName = "worker", MetaValue = "<worker_identifier>", Required = true, HelpText = "Worker name presented to pool.")]
        public string WorkerName { get; set; } = "";

        [Value(2, MetaName = "misc", MetaValue = "magic", Required = true, HelpText = "Originally intended as a very cheap password, for various things. Note there's no point in providing a password for real and please don't use real passwords here!")]
        public string SillyPassword { get; set; } = "";

        [Option('f', "fakeVersion", Required = false, HelpText = "Want to mess with the server? Specify your preferred program version to the server!")]
        public string? SubscribeAs { get; set; }
    }
}
