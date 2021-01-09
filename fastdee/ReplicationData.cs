using Newtonsoft.Json;

namespace fastdee
{
    /// <summary>
    /// This ugly thing allows me to load a quick-n-dirty json file with samples of observed traffic.
    /// Passing all this data from command line looks bad.
    /// 
    /// I could also explore validation more but I keep it simple and load basically everything as strings!
    /// </summary>
    class ReplicationData
    {
        [JsonProperty("algo")]
        internal string algo = "";

        [JsonProperty("subscribe")]
        internal Subscribe subscribe = new Subscribe();

        [JsonProperty("job")]
        internal Job job = new Job();
        
        [JsonProperty("shareDiff")]
        internal double shareDiff;

        [JsonProperty("startingNonce2")]
        internal ulong nonce2off;

        internal class Subscribe
        {
            [JsonProperty("extranonce1")]
            internal string extraNonce1 = "";

            [JsonProperty("n2sz")]
            internal ushort nonce2sz = 4;
        }

        internal class Job
        {
            [JsonProperty("prevHash")]
            internal string prevHash = "";

            [JsonProperty("cbHead")]
            internal string cbHead = "";

            [JsonProperty("cbTail")]
            internal string cbTail = "";

            [JsonProperty("merkles")]
            internal string[]? merkles;

            [JsonProperty("blockVersion")]
            internal string blockVersion = "";

            [JsonProperty("networkDiff")]
            internal string networkDiff = "";

            [JsonProperty("networkTime")]
            internal string networkTime = "";

        }
    }
}
