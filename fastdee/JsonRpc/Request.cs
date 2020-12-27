using Newtonsoft.Json;

namespace fastdee.JsonRpc
{
    class Request
    {
        [JsonProperty("id")]
        public ulong id;

        [JsonProperty("method")]
        public string method;

        [JsonProperty("params")]
        public object[] args;

        public Request(ulong id, string method, object[] args)
        {
            this.id = id;
            this.method = method;
            this.args = args;
        }
    }
}
