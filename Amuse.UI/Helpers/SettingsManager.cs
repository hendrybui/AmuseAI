using Amuse.UI.Models;
using OnnxStack.Common.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amuse.UI.Helpers
{
    public class SettingsManager
    {

        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static AmuseSettings LoadSettings()
        {
            var appDefaultsFile = Path.Combine(App.DataDirectory, "appdefaults.json");
            var appDefaultsBakupFile = Path.Combine(App.DataDirectory, "appdefaults.backup");
            var appSettingsFile = Path.Combine(App.DataDirectory, "appsettings.json");

            try
            {
                // Is New Install
                if (File.Exists(appDefaultsFile) && !File.Exists(appSettingsFile))
                {
                    File.Copy(appDefaultsFile, appSettingsFile);
                    return LoadConfiguration<AmuseSettings>(appSettingsFile);
                }

                // Is Update
                if (File.Exists(appDefaultsFile) && File.Exists(appSettingsFile))
                {
                    // merge
                    var appDefaults = LoadConfiguration<AmuseSettings>(appDefaultsFile);
                    var appSettings = LoadConfiguration<AmuseSettings>(appSettingsFile);
                    return MergeSettings(appSettingsFile, appSettings, appDefaults);
                }

                // Is Installed
                if (File.Exists(appSettingsFile))
                    return LoadConfiguration<AmuseSettings>(appSettingsFile);

                // Both files missing
                throw new FileNotFoundException(appSettingsFile);
            }
            catch
            {
                File.Copy(appDefaultsFile, appSettingsFile, true);
                return LoadConfiguration<AmuseSettings>(appSettingsFile);
            }
            finally
            {
                if (File.Exists(appDefaultsFile))
                    File.Move(appDefaultsFile, appDefaultsBakupFile, true);
            }
        }


        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration">The configuration.</param>
        public static void SaveSettings(AmuseSettings settings)
        {
            SaveConfiguration(App.DataDirectory, typeof(AmuseSettings).Name, settings);
        }


        /// <summary>
        /// Merges the settings.
        /// </summary>
        /// <param name="currentSettings">The user settings.</param>
        /// <param name="defaultSettings">The default settings.</param>
        /// <returns></returns>
        private static AmuseSettings MergeSettings(string appSettingsFile, AmuseSettings currentSettings, AmuseSettings defaultSettings)
        {
            if (defaultSettings.FileVersion != currentSettings.FileVersion)
            {
                // No Update Path
                BackupFile(appSettingsFile);
                defaultSettings.Initialize();
                return defaultSettings;
            }

            // Map all user settings to new defaults
            var properties = typeof(AmuseSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var isDefaultValue = Attribute.IsDefined(property, typeof(AppDefaultAttribute));
                if (isDefaultValue)
                {
                    // Merge Templates
                    if (property.Name == nameof(defaultSettings.Templates))
                    {
                        foreach (var currentTemplate in currentSettings.Templates)
                        {
                            var defaultTemplate = defaultSettings.Templates.FirstOrDefault(x => x.Id == currentTemplate.Id);
                            if (defaultTemplate is not null)
                            {
                                if (currentTemplate.FileVersion != defaultTemplate.FileVersion)
                                {
                                    defaultTemplate.IsUpdateAvailable = true;
                                    continue;
                                }
                                defaultTemplate.IsUpdateAvailable = currentTemplate.IsUpdateAvailable;
                                continue;
                            }

                            // Add back any templates the user has created/imported
                            defaultSettings.Templates.Add(currentTemplate);
                        }
                    }
                    continue;
                }

                var defaultValue = property.GetValue(defaultSettings);
                var existingValue = property.GetValue(currentSettings);
                if (existingValue != null)
                    property.SetValue(defaultSettings, existingValue);
            }

            defaultSettings.Initialize();
            return defaultSettings;
        }


        /// <summary>
        /// Loads a custom IConfigSection object from appsetting.json
        /// </summary>
        /// <typeparam name="T">The custom IConfigSection class type, NOTE: json section name MUST match class name</typeparam>
        /// <returns>The deserialized custom configuration object</returns>
        private static T LoadConfiguration<T>(string filePath, string sectionName = null, params JsonConverter[] converters) where T : class, IConfigSection
        {
            return LoadConfigurationSection<T>(filePath, sectionName, converters);
        }


        /// <summary>
        /// Loads a configuration section.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converters">The converters.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Failed to parse json element</exception>
        private static T LoadConfigurationSection<T>(string filePath, string sectionName, params JsonConverter[] converters) where T : class, IConfigSection
        {
            var name = sectionName ?? typeof(T).Name;
            var serializerOptions = GetSerializerOptions(converters);
            var jsonDocument = GetJsonDocument(filePath, serializerOptions);
            var configElement = jsonDocument.RootElement.GetProperty(name);
            var configuration = configElement.Deserialize<T>(serializerOptions)
                ?? throw new Exception($"Failed to parse {name} json element");
            configuration.Initialize();
            return configuration;
        }


        /// <summary>
        /// Gets and loads the appsettings.json document and caches it
        /// </summary>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.Exception">Failed to parse appsetting document</exception>
        private static JsonDocument GetJsonDocument(string filePath, JsonSerializerOptions serializerOptions)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            using var appsettingStream = File.OpenRead(filePath);
            var appSettingsDocument = JsonSerializer.Deserialize<JsonDocument>(appsettingStream, serializerOptions)
                  ?? throw new Exception("Failed to parse appsetting document");

            return appSettingsDocument;
        }


        /// <summary>
        /// Gets the serializer options.
        /// </summary>
        /// <param name="jsonConverters">The json converters.</param>
        /// <returns>JsonSerializerOptions</returns>
        private static JsonSerializerOptions GetSerializerOptions(params JsonConverter[] jsonConverters)
        {
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new JsonStringEnumConverter());
            if (jsonConverters is not null)
            {
                foreach (var jsonConverter in jsonConverters)
                    serializerOptions.Converters.Add(jsonConverter);
            }
            return serializerOptions;
        }


        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="configuration">The configuration.</param>
        private static void SaveConfiguration<T>(string filePath, string sectionName, T configuration) where T : class, IConfigSection
        {
            var tempJson = Path.Combine(filePath, "appsettings.temp");
            var settingsJson = Path.Combine(filePath, "appsettings.json");

            // Read In File
            Dictionary<string, object> appSettings;
            using (var appsettingReadStream = File.OpenRead(settingsJson))
                appSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(appsettingReadStream, GetSerializerOptions());

            // Set Object
            appSettings[sectionName] = configuration;

            // Write out file
            var serializerOptions = GetSerializerOptions();
            serializerOptions.WriteIndented = true;
            using (var appsettingWriteStream = File.Open(tempJson, FileMode.Create))
                JsonSerializer.Serialize(appsettingWriteStream, appSettings, serializerOptions);

            // Atomic overwite
            File.Move(tempJson, settingsJson, true);
        }


        private static void BackupFile(string filePath)
        {
            try
            {
                File.Copy(filePath, filePath.Replace(".json", ".backup"), true);
            }
            catch { }
        }
    }
}
