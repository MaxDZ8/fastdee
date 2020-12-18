namespace fastdee
{
    /// <summary>
    /// Connect to some server with this login information.
    /// </summary>
    class ServerConnectionInfo
    {
        public readonly string poolurl;
        public readonly ushort poolport;
        public readonly string presentingAs;
        public readonly string userName, workerName, sillyPassword;


        public ServerConnectionInfo(string poolurl, ushort poolport, string presentingAs, string userName, string workerName, string sillyPassword)
        {
            this.poolurl = poolurl;
            this.poolport = poolport;
            this.presentingAs = presentingAs;
            this.userName = userName;
            this.workerName = workerName;
            this.sillyPassword = sillyPassword;
        }
    }
}
