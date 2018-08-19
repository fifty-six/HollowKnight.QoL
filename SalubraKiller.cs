using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL
{
    [UsedImplicitly]
    public class SalubraKiller : Mod, ITogglableMod
    {
        public override void Initialize()
        {
            ModHooks.Instance.AfterSavegameLoadHook += AddSaveGame;
            ModHooks.Instance.NewGameHook += AddComponent;

            // in game
            if (HeroController.instance == null && GameManager.instance.gameObject.GetComponent<SalubraBehaviour>() == null)
            {
                AddComponent();
            }
        }

        private void AddSaveGame(SaveGameData data) => AddComponent();

        private void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<SalubraBehaviour>();
        }

        public void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AddSaveGame;
            ModHooks.Instance.NewGameHook -= AddComponent;

            // in game
            if (GameManager.instance != null)
            {
                UObject.Destroy(GameManager.instance.gameObject.GetComponent<SalubraBehaviour>());
            }
        }
    }

    internal class SalubraBehaviour : MonoBehaviour
    {
        private GameObject BlessingGhost;

        public void Start()
        {
            USceneManager.activeSceneChanged += Reset;
        }

        private void Reset(Scene arg0, Scene arg1)
        {
            BlessingGhost = null;
        }

        public void Update()
        {
            if (BlessingGhost != null) return;
            BlessingGhost = GameObject.Find("Blessing Ghost");
            if (BlessingGhost == null) return;
            BlessingGhost
                .LocateMyFSM("Blessing Control")
                .GetAction<ActivateGameObject>("Start Blessing", 0)
                .activate
                .Value = false;
        }
    }
}