using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine;
using SimpleJson;

namespace Game
{
    public static class LanguageControl
    {
        public static Dictionary<string, Dictionary<string, string>> items;
        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>> items2;

        public enum LanguageType
        {
            zh_cn,
            en_us
        }
        public static void init(LanguageType languageType)
        {
            items = new Dictionary<string, Dictionary<string, string>>();
            items2 = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            Stream ssa = Storage.OpenFile("app:lang/" + languageType.ToString() + ".json", OpenFileMode.Read);
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
                foreach (KeyValuePair<string, object> lla in obj)
                {
                    JsonObject json = (JsonObject)lla.Value;
                    Dictionary<string, string> values = new Dictionary<string, string>();
                    Dictionary<string, Dictionary<string, string>> values2 = new Dictionary<string, Dictionary<string, string>>();
                    foreach (KeyValuePair<string, object> llb in json)
                    {
                        JsonObject json2 = llb.Value as JsonObject;
                        if (json2 != null)
                        {
                            Dictionary<string, string> values3 = new Dictionary<string, string>();
                            foreach (KeyValuePair<string, object> llc in json2)
                            {
                                if (values3.ContainsKey(llc.Key)) values3[llc.Key] = llc.Value.ToString();
                                else values3.Add(llc.Key, llc.Value.ToString());
                            }
                            if (values2.ContainsKey(llb.Key)) values2[llb.Key] = values3;
                            else values2.Add(llb.Key, values3);
                        }
                        else
                        {
                            if (values.ContainsKey(llb.Key)) values.Add(llb.Key, llb.Value.ToString());//遇到重复自动覆盖
                            else values[llb.Key] = llb.Value.ToString();
                        }
                    }
                    if (items.ContainsKey(lla.Key)) items[lla.Key] = values;
                    else items.Add(lla.Key, values);
                    if (values2.Count > 0)
                    {
                        if (items2.ContainsKey(lla.Key)) items2[lla.Key] = values2;
                        else items2.Add(lla.Key, values2);
                    }
                }
            }
            List<FileEntry> langs = ModsManager.GetEntries(".lang");
            foreach (FileEntry entry in langs)
            {
                string filename = Storage.GetFileName(entry.Filename);
                if (filename.StartsWith(languageType.ToString()))
                { //加载该语言包
                    JsonObject obj = (JsonObject)WebManager.JsonFromBytes(ModsManager.StreamToBytes(entry.Stream));
                    foreach (KeyValuePair<string, object> lla in obj)
                    {
                        JsonObject json = (JsonObject)lla.Value;
                        Dictionary<string, string> values = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, object> llb in json)
                        {
                            if (values.ContainsKey(llb.Key)) values.Add(llb.Key, llb.Value.ToString());//遇到重复自动覆盖
                            else values[lla.Key] = llb.Value.ToString();
                        }
                        if (items.ContainsKey(lla.Key)) items[lla.Key] = values;
                        else items.Add(lla.Key, values);
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
        public static string Get(string className, int key)
        {//获得键值
            return Get(className,key.ToString());
        }

        public static string Get(string className, string key)
        {//获得键值
            if (items.TryGetValue(className, out Dictionary<string, string> item))
            {
                if (item.TryGetValue(key, out string value))
                {
                    if (string.IsNullOrEmpty(value)) return key.ToString();
                    else return value;
                }
            }
            return string.Format("NF[Get][{0}][{1}]", className, key);
        }
        public static string GetBlock(string name, string prop)
        {
            string[] hn = name.Split(new char[] { ':' });
            if (items2.TryGetValue("Blocks", out Dictionary<string, Dictionary<string, string>> ma))
            {
                if (ma.TryGetValue(name, out Dictionary<string, string> mb))
                {
                    if (mb.TryGetValue(prop, out string mc))return mc;
                } else if (ma.TryGetValue(hn[0] + ":0", out Dictionary<string, string> mbc)) {
                    if (mbc.TryGetValue(prop, out string mc)) return mc;
                }

            }
            return string.Format("NF[Block][{0}][{1}]", name, prop);
        }
        public static string GetContentWidgets(string name, string prop)
        {
            if (items2.TryGetValue("ContentWidgets", out Dictionary<string, Dictionary<string, string>> ma))
            {
                if (ma.TryGetValue(name, out Dictionary<string, string> mb))
                    if (mb.TryGetValue(prop, out string mc))return mc;
            }
            return string.Format("NF[CW][{0}][{1}]", name, prop);
        }
        public static string GetDatabase(string name, string prop)
        {
            if (items2.TryGetValue("Database", out Dictionary<string, Dictionary<string, string>> ma))
            {
                if (ma.TryGetValue(name, out Dictionary<string, string> mb)) {
                    if (mb.TryGetValue(prop, out string mc)) return mc;
                }
            }
            return string.Format("NF[DB][{0}][{1}]", name, prop);
        }
        public static string GetFireworks(string name, string prop)
        {
            if (items2.TryGetValue("FireworksBlock", out Dictionary<string, Dictionary<string, string>> ma))
            {
                if (ma.TryGetValue(name, out Dictionary<string, string> mb))
                    if (mb.TryGetValue(prop, out string mc)) return mc;
            }
            return string.Format("NF[FW][{0}][{1}]", name, prop);
        }

    }
}
