using System;
using System.Collections.Generic;
using System.Text;

namespace Tup.SuffixTree
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Collections
    {
        public static readonly ICollection<int> EMPTY_LIST = new List<int>(0);
    }
    /// <summary>
    /// 
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Normalize an input string
        /// </summary>
        /// <param name="in">in the input string to normalize</param>
        /// <returns><tt>in</tt> all lower-case, without any non alphanumeric character</returns>
        public static string Normalize(string @in)
        {
            var @out = new StringBuilder();
            var l = @in.ToLower();
            foreach (var c in l)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    @out.Append(c);
                }
            }
            return @out.ToString();
        }
        /// <summary>
        /// Computes the set of all the substrings contained within the <tt>str</tt>
        /// It is fairly inefficient, but it is used just in tests ;)
        /// </summary>
        /// <param name="str">str the string to compute substrings of</param>
        /// <returns>the set of all possible substrings of str</returns>
        public static ISet<string> GetSubstrings(string str)
        {
            var ret = new HashSet<String>();
            for (int len = 1; len <= str.Length; ++len)
            {
                for (int start = 0; start + len <= str.Length; ++start)
                {
                    ret.Add(str.Substring(start, len));
                }
            }
            return ret;
        }

        /// <summary>
        /// Check that a string is not null or empty
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>bool</returns>
        public static bool HasValue(this string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        #region IsEmpty
        /// <summary>
        /// 指示指定的 System.String 对象是 null 还是 System.String.Empty 字符串。
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>bool</returns>
        public static bool IsEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }
        /// <summary>
        /// 指示指定类型的 数组对象是 null 或者 Length = 0。
        /// </summary>
        /// <param name="input">array to check</param>
        /// <returns>bool</returns>
        public static bool IsEmpty<T>(this T[] input)
        {
            return input == null || input.Length <= 0;
        }
        /// <summary>
        /// 指示指定类型的 数组对象是 null 或者 Length = 0。
        /// </summary>
        /// <param name="input">array to check</param>
        /// <returns>bool</returns>
        public static bool IsEmpty<T>(this ICollection<T> input)
        {
            return input == null || input.Count <= 0;
        }
        #endregion
    }
}
