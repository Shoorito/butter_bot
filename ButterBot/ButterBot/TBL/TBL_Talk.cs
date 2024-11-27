//////////////////////////////////////////////////////////////////////////
//
// TBL_Talk
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHUtils;
using SHUtils.I18N;

//----------------------------------------------------------------------------------
public class TBL_TalkManager : TableBaseManager<TBL_Talk, TBL_TalkManager>
{
    public override string DATA_ID => typeof(TBL_Talk).ToString();
    public override bool CheckSameIDN => true;
    public override bool CheckSameID => false;

    //----------------------------------------------------------------------------------
    protected override void ParsingRow(XmlSelector row_node)
    {
        CreateIDNBaseParse(row_node);
    }

    //----------------------------------------------------------------------------------
    public string GetTalkMessage(string region_code, string id, BotCommandType cmd_type, string event_group = "", string event_condition = "")
    {
        List<string> talk_list = GetTalkMessages(id, cmd_type, event_group, event_condition);
        if (talk_list == null)
            return "";

        string message_key = "";
        switch (cmd_type)
        {
            case BotCommandType.Talk:
                message_key = BotUtils.GetRandomTalkText(talk_list);
                break;

            default:
                message_key = talk_list.FirstOrDefault();
                break;
        }

        return I18NTextMultiLanguage.Instance.GetDefaultText(region_code, $"{typeof(TBL_Talk)}_Description", message_key);
    }

    //----------------------------------------------------------------------------------
    public List<string> GetTalkMessages(string id, BotCommandType cmd_type, string event_group = "", string event_condition = "")
    {
        TBL_Talk talk_info = GetInfoByStrKey(id);
        if (talk_info == null)
        {
            SHLog.LogError("[ERROR] Not found {id} TalkInfo, please check 'TBL_Talk.xml'");
            return null;
        }

        List<string> description_list = talk_info.TalkList.Where(a =>
        {
            if (cmd_type != a.CommandEventType)
                return false;

            if (string.IsNullOrEmpty(event_group) == false)
            {
                if (a.CommandEventGroup.Equals(event_group) == false)
                    return false;
            }

            if (string.IsNullOrEmpty(event_condition) == false)
            {
                if (a.CommandEventCondition.Equals(event_condition) == false)
                    return false;
            }

            return true;
        }).Select(a => a.MessageKey).ToList();

        return description_list;
    }

    //----------------------------------------------------------------------------------
    public string GetAllCommandInfoText(string region_code)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(I18NTextMultiLanguage.Instance.GetDefaultText(region_code, $"{typeof(TBL_Talk)}_Description", "COMMAND_SHOW_INFO"));

        List<BotCommandType> cmd_all_types = Enum.GetValues(typeof(BotCommandType)).Cast<BotCommandType>().ToList();
        cmd_all_types.Remove(BotCommandType.None);

        foreach (BotCommandType cmd_type in cmd_all_types)
        {
            string add_line = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, $"{typeof(TBL_Talk)}_Description", $"COMMAND_SHOW_INFO_{cmd_type}");
            if (string.IsNullOrEmpty(add_line) == false)
                builder.AppendLine(add_line);
        }

        return builder.ToString();
    }
}

//----------------------------------------------------------------------------------
public class TBL_Talk : TableInfoBase
{
    public List<TalkInfo> TalkList { get; private set; }

    public class TalkInfo
    {
        public BotCommandType CommandEventType { get; private set; }
        public int IDN { get; private set; }
        public string CommandEventGroup { get; private set; }
        public string CommandEventCondition { get; private set; }
        public string MessageKey { get; private set; }

        //----------------------------------------------------------------------------------
        public void Setup(XmlSelector node, TBL_Talk owner)
        {
            IDN = XmlUtil.ParseAttribute<int>(node, "IDN", owner.IDN);
            CommandEventType = XmlUtil.ParseAttribute<BotCommandType>(node, "CommandType", BotCommandType.None);
            CommandEventGroup = XmlUtil.ParseAttribute<string>(node, "EventGroup", "");
            CommandEventCondition = XmlUtil.ParseAttribute<string>(node, "EventCondition", "");
            MessageKey = XmlUtil.ParseAttribute<string>(node, "MessageKey", owner.ID);
        }
    }

    //----------------------------------------------------------------------------------
    public override void Load(XmlSelector node)
    {
        if (TalkList == null)
            TalkList = new List<TalkInfo>();

        TalkInfo talk_info = new TalkInfo();
        talk_info.Setup(node, this);
        TalkList.Add(talk_info);
    }

    //----------------------------------------------------------------------------------
    public override void LoadAppend(XmlSelector node)
    {
        TalkInfo talk_info = new TalkInfo();
        talk_info.Setup(node, this);
        TalkList.Add(talk_info);
    }
}
