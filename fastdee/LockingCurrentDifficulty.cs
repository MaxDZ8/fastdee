using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee
{
    /// <summary>
    /// Generate a <see cref="ICurrentDifficulty"/> and pass it to me.
    /// I will own it and use it as a lock to protect it from concurrent access.
    /// </summary>
    class LockingCurrentDifficulty : ICurrentDifficulty
    {
        readonly ICurrentDifficulty realDeal;

        internal LockingCurrentDifficulty(ICurrentDifficulty gimme)
        {
            realDeal = gimme;
        }
        public DifficultyTarget DifficultyTarget
        {
            get
            {
                lock (realDeal) return realDeal.DifficultyTarget;
            }
        }

        public void Set(double difficulty)
        {
            lock (realDeal) realDeal.Set(difficulty);
        }
    }
}
