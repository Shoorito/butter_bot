using System;
using System.Collections.Generic;
using System.IO;

public static class BotUtils
{
    private static Random sUtilRandom = new Random();

    //----------------------------------------------------------------------------------
    public static string GetCurrentDirectoryPath()
    {
        return Directory.GetCurrentDirectory();
    }

    //----------------------------------------------------------------------------------
    public static string GetRandomTalkText(List<string> talk_list)
    {
        if (talk_list == null || talk_list.Count <= 0)
            return "";

        int rand_idx = sUtilRandom.Next(talk_list.Count);
        return talk_list[rand_idx];
    }
}
