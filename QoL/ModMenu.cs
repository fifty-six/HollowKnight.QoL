using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using QoL.Modules;
using UnityEngine.UI;

namespace QoL
{
    public static class ModMenu
    {
        private static readonly List<string> togglableModuleNames;

        private static MenuScreen? ModMenuScreen;
        private static MenuScreen? ModuleToggleScreen;
        private static MenuScreen? ModuleFieldToggleScreen;

        public static MenuScreen GetMenuScreen(MenuScreen returnScreen, ModToggleDelegates dels)
        {
            MenuBuilder builder = MenuUtils.CreateMenuBuilderWithBackButton("QoL", returnScreen, out _);

            builder.AddContent
            (
                RegularGridLayout.CreateVerticalLayout(105f),
                c =>
                {
                    c.AddHorizontalOption("QoL", new HorizontalOptionConfig()
                    {
                        Label = "QoL",
                        Options = new[] { "On", "Off" },
                        ApplySetting = (_, i) => dels.SetModEnabled(Convert.ToBoolean(i)),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(dels.GetModEnabled())),
                        CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(returnScreen),
                        Style = HorizontalOptionStyle.VanillaStyle
                    });

                    c.AddMenuButton("Module Toggles", new MenuButtonConfig()
                    {
                        Label = "Module Toggles",
                        Proceed = true,
                        SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(ModuleToggleScreen),
                        CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(returnScreen),
                    });

                    c.AddMenuButton("Module Field Toggles", new MenuButtonConfig()
                    {
                        Label = "Module Field Toggles",
                        Proceed = true,
                        SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(ModuleFieldToggleScreen),
                        CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(returnScreen),
                    });
                }
            );

            ModMenuScreen = builder.Build();

            ModuleToggleScreen = MenuUtils.CreateMenuScreen("Module Toggles", GetModuleToggleMenuData(), ModMenuScreen);
            ModuleFieldToggleScreen = MenuUtils.CreateMenuScreen("Module Field Toggles", GetModuleFieldMenuData(), ModMenuScreen);

            return ModMenuScreen;
        }

        public static List<IMenuMod.MenuEntry> GetModuleToggleMenuData()
        {
            List<IMenuMod.MenuEntry> li = new();

            string[] bools = { "false", "true" };

            foreach (string name in togglableModuleNames)
            {
                if (SettingsOverride.TryGetModuleOverride(name, out _))
                    continue;

                li.Add
                (
                    new IMenuMod.MenuEntry
                    (
                        name,
                        bools,
                        String.Empty,
                        i => QoL.ToggleModule(name, Convert.ToBoolean(i)),
                        () => Convert.ToInt32(QoL._globalSettings.EnabledModules[name])
                    )
                );
            }

            return li;
        }

        public static List<IMenuMod.MenuEntry> GetModuleFieldMenuData()
        {
            List<IMenuMod.MenuEntry> li = new();

            string[] bools = { "false", "true" };

            foreach ((FieldInfo fi, Type t) in QoL._globalSettings.Fields)
            {
                if (fi.FieldType != typeof(bool) || SettingsOverride.TryGetSettingOverride($"{t.Name}:{fi.Name}", out _))
                    continue;

                li.Add
                (
                    new IMenuMod.MenuEntry
                    (
                        Regex.Replace(fi.Name, "([A-Z])", " $1").TrimEnd(),
                        bools,
                        $"Comes from {t.Name}",
                        i => fi.SetValue(null, Convert.ToBoolean(i)),
                        () => Convert.ToInt32(fi.GetValue(null))
                    )
                );
            }

            return li;
        }

        static ModMenu()
        {
            Type[] types = typeof(ModMenu).Assembly.GetTypes();

            togglableModuleNames = types.Where(t => t.IsSubclassOf(typeof(FauxMod)) && t.GetMethod(nameof(FauxMod.Unload))!.DeclaringType != typeof(FauxMod))
                                   .Select(t => t.Name)
                                   .ToList();
        }
    }
}
