using System.Data.SQLite;
using System;
using System.Data;
using VocsControlHELP.model;
using System.Collections.Generic;
using System.IO;
using VocsControlHELP.Log4Net;

namespace VocsControlHELP.util
{
    public class SQLiteHelper
    {
        public static void InitSqlite()
        {
            try
            {
                if (!File.Exists(DefaultArgument.DB_PATH))
                {
                    SQLiteConnection.CreateFile(DefaultArgument.DB_PATH);
                }
                SQLiteConnection sqliteConn = new SQLiteConnection("data source=" + DefaultArgument.DB_PATH);
                if (sqliteConn.State != ConnectionState.Open)
                {
                    sqliteConn.Open();
                    SQLiteCommand cmd = new SQLiteCommand
                    {
                        Connection = sqliteConn,
                        CommandText = "CREATE TABLE IF NOT EXISTS " + DefaultArgument.TABLE_NAME + "(id integer NOT NULL PRIMARY KEY AUTOINCREMENT, time_code integer NOT NULL, date_time text NOT NULL DEFAULT CURRENT_TIMESTAMP, state smallint NOT NULL, zt real NOT NULL, jw real NOT NULL)"
                    };
                    if(cmd.ExecuteNonQuery() > 0)
                    {
                        cmd.CommandText = "CREATE INDEX un_md_date_time ON " + DefaultArgument.TABLE_NAME + " (date_time COLLATE BINARY DESC)";
                        cmd.ExecuteNonQuery();
                    }
                }
                sqliteConn.Close();
            }
            catch(Exception e)
            {
                Log4NetUtil.Error("新建数据库文件" + DefaultArgument.DB_PATH + "失败，原因：" + e.Message);
            }
        }

        public static bool IsNewData(long timeCode)
        {
            bool flag = true;
            try
            {
                SQLiteConnection sqliteConn = new SQLiteConnection("data source = " + DefaultArgument.DB_PATH);
                if (sqliteConn.State != ConnectionState.Open)
                {
                    sqliteConn.Open();
                }
                string sql = "SELECT time_code FROM " + DefaultArgument.TABLE_NAME + " ORDER BY id DESC LIMIT 0,1";
                SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    flag = timeCode != (long)reader["time_code"];
                }
                cmd.Dispose();
                sqliteConn.Close();
                return flag;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static void InsertData(long timeCode, string time, short state, float zt, float jw)
        { 
            try
            {
                SQLiteConnection sqliteConn = new SQLiteConnection("data source = " + DefaultArgument.DB_PATH);
                if (sqliteConn.State != ConnectionState.Open)
                {
                    sqliteConn.Open();
                }
                string sql = "INSERT INTO " + DefaultArgument.TABLE_NAME + " (time_code, date_time, state, zt, jw) VALUES (" + timeCode + ",\"" + time + "\"," + state + "," + zt + "," + jw + ")";
                SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConn);
                if(cmd.ExecuteNonQuery() > 0)
                {
                    Log4NetUtil.Info("最新数据添加完成！");
                }
                cmd.Dispose();
                sqliteConn.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static List<DataModel> QueryByDateTime(string startTime, string stopTime)
        {
            try
            {
                SQLiteConnection sqliteConn = new SQLiteConnection("data source = " + DefaultArgument.DB_PATH);
                if (sqliteConn.State != ConnectionState.Open)
                {
                    sqliteConn.Open();
                }
                string sql = "SELECT date_time AS Time, state AS State, zt AS Zt, jw AS Jw FROM " + DefaultArgument.TABLE_NAME + " WHERE date_time >= datetime('" + startTime + "', 'start of day', '+0 day') and date_time < datetime('" + stopTime + "','start of day','+1 day')";
                SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConn);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                cmd.Dispose();
                sqliteConn.Close();
                return MyConvert.DataTableToList<DataModel>(dataTable);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
