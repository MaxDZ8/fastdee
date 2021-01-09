using CommandLine;

namespace fastdee
{
    [Verb("simulate", HelpText = "Distribute replicated work as observed from a true connection.")]
    class SimulateArgs
    {
        [Value(0, MetaName = "source", MetaValue = "file.json", Required = true, HelpText = "Server replication configuration. Contains algorithm, subscribe and job information.")]
        public string Source { get; set; } = "";
    }
}
