using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPN
{
    public class RPNCalculator
    {

        private List<int> rpnCache;
        ///Any RPN must end in 2, 3 , 5, 7 so e can use these as a basis
        private int[] endPrimes = new int[4] { 2, 3, 5, 7 };

        /// <summary>
        /// Calculate the Robustly Prime Number ata given possible index
        /// </summary>
        /// <param name="input">Argument convertable to n where 1 ≤ n ≤ 2209</param>
        /// <param name="testCache">Whether to run a check on the calculated values at the end</param>
        /// <returns>RNP at index</returns>
        public string CalculateRPN(string input, bool testCache)
        {
            try
            {
                //mark the time to add a time taken to output
                Int64 startMS = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                int output = 0;
                if (int.TryParse(input, out int RPNindex))
                {
                    output = CalculateRPN(RPNindex, testCache);
                }
                else
                {
                    return string.Format("Could not parse '{0}' as int", input);
                }
                
                //Time build took to run.
                Int64 endMS = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startMS;

                //Full success message
                return string.Format("RPN index of {0} has a value of {1}, calculated in {2}ms", RPNindex, output, endMS);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Calculate the Robustly Prime Number ata given possible index
        /// </summary>
        /// <param name="RPNindex">integer n where 1 ≤ n ≤ 2209</param>
        /// <param name="testCache">Whether to run a check on the calculated values at the end</param>
        /// <returns>RNP at index</returns>
        public int CalculateRPN(int RPNindex, bool testCache)
        {
            if (RPNindex < 1 || RPNindex > 2209)
                throw new Exception(string.Format("Invalid RPN index of {0}, value must be and integer n where 1 <= n <= 2209", RPNindex));


            PopulateRPNCache(testCache);

            int output = 0;
            if (rpnCache.Count >= RPNindex)
                output = rpnCache[RPNindex - 1];

            return output;
        }

        /// <summary>
        /// Add All RPN up toi 2^31 to rpnCache
        /// </summary>
        /// <param name="test">Whether to run a check on the calculated values at the end</param>
        private void PopulateRPNCache(bool test)
        {
            rpnCache = new List<int>();
            //The end primes are the list of known primes < 10 that wwe can start with
            rpnCache.AddRange(endPrimes);
            foreach (int endPrim in endPrimes)
            {
                BuildRPN(endPrim.ToString());
            }

            //Option to run RPN Test on all the values, shouldnt be necessary, but can be used for debugging incorrect indexes.
            if (test)
            {
                foreach (int val in rpnCache)
                {
                    if (!IsRPN(val))
                        throw new Exception(string.Format("{0} in not a valid Robustly Prime Number but has been added to the cache", val));
                }
            }

            //Ordered values in the generated list then become our indexed wway of getting values for display
            rpnCache = rpnCache.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Take a known prime val
        /// </summary>
        /// <param name="RPNValue"></param>
        private void BuildRPN(string RPNValue)
        {

            //Take a known to be Prime value such as 13
            //Since we know that part is prime, we only need to check if [1-9]13 are prime
            //So we loop through 1-9 adding the it to value and testing for primeness
            // e.g. 1 + 13 = 113 -> Prime
            //      2 + 13 = 213 -> Not Prime
            //
            //We can then ignore ......213 as no numbers that end in that can be RPN
            //We add 113 to the list of all RPN numbers and pass it recursivly into this function to check [1-9]113

            //Process can be run in parallel to speed up the build
            Parallel.For(1, 5, i =>
            {
                string testVal = i.ToString() + RPNValue;
                //Value will fail parse over 2^31 so that cats as our limit.
                if (int.TryParse(testVal, out int newval))
                {
                    if (IsPrime(newval))
                    {
                        //Threaded process to add to a list means we need to lock the list 1st
                        lock (rpnCache)
                        {
                            rpnCache.Add(newval);
                        }
                        BuildRPN(testVal);
                    }
                }
            });
        }

        /// <summary>
        /// 'Slow' function designed to test if a value is RPN
        /// </summary>
        /// <param name="RPN">Input number to check if its a Robustly Prime Number</param>
        /// <returns></returns>
        private bool IsRPN(int RPN)
        {
            //Use the inherant NPR checks 1st, before looping through the value
            if (Contains0(RPN))
                return false;
            //Make sure the last value is prime
            if (!EndsPrime(RPN))
                return false;

            //Loop through the rightmost parts of the value with an increasing start indexed substring and check for primeness on each one
            //if any parts fail then the input isnt RPN
            // 5167 -> 167 -> 67 -> 7 = Each part is prime, so result is RPN
            // 2179 -> 179 -> 79 -> 9 = Not all parts are prime, so not RPN
            string rpnString = RPN.ToString();
            bool prime = true;
            for (int i = 0; i < rpnString.Length; i++)
            {
                if (!IsPrime(int.Parse(rpnString.Substring(i))))
                {
                    //cancel out loop on 1st instance of a non prime number
                    prime = false;
                    break;
                }
            }

            return prime;
        }

        /// <summary>
        /// Check whether a value is prime, fast
        /// </summary>
        /// <returns></returns>
        private bool IsPrime(int value)
        {
            // we arent treating 1 as prime and negatives cant be prime.
            if (value <= 1) return false;
            // treat 2 as prime, even though its divisible by 2
            if (value == 2) return true;
            //Value divisible by 2 so can be excluded from our check (except 2)
            if (value % 2 == 0) return false;

            //Get the Root of the value we want to check,
            //so we can check for 'primeness' with as small list of numbers as possible as the root would be the highest multiple.
            var sqr = (int)Math.Sqrt(value);

            //Loop through all of all ints up to the root 
            for (int i = 3; i <= sqr; i += 2) // advance by 2 so we skip even numbers, dont need to check them.
            {
                //If the modulus leaves no value, then its not prime
                if (value % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Exclude values that contain 0 in them.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool Contains0(int value)
        {
            return value.ToString().Contains("0");
        }

        /// <summary>
        /// Check that the final value ends in one of the base prime numbers
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool EndsPrime(int value)
        {
            ///Any RPN must end in 2, 3 , 5, 7
            return endPrimes.Contains((int)(value % 10));
        }
    }
}
