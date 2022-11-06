using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Modding;
using Newtonsoft.Json;

namespace QoL
{
    internal class I18n
    {
        private static Dictionary<string, string>? Entries;
        
        public static bool Available => Entries is not null;
        
        static I18n()
        {
            Assembly? asm = typeof(I18n).Assembly;
            
            string lang = Language.Language.CurrentLanguage().ToString().ToLower();
            string? path = Path.GetDirectoryName(asm.Location);
            
            if (TryLoadFromFile(path, lang)) 
                return;

            using Stream? stream = asm.GetManifestResourceStream($"QoL.Locales.{lang}.json");
            
            if (stream is null) 
                return;
            
            byte[] bs = new byte[stream.Length];
            
            if (stream.Read(bs, 0, bs.Length) != bs.Length)
                throw new InvalidOperationException();
            
            ParseLanguage(Encoding.UTF8.GetString(bs), lang);
        }

        private static void ParseLanguage(string data, string lang)
        {
            try
            {
                Entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            }
            catch (Exception e)
            {
                Logger.LogError($"[QoL]: Failed to parse {lang} locale! {e}");
            }
        }
        
        private static bool TryLoadFromFile(string? path, string lang)
        {
            if (path is null)
                return false;
            
            string file = Path.Combine(path, $"QoL.{lang}.json");

            if (!File.Exists(file)) 
                return false;
            
            ParseLanguage(File.ReadAllText(file), lang);
            
            return true;
        }

        public static string Get(string key)
        {
            if (Entries is null) 
                return key;
            
            return Entries.TryGetValue(key, out string? v) ? v : key;
        }
    }
}
