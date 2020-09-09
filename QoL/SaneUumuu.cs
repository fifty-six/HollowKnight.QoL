using System.Collections;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using Object = UnityEngine.Object;

namespace QoL
{
    [UsedImplicitly]
    public class SaneUumuu : FauxMod
    {
        private NonBouncer _coroutineStarter;
        public override void Initialize()
        {
            GameObject go = new GameObject();
            _coroutineStarter = go.AddComponent<NonBouncer>();
            Object.DontDestroyOnLoad(_coroutineStarter);
            
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += StartRoutine;
        }

        private void StartRoutine(Scene arg0, LoadSceneMode loadSceneMode)
        {
            if (arg0.name == "Fungus3_archive_02_boss")
                _coroutineStarter.StartCoroutine(FixUumuu());
        }

        private static IEnumerator FixUumuu()
        {

            // Need to wait a bit for Uumuu to exist
            yield return null;
            // Find Uumuu and the FSM
            GameObject uumuu = GameObject.Find("Mega Jellyfish");
            if (uumuu == null)
                yield break;
            
            PlayMakerFSM uumuuFSM = uumuu.LocateMyFSM("Mega Jellyfish");
            
            // Fix the waits and the number of attacks
            uumuuFSM.GetState("Idle").GetAction<WaitRandom>().timeMax = 1.5f;
            uumuuFSM.GetState("Set Timer").GetAction<RandomFloat>().max = 2f;
            uumuuFSM.FsmVariables.GetFsmFloat("Quirrel Time").Value = 4f;

            // Fix the pattern to 2 quick, then 1 long if it still needs to attack
            FsmState choiceState = uumuuFSM.GetState("Choice");
            choiceState.RemoveAction<SendRandomEventV2>();
            choiceState.AddMethod( () => SetUumuuPattern(uumuuFSM) );
            
            // Reset the multizap counter to 0 so the pattern remains 2 quick 1 optional long
            uumuuFSM.GetState("Recover").AddMethod(() => uumuuFSM.FsmVariables.GetFsmInt("Ct Multizap").Value = 0); 
            
            //TODO knockback in platforms

            // Set the initial RecoilSpeed to 0 so that dream nailing her on the first cycle doesn't push her
            uumuu.GetComponent<Recoil>().SetRecoilSpeed(0);

            // Set her HP to 1028 value
            uumuu.GetComponent<HealthManager>().hp = 250;
            
        }

        private static void SetUumuuPattern(PlayMakerFSM uumuuFSM)
        {
            if (uumuuFSM.FsmVariables.GetFsmInt("Ct Multizap").Value < 2)
            {
                uumuuFSM.Fsm.Event(uumuuFSM.FsmEvents.First(fsmevent => fsmevent.Name == "MULTIZAP"));
                uumuuFSM.FsmVariables.GetFsmInt("Ct Multizap").Value++;
            }
            else
                uumuuFSM.Fsm.Event(uumuuFSM.FsmEvents.First(fsmevent => fsmevent.Name == "CHASE"));
        }

        public override void Unload()
        {
            Object.Destroy(_coroutineStarter);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= StartRoutine;
        }
    }
}