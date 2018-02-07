using System;
using System.Collections;
using System.Data;

namespace Admy.Common
{
    public interface ITable
    {
        string GetTableName();      //表名
        string GetFieldList();      //字段
        string GetPrimayField(ref Type type);
        Hashtable GetFields();      //所有字段，以hash保存
    }

    public interface IStoredProc
    {
        string GetProcName();
        string GetCursorFieldList();
        Hashtable GetFields();
    }

    public delegate void OnMessageEvent(string strMsg);

    public interface IDBHelper
    {
        T Insert<T>(ITable objTable, bool CreateNewId); 
        T Update<T>(ITable objTable);                   
        T Delete<T>(ITable objTable, T id);             
        bool ExecSQL(string sqlstr);                    
        bool CallProc(IStoredProc objProc);

        OnMessageEvent OnMessage { get; set; }
        void WriteLogInfo(string strMsg);

        DataTable GetDtBySql(string sqlstr);
        DataTable GetDtBySql(string sqlstr, int StartRecord, int MaxRecord);
        string GetFieldValue(string sqlstr);
    }

    /// <summary>
    /// DBHelper基础类
    ///     在ITable的基础上，增加基础抽象类，提供公共接口的实现，减少实体类代码量
    /// xiang   20171218 
    /// </summary>
    [Serializable]
    public abstract class BaseTable<T> : ITable
    {
        #region 构造函数

        /// <summary>
        /// 构造一个类,ID号置为空/0
        /// </summary>
        /// <param name="helper">dbhelper</param>
        public BaseTable(IDBHelper helper)
        {
            this.Init(helper);
        }

        /// <summary>
        /// 构造一个实体类，按指定ID从数据库中取数据进行填充
        /// 如果数据库中不存在指定ID的记录，则指定类ID置为空
        /// </summary>
        /// <param name="helper">dbhelper</param>
        /// <param name="id">数据ID号</param>
        public BaseTable(IDBHelper helper, T id)
        {
            this.Init(helper);
            string sqlstr = string.Format("select {0} from {1} where {2}={3}",
                    this.GetFieldList(), this.GetTableName(), this.GetPrimayField(), value2string(id));
            DataTable dt = dbHelper.GetDtBySql(sqlstr);
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                this.Bind(dr);
            }
        }

        /// <summary>
        /// 根据数据行构造一个实体类
        /// </summary>
        /// <param name="helper">dbhelper</param>
        /// <param name="datarow">数据行</param>
        public BaseTable(IDBHelper helper, DataRow dr)
        {
            this.Init(helper);
            this.Bind(dr);
        }

        /// <summary>
        /// 直接从内存datatable中按指定的ID检索结果构造一个实体类
        /// 如果数据库中不存在指定ID的记录，则指定类ID置为空
        /// </summary>
        /// <param name="helper">dbhelper</param>
        /// <param name="dt">数据表</param>
        /// <param name="id">数据ID号</param>
        public BaseTable(IDBHelper helper, DataTable dt, T id)
        {
            this.Init(helper);
            string str = string.Format("{0}={1}", this.GetPrimayField(), value2string(id));
            DataRow[] drs = dt.Select(str);
            if (drs.Length > 0)
                this.Bind(drs[0]);
        }

        #endregion

        #region 变量及私有方法

        public IDBHelper dbHelper;
        protected Hashtable htFields;

        private static string value2string(object obj)
        {
            if (obj.GetType() == typeof(int))
            {
                return obj.ToString();
            }
            else
                return "'" + obj.ToString() + "'";
        }

        private void Init(IDBHelper helper)
        {
            this.dbHelper = helper;
            this.__Init();
            this.__UpdateHash();
        }

        private void Bind(DataRow dr)
        {
            if (dr != null)
            {
                this.__Bind(dr);
                this.__UpdateHash();
            }
        }

        #endregion

        #region 类方法(部分虚方法，待重载）

        public T GetID()
        {
            return (T)this.htFields[this.GetPrimayField()];
        }

        //子类必须实现
        protected abstract void __SetID(T id);
        protected abstract void __Init();
        protected abstract void __Bind(DataRow dr);
        protected abstract void __UpdateHash();

        #endregion

        #region ITable 成员(部分虚方法，待重载）

        public abstract string GetTableName();
        public abstract string GetPrimayField();
        public abstract string GetFieldList();
        public abstract string GetFieldListWithoutBlob();

        public string GetPrimayField(ref Type type)
        {
            //保持兼容
            type = typeof(T);
            return GetPrimayField();
        }

        public Hashtable GetFields()
        {
            return this.htFields;
        }

        #endregion

        #region Insert/Update/Delete

        public T Insert()
        {
            T id = dbHelper.Insert<T>(this, true);
            this.__SetID(id);
            return id;
        }

        public T Update()
        {
            if (this.GetID().Equals(default(T)))
                return default(T);
            return dbHelper.Update<T>(this);
        }

        public T Delete(T id)
        {
            return dbHelper.Delete<T>(this, id);
        }

        #endregion

        #region Query/GetDataTable

        public QueryParam CreateQueryParam()
        {
            QueryParam param = new QueryParam();
            param.order = this.GetPrimayField();
            return param;
        }

