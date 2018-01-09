//Code auto generated by dbcoder V20140304.1010
//template version: 20171218.1030

using System;
using System.Data;
using System.Collections;
using Admy.Common;

namespace App.db
{
    /// <summary>
    /// 测试表
    /// </summary>
    [Serializable]
    public class Sys_test : BaseTable<string>
    {
        #region ITable 成员

        public override string GetTableName()
        {
            return "SYS_TEST";
        }

        public override string GetPrimayField()
        {
            return "ST_ID";
        }

        public override string GetFieldList()
        {
            return "ST_ID,ST_NUM,ST_TIME,ST_USERNAME,ST_SIZE,ST_CONTENT";
        }

        public override string GetFieldListWithoutBlob()
        {
            return "ST_ID,ST_NUM,ST_TIME,ST_USERNAME,ST_SIZE";
        }

        #endregion

        #region 私有变量

        private string _St_id;	//
        private int _St_num;	//测试
        private DateTime _St_time;	//登录时间
        private string _St_username;	//姓名
        private double _St_size;	//正文大小
        private byte[] _St_content;	//正文内容

        #endregion

        #region 公开属性

        /// <summary>
        /// 
        /// </summary>
        public string St_id
        {
            get { return _St_id; }
        }

        /// <summary>
        /// 测试
        /// </summary>
        public int St_num
        {
            get { return _St_num; }
            set { _St_num = value; htFields["ST_NUM"] = _St_num; }
        }

        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime St_time
        {
            get { return _St_time; }
            set { _St_time = value; htFields["ST_TIME"] = _St_time; }
        }

        /// <summary>
        /// 姓名
        /// </summary>
        public string St_username
        {
            get { return _St_username; }
            set { _St_username = MyType.TrimChineseStr(value, 20); htFields["ST_USERNAME"] = _St_username; }
        }

        /// <summary>
        /// 正文大小
        /// </summary>
        public double St_size
        {
            get { return _St_size; }
            set { _St_size = value; htFields["ST_SIZE"] = _St_size; }
        }

        /// <summary>
        /// 正文内容
        /// </summary>
        public byte[] St_content
        {
            get { return _St_content; }
            set { _St_content = value; htFields["ST_CONTENT"] = _St_content; }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造一个Sys_test类,ID号置为空
        /// </summary>
        /// <param name="helper">dbhelper</param>
        public Sys_test(IDBHelper helper)
            : base(helper)
        {
        }

        /// <summary>
        /// 构造一个Sys_test类，按指定ID从数据库中取数据进行填充
        /// 如果数据库中不存在指定ID的记录，则指定类ID置为空
        /// </summary>
        /// <param name="helper">dbhelper</param>
        /// <param name="id">数据ID号</param>
        public Sys_test(IDBHelper helper, string id)
            : base(helper, id)
        {
        }

        /// <summary>
        /// 根据数据行构造一个Sys_test类
        /// </summary>
        /// <param name="helper">dbhelper</param>
        /// <param name="datarow">数据行</param>
        public Sys_test(IDBHelper helper, DataRow dr)
            : base(helper, dr)
        {
        }

        /// <summary>
        /// 直接从内存datatable中按指定的ID检索结果构造一个Sys_test类
        /// 如果数据库中不存在指定ID的记录，则指定类ID置为空
        /// </summary>
        /// <param name="helper">dbhelper</param>
        /// <param name="dt">数据表</param>
        /// <param name="id">数据ID号</param>
        public Sys_test(IDBHelper helper, DataTable dt, string id)
            : base(helper, dt, id)
        {
        }

        #endregion

        #region 虚方法实现

        protected override void __SetID(string id)
        {
            this._St_id = id;
            this.htFields[this.GetPrimayField()] = _St_id;
        }

        protected override void __Init()
        {
            #region 数据初始化

            _St_id = string.Empty;
            _St_num = 1;
            _St_time = DateTime.Now;
            _St_username = string.Empty;
            _St_size = 0;
            _St_content = new byte[0];

            #endregion
        }

        protected override void __Bind(DataRow dr)
        {
            #region 从数据库中读取

            if (dr != null)
            {
                try
                {
                    _St_id = (dr["ST_ID"] != DBNull.Value) ? dr["ST_ID"].ToString() : string.Empty;
                    _St_num = (dr["ST_NUM"] != DBNull.Value) ? Convert.ToInt32(dr["ST_NUM"]) : 1;
                    _St_time = (dr["ST_TIME"] != DBNull.Value) ? Convert.ToDateTime(dr["ST_TIME"]) : DateTime.Now;
                    _St_username = (dr["ST_USERNAME"] != DBNull.Value) ? dr["ST_USERNAME"].ToString() : string.Empty;
                    _St_size = (dr["ST_SIZE"] != DBNull.Value) ? Convert.ToDouble(dr["ST_SIZE"]) : 0;
                    _St_content = (dr["ST_CONTENT"] != DBNull.Value) ? (Byte[])(dr["ST_CONTENT"]) : new byte[0];
                }
                catch (Exception e)
                {
                    MyLog.WriteExceptionLog("Sys_test.__Bind", e, "");
                }
            }

            #endregion
        }

        protected override void __UpdateHash()
        {
            #region 保存hashtable

            htFields = new Hashtable();
            htFields.Add("ST_ID", _St_id);
            htFields.Add("ST_NUM", _St_num);
            htFields.Add("ST_TIME", _St_time);
            htFields.Add("ST_USERNAME", _St_username);
            htFields.Add("ST_SIZE", _St_size);
            htFields.Add("ST_CONTENT", _St_content);

            #endregion
        }

        #endregion
    }
}
