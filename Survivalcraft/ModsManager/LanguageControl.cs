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
        public static Dictionary<string, Dictionary<string,string>> items;        
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
                items = new Dictionary<string, Dictionary<string,string>>();
            }
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
                    string d = lla.Value.ToString();
                    JsonObject json = (JsonObject)lla.Value;
                    Dictionary<string, string> values = new Dictionary<string, string>();
                    foreach (KeyValuePair<string,object> llb in json) {
                        if (values.ContainsKey(llb.Key)) values.Add(llb.Key, llb.Value.ToString());//遇到重复自动覆盖
                        else values[llb.Key] = llb.Value.ToString();
                    }
                    if (items.ContainsKey(lla.Key)) items[lla.Key] = values;
                    else items.Add(lla.Key,values);
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
        public static string Get(string className, string key)
        {//获得键值
            try
            {
                
                items.TryGetValue(className, out Dictionary<string, string> item);
                item.TryGetValue(key, out string value);
                if (string.IsNullOrEmpty(value)) return key;
                return value;
            }
            catch {
                return string.Format("not found [{0}][{1}]",className,key) ;
            }
        }
        public static string Get(string className, int key)
        {//获得键值
            try
            {

                items.TryGetValue(className, out Dictionary<string, string> item);
                item.TryGetValue(key.ToString(), out string value);
                if (string.IsNullOrEmpty(value)) return key.ToString();
                return value;
            }
            catch
            {
                return string.Format("not found [{0}][{1}]", className, key);
            }
        }
    }
}
