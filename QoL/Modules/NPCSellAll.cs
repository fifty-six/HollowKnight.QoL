﻿using System.Linq;
using HutongGames.PlayMaker;
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

        [SerializeToSetting]
        public static bool NailsmithBuyAll = true;

        private const int EGG_COST = 450;
        private static readonly int[] RELIC_COST = { 200, 450, 800, 1200 };
        private static readonly (int ore, int geo)[] NAIL_UPGRADE_COSTS =
        {
            (0, 250),
            (1, 800),
            (2, 2000),
            (3, 4000)
        };

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
                case "Room_nailsmith" when NailsmithBuyAll:
                    NailsmithBuy(scene);
                    break;
            }
        }

        private static void LemmSell(Scene scene)
        {
            GameObject? lemm = scene.GetRootGameObjects().FirstOrDefault(obj => obj.name == "Relic Dealer");

            if (lemm == null)
                return;

            lemm.LocateMyFSM("npc_control")
                .GetState("Convo End")
                .AddMethod(SellRelics);
        }

        private static void SellRelics()
        {
            var pd = PlayerData.instance;

            if (pd.GetBool("equippedCharm_10")) 
                return;

            for (int i = 1; i <= 4; i++)
            {
                int amount = pd.GetInt($"trinket{i}");

                if (amount == 0) 
                    continue;

                int price = amount * RELIC_COST[i - 1];

                pd.SetInt($"soldTrinket{i}", pd.GetInt($"soldTrinket{i}") + amount);
                pd.SetInt($"trinket{i}", 0);
                
                HeroController.instance.AddGeo(price);
            }
        }

        private static void JinnSell(Scene scene)
        {
            GameObject? jinn = scene.GetRootGameObjects().FirstOrDefault(obj => obj.name == "Jinn NPC");

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

            if (amount == 0) 
                return;

            int price = amount * EGG_COST;

            pd.SetInt(nameof(pd.jinnEggsSold), pd.GetInt(nameof(pd.jinnEggsSold)) + amount);
            pd.SetInt(nameof(pd.rancidEggs), 0);

            HeroController.instance.AddGeo(price);
        }

        private static void NailsmithBuy(Scene scene)
        {
            bool right = false;

            GameObject? nailsmith = scene.GetRootGameObjects().FirstOrDefault(obj => obj.name == "Nailsmith");
            
            if (nailsmith == null)
                return;

            PlayMakerFSM convo = nailsmith.LocateMyFSM("Conversation Control");
            FsmState box = convo.GetState("Box Up");
            
            box.AddTransition("BOUGHT ALL", "Box Up 4");
            box.InsertMethod(0, () =>
            {
                if (right && BuyNailUpgrades())
                    convo.SendEvent("BOUGHT ALL");
            });

            nailsmith.LocateMyFSM("npc_control").GetState("Move Hero Left").InsertMethod(0, () =>
            {
                float heroMin = HeroController.instance.GetComponent<Collider2D>().bounds.min.x;
                // Check if the knight is completely to the right of the Nailsmith's dream dialogue hitbox
                right = 18.5f < heroMin;
            });
        }

        private static bool BuyNailUpgrades()
        {
            var pd = PlayerData.instance;

            int current = pd.GetInt(nameof(pd.nailSmithUpgrades));

            if (current > 3) 
                return false;

            int bought = 0;

            var upgrades = NAIL_UPGRADE_COSTS.Skip(current).TakeWhile(
                x => x.ore <= pd.GetInt(nameof(pd.ore)) && x.geo <= pd.GetInt(nameof(pd.geo))
            );

            foreach (var (ore, geo) in upgrades)
            {
                pd.IntAdd(nameof(pd.ore), -ore);
                HeroController.instance.TakeGeo(geo);
                bought++;
            }

            if (bought <= 0) 
                return false;

            var gm = GameManager.instance;
            
            pd.SetBool(nameof(PlayerData.honedNail), true);
            pd.IntAdd(nameof(PlayerData.nailDamage), bought * 4);
            pd.IntAdd(nameof(PlayerData.nailSmithUpgrades), bought);
            
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            
            gm.ResetSemiPersistentItems();
            gm.TimePasses();
            gm.StoryRecord_upgradeNail();

            return true;
        }
    }
}