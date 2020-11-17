using System;

namespace VocsControlHELP.util
{ 
    public class VerifyData
    {
        /// <summary>
        /// 判断字符串的是否为数字
        /// </summary>
        /// <param name="oText">源文本</param>
        /// <returns></returns>
        public static bool IsNumberic(string oText)
        {
            try
            {
                int var1 = Convert.ToInt32(oText);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
