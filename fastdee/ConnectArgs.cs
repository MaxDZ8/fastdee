using CommandLine;

namespace fastdee
{
    [Verb("connect", isDefault: true, HelpText = "Perform real work by connecting to a pool.")]
    class ConnectArgs
    {
        [Value(0, MetaName = "pool", MetaValue = "IP_OR_URI:PORT", Required = true, HelpText = "Pool server (stratum) endpoint.")]
        public string Pool { get; set; } = "";

        [Value(1, MetaName = "user", MetaValue = "<login>", Required = true, HelpText = "Username to pool.")]
        public string UserName { get; set; } = "";

        [Value(2, MetaName = "worker", MetaValue = "<worker_id>", Required = true, HelpText = "Worker name presented to pool.")]
        public string WorkerName { get; set; } = "";

        [Value(2, MetaName = "misc", MetaValue = "magic", Required = true, HelpText = "Originally intended as a very cheap password, for various things. Note there's no point in providing a password for real and please don't use real passwords here!")]
        public string SillyPassword { get; set; } = "";

        [Option('f', "fakeVersion", Required = false, HelpText = "Want to mess with the server? Specify your preferred program version to the server!")]
        public string? SubscribeAs { get; set; }

        [Option('a', "algorithm", Required = true, HelpText = "Algorithm to be expected at the pool.")]
        public string Algorithm { get; set; } = "";

        [Option('d', "diffMul", Required = false, HelpText = "For some reason, it is sometimes necessary to change difficulty based on server. This is not really difficulty but rather a constant difficulty multiplicator to override algorithm default.")]
        public double? DifficultyMultiplier { get; set; }
    }
}
