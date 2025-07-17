using System;
using SFS.IO;
using SFS.Parsers.Json;
using ModLoader;

namespace Enhancedsettings
{
    [Serializable]
    public class EnhancedSettingsConfig
    {
        public int aaType = 0; // 0=Off, 1=2x, 2=4x, 3=8x, 4=FXAA
        public int fps = 144;
    }

    public static class SettingsManager
    {
        // 获取Mod文件夹路径
        private static string GetSettingsPath()
        {
            string folder = "Mods/Enhancedsettings";
            if (ModLoader.Loader.main != null)
            {
                foreach (var mod in ModLoader.Loader.main.GetAllMods())
                {
                    if (mod.ModNameID == "EnhancedSettings")
                    {
                        folder = mod.ModFolder;
                        break;
                    }
                }
            }
            return System.IO.Path.Combine(folder, "Settings.txt");
        }

        public static void SaveSettings(EnhancedSettingsConfig settings)
        {
            var settingsPath = new FilePath(GetSettingsPath());
            JsonWrapper.SaveAsJson(settingsPath, settings, true);
        }

        public static EnhancedSettingsConfig LoadSettings()
        {
            var settingsPath = new FilePath(GetSettingsPath());
            EnhancedSettingsConfig settings;
            if (!JsonWrapper.TryLoadJson(settingsPath, out settings))
                settings = new EnhancedSettingsConfig();
            return settings;
        }
    }
} 