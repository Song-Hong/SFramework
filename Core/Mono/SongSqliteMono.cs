﻿using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using SFramework.Core.Extends.DB;
using Song.Core.Module;
using UnityEngine;

namespace SFramework.Core.Mono
{
    /// <summary>
    /// Song 快捷连接 Sqlite
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
        
        /// <summary>
        /// 执行Sql语句,不返回结果
        /// </summary>
        public void ExecSqlNotQuery(string sql)
        {
            DB.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 开启事物批量执行Sql语句
        /// </summary>
        /// <param name="sqls">全部sql语句</param>
        public void TransactionExec(List<string> sqls)
        {
            DB.TransactionExec(sqls);
        }

        /// <summary>
        /// 销毁时释放数据库连接
        /// </summary>
        private void OnDestroy()
        {
            Debug.Log("关闭并释放数据库连接");
            DB.Dispose();
        }
    }
}