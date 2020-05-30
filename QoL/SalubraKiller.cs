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
    public class SalubraKiller : FauxMod
    {
        public override void Initialize()
        {
            ModHooks.Instance.AfterSavegameLoadHook += AddSaveGame;
            ModHooks.Instance.NewGameHook += AddComponent;

            if (GameManager.instance && !GameManager.instance.gameObject.GetComponent<SalubraBehaviour>())
                AddComponent();
        }

        private static void AddSaveGame(SaveGameData data) => AddComponent();

        private static void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<SalubraBehaviour>();
        }

        public override void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AddSaveGame;
            ModHooks.Instance.NewGameHook -= AddComponent;

            if (GameManager.instance)
                UObject.Destroy(GameManager.instance.gameObject.GetComponent<SalubraBehaviour>());
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