using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer
{
    class StringUtils
    {
        /// <summary>
        /// 检测字符串是否是空白字符串.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isBlank(string str)
        {
            if (str == null || str.Trim().Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检测字符串是否是非空白字符串.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isNotBlank(string str)
        {
            return !isBlank(str);
        }

        /// <summary>
        /// 检测两个字符串是否相等, 如果两个字符串同时为null也认为是相等的.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static bool equals(string str1, string str2)
        {
            return str1 == null ? str2 == null : str1.Equals(str2);
        }

        public static string filterSymbol(string str)
        {
            if (str != null)
            {
                str = str.Replace("'", "''");
            }
            else
            {
                str = "";
            }
            return str;
        }

        #region 生成20位随机字符串
        static public string GenRnd20LenStr()
        {// 
            Byte[] bt = Guid.NewGuid().ToByteArray();
            uint bt0 = (uint)bt[0];
            ulong ulNow = (ulong)DateTime.Now.ToBinary();
            int sd = (int)bt0 + (int)ulNow;

            Random rndObj = new Random(sd);
            int rnd = rndObj.Next(); rnd = rndObj.Next();

            DateTime dtNow = DateTime.Now;
            dtNow = dtNow.AddYears(-1970);
            ulNow = (ulong)dtNow.ToBinary();

            string s1 = ulNow.ToString();
            string s2 = s1 + rndObj.Next().ToString() + rndObj.Next().ToString() + "123456";

            return s2.Substring(0, 20);
        }
        #endregion
    }
}
