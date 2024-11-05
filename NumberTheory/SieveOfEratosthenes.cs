using System.Collections.ObjectModel;

namespace NumberTheory;

public static class SieveOfEratosthenes
{
    public static readonly object Locker = new object();

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a sequential approach.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersSequentialAlgorithm(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
        Collection<int> candidates = [];
        bool[] prime = new bool[n + 1];

        prime = PopulateArray(prime);

        for (int p = 2; p * p <= n; p++)
        {
            if (prime[p])
            {
                for (int i = p * p; i <= n; i += p)
                {
                    prime[i] = false;
                }
            }
        }

        for (int i = 2; i <= n; i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }

        return candidates;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a modified sequential approach.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersModifiedSequentialAlgorithm(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
        Collection<int> candidates = [];

        bool[] prime = new bool[n + 1];

        for (int i = 0; i <= n; i++)
        {
            prime[i] = true;
        }

        for (int i = 3; i <= (int)Math.Sqrt(n); i++)
        {
            if (i % 2 == 0)
            {
                prime[i] = false;
            }
        }

        for (int p = 3; p <= (int)Math.Sqrt(n); p++)
        {
            if (prime[p])
            {
                for (int i = p * p; i <= (int)Math.Sqrt(n); i += p)
                {
                    prime[i] = false;
                }
            }
        }

        candidates = ConvertToList(2, (int)Math.Sqrt(n), prime, candidates);

        /*
        for (int i = 2; i <= (int)Math.Sqrt(n); i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        for (int i = (int)Math.Sqrt(n) + 1; i <= n; i++)
        {
            foreach (int prim in candidates)
            {
                if (i % prim == 0)
                {
                    prime[i] = false;
                    break;
                }
            }
        }

        candidates = ConvertToList((int)Math.Sqrt(n) + 1, n, prime, candidates);

        /*
        for (int i = (int)Math.Sqrt(n) + 1; i <= n; i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        return candidates;
    }

    public static bool[] PopulateArray(bool[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = true;
        }

        return array;
    }

    public static Collection<int> ConvertToList(int from, int to, bool[] array, Collection<int> result)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentNullException.ThrowIfNull(result);
        for (int i = from; i <= to; i++)
        {
            if (array[i])
            {
                result.Add(i);
            }
        }

        return result;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a concurrent approach by data decomposition.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentDataDecomposition(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
        Collection<int> candidates = [];

        bool[] prime = new bool[n + 1];

        prime = PopulateArray(prime);

        for (int i = 3; i <= (int)Math.Sqrt(n); i++)
        {
            if (i % 2 == 0)
            {
                prime[i] = false;
            }
        }

        for (int p = 3; p <= (int)Math.Sqrt(n); p++)
        {
            if (prime[p])
            {
                for (int i = p * p; i <= (int)Math.Sqrt(n); i += p)
                {
                    prime[i] = false;
                }
            }
        }

        candidates = ConvertToList(2, (int)Math.Sqrt(n), prime, candidates);

        /*
        for (int i = 2; i <= (int)Math.Sqrt(n); i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        int partTwoStartItem = (int)Math.Sqrt(n) + 1;

        int remainingItems = n - partTwoStartItem;
        int partitionSize = remainingItems / 3;

        Thread thread1Opt = new Thread(() => FindPrimes(partTwoStartItem, partTwoStartItem + partitionSize, candidates, prime));
        Thread thread2Opt = new Thread(() => FindPrimes(partTwoStartItem + partitionSize, partTwoStartItem + (2 * partitionSize), candidates, prime));
        Thread thread3Opt = new Thread(() => FindPrimes(partTwoStartItem + (2 * partitionSize), n + 1, candidates, prime));
        thread1Opt.Start();
        thread2Opt.Start();
        thread3Opt.Start();

        thread1Opt.Join();
        thread2Opt.Join();
        thread3Opt.Join();

        static void FindPrimes(int from, int to, Collection<int> dividers, bool[] primes)
        {
            for (int i = from; i < to; i++)
            {
                foreach (int prim in dividers)
                {
                    if (i % prim == 0)
                    {
                        lock (Locker)
                        {
                            primes[i] = false;
                        }

                        break;
                    }
                }
            }
        }

        candidates = ConvertToList(partTwoStartItem, n, prime, candidates);

        /*
        for (int i = partTwoStartItem; i <= n; i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        return candidates;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a concurrent approach by "basic" primes decomposition.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentBasicPrimesDecomposition(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
        Collection<int> candidates = [];
        bool[] prime = new bool[n + 1];

        prime = PopulateArray(prime);

        for (int i = 3; i <= (int)Math.Sqrt(n); i++)
        {
            if (i % 2 == 0)
            {
                prime[i] = false;
            }
        }

        for (int p = 3; p <= (int)Math.Sqrt(n); p++)
        {
            if (prime[p])
            {
                for (int i = p * p; i <= (int)Math.Sqrt(n); i += p)
                {
                    prime[i] = false;
                }
            }
        }

        candidates = ConvertToList(2, (int)Math.Sqrt(n), prime, candidates);

        /*
        for (int i = 2; i <= (int)Math.Sqrt(n); i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        List<int> dividerZeroes = [];
        List<int> dividerOnes = [];
        List<int> dividerTwos = [];

        for (int i = 0; i < candidates.Count; i++)
        {
            switch (i % 3)
            {
                case 0: dividerZeroes.Add(candidates[i]); break;
                case 1: dividerOnes.Add(candidates[i]); break;
                case 2: dividerTwos.Add(candidates[i]); break;
                default: break;
            }
        }

        Thread thread1Opt = new Thread(() => FindPrimesBasic(dividerZeroes, prime));
        Thread thread2Opt = new Thread(() => FindPrimesBasic(dividerOnes, prime));
        Thread thread3Opt = new Thread(() => FindPrimesBasic(dividerTwos, prime));
        thread1Opt.Start();
        thread2Opt.Start();
        thread3Opt.Start();

        thread1Opt.Join();
        thread2Opt.Join();
        thread3Opt.Join();

        static void FindPrimesBasic(List<int> dividers, bool[] primes)
        {
            int partTwoStartItem = (int)Math.Sqrt(primes.Length - 1) + 1;
            for (int i = partTwoStartItem; i < primes.Length; i++)
            {
                foreach (int prim in dividers)
                {
                    if (i % prim == 0)
                    {
                        lock (Locker)
                        {
                            primes[i] = false;
                        }

                        break;
                    }
                }
            }
        }

        int partTwoStartItem = (int)Math.Sqrt(n) + 1;
        candidates = ConvertToList(partTwoStartItem, n, prime, candidates);
        /*
        for (int i = partTwoStartItem; i <= n; i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        return candidates;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using thread pool and signaling construct.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentWithThreadPool(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
        Collection<int> candidates = [];
        bool[] prime = new bool[n + 1];

        prime = PopulateArray(prime);

        for (int i = 3; i <= (int)Math.Sqrt(n); i++)
        {
            if (i % 2 == 0)
            {
                prime[i] = false;
            }
        }

        for (int p = 3; p <= (int)Math.Sqrt(n); p++)
        {
            if (prime[p])
            {
                for (int i = p * p; i <= (int)Math.Sqrt(n); i += p)
                {
                    prime[i] = false;
                }
            }
        }

        candidates = ConvertToList(2, (int)Math.Sqrt(n), prime, candidates);

        /*
        for (int i = 2; i <= (int)Math.Sqrt(n); i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        int partTwoStartItem = (int)Math.Sqrt(prime.Length - 1) + 1;

        using (var countdownEvent = new CountdownEvent(candidates.Count))
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                int temp = i;
                _ = ThreadPool.QueueUserWorkItem(
                    x =>
                    {
                        for (int j = partTwoStartItem; j < prime.Length; j++)
                        {
                            if (j % candidates[temp] == 0)
                            {
                                lock (Locker)
                                {
                                    prime[j] = false;
                                }
                            }
                        }

                        _ = countdownEvent.Signal();
                    });
            }

            countdownEvent.Wait();
        }

        candidates = ConvertToList(partTwoStartItem, n, prime, candidates);
        /*
        for (int i = partTwoStartItem; i <= n; i++)
        {
            if (prime[i])
            {
                candidates.Add(i);
            }
        }
        */

        return candidates;
    }
}
