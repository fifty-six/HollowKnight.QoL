﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using QoL.Modules;
using UnityEngine.UI;
using Lang = Language.Language;

namespace QoL
{
    public static class ModMenu
    {
        private static readonly string[] Bools = { "false", "true" };
        
        private static readonly List<string> _TogglableModuleNames;

        private static MenuScreen? _ModMenuScreen;
        private static MenuScreen? _ModuleToggleScreen;
        private static MenuScreen? _ModuleFieldToggleScreen;

        public static MenuScreen GetMenuScreen(MenuScreen returnScreen, ModToggleDelegates dels)
        {
            MenuBuilder builder = MenuUtils.CreateMenuBuilderWithBackButton("QoL", returnScreen, out _);

            void Return(MenuSelectable _)         => UIManager.instance.UIGoToDynamicMenu(returnScreen);
            static void Fields(MenuSelectable _)  => UIManager.instance.UIGoToDynamicMenu(_ModuleFieldToggleScreen);
            static void Modules(MenuSelectable _) => UIManager.instance.UIGoToDynamicMenu(_ModuleToggleScreen);
            
            builder.AddContent
            (
                RegularGridLayout.CreateVerticalLayout(105f),
                c =>
                {
                    c.AddHorizontalOption("QoL", new HorizontalOptionConfig
                    {
                        Label = "QoL",
                        Options = new[]
                        {
                            Lang.Get("MOH_OFF", "MainMenu"),
                            Lang.Get("MOH_ON", "MainMenu")
                        },
                        ApplySetting   = (_, i) => dels.SetModEnabled(Convert.ToBoolean(i)),
                        RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(dels.GetModEnabled())),
                        CancelAction = Return,
                        Style = HorizontalOptionStyle.VanillaStyle
                    });

                    c.AddMenuButton("Module Toggles", new MenuButtonConfig
                    {
                        Label = "Module Toggles",
                        Proceed = true,
                        SubmitAction = Modules,
                        CancelAction = Return,
                    });

                    c.AddMenuButton("Module Field Toggles", new MenuButtonConfig
                    {
                        Label = "Module Field Toggles",
                        Proceed = true,
                        SubmitAction = Fields,
                        CancelAction = Return,
                    });
                }
            );

            _ModMenuScreen = builder.Build();

            _ModuleToggleScreen = MenuUtils.CreateMenuScreen
            (
                "Module Toggles",
                GetModuleToggleMenuData(),
                _ModMenuScreen
            );
            _ModuleFieldToggleScreen = MenuUtils.CreateMenuScreen
            (
                "Module Field Toggles",
                GetModuleFieldMenuData(),
                _ModMenuScreen
            );

            return _ModMenuScreen;
        }

        private static List<IMenuMod.MenuEntry> GetModuleToggleMenuData()
        {
            IMenuMod.MenuEntry CreateEntry(string name) => new
            (
                    name,
                    Bools,
                    string.Empty,
                    i => QoL.ToggleModule(name, Convert.ToBoolean(i)),
                    () => Convert.ToInt32(QoL.GlobalSettings.EnabledModules[name])
            );

            return _TogglableModuleNames
                   .Where(x => !SettingsOverride.TryGetModuleOverride(x, out _))
                   .Select(CreateEntry)
                   .ToList();
        }

        private static List<IMenuMod.MenuEntry> GetModuleFieldMenuData()
        {
            static string PascalToSpaces(string s) => Regex.Replace(s, "([A-Z])", " $1").TrimStart();
            
            List<IMenuMod.MenuEntry> li = new ();

            foreach ((FieldInfo fi, Type t) in QoL.GlobalSettings.Fields)
            {
                if (fi.FieldType != typeof(bool) || SettingsOverride.TryGetSettingOverride($"{t.Name}:{fi.Name}", out _))
                    continue;

                li.Add
                (
                    new IMenuMod.MenuEntry
                    (
                        PascalToSpaces(fi.Name),
                        Bools,
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

            static bool Toggleable(Type t) => t.IsSubclassOf(typeof(FauxMod)) && t.GetMethod(nameof(FauxMod.Unload))!.DeclaringType != typeof(FauxMod);

            _TogglableModuleNames = types.Where(Toggleable)
                                        .Select(t => t.Name)
                                        .ToList();
        }
    }
}