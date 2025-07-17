using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using Song.Core.Extends.DB;
using Song.Core.Module;
using UnityEngine;

namespace Song.Scripts.Core.Mono
{
    /// <summary>
    /// Song Sqlite 模块
    /// </summary>
    public class SongSqliteMono:MonoSingleton<SongSqliteMono>
    {
        [Header("数据库名称")]
        public string dbName = "database";

        [Header("数据库,游戏开始后自动创建")]
        public SongSqlite DB;
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
            var dbpath = $"{Application.streamingAssetsPath}/{dbName}.db";
            DB = new SongSqlite(dbpath);
        }
        
        /// <summary>
        /// 执行Sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="callback">返回数据回调</param>
        public void ExecSql(string sql,Action<SqliteDataReader> callback = null)
        {
            var data = DB.ExecuteQuery(sql);
            while (data.Read())
            {
                callback?.Invoke(data);
            }
            data.Close();
        }
        
        // public List<T> ExecSql<T>(string sql)
        // {
        //     var data = DB.ExecuteQuery(sql);
        //     var list = new List<T>();
        //     while (data.Read())
        //     {
        //         
        //     }
        //     data.Close();
        //     return list;
        // }
        
        /// <summary>
        /// 执行Sql语句,不返回结果
        /// </summary>
        public void ExecSqlNotQuery(string sql)
        {
            DB.ExecuteNonQuery(sql);
        }
    }
}