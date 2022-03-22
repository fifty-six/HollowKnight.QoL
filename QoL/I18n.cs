using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QoL
{
    internal class I18n
    {
        public static Dictionary<string, string>? dict = null;
        private static void ParseLanguage(string data)
        {
            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            }catch(Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }
        static I18n()
        {
            var ass = typeof(I18n).Assembly;
            var currentLanguage = Language.Language.CurrentLanguage();
            var languageCode = currentLanguage.ToString().ToLower();
            var path = Path.GetDirectoryName(ass.Location);

            var extLangFile = Path.Combine(path, $"QoL.{languageCode}.json");
            if(File.Exists(extLangFile))
            {
                ParseLanguage(File.ReadAllText(extLangFile));
                return;
            }

            using Stream? lang = ass.GetManifestResourceStream($"QoL.Locales.{languageCode}.json");
            if (lang is not null)
            {
                var bs = new byte[lang.Length];
                lang.Read(bs, 0, bs.Length);
                ParseLanguage(Encoding.UTF8.GetString(bs));
            }
        }
        public static bool needReplace => dict is not null;
        public static string Get(string key)
        {
            if (dict is null) return key;
            if (dict.TryGetValue(key, out var v)) return v;
            return key;
        }
    }
}
