//////////////////////////////////////////////////////////////////////////
//
// BotMain
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

using Discord;
using Discord.WebSocket;
using SHUtils;
using System;
using System.Threading.Tasks;
using System.Globalization;
using SHUtils.I18N;
using System.Collections.Generic;

public class BotMain : Singleton<BotMain>
{
    public class BotCommandItem
    {
        public string str_data;
    }

    private BotState mBotState = BotState.None;
    private DiscordSocketClient mDiscordClient = null;
    private DiscordSocketConfig mDiscordConfig = null;

    private Dictionary<BotCommandType, command_callback> mCmdCallbacks = new Dictionary<BotCommandType, command_callback>();

    private delegate void command_callback(string region_code, SocketUserMessage user_msg, params object[] parms);

    //----------------------------------------------------------------------------------
    public void Init()
    {
        SHLog.s_Log = LogDefault;
        SHLog.s_LogError = LogError;
        SHLog.s_LogWarning = LogWarning;

        // DO NOT Change this orders...
        BotConfig.Instance.Load();
        BotTableManager.Instance.Init();
        BotDB.Instance.Init();
        InitCommands();

        mDiscordConfig = new DiscordSocketConfig();
        mDiscordConfig.GatewayIntents = GatewayIntents.All;

        mDiscordClient = new DiscordSocketClient(mDiscordConfig);
        mDiscordClient.Log += BotLog;

        Task.Run(() => StartBot());
    }

    //----------------------------------------------------------------------------------
    private void InitCommands()
    {
        mCmdCallbacks.Add(BotCommandType.Info, OnCommandInfo);
        mCmdCallbacks.Add(BotCommandType.Talk, OnCommandTalk);
        mCmdCallbacks.Add(BotCommandType.WakeUp, OnCommandWakeUp);
        mCmdCallbacks.Add(BotCommandType.Sleep, OnCommandSleep);
        mCmdCallbacks.Add(BotCommandType.Refresh, OnCommandRefresh);
        mCmdCallbacks.Add(BotCommandType.AddUsableUser, OnCommandAddUsableUser);
    }

    //----------------------------------------------------------------------------------
    private async Task StartBot()
    {
        SHLog.Log("[INFO] Start Bot");

        if (mDiscordClient == null)
        {
            SHLog.LogError("[ERROR] Bot Login Process Fail.. please check source or network");
            return;
        }

        Task login_task = mDiscordClient.LoginAsync(TokenType.Bot, BotConfig.Instance.BotToken);
        await login_task;

        if (login_task.IsFaulted)
        {
            SHLog.LogError("[ERROR] Bot Login Process Fail.. please check source or network");
            return;
        }

        Task start_task = mDiscordClient.StartAsync();
        await start_task;

        if (start_task.IsFaulted)
        {
            SHLog.LogError("Bot Start Process Fail.. please check source or network");
            return;
        }

        mDiscordClient.MessageReceived += HandleCommandAsync;
        mDiscordClient.PresenceUpdated += HandlePresenceUpdated;

        mBotState = BotState.Awake;

        SHLog.Log("[INFO] Bot Start Process Successful..");
    }

