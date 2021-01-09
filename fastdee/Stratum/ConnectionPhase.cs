namespace fastdee.Stratum
{
    /// <summary>
    /// Quickly assert the state of a connection to a server.
    /// </summary>
    enum ConnectionPhase
    {
        /// <summary>
        /// Just created. Program start. Nothing done so far.
        /// </summary>
        Pristine,
        /// <summary>
        /// Successfully resolved server endpoint and connecting to it.
        /// </summary>
        Connecting,
        /// <summary>
        /// Connection completed, sending "mining.subscribe" and waiting for corresponding reply.
        /// </summary>
        Subscribing,
        /// <summary>
        /// Got a first server reply, sending "mining.authorize". Some servers don't send back a reply
        /// but rather go straight into notificating new work.
        /// </summary>
        Authorizing,
        /// <summary>
        /// Authorized, but no work got yet.
        /// </summary>
        Idle,
        /// <summary>
        /// Got a new job notification from server and ready to provide dispatch data to kernels.
        /// </summary>
        GotWork,

        /// <summary>
        /// Something gone awry. Usually transport interrupted. Something will happen at some point.
        /// </summary>
        Failed
    }
}
