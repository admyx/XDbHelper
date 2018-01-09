using System;
using System.IO;

namespace Admy.Common
{
    public class MyLog
    {
        /// <summary>
        /// 保存系统出错日志，直接保存到文件中
        /// </summary>
        /// <param name="ErrorMsg"></param>
        public static void WriteLogInfo(string LogMsg)
        {
            string FilePath = AppDomain.CurrentDomain.BaseDirectory + "log";
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);
            string FileName = string.Format("{0}\\{1}.log", FilePath, DateTime.Now.ToString("yyyyMMdd"));
            writeToFile(FileName, LogMsg);  
        }

        public static void WriteExceptionLog(string funcname, Exception e, string strSource)
        {
            MyLog.WriteLogInfo(string.Format("{0}执行出错：{1}\r\n相关信息：{2}", funcname, e.Message, strSource));
        }

        #region 文件操作的私有函数

        private static bool createFile(string fileName)
        {
            try
            {
                StreamWriter sr = File.CreateText(fileName);
                sr.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool writeToFile(string fileName, string sMsg)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    createFile(fileName);
                }

                using (StreamWriter sw = File.AppendText(fileName))
                {
                    // 文本文件格式
                    sw.Write("时间：");
                    sw.WriteLine(DateTime.Now);
                    sw.WriteLine("详细信息：");
                    sw.WriteLine(sMsg);
                    sw.WriteLine("-----------------------------");

                    /*.
                    //Html文件
                    string textColorPre ="<font color=black>";
                    string fontEnd ="</font>";
                    string msgColorPre ="<font color=blue>";
                    string pEnd ="<br>";
                    sw.Write(textColorPre +"<b>时间：</b>");
                    sw.WriteLine(msgColorPre +DateTime.Now +fontEnd +pEnd);
                    sw.WriteLine(textColorPre +"<b>详细信息：</b>" +fontEnd +pEnd);
                    sw.WriteLine(msgColorPre +sMsg +fontEnd +pEnd);
                    sw.WriteLine(textColorPre +"-------------------" +fontEnd +pEnd);
                    */
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
