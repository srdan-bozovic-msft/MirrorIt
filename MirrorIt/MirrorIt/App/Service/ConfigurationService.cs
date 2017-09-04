using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MirrorIt.App.Model;
using System.IO;
using System.Globalization;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace MirrorIt.App.Service
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _appConfigPath;
        private readonly string _executablePath;
        private readonly XmlSerializer _xmlSerializer;

        public ConfigurationService(string basePath, string executablePath)
        {
            _appConfigPath = Path.Combine(basePath, "config.txt");
            _executablePath = executablePath;
            _xmlSerializer = new XmlSerializer(typeof(Configuration));
        }

        public Configuration Load()
        {
            if (File.Exists(_appConfigPath))
            {
                using (var reader = new StreamReader(_appConfigPath))
                {
                    return (Configuration)_xmlSerializer.Deserialize(reader);
                }
            }
            else
            {
                var result = new Configuration
                {
                    RunAppOnStartup = false,
                    MirrorInterval = 1,
                    MirrorConfiguration = new MirrorConfiguration()
                };
                return result;
            }
        }

        public bool Store(Configuration configuration)
        {
            if (SetRunAppOnStartup(configuration.RunAppOnStartup))
            {
                try
                {
                    using (var writer = new StreamWriter(_appConfigPath))
                    {
                        _xmlSerializer.Serialize(writer, configuration);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool SetRunAppOnStartup(bool run)
        {
            try
            {
                using (var main = Registry.CurrentUser)
                {
                    using (var key = main.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", run))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(_executablePath);

                        if (key.GetValue(fileName) == null)
                            key.SetValue(fileName, _executablePath);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
