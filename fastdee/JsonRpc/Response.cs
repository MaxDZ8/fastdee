using Newtonsoft.Json;

namespace fastdee.JsonRpc
{
#pragma warning disable 649 // set by json library using reflection
    class Response
    {
        [JsonProperty("id")]
        public ulong id;

        [JsonProperty("result")]
        public object? rawRes;

        [JsonProperty("error")]
        public object? rawErr;
    }
#pragma warning restore
}
