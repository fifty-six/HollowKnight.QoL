using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using Vasi;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class FixFireballs : FauxMod
    {
        private class NextPostPhysicsFrameEvent : FsmStateAction
        {
            [RequiredField]
            public FsmEvent? sendEvent;

            private bool _fixedUpdate;

            public override void Reset() => sendEvent = null;

            public override void OnEnter() => _fixedUpdate = false;

            public override void OnFixedUpdate() => _fixedUpdate = true;

            public override void OnUpdate()
            {
                if (_fixedUpdate)
                {
                    Finish();
                    Fsm.Event(sendEvent);
                }
            }
        }

        public override void Initialize()
        {
            On.PlayMakerFSM.OnEnable += CheckFireball;
        }

        public override void Unload()
        {
            On.PlayMakerFSM.OnEnable -= CheckFireball;
        }

        private static void CheckFireball(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            // Wall Impact state doesn't exist in shade soul control
            // Fireballs get recycled so we've gotta check for the action this method adds as well
            if
            (
                self.FsmName != "Fireball Control"
                || !self.TryGetState("Wall Impact", out _)
                || self.GetAction<FsmStateAction>("Pause", 0) is NextPostPhysicsFrameEvent
            )
            {
                orig(self);
                return;
            }

            // Always allow a physics update before enabling collision
            self.GetState("Pause").RemoveAction<NextFrameEvent>();

            self.GetState("Pause").InsertAction(0, new NextPostPhysicsFrameEvent
            {
                sendEvent = FsmEvent.FindEvent("FINISHED")
            });

            orig(self);
        }
    }
}
