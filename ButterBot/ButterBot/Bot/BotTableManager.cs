//////////////////////////////////////////////////////////////////////////
//
// BotTableManager
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
using System;
using System.Collections.Generic;
using System.IO;

public class BotTableManager : Singleton<BotTableManager>
{
    //----------------------------------------------------------------------------------
    public void Init()
    {
        TBL_TalkManager.Instance.RegistClient(false, false, true);
        TBL_CommandManager.Instance.RegistClient(false, false, true);
        Reload();
    }

    //----------------------------------------------------------------------------------
    public void Reload()
    {
        List<string> data_id_list = DataListManager.Instance.GetDataIDList();
        for (int i = 0; i < data_id_list.Count; i++)
        {
            LoadTable(data_id_list[i]);
        }

        I18NTextMultiLanguage.Instance.StaticPath = Path.Combine(BotUtils.GetCurrentDirectoryPath(), "DATA/Table/_I18NText");
        I18NTextMultiLanguage.Instance.ReloadData();
    }

    //----------------------------------------------------------------------------------
    private void LoadTable(string data_id)
    {
        SHLog.Log($"[Load] Load Start {data_id} Table!");

        bool use_binary = DataListManager.Instance.UseXmlBinary(data_id);
        string res_path = Path.Combine(BotUtils.GetCurrentDirectoryPath(), "DATA", TableInfoBase.DEFAULT_PATH_ROOT, data_id);

        bool is_binary = false;
        string load_path = "";
        if (use_binary == true)
        {
            string path_by_bin = $"{res_path}{TableInfoBase.EXTENSION_XML_BINARY}";
            if (File.Exists(path_by_bin) == true)
            {
                load_path = path_by_bin;
                is_binary = true;
            }
        }

        bool is_encrypt = false;
        if (string.IsNullOrEmpty(load_path) == true)
        {
            string path_by_encrypt_xml = $"{res_path}{TableInfoBase.EXTENSION_XML_ENCRYPT}";
            if (File.Exists(path_by_encrypt_xml) == true)
            {
                load_path = path_by_encrypt_xml;
                is_encrypt = true;
            }
        }

        if (string.IsNullOrEmpty(load_path) == true)
        {
            string path_by_encrypt_bin = $"{res_path}{TableInfoBase.EXTENSION_XML_BINARY_ENCRYPT}";
            if (File.Exists(path_by_encrypt_bin) == true)
            {
                load_path = path_by_encrypt_bin;
                is_binary = true;
                is_encrypt = true;
            }
        }

        if (string.IsNullOrEmpty(load_path) == true)
        {
            string path_xml = $"{res_path}{TableInfoBase.EXTENSION_XML}";
            if (File.Exists(path_xml) == true)
                load_path = path_xml;
        }

        if (string.IsNullOrEmpty(load_path) == true)
        {
            SHLog.LogError($"[Error] Not found {data_id} table...");
            return;
        }

        bool load_success = false;
        try
        {
            if (is_binary == true)
            {
                _LoadTable(data_id, null, load_path, true, is_encrypt ? GlobalData.GET_EKEY : "");
            }
            else
            {
                string xml_str = "";
                if (is_encrypt == true)
                    xml_str = XmlUtil.LoadXmlStrFromEncryptFile(load_path, GlobalData.GET_EKEY);
                else
                    xml_str = XmlUtil.LoadXmlStrFromFile(load_path);

                if (string.IsNullOrEmpty(xml_str) == false)
                    load_success = _LoadTable(data_id, xml_str);
            }
        }
        catch (Exception e)
        {
            SHLog.LogError($"[ERROR] Exception occurred from LoadTable, Exception: {e}");
        }

        if (load_success == true)
            SHLog.Log($"[Completed] Load Complete {data_id} table!");
        else
            SHLog.LogError($"[Error] Failed Load {data_id} table...");
    }

    //----------------------------------------------------------------------------------
    private bool _LoadTable(string data_id, string xml_str)
    {
        return DataListManager.Instance.Load(data_id, xml_str);
    }

    //----------------------------------------------------------------------------------
    private bool _LoadTable(string data_id, byte[] bytes, string file_path, bool is_binary, string encrypt_key)
    {
        return DataListManager.Instance.Load(data_id, bytes, file_path, is_binary, encrypt_key);
    }
}
