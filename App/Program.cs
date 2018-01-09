using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Admy.Common;
using App.db;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化数据连接
            string conn = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Persist Security Info=True","test.mdb");
            IDBHelper helper = new DBOleDBHelper(conn);
            helper.OnMessage += new OnMessageEvent(MyLog.WriteLogInfo);


            //数据表脚本（access）
            //--测试表
            //create table sys_test(
            //    st_id varchar(32) primary key,
            //    st_num int ,
            //    st_time date,		        
            //    st_username varchar(20),  
            //    st_size number,       
            //    st_content memo
            //)

            //Insert;
            Sys_test mTest = new Sys_test(helper);
            for (int i = 0; i < 10; i++)
            {
                mTest.St_num = i;
                mTest.St_username = "username_"+i;
                mTest.St_content = MyType.ToBytes("这里是文件内容"+i);
                mTest.Insert();
                Console.WriteLine("创建记录成功："+mTest.St_id);
            }

            //Query （查寻结果数据集不包括st_content字段）
            DataTable dt = mTest.Query();
            Console.WriteLine("查寻结果，记录数："+dt.Rows.Count);

            //Update
            string id = dt.Rows[0]["ST_ID"].ToString();
            mTest = new Sys_test(helper, id); //根据ID，从数据库中读取记录，创建实体类
            Console.WriteLine("记录字段内容：" + mTest.St_username);
            mTest.St_username = "new username";
            mTest.St_num = mTest.St_num + 100;
            mTest.Update();
            Console.WriteLine("更新后的字段内容：" + mTest.St_username);
            mTest = new Sys_test(helper, id);  //再次从数据库中读取
            Console.WriteLine("从数据库重新获取的字段内容：" + mTest.St_username);

            //delete
            mTest.Delete(id);
            string strsql = "select count(*) from Sys_test";
            int cnt = MyType.ToInt(helper.GetFieldValue(strsql));
            Console.WriteLine("删除后，记录数：" + cnt);

            Console.ReadKey();
        }

        
    }
}
