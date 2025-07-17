using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
            }

            // 帧率
            if (__instance != null && __instance.fpsDropdown != null)
            {
                var fpsOptions = __instance.fpsDropdown.options;
                if (!fpsOptions.Exists(o => o.text == "360"))
                    fpsOptions.Add(new TMP_Dropdown.OptionData("360"));
                if (!fpsOptions.Exists(o => o.text == "480"))
                    fpsOptions.Add(new TMP_Dropdown.OptionData("480"));
                __instance.fpsDropdown.RefreshShownValue();
            }
        }
    }

    [HarmonyPatch(typeof(SFS.VideoSettingsPC), "AntiAliasingDropdownChanged")]
    public static class Patch_AntiAliasingDropdownChanged
    {
        static void Postfix(SFS.VideoSettingsPC __instance, int index)
        {
            // 0: Off, 1: MSAA 2x, 2: MSAA 4x, 3: MSAA 8x, 4: FXAA
            if (index == 0)
                QualitySettings.antiAliasing = 0;
            else if (index == 1)
                QualitySettings.antiAliasing = 2;
            else if (index == 2)
                QualitySettings.antiAliasing = 4;
            else if (index == 3)
                QualitySettings.antiAliasing = 8;
            else if (index == 4)
                QualitySettings.antiAliasing = 0; // FXAA由原游戏控制
            // 调用protected Save方法，保存设置
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
                string text = __instance.fpsDropdown.options[index].text;
                int fps = -1;
                if (text == "360") fps = 360;
                else if (text == "480") fps = 480;
                else if (text == "Unlimited") fps = -1;
                else int.TryParse(text, out fps);
                __instance.settings.fps = fps;
                Application.targetFrameRate = fps;
                var saveMethod = typeof(SFS.VideoSettingsPC).GetMethod("Save", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (saveMethod != null)
                    saveMethod.Invoke(__instance, null);
            }
        }
    }
} 