﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine;
using SimpleJson;

namespace Game
{
    public static class LanguageControl
    {
        public static Dictionary<string, string> items;
        public enum LanguageType
        {
            zh_cn
        }
        public static void init(LanguageType languageType)
        {
            if (items != null)
            {
                items.Clear();
            }
            else
            {
                items = new Dictionary<string, string>();
            }
            Stream ssa = Storage.OpenFile("app:lang/" + languageType.ToString() + ".lang", OpenFileMode.Read);
            MemoryStream memoryStream = new MemoryStream();
            byte[] data;
            if (!ssa.CanSeek)
            {
                ssa.CopyTo(memoryStream);
                data = memoryStream.ToArray();
                ssa.Dispose();
            }
            else
            {
                ssa.CopyTo(memoryStream);
                data = memoryStream.ToArray();
                ssa.Dispose();
            }
            if (data != null)
            {//加载原版语言包
                string txt = System.Text.Encoding.UTF8.GetString(data);
                txt = txt.Substring(1, txt.Length - 1);
                JsonObject obj = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(txt);
                foreach (KeyValuePair<string, object> lla in obj.ToArray())
                {
                    if (items.ContainsKey(lla.Key)) items.Add(lla.Key, lla.Value.ToString());//遇到重复自动覆盖
                    else items[lla.Key] = lla.Value.ToString();
                }
            }
            List<FileEntry> langs = ModsManager.GetEntries(".lang");
            foreach (FileEntry entry in langs)
            {
                string filename = Storage.GetFileName(entry.Filename);
                if (filename.StartsWith(languageType.ToString()))
                { //加载该语言包
                    JsonObject obj = (JsonObject)WebManager.JsonFromBytes(ModsManager.StreamToBytes(entry.Stream));
                    foreach (KeyValuePair<string, object> lla in obj.ToArray())
                    {
                        if (items.ContainsKey(lla.Key)) items.Add(lla.Key, lla.Value.ToString());//遇到重复自动覆盖
                        else items[lla.Key] = lla.Value.ToString();
                    }
                }
            }

        }
        public static string getShow(LanguageType language)
        {
            int d = (int)language;
            string[] list = new string[] { "中文", "English" };
            return list[d];
        }
        public static string getTranslate(string key)
        {//获得键值
            items.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value)) return key;
            return value;
        }
    }
}