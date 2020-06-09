using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace QoL
{
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
            self.RemoveAction("Pause", 3);
            
            // Create a new state before the regular idle
            self.CopyState("R", "Idle (No Collision)");
            self.RemoveAction("Idle (No Collision)", 0);
            
            self.ChangeTransition("L", "FINISHED", "Idle (No Collision)");
            self.ChangeTransition("R", "FINISHED", "Idle (No Collision)");
            self.AddTransition("Idle (No Collision)", "FINISHED", "Idle");
            
            // New state needs to start the fireball moving
            self.AddAction("Idle (No Collision)", new SetVelocity2d
            {
                gameObject = new FsmOwnerDefault(),
                vector = Vector2.zero,
                x = self.FsmVariables.FindFsmFloat("Velocity"),
                y = 0,
                everyFrame = false
            });
            
            // Small waiting period before proceeding to the old idle state
            self.AddAction("Idle (No Collision)", new Wait
            {
                time = NO_COLLISION_TIME,
                finishEvent = FsmEvent.FindEvent("FINISHED"),
                realTime = false
            });
            
            // Idle state needs to activate the collision now
            self.InsertAction("Idle", new ActivateGameObject
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
            }, 0);
            
            // Account for the additional waiting time before Idle
            self.GetAction<Wait>("Idle", 8).time.Value -= NO_COLLISION_TIME;
            
            orig(self);
        }
    }
}
