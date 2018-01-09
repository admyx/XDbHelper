#define DT_OLEDB

using System;
using System.Data;
using System.Collections;

#if DT_ORACLE
using System.Data.OracleClient;     //Oracle
using Connection = System.Data.OracleClient.OracleConnection;
using DataAdapter = System.Data.OracleClient.OracleDataAdapter;
using Transaction = System.Data.OracleClient.OracleTransaction;
using DbCommand = System.Data.OracleClient.OracleCommand;
using DbParameter = System.Data.OracleClient.OracleParameter;
using DbCommandBuilder = System.Data.OracleClient.OracleCommandBuilder;
#elif DT_MSSQL
using System.Data.SqlClient;        //MSSQL
using Connection = System.Data.SqlClient.SqlConnection;     
using DataAdapter = System.Data.SqlClient.SqlDataAdapter;
using Transaction = System.Data.SqlClient.SqlTransaction;
using DbCommand = System.Data.SqlClient.SqlCommand;
using DbParameter = System.Data.SqlClient.SqlParameter;
using DbCommandBuilder=System.Data.SqlClient.SqlCommandBuilder;
#elif DT_OLEDB
using System.Data.OleDb;            //OLEDB
using Connection = System.Data.OleDb.OleDbConnection;
using DataAdapter = System.Data.OleDb.OleDbDataAdapter;
using Transaction = System.Data.OleDb.OleDbTransaction;
using DbCommand = System.Data.OleDb.OleDbCommand;
using DbParameter = System.Data.OleDb.OleDbParameter;
using DbCommandBuilder = System.Data.OleDb.OleDbCommandBuilder;
#elif DT_SQLITE
using System.Data.SQLite;            //OLEDB
using Connection = System.Data.SQLite.SQLiteConnection;
using DataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using Transaction = System.Data.SQLite.SQLiteTransaction;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbParameter = System.Data.SQLite.SQLiteParameter;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
#elif DT_POSTGRES
using System.Data.SqlClient;        //POSTGRES
using Connection = Npgsql.NpgsqlConnection;
using DataAdapter = Npgsql.NpgsqlDataAdapter;
using Transaction = Npgsql.NpgsqlTransaction;
using DbCommand = Npgsql.NpgsqlCommand;
using DbParameter = Npgsql.NpgsqlParameter;
using DbCommandBuilder = Npgsql.NpgsqlCommandBuilder;
#endif

namespace Admy.Common
{
    public class DBOleDBHelper : IDBHelper
    {
        #region 构造函数

        public DBOleDBHelper(string ConnectStr)
        {
            #region 从连接串中解析出来的datasource，作为当前类的关键字

            string str = ConnectStr + ";";
            string strFlag = "Data Source=";
            if (str.IndexOf(strFlag) > -1)
            {
                str = str.Remove(0, str.IndexOf(strFlag) + strFlag.Length);
                str = str.Substring(0, str.IndexOf(";"));
            }
            this.mDataSource = str;
            //this.mDataSource = ConnectStr; //还是直接用连接串吧

#if DT_SQLITE
            if (!System.IO.File.Exists(str))
            {
                Connection.CreateFile(str);
            }
#endif

            #endregion

            mConn = new Connection(ConnectStr);
        }

        /// <summary>
        /// 断开数据库连接
        /// </summary>
        public void DisConnect()
        {
            mConn.Close();
        }

        #endregion

        #region 变量定义

        //缓存，记录与每个表、存贮过程相关的信息（第一次从数据库、用户配置中提取，以后通过缓存获取）
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        //缓存，记录每个数据表（如果ID号为数值的话）的最大ID值
        private static Hashtable maxIDCache = Hashtable.Synchronized(new Hashtable());


        /// <summary>
        /// 从连接串中解析出来的datasource，作为当前类的关键字
        /// </summary>
        private string mDataSource;

        private Connection mConn;
        /// <summary>
        /// 当前数据库连接
        /// </summary>
        public Connection Connection
        {
            get
            {
                return mConn;
            }
        }

        #endregion

        #region 公开方法：IDBHelper成员

        #region 与事务无关的方法，不提供事务功能，主要是供ITable调用

        public T Insert<T>(ITable objTable, bool CreateNewId)
        {
            return Insert<T>(objTable, CreateNewId, null);
        }

        public T Update<T>(ITable objTable)
        {
            return Update<T>(objTable, null);
        }

        public T Delete<T>(ITable objTable, T id)
        {
            return Delete<T>(objTable, id, null);
        }

        public bool CallProc(IStoredProc objProc)
        {
            return CallProc(objProc, null);
        }