        public QueryParam CreateQueryParam(int page, int rows, string sort)
        {
            QueryParam param = new QueryParam();
            param.order = sort.Equals(string.Empty) ? this.GetPrimayField() : sort;
            param.page = page;
            param.rows = rows;
            return param;
        }

        public bool SetOrderField(QueryParam param)
        {
            return SetOrderField(param, false);
        }

        public bool SetOrderField(QueryParam param,bool force)
        {
            if (param.order.Trim().Equals(string.Empty) || force)
            {
                param.order = this.GetPrimayField();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 按指定条件获得数据集(不含大字段内容)
        /// </summary>
        /// <param name="ExtraFieldStr">额外的字段内容</param>
        /// <param name="ConditionStr">额外的查询条件，格式为"and xxx=xxx and yyy=yyy";如查询全部，将本参数设为""</param>
        /// <param name="OrderField">指定的排序方式，如不指定排序方式，则不排序</param>
        /// <returns>从数据表中所查询出的数据集，字段内容包括该表所有字段和指定的额外字段，查询条件为用户指定的条件</returns>
        public DataTable Query(QueryParam param)
        {
            DataTable dt;
            string sqlStr = string.Format("select {0}{1} from {2} where 1=1 {3}"
                , this.GetFieldListWithoutBlob(), param.ExtraFieldStr, this.GetTableName(), param.ConditionStr);
            if (param.order.Trim() != string.Empty)
                sqlStr += " order by " + param.order;
            if (param.rows > 0)
            {
                dt= dbHelper.GetDtBySql(sqlStr, (param.page - 1) * param.rows, param.rows);
                //有分页参数，则计算总数
                sqlStr = string.Format("select {0}{1} from {2} where 1=1 {3}"
                    , "count(*)", "", this.GetTableName(), param.ConditionStr);
                param.total = MyType.ToInt(dbHelper.GetFieldValue(sqlStr));
            }
            else
            {
                dt = dbHelper.GetDtBySql(sqlStr);
                param.total = (dt != null) ? dt.Rows.Count : 0;
            }
            return dt;
        }

        /// <summary>
        /// 获得数据集(不含大字段内容)
        /// </summary>
        /// <returns>从数据表中所查询出的数据集，字段内容包括该表所有字段，按主键排序</returns>
        public DataTable Query()
        {
            QueryParam param = this.CreateQueryParam();
            return Query(param);
        }

        #region 查询方式(保留兼容)

        /// <summary>
		/// 按指定条件获得数据集(不含大字段内容)
		/// </summary>
		/// <param name="ExtraFieldStr">额外的字段内容</param>
		/// <param name="ConditionStr">额外的查询条件，格式为"and xxx=xxx and yyy=yyy";如查询全部，将本参数设为""</param>
		/// <param name="OrderField">指定的排序方式，如不指定排序方式，则不排序</param>
		/// <returns>从数据表中所查询出的数据集，字段内容包括该表所有字段和指定的额外字段，查询条件为用户指定的条件</returns>
        [Obsolete("建议采用Query方法")] 
        public DataTable GetDataTable(string ExtraFieldStr, string ConditionStr, string OrderField)
        {
            QueryParam param = this.CreateQueryParam();
            param.ExtraFieldStr = ExtraFieldStr;
            param.ConditionStr = ConditionStr;
            param.order = OrderField;
            return Query(param);
        }

        /// <summary>
        /// 按指定条件获得数据集(不含大字段内容)
        /// </summary>
        /// <param name="ExtraFieldStr">额外的字段内容</param>
        /// <param name="ConditionStr">额外的查询条件，格式为"and xxx=xxx and yyy=yyy";如查询全部，将本参数设为""</param>
        /// <returns>从Sys_blob表中所查询出的数据集，字段内容包括该表所有字段和指定的额外字段，查询条件为用户指定的条件，按主键(SB_ID)排序</returns>
        [Obsolete("建议采用Query方法")]
        public DataTable GetDataTable(string ExtraFieldStr, string ConditionStr)
        {
            QueryParam param = this.CreateQueryParam();
            param.ExtraFieldStr = ExtraFieldStr;
            param.ConditionStr = ConditionStr;
            return Query(param);
        }

        /// <summary>
        /// 获得数据集(不含大字段内容)
        /// </summary>
        /// <returns>从Sys_blob表中所查询出的数据集，字段内容包括该表所有字段，按主键(SB_ID)排序</returns>
        [Obsolete("建议采用Query方法")]
        public DataTable GetDataTable()
        {
            return Query();
        }

        #endregion

        #endregion

    }

    /// <summary>
    /// 通用数据表查询参数
    /// （做成一个独立的类，以便于进行参数扩展）
    /// </summary>
    public class QueryParam
    {
        public string ExtraFieldStr = "";   //扩展字段，若非空，则须以“,”开头
        public string ConditionStr = "";    //查询条件，若非空，则须以and 开头
        public string order = "";      //排序字段

        //分页设置，显示页(从1开始，与easyui保持一致）
        private int __page = 1;
        public int page
        {
            get { return __page; }
            set { __page = value > 0 ? value : 1; }
        }

        //每页显示记录数据，0表示不分页
        private int __rows = 0;
        public int rows
        {
            get { return __rows; }
            set { __rows = value > 0 ? value : 0; }
        }

        //总记录数，用于返回(easyui)
        public int total = 0;      
    }
}
