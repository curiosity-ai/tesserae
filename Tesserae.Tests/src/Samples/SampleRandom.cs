using System;
using System.Text;

namespace Tesserae.Tests.Samples
{
    /// <summary>
    /// A seeded random number generator for the samples. Samples that fake data (uptime history,
    /// metrics, card heights, chat replies, node ids) used to call <c>new Random()</c>,
    /// <c>Math.Random()</c> or <c>Guid.NewGuid()</c>, so every reload rendered something different
    /// and any two captures of the gallery differed for reasons that had nothing to do with the
    /// change under test. Each sample owns a <see cref="SampleRandom"/> with a fixed seed instead,
    /// so a page renders the same numbers on every run and a text or pixel diff only shows real
    /// differences.
    /// </summary>
    /// <remarks>
    /// It is a thin wrapper over a seeded <see cref="Random"/> rather than its own generator:
    /// Transpose's <c>Random</c> is a faithful port of the .NET one, sequence-compatible value for
    /// value, so a seed is all determinism takes. What this type adds is that a seed is the *only*
    /// way to construct it, a name that says why, and the few helpers the samples need.
    /// <para>
    /// Each sample passes its own seed so two samples on screen do not show the same shape of data,
    /// and so the sequence a page sees does not depend on which pages were opened before it. The
    /// seed value itself carries no meaning beyond "always the same".
    /// </para>
    /// </remarks>
    public sealed class SampleRandom
    {
        /// <summary>The seed used when a sample does not care to pick one.</summary>
        public const int DefaultSeed = 20240613;

        private const string HexDigits = "0123456789abcdef";

        private readonly int    _seed;
        private          Random _random;

        /// <param name="seed">Any integer, as long as it is a constant.</param>
        public SampleRandom(int seed = DefaultSeed)
        {
            _seed   = seed;
            _random = new Random(seed);
        }

        /// <summary>Rewinds to the seed, so the next call starts the same sequence over again.</summary>
        public void Reset() => _random = new Random(_seed);

        /// <summary>Returns a value in [0, 1).</summary>
        public double NextDouble() => _random.NextDouble();

        /// <summary>Returns a value in [minInclusive, maxExclusive).</summary>
        public double NextDouble(double minInclusive, double maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            return minInclusive + NextDouble() * (maxExclusive - minInclusive);
        }

        /// <summary>Returns an integer in [0, maxExclusive).</summary>
        public int Next(int maxExclusive) => maxExclusive <= 0 ? 0 : _random.Next(maxExclusive);

        /// <summary>Returns an integer in [minInclusive, maxExclusive).</summary>
        public int Next(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            return _random.Next(minInclusive, maxExclusive);
        }

        /// <summary>Picks one of the items, or the type's default when the array is empty.</summary>
        public T Pick<T>(T[] items)
        {
            if (items == null || items.Length == 0) return default(T);
            return items[Next(items.Length)];
        }

        /// <summary>
        /// A stand-in for <c>Guid.NewGuid().ToString()</c>: an id of the same shape
        /// (<c>8-4-4-4-12</c> lowercase hex) drawn from this generator, so a sample that shows its
        /// own ids — the NodeView sample prints its state as JSON — shows the same ones every run.
        /// Unique within a sequence, not universally unique; these ids never leave the page.
        /// </summary>
        public string NextId()
        {
            var id = new StringBuilder(36);

            for (var i = 0; i < 32; i++)
            {
                if (i == 8 || i == 12 || i == 16 || i == 20) id.Append('-');
                id.Append(HexDigits[Next(16)]);
            }

            return id.ToString();
        }
    }
}
