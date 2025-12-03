using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Mono.Data.Sqlite;
using UnityEngine;

namespace SFramework.SFDatabase.Module
{
    public class SfSqlite
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        private SqliteConnection _sqlConnection;

        /// <summary>
        /// 数据库命令
        /// </summary>
        private SqliteCommand _sqlCommand;

        /// <summary>
        /// 数据库读取
        /// </summary>
        private SqliteDataReader _sqlDataReader;

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// 构造函数 - 使用默认连接
        /// </summary>
        public SfSqlite()
        {
            _sqlConnection = new SqliteConnection();
            _sqlConnection.Open();
            _sqlCommand = _sqlConnection.CreateCommand();
        }

        /// <summary>
        /// 构造函数 - 指定数据库路径
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        public SfSqlite(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
            _sqlConnection = new SqliteConnection(_connectionString);
            _sqlConnection.Open();
            _sqlCommand = _sqlConnection.CreateCommand();
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        public void OpenConnection(string dbPath)
        {
            if (_sqlConnection != null && _sqlConnection.State == ConnectionState.Open)
            {
                _sqlConnection.Close();
            }

            _connectionString = $"Data Source={dbPath};Version=3;";
            _sqlConnection = new SqliteConnection(_connectionString);
            _sqlConnection.Open();
            _sqlCommand = _sqlConnection.CreateCommand();
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void CloseConnection()
        {
            if (_sqlDataReader != null && !_sqlDataReader.IsClosed)
            {
                _sqlDataReader.Close();
            }

            if (_sqlCommand != null)
            {
                _sqlCommand.Dispose();
            }

            if (_sqlConnection != null && _sqlConnection.State == ConnectionState.Open)
            {
                _sqlConnection.Close();
            }
        }

        /// <summary>
        /// 执行SQL查询语句
        /// </summary>
        /// <param name="sqlQuery">SQL查询语句</param>
        /// <returns>SqliteDataReader对象</returns>
        public SqliteDataReader ExecuteQuery(string sqlQuery)
        {
            var sqlCommand = _sqlConnection.CreateCommand();
            sqlCommand.CommandText = sqlQuery;
            var sqlDataReader = sqlCommand.ExecuteReader();
            return sqlDataReader;
        }

        /// <summary>
        /// 执行SQL非查询语句（INSERT、UPDATE、DELETE等）
        /// </summary>
        /// <param name="sqlNonQuery">SQL非查询语句</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteNonQuery(string sqlNonQuery)
        {
            _sqlCommand.CommandText = sqlNonQuery;
            return _sqlCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行SQL标量查询（返回单个值）
        /// </summary>
        /// <param name="sqlQuery">SQL查询语句</param>
        /// <returns>查询结果的第一行第一列的值</returns>
        public object ExecuteScalar(string sqlQuery)
        {
            _sqlCommand.CommandText = sqlQuery;
            return _sqlCommand.ExecuteScalar();
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnDefinitions">列定义</param>
        /// <returns>是否创建成功</returns>
        public bool CreateTable(string tableName, string columnDefinitions)
        {
            try
            {
                var sql = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefinitions})";
                ExecuteNonQuery(sql);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建表失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>是否删除成功</returns>
        public bool DropTable(string tableName)
        {
            try
            {
                var sql = $"DROP TABLE IF EXISTS {tableName}";
                ExecuteNonQuery(sql);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"删除表失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columns">列名</param>
        /// <param name="values">值</param>
        /// <returns>是否插入成功</returns>
        public bool Insert(string tableName, string columns, string values)
        {
            try
            {
                var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
                ExecuteNonQuery(sql);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"插入数据失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="setClause">SET子句</param>
        /// <param name="whereClause">WHERE子句</param>
        /// <returns>受影响的行数</returns>
        public int Update(string tableName, string setClause, string whereClause = "")
        {
            try
            {
                var sql = $"UPDATE {tableName} SET {setClause}";
                if (!string.IsNullOrEmpty(whereClause))
                {
                    sql += $" WHERE {whereClause}";
                }
                return ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                Debug.LogError($"更新数据失败: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="whereClause">WHERE子句</param>
        /// <returns>受影响的行数</returns>
        public int Delete(string tableName, string whereClause = "")
        {
            try
            {
                var sql = $"DELETE FROM {tableName}";
                if (!string.IsNullOrEmpty(whereClause))
                {
                    sql += $" WHERE {whereClause}";
                }
                return ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                Debug.LogError($"删除数据失败: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 查询数据是否存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="whereClause">WHERE子句</param>
        /// <returns>是否存在</returns>
        public bool Exists(string tableName, string whereClause)
        {
            try
            {
                var sql = $"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}";
                var result = ExecuteScalar(sql);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"查询数据存在性失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 执行事物
        /// </summary>
        /// <param name="queryString">多条sql语句</param>
        /// <returns></returns>
        public int TransactionExec(List<string>queryString)
        {
            var effectRow = 0;
            DbTransaction trans = _sqlConnection.BeginTransaction();
            try
            {
                var dbCommand = _sqlConnection.CreateCommand();

                foreach (var t in queryString)
                {
                    dbCommand.CommandText = t;
                    effectRow += dbCommand.ExecuteNonQuery();
                }

                trans.Commit();

            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            return effectRow;
        }
        
        /// <summary>
        /// 检查连接状态
        /// </summary>
        /// <returns>是否已连接</returns>
        public bool IsConnected()
        {
            return _sqlConnection != null && _sqlConnection.State == ConnectionState.Open;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            CloseConnection();
            _sqlConnection?.Dispose();
            _sqlCommand?.Dispose();
            _sqlDataReader?.Dispose();
        }
    }
}