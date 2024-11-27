//////////////////////////////////////////////////////////////////////////
//
// BotDB
// 
// Created by Shoori.
//
// Copyright 2024 SongMyeongWon.
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using MySql.Data.MySqlClient;
using SHUtils;
using System;

public class BotDB : Singleton<BotDB>
{
    public const string USER_INFO_TABLE_NAME = "user_info";

    //----------------------------------------------------------------------------------
    public void Init()
    {
        if (ExistsUser(BotConfig.Instance.Administrator) == false)
        {
            AddUser(BotConfig.Instance.Administrator);
        }
    }

    //----------------------------------------------------------------------------------
    public bool AddUser(string user_name)
    {
        if (string.IsNullOrEmpty(BotConfig.Instance.BotDBConnectionStr) == true)
            return false;

        using (MySqlConnection conn = new MySqlConnection(BotConfig.Instance.BotDBConnectionStr))
        {
            try
            {
                conn.Open();

                string query = $"INSERT INTO {USER_INFO_TABLE_NAME} (user_id) VALUES (@user_id)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user_id", user_name);

                int row_affected = cmd.ExecuteNonQuery();
                if (row_affected > 0)
                {
                    SHLog.Log($"[INFO] Successful AddUser: {user_name}");
                    return true;
                }

                SHLog.LogError($"[ERROR] Failed AddUser: {user_name}...");
                return false;
            }
            catch (Exception e)
            {
                SHLog.LogError($"[ERROR] Exception Occurred... reason is {e}");
                return false;
            }
        }
    }

    //----------------------------------------------------------------------------------
    public bool ExistsUser(string user_name)
    {
        if (string.IsNullOrEmpty(BotConfig.Instance.BotDBConnectionStr) == true)
            return false;

        using (MySqlConnection conn = new MySqlConnection(BotConfig.Instance.BotDBConnectionStr))
        {
            try
            {
                conn.Open();

                string query = $"SELECT COUNT(*) FROM {USER_INFO_TABLE_NAME} WHERE user_id = @user_id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user_id", user_name);

                int user_count = Convert.ToInt32(cmd.ExecuteScalar());
                return user_count > 0;
            }
            catch (Exception e)
            {
                SHLog.LogError($"[ERROR] Exception Occurred... reason is {e}");
                return false;
            }
        }
    }
}
