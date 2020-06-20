using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;

namespace QoL
{
    [UsedImplicitly]
    public class LemmSellAll : FauxMod
    {
        public override void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += LemmSell;   
        }

        public override void Unload()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= LemmSell;
        }

        private void LemmSell(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            if (to.name == "Ruins1_05b")
            {
                FsmState convo = GameObject.Find("Relic Dealer").LocateMyFSM("npc_control").GetState("Convo End");
                List<FsmStateAction> actions = convo.Actions.ToList();
                actions.Add(new SellAllRelics());
                convo.Actions = actions.ToArray();
            }
        }
    }

    class SellAllRelics : FsmStateAction
    {
        public override void OnEnter()
        {
            if (!PlayerData.instance.GetBool("equippedCharm_10"))
            {
                int money = PlayerData.instance.trinket1 * 200;
                money += PlayerData.instance.trinket2 * 450;
                money += PlayerData.instance.trinket3 * 800;
                money += PlayerData.instance.trinket4 * 1200;

                if (money > 0)
                {
                    HeroController.instance.AddGeo(money);
                }

                PlayerData.instance.soldTrinket1 += PlayerData.instance.trinket1;
                PlayerData.instance.soldTrinket2 += PlayerData.instance.trinket2;
                PlayerData.instance.soldTrinket3 += PlayerData.instance.trinket3;
                PlayerData.instance.soldTrinket4 += PlayerData.instance.trinket4;

                PlayerData.instance.trinket1 = 0;
                PlayerData.instance.trinket2 = 0;
                PlayerData.instance.trinket3 = 0;
                PlayerData.instance.trinket4 = 0;
            }

            Finish();
        }
    }
}
