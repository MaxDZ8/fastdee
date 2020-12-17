using Newtonsoft.Json;

namespace fastdee.JsonRpc
{
#pragma warning disable 649 // set by json library using reflection
    /// <summary>
    /// Can parse everything the server might send to client. A bit overabundant.
    /// </summary>
    class Message
    {
        /// <summary>
        /// Notifications sent from server on its initiative have id=undefined/null.
        /// </summary>
        [JsonProperty("id")]
        public ulong? id;

        /// <summary>
        /// Reply from server to client, successful.
        /// </summary>
        [JsonProperty("result")]
        public object? rawRes;

        /// <summary>
        /// Reply from server to client, failed.
        /// </summary>
        [JsonProperty("error")]
        public object? rawErr;

        /// <summary>
        /// Notification from server to client.
        /// </summary>
        [JsonProperty("method")]
        public string? method;

        /// <summary>
        /// Notification from server to client, event parameters, so to speak.
        /// </summary>
        [JsonProperty("params")]
        public object? evargs;
    }
#pragma warning restore
}
