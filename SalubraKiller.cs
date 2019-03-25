using System.Reflection;
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
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

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

        private static void AddSaveGame(SaveGameData data) => AddComponent();

        private static void AddComponent()
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
        private GameObject _blessingGhost;

        public void Start()
        {
            USceneManager.activeSceneChanged += ResetScene;
        }

        private void ResetScene(Scene arg0, Scene arg1)
        {
            _blessingGhost = null;
        }

        public void Update()
        {
            if (_blessingGhost != null) return;

            _blessingGhost = GameObject.Find("Blessing Ghost");

            if (_blessingGhost == null) return;

            _blessingGhost
                .LocateMyFSM("Blessing Control")
                .GetAction<ActivateGameObject>("Start Blessing", 0)
                .activate
                .Value = false;
        }
    }
}