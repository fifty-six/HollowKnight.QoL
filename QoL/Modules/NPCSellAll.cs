using System.Linq;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class NPCSellAll : FauxMod
    {
        [SerializeToSetting]
        public static bool LemmSellAll = true;

        [SerializeToSetting]
        public static bool JinnSellAll = true;

        private static readonly int[] RELIC_COST = { 200, 450, 800, 1200 };
        private const int EGG_COST = 450;

        public override void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += PatchVendorNPCs;
        }

        public override void Unload()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= PatchVendorNPCs;
        }

        private static void PatchVendorNPCs(Scene scene, LoadSceneMode lsm)
        {
            switch (scene.name)
            {
                case "Ruins1_05b" when LemmSellAll:
                    LemmSell(scene);
                    break;
                case "Room_Jinn" when JinnSellAll:
                    JinnSell(scene);
                    break;
            }
        }

        private static void LemmSell(Scene scene)
        {
            GameObject lemm = scene.GetRootGameObjects().FirstOrDefault(obj => obj.name == "Relic Dealer");

            if (lemm == null)
                return;
                 
            lemm.LocateMyFSM("npc_control")
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

        private static void JinnSell(Scene scene)
        {
            GameObject jinn = scene.GetRootGameObjects().FirstOrDefault(obj => obj.name == "Jinn NPC");

            if (jinn == null)
                return;

            jinn.LocateMyFSM("Wake and Animate")
                .GetState("Talk NPC?")
                .RemoveAction<PlayerDataBoolTest>();

            jinn.transform.Find("Talk NPC").gameObject
                .LocateMyFSM("Conversation Control")
                .GetState("Talk Finish")
                .InsertMethod(0, SellEggs);
        }

        private static void SellEggs()
        {
            var pd = PlayerData.instance;

            int amount = pd.GetInt(nameof(pd.rancidEggs));

            if (amount == 0) return;

            int price = amount * EGG_COST;

            pd.SetInt(nameof(pd.jinnEggsSold), pd.GetInt(nameof(pd.jinnEggsSold)) + amount);

            HeroController.instance.AddGeo(price);

            pd.SetInt(nameof(pd.rancidEggs), 0);
        }
    }
}