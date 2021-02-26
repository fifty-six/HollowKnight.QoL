using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.UI;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class FastMenus : FauxMod
    {
        private readonly (Type, string, ILContext.Manipulator)[] ILHooks =
        {
            (typeof(UIManager), "<HideSaveProfileMenu>d__215", DecreaseWait),
            (typeof(UIManager), "<HideCurrentMenu>d__211", DecreaseWait),
            (typeof(UIManager), "<HideMenu>d__213", DecreaseWait),
            (typeof(UIManager), "<ShowMenu>d__212", DecreaseWait),
            (typeof(UIManager), "<GoToProfileMenu>d__152", DecreaseWait),
            (typeof(GameManager), "<PauseGameToggle>d__338", PauseGameToggle),
            (typeof(GameManager), "<RunContinueGame>d__349", RunContinueGame),
            (typeof(SaveSlotButton), "<AnimateToSlotState>d__68", DecreaseWait),
        };

        private readonly List<ILHook> _hooked = new List<ILHook>();

        public override void Initialize()
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            
            foreach ((Type t, string nested, ILContext.Manipulator method) in ILHooks)
            {
                Log(nested);
                
                _hooked.Add
                (
                    new ILHook
                    (
                        t.GetNestedType(nested, flags).GetMethod("MoveNext", flags),
                        method
                    )
                );
            }

            On.UIManager.FadeInCanvasGroupAlpha += FadeInCanvasGroupAlpha;
            On.UIManager.FadeOutCanvasGroup += FadeOutCanvasGroup;
            On.UIManager.FadeInSprite += FadeInSprite;
            On.UIManager.FadeOutSprite += FadeOutSprite;
            On.UnityEngine.UI.SaveSlotButton.FadeInCanvasGroupAfterDelay += FadeInAfterDelay;
        }

        public override void Unload()
        {
            foreach (ILHook hook in _hooked)
            {
                hook?.Dispose();
            }

            _hooked.Clear();

            On.UIManager.FadeInCanvasGroupAlpha -= FadeInCanvasGroupAlpha;
            On.UIManager.FadeOutCanvasGroup -= FadeOutCanvasGroup;
            On.UIManager.FadeInSprite -= FadeInSprite;
            On.UIManager.FadeOutSprite -= FadeOutSprite;
            On.UnityEngine.UI.SaveSlotButton.FadeInCanvasGroupAfterDelay -= FadeInAfterDelay;
        }

        private static void RunContinueGame(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext(x => x.MatchLdcR4(2.6f)))
            {
                // We need to wait a bit for the animator.
                cursor.Next.Operand = 0.05f;
            }
        }

        private static void PauseGameToggle(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext(x => x.MatchLdcR4(out float _)))
            {
                // Don't change the SetTimescale(1f) call.
                if (Mathf.Abs((float) cursor.Next.Operand - 1) < Mathf.Epsilon)
                    continue;

                cursor.Next.Operand = 0f;
            }
        }

        private static void DecreaseWait(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext(x => x.MatchLdcR4(out _)))
            {
                cursor.Next.Operand = 0f;
            }
        }

        private static IEnumerator FadeOutSprite(On.UIManager.orig_FadeOutSprite orig, UIManager self, SpriteRenderer sprite)
        {
            sprite.color = new Color
            (
                sprite.color.r,
                sprite.color.g,
                sprite.color.b,
                0
            );

            yield break;
        }

        private static IEnumerator FadeInSprite(On.UIManager.orig_FadeInSprite orig, UIManager self, SpriteRenderer sprite)
        {
            sprite.color = new Color
            (
                sprite.color.r,
                sprite.color.g,
                sprite.color.b,
                1
            );

            yield break;
        }

        private static IEnumerator FadeInAfterDelay(On.UnityEngine.UI.SaveSlotButton.orig_FadeInCanvasGroupAfterDelay orig, SaveSlotButton self, float delay, CanvasGroup cg)
        {
            cg.gameObject.SetActive(true);
            cg.alpha = 1;
            cg.interactable = true;

            yield break;
        }

        private static IEnumerator FadeOutCanvasGroup(On.UIManager.orig_FadeOutCanvasGroup orig, UIManager self, CanvasGroup cg)
        {
            cg.interactable = false;
            cg.alpha = 0f;
            cg.gameObject.SetActive(false);

            yield break;
        }

        private static IEnumerator FadeInCanvasGroupAlpha(On.UIManager.orig_FadeInCanvasGroupAlpha orig, UIManager self, CanvasGroup cg, float end)
        {
            cg.gameObject.SetActive(true);
            cg.alpha = end;
            cg.interactable = true;

            yield break;
        }
    }
}