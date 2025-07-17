using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic; // Added for List

namespace Enhanced_settings
{
    public class Main : Mod
    {
        public override string ModNameID => "EnhancedSettings";
        public override string DisplayName => "Enhanced settings";
        public override string Author => "SFSGamer";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v1.0.0";
        public override string Description => "Extends the upper limits of some settings";

        static Harmony patcher;

        public override void Load()
        {
        
        }

        public override void Early_Load()
        {
            // 优先加载并应用自定义设置
            var settings = Enhancedsettings.SettingsManager.LoadSettings();
            // 应用抗锯齿
            int msaa = 0;
            if (settings.aaType == 1) msaa = 2;
            else if (settings.aaType == 2) msaa = 4;
            else if (settings.aaType == 3) msaa = 8;
            else msaa = 0;
            UnityEngine.QualitySettings.antiAliasing = msaa;
            // 应用帧率
            UnityEngine.Application.targetFrameRate = settings.fps;
            // UnityEngine.Debug.Log($"[EnhancedSettings] Early_Load: Applied settings (aaType={settings.aaType}, msaa={msaa}, fps={settings.fps})");

            patcher = new Harmony("sfsgamer.enhancedsettings");
            patcher.PatchAll();
        }
    }

    [HarmonyPatch(typeof(SFS.VideoSettingsPC), "UpdateUI")]
    public static class Patch_VideoSettingsPC_UpdateUI
    {
        static void Postfix(SFS.VideoSettingsPC __instance)
        {
            if (__instance != null && __instance.uiScaleSlider != null)
            {
                __instance.uiScaleSlider.minValue = 0.5f; // 50%
                __instance.uiScaleSlider.maxValue = 1.5f;  // 150%
            }
        }
    }