        public bool ExecSQL(string sqlstr)
        {
            return ExecSQL(sqlstr, null);
        }

        #endregion

        #region WriteLogInfo

        private OnMessageEvent _OnMessage;
        public OnMessageEvent OnMessage
        {
            get
            {
                return _OnMessage;
            }
            set
            {
                _OnMessage = value;
            }
        }

        public void WriteLogInfo(string strMsg)
        {
            if (_OnMessage != null)
                _OnMessage(strMsg);
        }

        #endregion

        #region GetDtBySql

        /// <summary>
        /// 根据sql语句生成数据表
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <returns>数据表，如果sql语句执行异常，返回null</returns>
        public DataTable GetDtBySql(string sqlstr)
        {
            DataAdapter adapter = new DataAdapter(sqlstr, mConn);
            DataTable dt = new DataTable();
            try
            {
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception e)
            {
                WriteLogInfo("error on " + this.GetType().Name + " GetDtBySql:" + sqlstr + "\r\n" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 根据sql语句，指定其中若干条记录生成数据表
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <param name="StartRecord">从其开始的从零开始的记录号</param>
        /// <param name="MaxRecord">要检索的最大记录数</param>
        /// <returns>数据表，如果sql语句执行异常，返回null</returns>
        public DataTable GetDtBySql(string sqlstr, int StartRecord, int MaxRecord)
        {
            DataAdapter adapter = new DataAdapter(sqlstr, mConn);
            DataSet ds = new DataSet();
            try
            {
                adapter.Fill(ds, StartRecord, MaxRecord, "srcTable");
                return ds.Tables["srcTable"];
            }
            catch (Exception e)
            {
                WriteLogInfo("error on " + this.GetType().Name + " GetDtBySql:" + sqlstr + "\r\n" + e.Message);
                return null;
            }
        }

        #endregion

        #region GetFieldValue

        /// <summary>
        /// 通过sql语句获得字段值
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <returns>返回sql语句查询结果的第一记录的第一个字段内容
        /// 如果sql语句执行异常或者没有记录，返回空字符串""
        /// </returns>
        public string GetFieldValue(string sqlstr)
        {
            string result = string.Empty;
            DbCommand dbCommand = new DbCommand();
            dbCommand.Connection = mConn;
            dbCommand.CommandText = sqlstr;

            try
            {
                if (mConn.State == ConnectionState.Closed)
                    mConn.Open();
                object obj = dbCommand.ExecuteScalar();
                if (obj != null)
                    result = obj.ToString();
            }
            catch (Exception e)
            {
                WriteLogInfo("error on " + this.GetType().Name + " GetFieldValue:" + sqlstr + "\r\n" + e.Message);
                return result;
            }
            return result;
        }

        #endregion

        #endregion

        #region 公开方法：数据处理过程（提供事务接口）

        #region insert 

        /// <summary>
        /// 插入类对象到数据库中
        /// </summary>
        /// <typeparam name="T">类对象ID段类型</typeparam>
        /// <param name="objTable">类对象</param>
        /// <param name="CreateNewId">是否生成新的ID号</param>
        /// <param name="tr">事务,null表示不启用事务</param>
        /// <returns>插入记录成功，返回记录ID号，否则返回默认值(0，或者空串)</returns>
        public T Insert<T>(ITable objTable, bool CreateNewId, Transaction tr)
        {
            Type type = typeof(object);
            string pkField = objTable.GetPrimayField(ref type);

            string hashKey = this.mDataSource + ":table:" + objTable.GetTableName() + ":insert";
            string sqlstr = "";
            string strParamList = "";
            if (paramCache[hashKey] == null)
            {
                string str1 = "";
                string str2 = "";
                foreach (string strkey in objTable.GetFieldList().Split(new char[] { ',' }))
                {
                    str1 += "," + strkey;
                    str2 += "," + BuildParamFlag(strkey);
                }
                str1 = str1.Remove(0, 1);
                str2 = str2.Remove(0, 1);
                sqlstr = "INSERT INTO " + objTable.GetTableName() + " (" + str1 + ")VALUES(" + str2 + ")";
                strParamList = str1;
                paramCache.Add(hashKey, sqlstr);
                paramCache.Add(hashKey + ":param", strParamList);
            }
            sqlstr = paramCache[hashKey].ToString();
            strParamList = paramCache[hashKey+":param"].ToString();

            Hashtable htParam = objTable.GetFields();
            if (CreateNewId)
                htParam[pkField] = GetSerialNo<T>(objTable.GetTableName(), pkField);
            return Execute(objTable.GetTableName(), "insert", sqlstr, htParam,strParamList, tr) ? (T)htParam[pkField] : default(T);
        }

        #endregion 

        #region update

        /// <summary>
        /// 更新类对象到数据库中
        /// </summary>
        /// <typeparam name="T">类对象ID段类型</typeparam>
        /// <param name="objTable">类对象</param>
        /// <param name="tr">事务,null表示不启用事务</param>
        /// <returns>更新记录成功，返回记录ID号，否则返回默认值(0，或者空串)</returns>
        public T Update<T>(ITable objTable, Transaction tr)
        {
            Type type = typeof(object);
            string pkField = objTable.GetPrimayField(ref type);

            string hashKey = this.mDataSource + ":table:" + objTable.GetTableName() + ":update";
            string sqlstr = "";
            string strParamList = "";
            if (paramCache[hashKey] == null)
            {
                string str1 = "";
                foreach (string strkey in objTable.GetFieldList().Split(new char[] { ',' }))
                {
                    if (pkField != strkey)
                    {
                        str1 += "," + strkey + "=" + BuildParamFlag(strkey);
                        strParamList += "," + strkey;
                    }
                }
                str1 = str1.Remove(0, 1);                
                sqlstr = "UPDATE " + objTable.GetTableName() + " set " + str1 + " where " + pkField + "=" + BuildParamFlag( pkField);
                strParamList = strParamList.Remove(0, 1) + "," + pkField;
                paramCache.Add(hashKey, sqlstr);
                paramCache.Add(hashKey + ":param", strParamList);
            }
            sqlstr = paramCache[hashKey].ToString();
            strParamList = paramCache[hashKey + ":param"].ToString();

            Hashtable htParam = objTable.GetFields();
            return Execute(objTable.GetTableName(), "update", sqlstr, htParam, strParamList, tr) ? (T)htParam[pkField] : default(T);
        }

        #endregion

        #region delete

        /// <summary>
        /// 删除数据库中指定ID的数据记录
        /// </summary>
        /// <typeparam name="T">类对象ID段类型</typeparam>
        /// <param name="objTable">类对象</param>
        /// <param name="id">待删除的记录ID</param>
        /// <param name="tr">事务,null表示不启用事务</param>
        /// <returns>更新删除成功，返回记录ID号，否则返回默认值(0，或者空串)</returns>
        public T Delete<T>(ITable objTable, T id, Transaction tr)
        {
            Type type = typeof(object);
            string pkField = objTable.GetPrimayField(ref type);

            string hashKey = this.mDataSource + ":table:" + objTable.GetTableName() + ":delete";
            string sqlstr = "";
            string strParamList = "";
            if (paramCache[hashKey] == null)
            {
                sqlstr = "DELETE FROM " + objTable.GetTableName() + " where " + pkField + "=" + BuildParamFlag(pkField);
                strParamList = pkField;
                paramCache.Add(hashKey, sqlstr);
                paramCache.Add(hashKey + ":param", strParamList);
            }
            sqlstr = paramCache[hashKey].ToString();
            strParamList = paramCache[hashKey + ":param"].ToString();

            Hashtable htParam = objTable.GetFields();
            htParam[pkField] = id;
            return Execute(objTable.GetTableName(), "delete", sqlstr, htParam, strParamList, tr) ? (T)htParam[pkField] : default(T);
        }

        #endregion

        #region ExecSQL

        private string BuildParamFlag(string fieldname)
        {
#if DT_ORACLE
            string PARAM_FLAG = ":FIELDNAME";
#elif DT_MSSQL
            string PARAM_FLAG = "@FIELDNAME";
#elif DT_OLEDB
            string PARAM_FLAG = "?";
#elif DT_SQLITE
            string PARAM_FLAG = "@FIELDNAME";
#elif DT_POSTGRES
            string PARAM_FLAG = "@FIELDNAME";
#endif
            return PARAM_FLAG.Replace("FIELDNAME", fieldname);
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <param name="tr">事务,null表示不启用事务</param>
        /// <returns>是否执行成功</returns>
        public bool ExecSQL(string sqlstr, Transaction tr)
        {
            DbCommand dbCommand = new DbCommand();
            dbCommand.Connection = mConn;
            dbCommand.CommandText = sqlstr;
            dbCommand.Transaction = tr;

            try
            {
                if (mConn.State == ConnectionState.Closed)
                    mConn.Open();
                dbCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                WriteLogInfo("errorn on " + this.GetType().Name + " ExecSQL:" + sqlstr + "\r\n" + e.Message);
                return false;
            }
        }

        #endregion

        #region 相关私有过程

        /// <summary>
        /// 取系列号，根据不同的数据表，返回不同值，用于生成数据记录的ID字段内容
        /// </summary>
        /// <param name="TableName">数据表名</param>
        /// <param name="IdFieldName">主键字段</param>
        /// <returns>数据值</returns>
        private T GetSerialNo<T>(string TableName, string IdFieldName)
        {
            if (typeof(T) == typeof(int))
            {
                string strKey= TableName+"."+IdFieldName;
                int intMax = 0;
                if (maxIDCache[strKey] != null)
                {
                    intMax = (int)maxIDCache[strKey];
                    maxIDCache[strKey] = ++intMax;
                }
                else
                {
                    string sqlstr = "select max(" + IdFieldName + ") from " + TableName;
                    string str = GetFieldValue(sqlstr);
                    intMax = int.Parse("0" + str) + 1;
                    maxIDCache.Add(strKey, intMax);
                }
                return (T)((object)intMax);
            }
            else if (typeof(T) == typeof(string))
            {
                //字符串ID，取guid
                //N xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx 
                //D xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx 
                //B {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx} 
                //P (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx) 
                return (T)((object)System.Guid.NewGuid().ToString("N"));
            }
            else
                return default(T);
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="optype"></param>
        /// <param name="sqlstr"></param>
        /// <param name="htParam"></param>
        /// <param name="strParamList"></param>
        /// <param name="tr"></param>
        /// <returns></returns>
        private bool Execute(string tablename, string optype, string sqlstr, Hashtable htParam, string strParamList, Transaction tr)
        {
            #region 创建命令

            DbCommand dbCommand = new DbCommand();
            dbCommand.Connection = mConn;
            dbCommand.CommandText = sqlstr;
            dbCommand.Transaction = tr;

            #endregion

            #region 设置参数（对于OLEDB方式而言，参数的顺序必须与SQL语句中一致）
            
            foreach (string strkey in strParamList.Split(new char[] { ',' }))
            {
#if DT_OLEDB
                if (htParam[strkey].GetType() != typeof(DateTime))
                {
                    dbCommand.Parameters.Add(new DbParameter(strkey, htParam[strkey]));
                }
                else
                {
                    //OleDB连Access时，这个日期类型必须特别指定；连Oracle不需要；SQL没测过
                    dbCommand.Parameters.Add(new DbParameter(strkey, System.Data.OleDb.OleDbType.Date)).Value = htParam[strkey];
                }
#else
                dbCommand.Parameters.Add(new DbParameter(strkey, htParam[strkey]));
#endif
            }

            #endregion

            #region 提交命令

            try
            {
                if (mConn.State == ConnectionState.Closed)
                    mConn.Open();
                dbCommand.ExecuteNonQuery();
                //DisConnection();
                return true;
            }
            catch (Exception e)
            {
                this.WriteLogInfo("error on  " + this.GetType().Name + " " + tablename + ".Execute." + optype + ":\r\nsql:" + sqlstr + "\r\n" + e.Message);
                return false;
            }

            #endregion
        }

        //private Transaction CreateTransaction()
        //{
        //      Transaction tr=mConn.BeginTransaction();
        //      tr.Commit();
        //}

        #endregion

        #endregion

        #region 公开方法：存贮过程相关

        #region CallProc

        public bool CallProc(IStoredProc objProc, Transaction tr)
        {
            #region 创建命令

            DbCommand dbCommand = new DbCommand();
            dbCommand.Connection = mConn;
            dbCommand.CommandType = CommandType.StoredProcedure; 
            dbCommand.CommandText = objProc.GetProcName();
            dbCommand.Transaction = tr;
            
            #endregion

            #region 参数定义

#if DT_ORACLE
            DbParameter[] dbparams = this.DeriveParameters(objProc.GetProcName(), false);
#elif DT_MSSQL
            DbParameter[] dbparams = this.DeriveParameters(objProc.GetProcName(), true);
#else // DT_OLEDB || DT_SQLITE || DT_POSTGRES 
            //DbParameter[] dbparams = new DbParameter[0];
            this.WriteLogInfo(this.GetType().Name + "不支持取存贮过程参数");
            return false;
#endif
            #endregion

#if (DT_ORACLE || DT_MSSQL)

            #region 参数赋值

            Hashtable htParam=objProc.GetFields(); 
            foreach (DbParameter dbparam in dbparams)
            {
                if (dbparam.Direction == ParameterDirection.Input || dbparam.Direction == ParameterDirection.InputOutput)
                {
                    dbparam.Value = htParam[dbparam.ParameterName.ToUpper()];
                }
                else
                {
                    dbparam.Value = DBNull.Value;
                }
                dbCommand.Parameters.Add(dbparam);
            }

            #endregion

            #region 执行

            DataSet mDataSet = new DataSet();
            using (DataAdapter mDataAdapter = new DataAdapter(dbCommand))
            {
                try
                {
                    mDataAdapter.Fill(mDataSet);
                }
                catch (Exception e)
                {
                    WriteLogInfo("error on  " + this.GetType().Name + " " + objProc.GetProcName() + ".excute:\r\n" + e.Message);
                    return false;
                }
            }

            #endregion

            #region 读取参数

            foreach (DbParameter dbparam in dbparams)
            {
                if (dbparam.Direction == ParameterDirection.Output || dbparam.Direction == ParameterDirection.InputOutput)
                {
                    htParam[dbparam.ParameterName.ToUpper()] = dbparam.Value;
                }
            }

            if (objProc.GetCursorFieldList() != "")
            {
                string[] cursorFields = objProc.GetCursorFieldList().Split(new char[] { ',' });
                if (mDataSet.Tables.Count != cursorFields.Length)
                {
                    WriteLogInfo("error on  " + this.GetType().Name + " " + objProc.GetProcName() + ".excute:\r\n 返回的游标数量与参数不匹配："
                            + "\r\n\t游标参数:" + objProc.GetCursorFieldList()
                            + "\r\n\t游标返回数:" + mDataSet.Tables.Count);
                    return false;
                }
                int idx = 0;
                foreach (string cursorField in cursorFields)
                {
                    htParam[cursorField.ToUpper()] = mDataSet.Tables[idx++];
                }
            }

            #endregion

            return true;
#endif
        }

        #endregion

        #region 相关私有过程

#if (DT_ORACLE || DT_MSSQL)

        /// <summary>
        /// 获取存储过程的参数
        /// </summary>
        /// <param name="strProcedureName">存贮过程名称</param>
        /// <param name="includeReturnValueParameter">返回参数是否包括ResultValue，如果有，就删掉</param>
        /// <returns></returns>
        private DbParameter[] DeriveParameters(string strProcedureName, bool includeReturnValueParameter)
        {
            // 缓存规则
            string hashKey = this.mDataSource + ":proc:" + strProcedureName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");
            DbParameter[] cachedParameters = paramCache[hashKey] as DbParameter[];
            if (cachedParameters == null)
            {
                DbParameter[] spParameters = DiscoverSpParameterSet(strProcedureName, includeReturnValueParameter);
                paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }

            // 深拷贝一个参数数组返回,不管是缓存中,还是从数据库返回的
            return CloneParameters(cachedParameters);
        }

        private DbParameter[] DiscoverSpParameterSet(string strProcedureName, bool includeReturnValueParameter)
        {
            DbCommand dbCommand = new DbCommand();
            dbCommand.Connection = mConn;
            dbCommand.CommandText = strProcedureName;
            dbCommand.CommandType = CommandType.StoredProcedure;

            try
            {
                if (mConn.State == ConnectionState.Closed)
                    mConn.Open();

                // 查找存储过程的参数
                DbCommandBuilder.DeriveParameters(dbCommand);

                // 如果包含返回Result value参数（自动生成的，不需要传进去）,如果有，则删除
                // Oracle中，function产生此参数，procedure则不产生
                // SQL SERVER中，procedure 产生此参数,function 未测试，应该也会有 
                if (includeReturnValueParameter)
                {
                    dbCommand.Parameters.RemoveAt(0);
                }

                // 返回找到的参数的副本
                DbParameter[] discoveredParameters = new DbParameter[dbCommand.Parameters.Count];

                dbCommand.Parameters.CopyTo(discoveredParameters, 0);

                // 参数一刀两断,没有关系了.
                dbCommand.Parameters.Clear();

                // 初始化各参数为DBNull
                foreach (DbParameter discoveredParameter in discoveredParameters)
                {
                    discoveredParameter.Value = DBNull.Value;
                }
                return discoveredParameters;
            }
            catch (Exception e)
            {
                WriteLogInfo("error on  " + this.GetType().Name + " GetStoredProcedureParamters:" + strProcedureName + "\r\n" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 深拷贝参数数组
        /// </summary>
        /// <param name="originalParameters">源参数数组</param>
        /// <returns>返回参数数组的副本</returns>
        private static DbParameter[] CloneParameters(DbParameter[] originalParameters)
        {
            DbParameter[] clonedParameters = new DbParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (DbParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

#endif

        #endregion

        #endregion

    }
}
