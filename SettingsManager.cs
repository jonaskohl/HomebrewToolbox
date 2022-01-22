using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WiiBrewToolbox
{
    public static class SettingsManager
    {
        private static string SavePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.xml");

        private static Dictionary<string, string> settings = new Dictionary<string, string>()
        {
            { "skinFile", "" }
        };

        public static void LoadSettings()
        {
            if (!File.Exists(SavePath))
                return;

            var doc = XDocument.Load(SavePath);

            Debug.Assert(doc.Root.Name == "wtbSettings");
            Debug.Assert(doc.Root.Attribute("version")?.Value == "1");

            var newSettings = doc.Root.Elements("setting").ToDictionary(x => x.Attribute("key").Value, x => x.Value);

            foreach (var s in newSettings)
                settings[s.Key] = s.Value;
        }

        public static void SaveSettings()
        {
            new XDocument(
                new XElement(
                    "wtbSettings",
                    new XAttribute("version", "1"),
                    settings.Select(x => new XElement("setting", new XAttribute("key", x.Key), x.Value ?? ""))
                )
            ).Save(SavePath);
        }

        public static bool Has(string key)
        {
            return settings.ContainsKey(key);
        }

        public static string Get(string key)
        {
            if (!Has(key)) return null;
            return settings[key];
        }

        public static void Set(string key, string value)
        {
            settings[key] = value;
        }
    }
}
