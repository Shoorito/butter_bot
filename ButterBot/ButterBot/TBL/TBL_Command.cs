//////////////////////////////////////////////////////////////////////////
//
// TBL_Command
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

using SHUtils;
using SHUtils.I18N;

//----------------------------------------------------------------------------------
public class TBL_CommandManager : TableBaseManager<TBL_Command, TBL_CommandManager>
{
    public override string DATA_ID => typeof(TBL_Command).ToString();
    public override bool CheckSameIDN => true;
    public override bool CheckSameID => true;

    //----------------------------------------------------------------------------------
    protected override void ParsingRow(XmlSelector row_node)
    {
        CreateIDNBaseParse(row_node);
    }

    //----------------------------------------------------------------------------------
    public BotCommandType GetCommandType(string region_code, string command)
    {
        if (string.IsNullOrEmpty(command) == true)
            return BotCommandType.None;

        string command_id = I18NTextMultiLanguage.Instance.GetDefaultText(region_code, $"{typeof(TBL_Command)}_CommandText", command);
        if (string.IsNullOrEmpty(command_id) == true)
            return BotCommandType.None;

        TBL_Command cmd_info = GetInfoByStrKey(command_id);
        if (cmd_info == null)
            return BotCommandType.None;

        return cmd_info.CommandType;
    }
}

//----------------------------------------------------------------------------------
public class TBL_Command : TableInfoBase
{
    public BotCommandType CommandType;

    public override void Load(XmlSelector node)
    {
        CommandType = XmlUtil.ParseAttribute<BotCommandType>(node, "CommandType", BotCommandType.None);
    }

    public override void LoadAppend(XmlSelector node)
    {
        // DO NOTHING...
    }
}