using System;
using System.Collections.Generic;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using Modding;
using FsmUtil;
using InControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL
{
    public class MouseBindings : Mod
    {
        public override void Initialize()
        {
            On.InControl.PlayerAction.ctor += Cancer;
        }

        // Thanks, TC.
        private void Cancer(On.InControl.PlayerAction.orig_ctor orig, PlayerAction self, string name,
            PlayerActionSet owner)
        {
            orig(self, name, owner);
            IList<BindingSource> gay = self.Bindings.GetAttr<IList<BindingSource>>("list");
            if (gay == null) return;
            gay.Add(new MouseBindingSource(Mouse.Button4));
            gay.Add(new MouseBindingSource(Mouse.Button5));
            gay.Add(new MouseBindingSource(Mouse.Button6));
            self.Bindings.SetAttr("list", gay);
        }
    }
}