using GlobalEnums;
using JetBrains.Annotations;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QoL
{
    [UsedImplicitly]
    public class FixExplosionPogo : FauxMod
    {
        public override void Initialize()
        {
            ModHooks.Instance.ObjectPoolSpawnHook += FixExplosion;
        }

        public override void Unload()
        {
            ModHooks.Instance.ObjectPoolSpawnHook -= FixExplosion;
        }

        private static GameObject FixExplosion(GameObject go)
        {
            if (!go.name.StartsWith("Gas Explosion Recycle M"))
                return go;

            go.layer = (int) PhysLayers.ENEMIES;
            
            var bouncer = go.GetComponent<NonBouncer>();

            if (bouncer) 
                bouncer.active = false;

            return go;
        }
    }
}
