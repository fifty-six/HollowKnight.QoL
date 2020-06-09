using System.Collections;
using GlobalEnums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QoL
{
    public class FixVolume : FauxMod
    {
        [SerializeToSetting]
        public static float DoubleDamageVolumeModifier = .8f;
        
        private static float Volume => GameManager.instance.gameSettings.soundVolume / 10f;

        public override void Initialize()
        {
            On.HeroController.StartRecoil += StartRecoil;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        private static void SceneChanged(Scene arg0, Scene arg1)
        {
            if (!arg1.name.ToLower().Contains("dream")) return;

            foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
            {
                if (!go.name.Contains("grass")) continue;

                var source = go.GetComponent<AudioSource>();

                if (source != null)
                    source.volume = Volume;
            }
        }

        private static IEnumerator StartRecoil
        (
            On.HeroController.orig_StartRecoil orig,
            HeroController self,
            CollisionSide impactside,
            bool spawndamageeffect,
            int damageamount
        )
        {
            self.takeHitDoublePrefab.GetComponent<AudioSource>().volume = Mathf.Clamp(Volume * DoubleDamageVolumeModifier, 0, 1);

            yield return orig(self, impactside, spawndamageeffect, damageamount);
        }

        public override void Unload()
        {
            On.HeroController.StartRecoil -= StartRecoil;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
        }
    }
}