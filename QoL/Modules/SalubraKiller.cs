using System.Collections;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class SalubraKiller : FauxMod
    {
        public override void Initialize()
        {
            USceneManager.activeSceneChanged += SceneChanged;
        }

        private static void SceneChanged(Scene arg0, Scene arg1)
        {
            if (HeroController.instance == null) return;

            static IEnumerator KillSalubra()
            {
                yield return null;

                GameObject bg = GameObject.Find("Blessing Ghost");

                if (bg == null) yield break;

                bg
                    .LocateMyFSM("Blessing Control")
                    .GetAction<ActivateGameObject>("Start Blessing", 0)
                    .activate
                    .Value = false;
            }

            HeroController.instance.StartCoroutine(KillSalubra());
        }

        public override void Unload()
        {
            USceneManager.activeSceneChanged -= SceneChanged;
        }
    }
}