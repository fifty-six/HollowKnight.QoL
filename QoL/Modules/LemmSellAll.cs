using JetBrains.Annotations;
using UnityEngine;
using Vasi;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class LemmSellAll : FauxMod
    {
        private static readonly int[] RELIC_COST = { 200, 450, 800, 1200 };
        
        public override void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += LemmSell;
        }

        public override void Unload()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= LemmSell;
        }

        private static void LemmSell(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            if (to.name != "Ruins1_05b") return;

            GameObject.Find("Relic Dealer")
                      .LocateMyFSM("npc_control")
                      .GetState("Convo End")
                      .AddMethod(SellRelics);
        }

        private static void SellRelics()
        {
            var pd = PlayerData.instance;
            
            if (pd.GetBool("equippedCharm_10")) return;

            for (int i = 1; i <= 4; i++)
            {
                int amount = pd.GetInt($"trinket{i}");
                
                if (amount == 0) continue;
                
                int price = amount * RELIC_COST[i - 1];

                pd.SetInt($"soldTrinket{i}", pd.GetInt($"soldTrinket{i}") + amount);

                HeroController.instance.AddGeo(price);
                
                pd.SetInt($"trinket{i}", 0);
            }
        }
    }
}