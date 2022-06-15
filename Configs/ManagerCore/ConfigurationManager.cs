using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace NavigationDrawerStarter.Configs.ManagerCore
{
    public sealed class ConfigurationManager
    {
        /// <summary>
        /// holds a reference to the single created instance, if any.
        /// </summary>
        private static readonly Lazy<ConfigurationManager> lazy = new Lazy<ConfigurationManager>(() => new ConfigurationManager());

        /// <summary>
        /// Getting reference to the single created instance, creating one if necessary.
        /// </summary>
        public static ConfigurationManager ConfigManager { get; } = lazy.Value;

        public AppConfiguration JSONConfiguration { get; set; }
        private ConfigurationManager()
        {
            JSONConfiguration = this.Read();
        }
        /// <summary>
        /// Read the configuration files and return Configuration Object
        /// </summary>
        private AppConfiguration Read()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "NavigationDrawerStarter.Configs.ConfigBank.json";
            string jsonFile = "";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                jsonFile = reader.ReadToEnd(); //Make string equal to full file
            }

            var configs = JsonConvert.DeserializeObject<AppConfiguration>(jsonFile);

            return configs;
        }
    }
}