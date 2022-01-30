using System.Collections;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class SaneUumuu : FauxMod
    {
        private NonBouncer _coro = null!;

        public override void Initialize()
        {
            var go = new GameObject();

            _coro = go.AddComponent<NonBouncer>();

            Object.DontDestroyOnLoad(_coro);

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += StartRoutine;
        }

        private void StartRoutine(Scene scene, LoadSceneMode lsm)
        {
            if (scene.name == "Fungus3_archive_02_boss")
                _coro.StartCoroutine(FixUumuu());
        }

        private static IEnumerator FixUumuu()
        {
            yield return null;

            // Find Uumuu and the FSM
            GameObject uumuu = GameObject.Find("Mega Jellyfish");

            if (uumuu == null)
                yield break;

            PlayMakerFSM fsm = uumuu.LocateMyFSM("Mega Jellyfish");

            // Fix the waits and the number of attacks
            fsm.GetState("Idle").GetAction<WaitRandom>().timeMax = 1.5f;
            fsm.GetState("Set Timer").GetAction<RandomFloat>().max = 2f;
            fsm.FsmVariables.GetFsmFloat("Quirrel Time").Value = 4f;

            // Fix the pattern to 2 quick, then 1 long if it still needs to attack
            FsmState choice = fsm.GetState("Choice");
            choice.RemoveAction<SendRandomEventV2>();
            choice.AddMethod(() => SetUumuuPattern(fsm));

            // Reset the multizap counter to 0 so the pattern remains 2 quick 1 optional long
            fsm.GetState("Recover").AddMethod(() => fsm.FsmVariables.GetFsmInt("Ct Multizap").Value = 0);

            // Set the initial RecoilSpeed to 0 so that dream nailing her on the first cycle doesn't push her
            uumuu.GetComponent<Recoil>().SetRecoilSpeed(0);

            // Set her HP to 1028 value
            uumuu.GetComponent<HealthManager>().hp = 250;
        }

        private static void SetUumuuPattern(PlayMakerFSM fsm)
        {
            if (fsm.FsmVariables.GetFsmInt("Ct Multizap").Value < 2)
            {
                fsm.Fsm.Event(fsm.FsmEvents.First(e => e.Name == "MULTIZAP"));
                fsm.FsmVariables.GetFsmInt("Ct Multizap").Value++;
            }
            else
            {
                fsm.Fsm.Event(fsm.FsmEvents.First(e => e.Name == "CHASE"));
            }
        }

        public override void Unload()
        {
            Object.Destroy(_coro);

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= StartRoutine;
        }
    }
}