    [HarmonyPatch(typeof(SFS.VideoSettingsPC), "Init")]
    public static class Patch_AntiAliasingDropdown
    {
        static void Postfix(SFS.VideoSettingsPC __instance)
        {
            // 抗锯齿
            if (__instance != null && __instance.antiAliasingDropdown != null)
            {
                var aaOptions = __instance.antiAliasingDropdown.options;
                aaOptions.Clear();
                aaOptions.Add(new TMP_Dropdown.OptionData("Off"));
                aaOptions.Add(new TMP_Dropdown.OptionData("MSAA 2x"));
                aaOptions.Add(new TMP_Dropdown.OptionData("MSAA 4x"));
                aaOptions.Add(new TMP_Dropdown.OptionData("MSAA 8x"));
                aaOptions.Add(new TMP_Dropdown.OptionData("FXAA"));
                __instance.antiAliasingDropdown.RefreshShownValue();
                
                int aaType = PlayerPrefs.GetInt("EnhancedSettings_AAType", 0);
                __instance.antiAliasingDropdown.value = aaType;

                int msaa = 0;
                if (aaType == 1) msaa = 2;
                else if (aaType == 2) msaa = 4;
                else if (aaType == 3) msaa = 8;
                else msaa = 0;
                QualitySettings.antiAliasing = msaa;
                // UnityEngine.Debug.Log($"[EnhancedSettings] AA type restored in Init: {aaType}, MSAA: {msaa}");
            }

            // 帧率
            if (__instance != null && __instance.fpsDropdown != null)
            {
                var fpsOptions = __instance.fpsDropdown.options;
                if (!fpsOptions.Exists(o => o.text == "360"))
                    fpsOptions.Add(new TMP_Dropdown.OptionData("360"));
                if (!fpsOptions.Exists(o => o.text == "480"))
                    fpsOptions.Add(new TMP_Dropdown.OptionData("480"));
                if (!fpsOptions.Exists(o => o.text == "540"))
                    fpsOptions.Add(new TMP_Dropdown.OptionData("540"));
                if (!fpsOptions.Exists(o => o.text == "720"))
                    fpsOptions.Add(new TMP_Dropdown.OptionData("720"));
                if (!fpsOptions.Exists(o => o.text == "1440"))
                    fpsOptions.Add(new TMP_Dropdown.OptionData("1440"));
                __instance.fpsDropdown.RefreshShownValue();

                var fpsOptionsField = typeof(SFS.VideoSettingsPC).GetField("fpsOptions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fpsOptionsField != null)
                {
                    var fpsOptionsList = fpsOptionsField.GetValue(__instance) as List<string>;
                    if (fpsOptionsList != null)
                    {
                        if (!fpsOptionsList.Contains("360")) fpsOptionsList.Add("360");
                        if (!fpsOptionsList.Contains("480")) fpsOptionsList.Add("480");
                        if (!fpsOptionsList.Contains("540")) fpsOptionsList.Add("540");
                        if (!fpsOptionsList.Contains("720")) fpsOptionsList.Add("720");
                        if (!fpsOptionsList.Contains("1440")) fpsOptionsList.Add("1440");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SFS.VideoSettingsPC), "Apply")]
    public static class Patch_VideoSettingsPC_Apply
    {
        static void Postfix(SFS.VideoSettingsPC __instance)
        {
            // 恢复抗锯齿类型index
            if (__instance != null && __instance.antiAliasingDropdown != null)
            {
                int aaType = PlayerPrefs.GetInt("EnhancedSettings_AAType", 0);
                __instance.antiAliasingDropdown.value = aaType;
                int msaa = 0;
                if (aaType == 1) msaa = 2;
                else if (aaType == 2) msaa = 4;
                else if (aaType == 3) msaa = 8;
                else msaa = 0;
                QualitySettings.antiAliasing = msaa;
                // UnityEngine.Debug.Log($"[EnhancedSettings] AA type restored in Apply: {aaType}, MSAA: {msaa}");
            }
        }
    }

    // [HarmonyPatch(typeof(SFS.VideoSettingsPC), "Apply")]
    // public static class Patch_VideoSettingsPC_Apply
    // {
    //     static void Postfix(SFS.VideoSettingsPC __instance)
    //     {
    //         int msaa = PlayerPrefs.GetInt("EnhancedSettings_MSAA", 0);
    //         QualitySettings.antiAliasing = msaa;
    //         UnityEngine.Debug.Log($"[EnhancedSettings] MSAA restored in Apply: {msaa}");
    //     }
    // }

    [HarmonyPatch(typeof(SFS.VideoSettingsPC), "AntiAliasingDropdownChanged")]
    public static class Patch_AntiAliasingDropdownChanged
    {
        static void Postfix(SFS.VideoSettingsPC __instance, int index)
        {
            // 0: Off, 1: MSAA 2x, 2: MSAA 4x, 3: MSAA 8x, 4: FXAA
            int msaa = 0;
            if (index == 0)
                msaa = 0;
            else if (index == 1)
                msaa = 2;
            else if (index == 2)
                msaa = 4;
            else if (index == 3)
                msaa = 8;
            else if (index == 4)
                msaa = 0; // FXAA controlled by original game
            QualitySettings.antiAliasing = msaa;
            PlayerPrefs.SetInt("EnhancedSettings_AAType", index);
            PlayerPrefs.Save();
            // UnityEngine.Debug.Log($"[EnhancedSettings] MSAA set to: {index}, QualitySettings.antiAliasing: {QualitySettings.antiAliasing}");
            var settings = Enhancedsettings.SettingsManager.LoadSettings();
            settings.aaType = index;
            Enhancedsettings.SettingsManager.SaveSettings(settings);
            // UnityEngine.Debug.Log($"[EnhancedSettings] Saved aaType={index} to config");
            // 保存设置
            var saveMethod = typeof(SFS.VideoSettingsPC).GetMethod("Save", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (saveMethod != null)
                saveMethod.Invoke(__instance, null);
        }
    }

    [HarmonyPatch(typeof(SFS.VideoSettingsPC), "FpsDropdownChanged")]
    public static class Patch_FpsDropdownChanged
    {
        static void Postfix(SFS.VideoSettingsPC __instance, int index)
        {
            if (__instance != null && __instance.fpsDropdown != null)
            {
                
                if (index < 0 || index >= __instance.fpsDropdown.options.Count)
                {
                    // UnityEngine.Debug.LogError($"[EnhancedSettings] FPS index out of range: {index}");
                    return;
                }
                string text = __instance.fpsDropdown.options[index].text;
                int fps = -1;
                if (text == "360") fps = 360;
                else if (text == "480") fps = 480;
                else if (text == "540") fps = 540;
                else if (text == "720") fps = 720;
                else if (text == "1440") fps = 1440;
                else if (text == "Unlimited") fps = -1;
                else int.TryParse(text, out fps);
                __instance.settings.fps = fps;
                Application.targetFrameRate = fps;
                // UnityEngine.Debug.Log($"[EnhancedSettings] FPS set to: {fps}, Application.targetFrameRate: {Application.targetFrameRate}");
                // 保存配置
                var settings = Enhancedsettings.SettingsManager.LoadSettings();
                settings.fps = fps;
                Enhancedsettings.SettingsManager.SaveSettings(settings);
                var saveMethod = typeof(SFS.VideoSettingsPC).GetMethod("Save", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (saveMethod != null)
                    saveMethod.Invoke(__instance, null);
            }
        }
    }
} 