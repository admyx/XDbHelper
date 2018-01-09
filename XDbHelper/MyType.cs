using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Admy.Common
{
    public class MyType
    {
        #region Integer

        public static bool IsInt(string str)
        {
            int value = 0;
            return int.TryParse(str, out value);
        }

        public static int ToInt(string str)
        {
            return ToInt(str, 0);
        }

        public static int ToInt(string str, int defaultValue)
        {
            int value = defaultValue;
            bool parseOk = int.TryParse(str, out value);
            return parseOk ? value : defaultValue;
        }

        #endregion

        #region Double

        public static bool IsDouble(string str)
        {
            Double value = 0;
            return Double.TryParse(str, out value);
        }

        public static Double ToDouble(string str)
        {
            return ToDouble(str, 0);
        }

        public static Double ToDouble(string str, Double defaultValue)
        {
            Double value = defaultValue;
            bool parseOk = Double.TryParse(str, out value);
            return parseOk ? value : defaultValue;
        }

        #endregion

        #region DateTime

        public static bool IsDateTime(string str)
        {
            DateTime value = DateTime.Now;
            return DateTime.TryParse(str, out value);
        }

        public static DateTime ToDateTime(string str)
        {
            return ToDateTime(str, DateTime.Now);
        }

        public static DateTime ToDateTime(string str, DateTime defaultValue)
        {
            DateTime value = defaultValue;
            bool parseOk = DateTime.TryParse(str, out value);
            return parseOk ? value : defaultValue;
        }

        #endregion

        #region byte[]/string

        /// <summary>
        /// 将字符串转换成二进制
        /// </summary>
        /// <param name="str">待转换的字符串</param>
        /// <returns></returns>
        public static byte[] ToBytes(string str)
        {
            //return Encoding.GetEncoding("gb2312").GetBytes(str);
            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// 将字节流转换成二进制
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ToBytes(System.IO.Stream theStream)
        {
            byte[] tmpBuffer = new byte[theStream.Length];
            theStream.Read(tmpBuffer, 0, (int)theStream.Length);
            theStream.Close();
            return tmpBuffer;
        }

        /// <summary>
        /// 将二进制转换成字符串
        /// </summary>
        /// <param name="bytes">待转换的二进制</param>
        /// <returns></returns>
        public static string ToString(byte[] bytes)
        {
            //return Encoding.GetEncoding("gb2312").GetString(bytes);
            return Encoding.UTF8.GetString(bytes);
        }
        /// <summary>
        /// 将二进制转换成字符串
        /// </summary>
        /// <param name="bytes">待转换的二进制</param>
        /// <returns></returns>
        public static string ToString(byte[] bytes, int index, int count)
        {
            //return Encoding.GetEncoding("gb2312").GetString(bytes, index, count);
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        /// <summary>
        /// 将二进制数组转换成16进制字符串进行显示
        /// </summary>
        /// <param name="bytes">二进制数组</param>
        /// <param name="bytecountPerRow">每行显示的字节数</param>
        /// <param name="showChar">是否在每一行后面显示二进制字节对应的可见字符</param>
        /// <returns>字符串结果（多行）</returns>
        public static string ToHexString(byte[] bytes, int bytecountPerRow, bool showChar)
        {
            StringBuilder sb = new StringBuilder();
            string str1 = "";
            string str2 = "";
            for (int i = 0; i <= bytes.Length; i++)
            {
                if ((i % bytecountPerRow == 0) || (i == bytes.Length))
                {
                    if (str1 != "")
                    {
                        if (showChar)
                        {
                            str1 = str1.PadRight(3 * bytecountPerRow);
                            str1 += "\t" + str2;
                        }
                        sb.AppendLine(str1);
                        str1 = "";
                        str2 = "";
                    }
                    if (i == bytes.Length)
                        break;
                }

                str1 += string.Format(" {0:X2}", bytes[i]);
                if (('a' <= bytes[i] && bytes[i] <= 'z') || ('A' <= bytes[i] && bytes[i] <= 'Z') || ("~!@#$%^&*()_+<>?,./:\"\\\'`1234567890".IndexOf((char)bytes[i]) > -1))
                    str2 += (char)bytes[i];
                else
                    str2 += ".";
            }
            return sb.ToString();
        }

        #endregion

        #region 图片/byte[]

        ///// <summary>
        /////  ImageToByte(Image img) 将图片转换成二进制代码，然后存储数据库中
        ///// </summary>
        ///// <param name="img"></param>
        ///// <returns></returns>
        //public static byte[] ImageToByte(System.Net.Mime.MediaTypeNames.Image img)
        //{
        //    byte[] byt = null;
        //    ImageConverter imgCvt = new ImageConverter();
        //    object obj = imgCvt.ConvertTo(img, typeof(byte[]));
        //    byt = (byte[])obj;
        //    return byt;
        //}

        ///// <summary>
        /////  ByteToImage(byte[] byt) 读取数据库中的二进制文件将其转换为Image  
        ///// </summary>
        ///// <param name="bytImage"></param>
        ///// <returns></returns>
        //public static Image ByteToImage(byte[] bytImage)
        //{
        //    Image img = null;
        //    ImageConverter imgCvt = new ImageConverter();

        //    object obj = imgCvt.ConvertFrom(bytImage);
        //    img = (Image)obj;
        //    return img;
        //}

        #endregion

        #region 字符串特殊处理

        #region 将字符串转换成 Html 语言模式

        /// <summary>
        /// 将字符串转换成 Html 语言模式
        /// </summary>
        /// <param name="ResStr">原字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string StrToHtml(string ResStr)
        {
            if (ResStr == null) 
                return "";
            if (ResStr == "")
                return "";

            string tmpstr = ResStr;

            tmpstr = tmpstr.Replace("<", "&lt;");
            tmpstr = tmpstr.Replace(">", "&gt;");
            tmpstr = tmpstr.Replace("\n", "<br>");
            tmpstr = tmpstr.Replace("  ", "&nbsp; ");

            return tmpstr;
        }

        #endregion

        #region TrimChineseStr(DB层用）

        /// <summary>
        /// 按照给定的长度，截除字符串(汉字当两个符处理)
        /// </summary>
        /// <param name="theValue">给定的字符串</param>
        /// <param name="Length">给定的长度限制</param>
        /// <returns>处理结果</returns>
        public static string TrimChineseStr(string theValue, int Length)
        {
            string str = theValue;
            if (theValue != null && theValue != "")
            {
                string tmpStr = theValue;
                int mi = 0;
                int ml = 0;
                foreach (char mc in tmpStr.ToCharArray())
                {
                    //if ((long)mc > 383) //对±的判断失误，2015/3/10
                    if ((long)mc > 127) 
                        ml += 2;
                    else
                        ml += 1;

                    if (ml > Length)
                        break;
                    mi += 1;
                }
                if (ml > Length)
                    str = tmpStr.Substring(0, mi);
            }
            return str;
        }

        #endregion

        #region value2string(DB层用）

        public static string value2string(object obj)
        {
            if (obj.GetType() == typeof(int))
            {
                return obj.ToString();
            }
            else
                return "'" + obj.ToString() + "'";
        }

        #endregion

        #endregion
    }
}
