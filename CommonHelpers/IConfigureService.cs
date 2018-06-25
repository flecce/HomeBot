using System;
using System.Configuration;
using System.Linq;

namespace CommonHelpers
{
    public interface IConfigurationService
    {
        string GetConfigValue(string key, string defaultValue);

        string GetRequiredConfigValue(string key);
    }

    //public class AppSettingsJsonConfigurationService : IConfigurationService
    //{
    //    private IConfiguration _configuration;

    //    public AppSettingsJsonConfigurationService()
    //    {
    //        var builder = new ConfigurationBuilder()
    //            .SetBasePath(Directory.GetCurrentDirectory())
    //            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

    //        _configuration = builder.Build();
    //    }

    //    public T GetConfigValue<T>(string key, T defaultValue)
    //    {
    //        return _configuration[key] as T;
    //    }
    //}

    public class AppSettingsXMLConfigurationService : IConfigurationService
    {
        public string GetConfigValue(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings.AllKeys.Any(x => x == key) ? ConfigurationManager.AppSettings[key] : defaultValue;
        }

        public string GetRequiredConfigValue(string key)
        {
            return ConfigurationManager.AppSettings.AllKeys.Any(x => x == key) ? ConfigurationManager.AppSettings[key] : throw new ApplicationException($"Paramentro {key} non trovato");
        }
    }
}