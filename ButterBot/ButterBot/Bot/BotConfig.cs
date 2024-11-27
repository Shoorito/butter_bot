//////////////////////////////////////////////////////////////////////////
//
// BotConfig
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

public class BotConfig : Singleton<BotConfig>
{
    public string BotToken { get; private set; }
    public bool UseUsableChannelList { get; private set; }
    public List<string> UsableChannelList { get; private set; }
    public string BotDBConnectionStr { get; private set; } = "";
    public string Administrator => mAdministrator;

    private string mConfigPath = "";
    private string mAdministrator = "";
    private XmlDocument mConfigDoc = null;
    private Dictionary<string, RegionInfo> mCmdPrefixRegionDic = new Dictionary<string, RegionInfo>();

    public const string DIR_CONFIG = "DATA/Config";

    //----------------------------------------------------------------------------------
    public void Load(bool is_reload = false)
    {
        SHLog.Log("[INFO] Start Load BotConfig...");

        mConfigPath = Path.Combine(BotUtils.GetCurrentDirectoryPath(), DIR_CONFIG, "BotConfig.xml");
        if (File.Exists(mConfigPath) == false)
        {
            SHLog.LogError("[ERROR] Not found \"BotConfig.xml\".. please check config path..");
            return;
        }

        mConfigDoc = XmlUtil.LoadXmlFromFile(mConfigPath);
        if (mConfigDoc == null)
        {
            SHLog.LogError("[ERROR] An error occurred while loading 'BotConfig.xml' as an XmlDocument.");
            return;
        }

        XmlNodeList root_element_list = mConfigDoc.GetElementsByTagName("ButterBotConfig");
        if (is_reload == false)
        {
            if (root_element_list != null && root_element_list.Count > 0)
            {
                BotToken = XmlUtil.GetNodeAttributeValue<string>(root_element_list[0], "BotToken");
                mAdministrator = XmlUtil.GetNodeAttributeValue<string>(root_element_list[0], "Administrator");
            }
        }

        XmlNodeList prefix_region_infos = mConfigDoc.GetElementsByTagName("PrefixRegionInfos");
        if (prefix_region_infos != null && prefix_region_infos.Count > 0)
        {
            XmlNode prefix_region_nodes_root = prefix_region_infos[0];
            if (prefix_region_nodes_root.HasChildNodes == true)
            {
                XmlNodeList prefix_region_node_list = prefix_region_nodes_root.ChildNodes;
                foreach (XmlNode region_node in prefix_region_node_list)
                {
                    string key = XmlUtil.GetNodeAttributeValue<string>(region_node, "Key");
                    string region_name = XmlUtil.GetNodeAttributeValue<string>(region_node, "Value");
                    RegionInfo region_info = null;
                    if (string.IsNullOrEmpty(region_name) == false)
                        region_info = new RegionInfo(region_name);

                    if (string.IsNullOrEmpty(key) == false && region_info != null)
                        mCmdPrefixRegionDic.Add(key, region_info);
                }
            }
        }

        XmlNodeList usable_channel_node_list = mConfigDoc.GetElementsByTagName("UsableChannels");
        if (usable_channel_node_list != null && usable_channel_node_list.Count > 0)
        {
            XmlNode usable_channel_node_root = usable_channel_node_list[0];
            UseUsableChannelList = XmlUtil.GetNodeAttributeValue<bool>(usable_channel_node_root, "Enable");
            if (UseUsableChannelList == true && usable_channel_node_root.HasChildNodes == true)
            {
                UsableChannelList = new List<string>();

                XmlNodeList channel_list = usable_channel_node_root.ChildNodes;
                foreach (XmlNode channel_node in channel_list)
                {
                    string user = XmlUtil.GetNodeAttributeValue<string>(channel_node, "Name");
                    if (string.IsNullOrEmpty(user) == false)
                        UsableChannelList.Add(user);
                }
            }
        }

        XmlNode db_conn_info_node = XmlUtil.GetNode(mConfigDoc, "DBConnectionInfo");
        if (db_conn_info_node != null)
        {
            string server_name = XmlUtil.GetNodeAttributeValue<string>(db_conn_info_node, "server_name");
            string db_name = XmlUtil.GetNodeAttributeValue<string>(db_conn_info_node, "db_name");
            string user_id = XmlUtil.GetNodeAttributeValue<string>(db_conn_info_node, "user_id");
            string user_pw = XmlUtil.GetNodeAttributeValue<string>(db_conn_info_node, "password");
            BotDBConnectionStr = $"Server={server_name};Database={db_name};User ID={user_id};Password={user_pw};";
        }

        SHLog.Log("[INFO] Complete Load BotConfig...");
    }

    //----------------------------------------------------------------------------------
    public void Reload()
    {
        SHLog.Log("[INFO] Try Reload BotConfig...");

        if (UsableChannelList != null)
            UsableChannelList.Clear();

        if (mCmdPrefixRegionDic != null)
            mCmdPrefixRegionDic.Clear();

        Load(true);
    }

    //----------------------------------------------------------------------------------
    public RegionInfo GetPrefixRegion(string prefix)
    {
        if (mCmdPrefixRegionDic.ContainsKey(prefix) == true)
            return mCmdPrefixRegionDic[prefix];

        return null;
    }
}
