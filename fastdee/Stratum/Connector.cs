using fastdee.Stratum.Notification;
using fastdee.Stratum.Response;
using System;

namespace fastdee.Stratum
{
    /// <summary>
    /// Collect stimuli from the stratum recoverable pump and turn them in something we can consume easily.
    /// </summary>
    class Connector : ICallbacks
    {
        class Delicate
        {
            internal ConnectionPhase phase;
            /// <summary>
            /// Note: alive is meant to go true as long as we get at least a reply from the server we connected.
            /// It might be even at a previous connection, if there has never been something here replying correctly
            /// then we can be more aggressive if connection fails.
            /// </summary>
            internal bool alive;
        }
        readonly Delicate careful = new Delicate();
        readonly HeaderGenerator workGenerator;
        readonly IDifficultyCalculation diffCalc;
        DifficultyTarget target = new DifficultyTarget();

        internal Connector(HeaderGenerator workGenerator, IDifficultyCalculation diffCalc)
        {
            this.workGenerator = workGenerator;
            this.diffCalc = diffCalc;
        }

        public void Connecting()
        {
            lock (careful) careful.phase = ConnectionPhase.Connecting;
        }

        public void Subscribing()
        {
            lock (careful) careful.phase = ConnectionPhase.Subscribing;
        }

        public void Subscribed(MiningSubscribe reply)
        {
            lock (careful)
            {
                careful.alive = true;
                careful.phase = ConnectionPhase.Authorizing;
                workGenerator.NonceSettings(reply.extraNonceOne, reply.extraNonceTwoByteCount);
            }
        }

        public void Authorized(bool reply)
        {
            lock (careful)
            {
                careful.phase = ConnectionPhase.Idle;
                if (reply) Console.WriteLine("OK Authorized");
                else
                {
                    Console.WriteLine("ERR Refused auth");
                    throw new BadStratumAuthException();
                }
            }
        }

        public void SetDifficulty(double newDiff)
        {
            lock (careful)
            {
                diffCalc.Set(newDiff);
                target = diffCalc.DifficultyTarget;
            }
        }

        public void StartNewJob(NewJob newJob)
        {
            Console.WriteLine($"OK Job={newJob.jobid}, flushing={newJob.flush}");
            lock (careful)
            {
                workGenerator.NewJob(newJob);
                careful.phase = ConnectionPhase.GotWork;
            }

        }

        public void Failed()
        {
            lock (careful)
            {
                careful.phase = ConnectionPhase.Failed;
                workGenerator.Stop();
            }
        }
    }
}
