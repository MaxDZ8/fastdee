using fastdee.Stratum.Notification;
using fastdee.Stratum.Response;
using System;
using System.Linq; // .ToList()

namespace fastdee.Stratum
{
    /// <summary>
    /// Collect stimuli from the stratum recoverable pump and turn them in something we can consume easily.
    /// 
    /// I initially wanted to just bind all the things toghether but doing something more is too tempting!
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
            internal readonly WorkGenerator workMaker = new WorkGenerator();
            internal readonly HeaderGenerator headerGenerator;
            internal readonly IDifficultyCalculation diffCalc;

            public Delicate(HeaderGenerator headerGenerator, IDifficultyCalculation diffCalc)
            {
                this.headerGenerator = headerGenerator;
                this.diffCalc = diffCalc;
            }
        }
        readonly Delicate careful;

        internal Connector(HeaderGenerator headerMaker, IDifficultyCalculation diffCalc)
        {
            careful = new Delicate(headerMaker, diffCalc);
        }

        public void StartingNonce(uint nonceStart)
        {
            lock (careful) careful.workMaker.NextNonce(nonceStart);
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
                careful.headerGenerator.NonceSettings(reply.extraNonceOne, reply.extraNonceTwoByteCount);
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
                careful.diffCalc.Set(newDiff);
                careful.workMaker.SetTarget(careful.diffCalc.DifficultyTarget);
            }
        }

        public void StartNewJob(NewJob job)
        {
            Console.WriteLine($"OK Job={job.jobid}, flushing={job.flush}");
            lock (careful)
            {
                careful.headerGenerator.NewJob(job);
                careful.phase = ConnectionPhase.GotWork;
                var nonce2 = careful.headerGenerator.CopyNonce2();
                var tracking = new ShareSubmitInfo(job.jobid, nonce2, job.ntime);
                var persist = careful.headerGenerator.Header.ToArray();
                careful.workMaker.SetHeader(tracking, persist);
            }

        }

        internal Work? GenWork(uint nonceRange)
        {
            lock (careful)
            {
                if (careful.phase != ConnectionPhase.GotWork) return null;
                return careful.workMaker.WannaConsume(nonceRange); // TODO: seems like a good place for n2 roll
            }
        }

        public void Failed()
        {
            lock (careful)
            {
                careful.phase = ConnectionPhase.Failed;
                careful.headerGenerator.Stop();
            }
        }
    }
}
