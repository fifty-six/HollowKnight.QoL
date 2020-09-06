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
        private const float NO_COLLISION_TIME = 0.05f;

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
            // Fireballs get recycled so we've gotta check for the state this method adds as well
            if (self.FsmName != "Fireball Control" || self.GetState("Wall Impact") == null || self.GetState("Idle (No Collision)") != null)
            {
                orig(self);
                return;
            }
            
            // Store the terrain checker reference, prevent it from being enabled
            GameObject terrainChecker = self.GetAction<ActivateGameObject>("Pause", 3).gameObject.GameObject.Value;
            self.GetState("Pause").RemoveAction(3);
            
            // Create a new state before the regular idle
            FsmState idleNoCol = self.CopyState("R", "Idle (No Collision)");
            idleNoCol.RemoveAction(0);
            
            self.GetState("L").ChangeTransition("FINISHED", "Idle (No Collision)");
            self.GetState("R").ChangeTransition("FINISHED", "Idle (No Collision)");
            
            idleNoCol.AddTransition("FINISHED", "Idle");
            
            // New state needs to start the fireball moving
            idleNoCol.AddAction(new SetVelocity2d
            {
                gameObject = new FsmOwnerDefault(),
                vector = Vector2.zero,
                x = self.FsmVariables.FindFsmFloat("Velocity"),
                y = 0,
                everyFrame = false
            });
            
            // Small waiting period before proceeding to the old idle state
            idleNoCol.AddAction(new Wait
            {
                time = NO_COLLISION_TIME,
                finishEvent = FsmEvent.FindEvent("FINISHED"),
                realTime = false
            });

            FsmState idle = self.GetState("Idle");
            
            // Idle state needs to activate the collision now
            idle.InsertAction(0, new ActivateGameObject
            {
                gameObject = new FsmOwnerDefault
                {
                    GameObject = terrainChecker,
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject
                },
                activate = true,
                recursive = false,
                resetOnExit = false,
                everyFrame = false
            });
            
            // Account for the additional waiting time before Idle
            idle.GetAction<Wait>(8).time.Value -= NO_COLLISION_TIME;
            
            orig(self);
        }
    }
}
