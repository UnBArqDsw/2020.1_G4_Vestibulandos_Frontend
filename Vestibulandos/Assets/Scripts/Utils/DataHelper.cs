using System;

namespace Utils
{
    /// <summary>
    /// Data Operation Assistance
    /// </summary>
    public class DataHelper
    {
        /// <summary>
        /// Converts an array of strings to an Int array
        /// </summary>
        /// <param name="ss">Array of strings</param>
        /// <returns></returns>
        public static int[] StringArray2IntArray(string[] sa)
        {
            if (sa == null)
                return null;
            return StringArray2IntArray(sa, 0, sa.Length);
        }

        public static int[] StringArray2IntArray(string[] sa, int start, int count)
        {
            if (sa == null) return null;
            if (start < 0 || start >= sa.Length) return null;
            if (count <= 0) return null;
            if (sa.Length - start < count) return null;

            int[] result = new int[count];
            for (int i = 0; i < count; ++i)
            {
                string str = sa[start + i].Trim();
                str = string.IsNullOrEmpty(str) ? "0" : str;
                result[i] = Convert.ToInt32(str);
            }

            return result;
        }

        /// <summary>
        /// Secure string-to-integer conversion
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long SafeConvertToInt64(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            str = str.Trim();

            if (string.IsNullOrEmpty(str))
                return 0;

            try
            {
                return Convert.ToInt64(str);
            }
            catch (Exception)
            {
            }

            return 0;
        }

        /// <summary>
        /// Secure string to floating point conversion
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double SafeConvertToDouble(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0.0;

            str = str.Trim();

            if (string.IsNullOrEmpty(str))
                return 0.0;

            try
            {
                return Convert.ToDouble(str);
            }
            catch (Exception)
            {
            }

            return 0.0;
        }
    }
}