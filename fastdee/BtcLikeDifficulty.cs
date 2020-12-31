using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee
{
    class BtcLikeDifficulty : IDifficultyCalculation
    {
        double difficulty;
        DifficultyTarget target = new DifficultyTarget();

        readonly double stratumFactor;
        readonly double algoFactor;

        internal BtcLikeDifficulty(double stratumFactor, double algoFactor)
        {
            this.stratumFactor = stratumFactor;
            this.algoFactor = algoFactor;
        }

        public void Set(double diff)
        {
            if (diff <= 0.0) throw new ArgumentException("zero or less", nameof(diff));
            if (difficulty == diff) return;
            Console.WriteLine($"Changing diff {difficulty} -> {diff}");
            diff *= stratumFactor;
            target = BtcLikeDiffTarget(diff, algoFactor);
            difficulty = diff;
        }

        public DifficultyTarget DifficultyTarget => target;

        /// <summary>
        /// Difficulty target bits calculation used by most coins.
        /// </summary>
        /// <remarks>
        /// Before you wonder, the donor project here is M8M, AbstractWorkSource.cpp::186.
        /// MIT license.
        /// </remarks>
        static DifficultyTarget BtcLikeDiffTarget(double diff, double diffOneMul)
        {
            /*
            Ok, there's this constant, "truediffone" which is specified as a 256-bit value
            0x00000000FFFF0000000000000000000000000000000000000000000000000000
                          |------------------- 52 zeros --------------------|
            So it's basically aushort(0xFFFF) << (52 * 4)
            Or: 65535 * ... 2^208?
            Legacy miners have those values set up, so they can go use double-float division to effectively
            expand the bit representation and select the bits they care. By using multiple passes, they pull
            out successive ranges of reductions. They use the following constants:
            truediffone = 0x00000000FFFF0000000000000000000000000000000000000000000000000000
            bits192     = 0x0000000000000001000000000000000000000000000000000000000000000000
            bits128     = 0x0000000000000000000000000000000100000000000000000000000000000000
            bits64      = 0x0000000000000000000000000000000000000000000000010000000000000000
            Because all those integers have a reduced range, they can be accurately represented by a double.
            See diffCalc.html for a large-integer testing framework. */
            const double BITS_192 = 6277101735386680763835789423207666416102355444464034512896.0;
            const double BITS_128 = 340282366920938463463374607431768211456.0;
            const double BITS_64 = 18446744073709551616.0;

            double big = (diffOneMul * TRUE_DIFF_ONE) / diff;
            Span<double> k = stackalloc double[4] { BITS_192, BITS_128, BITS_64, 1 };
            Span<ulong> target = stackalloc ulong[4];
            for (var loop = 0; loop < 4; loop++)
            {
                double partial = big / k[loop];
                ulong magic = (ulong)partial;
                target[4 - loop - 1] = magic;
                // ^ note: both legacy and M8M here force this little endian. I don't.
                // ^ Endianess is something to care about when mix-matching with byte sequences but I plan to stay away this time.
                partial = magic * k[loop];
                big -= partial;
            }
            return new DifficultyTarget()
            {
                ShareDiff = diff,
                TargA = target[0],
                TargB = target[1],
                TargC = target[2],
                TargD = target[3]
            };
        }

        const double TRUE_DIFF_ONE = 26959535291011309493156476344723991336010898738574164086137773096960.0;
    }
}