    //----------------------------------------------------------------------------------
    private async Task HandleCommandAsync(SocketMessage msg)
    {
        if (msg.Author.IsBot == true)
            return;

        SHLog.Log($"[RECV] RECV MESSAGE: Channel:{msg.Channel.Name} Author:{msg.Author.GlobalName} MESSAGE:{msg.Content}");

        SocketUserMessage message = msg as SocketUserMessage;
        if (message == null)
            return;

        BotConfig config = BotConfig.Instance;
        if (config.UseUsableChannelList == true && config.UsableChannelList != null)
        {
            string cur_channel = message.Channel.Name;
            if (config.UsableChannelList.Exists(a => a == cur_channel) == false)
                return;
        }

        string message_content = message.Content;
        string[] split_contents = message_content.Split(' ');
        int content_len = split_contents.Length;
        if (content_len < 2)
            return;

        RegionInfo region_info = BotConfig.Instance.GetPrefixRegion(split_contents[0]);
        if (region_info == null)
            return;

        string region_code = region_info.TwoLetterISORegionName;
        if (BotDB.Instance.ExistsUser(message.Author.Username) == false)
        {
            string send_alert = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", "MESSAGE_NON_USABLE_USER_ALERT");
            if (string.IsNullOrEmpty(send_alert) == false)
                await message.Channel.SendMessageAsync(send_alert);

            return;
        }

        BotCommandType cmd_type = TBL_CommandManager.Instance.GetCommandType(region_code, split_contents[1]);
        if (mCmdCallbacks.ContainsKey(cmd_type) == false)
        {
            if (mBotState == BotState.Awake)
                await message.Channel.SendMessageAsync(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", $"MESSAGE_UNKNOWN_COMMAND"));
            else
                await message.Channel.SendMessageAsync(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", $"COMMAND_SLEEP_TALK_Sleep"));

            return;
        }

        object[] cmd_parameter = null;
        if (content_len >= 3)
        {
            int len_param = content_len - 2;
            cmd_parameter = new object[len_param];
            for (int i = 0; i < len_param; i++)
            {
                cmd_parameter[i] = split_contents[2 + i];
            }
        }

        mCmdCallbacks[cmd_type]?.Invoke(region_code, message, cmd_parameter);
    }

    //----------------------------------------------------------------------------------
    private void OnCommandInfo(string region_code, SocketUserMessage user_msg, params object[] parms)
    {
        string cmd = TBL_TalkManager.Instance.GetAllCommandInfoText(region_code);
        if (string.IsNullOrEmpty(cmd) == true)
            return;

        Task.Run(() => user_msg.Channel.SendMessageAsync(cmd));
    }

    //----------------------------------------------------------------------------------
    private void OnCommandTalk(string region_code, SocketUserMessage user_msg, params object[] parms)
    {
        string cmd = TBL_TalkManager.Instance.GetTalkMessage(region_code, "MESSAGE_NORMAL_TALK", BotCommandType.Talk, "", mBotState.ToString());
        if (string.IsNullOrEmpty(cmd) == true)
            return;

        Task.Run(() => user_msg.Channel.SendMessageAsync(cmd));
    }

    //----------------------------------------------------------------------------------
    private void OnCommandSleep(string region_code, SocketUserMessage user_msg, params object[] parms)
    {
        string cmd = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", $"COMMAND_SLEEP_TALK_{mBotState}");
        if (string.IsNullOrEmpty(cmd) == true)
            return;

        if (mBotState == BotState.Awake)
        {
            mBotState = BotState.Sleep;
            Task.Run(() => mDiscordClient.SetStatusAsync(UserStatus.AFK));
        }

        Task.Run(() => user_msg.Channel.SendMessageAsync(cmd));
    }

    //----------------------------------------------------------------------------------
    private void OnCommandWakeUp(string region_code, SocketUserMessage user_msg, params object[] parms)
    {
        string cmd = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", $"COMMAND_WAKE_UP_TALK_{mBotState}");
        if (string.IsNullOrEmpty(cmd) == true)
            return;

        if (mBotState == BotState.Sleep)
        {
            mBotState = BotState.Awake;
            Task.Run(() => mDiscordClient.SetStatusAsync(UserStatus.Online));
        }

        Task.Run(() => user_msg.Channel.SendMessageAsync(cmd));
    }

    //----------------------------------------------------------------------------------
    private void OnCommandRefresh(string region_code, SocketUserMessage user_msg, params object[] parms)
    {
        string cmd_end_msg = "";
        if (mBotState != BotState.Awake)
        {
            cmd_end_msg = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", $"COMMAND_SLEEP_TALK_Sleep");
        }
        else
        {
            SHLog.Log("[INFO] Start Reload All Table Data...");
            BotConfig.Instance.Reload();
            BotTableManager.Instance.Reload();
            SHLog.Log("[INFO] Complete Reload All Table Data...");

            cmd_end_msg = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", $"COMMAND_REFRESH_TALK");
        }

        if (string.IsNullOrEmpty(cmd_end_msg) == true)
            return;

        Task.Run(() => user_msg.Channel.SendMessageAsync(cmd_end_msg));
    }

    //----------------------------------------------------------------------------------
    private void OnCommandAddUsableUser(string region_code, SocketUserMessage user_msg, params object[] parms)
    {
        if (mBotState != BotState.Awake)
        {
            Task.Run(() => user_msg.Channel.SendMessageAsync(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", "COMMAND_SLEEP_TALK_Sleep")));
            return;
        }

        if (parms == null || parms.Length <= 0)
        {
            Task.Run(() => user_msg.Channel.SendMessageAsync(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", "COMMAND_PARAM_ERROR_AddUsableUser")));
            return;
        }

        string add_user_name = parms[0].ToString();
        if (BotDB.Instance.ExistsUser(add_user_name) == false)
        {
            Task.Run(() => user_msg.Channel.SendMessageAsync(string.Format(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", "COMMAND_ADD_USABLE_USER"), add_user_name)));

            string send_msg = "";
            if (BotDB.Instance.AddUser(add_user_name) == true)
                send_msg = string.Format(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", "COMMAND_ADD_USABLE_USER_SUCCESS"), add_user_name);
            else
                send_msg = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", "COMMAND_ADD_USABLE_USER_FAIL");

            Task.Run(() => user_msg.Channel.SendMessageAsync(send_msg));
        }
        else
        {
            Task.Run(() => user_msg.Channel.SendMessageAsync(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, "TBL_Talk_Description", "MESSAGE_ALREADY_USABLE_USER")));
        }
    }

    //----------------------------------------------------------------------------------
    private Task HandlePresenceUpdated(SocketUser user, SocketPresence old_presence, SocketPresence new_presence)
    {
        return Task.CompletedTask;
    }

    //----------------------------------------------------------------------------------
    private Task BotLog(LogMessage log_msg)
    {
        if (log_msg.Exception != null)
            SHLog.LogError(log_msg.Message);
        else
            SHLog.Log(log_msg.Message);

        return Task.CompletedTask;
    }

    //----------------------------------------------------------------------------------
    private void LogDefault(string message)
    {
        Console.WriteLine(message);
    }

    //----------------------------------------------------------------------------------
    private void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    //----------------------------------------------------------------------------------
    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
