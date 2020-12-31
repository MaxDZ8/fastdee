namespace fastdee.Stratum
{
    /// <summary>
    /// How to evolve the status of a stratum connection to operative and pump the various notifications.
    /// Initially, I hoped stratum would be easy enough I could just mash it together but the time for more
    /// separation has come and having a bunch of events doesn't satisfy me anymore.
    /// 
    /// This is how a <see cref="Stratificator"/> "writes" its data to consumers.
    /// </summary>
    interface ICallbacks
    {
        /// <summary>
        /// First call. Server address has been resolved, after calling this we're going to attempt a connect.
        /// </summary>
        void Connecting();

        /// <summary>
        /// Second call. Successfully connected to remote server, after this call, we're going to send "mining.subscribe".
        /// </summary>
        void Subscribing();

        /// <summary>
        /// Third call. Got something from the server for the first time in this connection.
        /// Initialize internal structures according to parameters.
        /// </summary>
        void Subscribed(Response.MiningSubscribe reply);

        /// <summary>
        /// Fourth call. Are we authorized or not?
        /// In truth, most servers automatically add you to a list if you're not in their lists.
        /// </summary>
        void Authorized(bool reply);

        /// <summary>
        /// This will (hopefully) be called after <see cref="Authorized(bool)"/> has been called,
        /// but you're better be ready for some multi-threaded galore.
        /// </summary>
        /// <param name="newDiff">Difficulty got from the message param array to be applied to next jobs.</param>
        void SetDifficulty(double newDiff);

        /// <summary>
        /// Called after <see cref="Subscribed(Response.MiningSubscribe)"/> has been called,
        /// some real magic to be done here.
        /// </summary>
        void StartNewJob(Notification.NewJob got);

        /// <summary>
        /// At any point in the process this could get called and you would need to roll back!
        /// </summary>
        void Failed();
    }
}
