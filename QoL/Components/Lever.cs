using System;
using GlobalEnums;
using UnityEngine;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL.Components
{
    public class Lever : MonoBehaviour
    {
        public PersistentBoolData BoolData { get; set; }

        public Action OnHit { get; set; }
        
        public Lever()
        {
            BoolData = new PersistentBoolData
            {
                sceneName = USceneManager.GetActiveScene().name,
                id = name
            };
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != (int) PhysLayers.HERO_ATTACK)
                return;

            if (!other.gameObject.name.Contains("Slash"))
                return;

            SceneData sd = GameManager.instance.sceneData;
            
            PersistentBoolData pbd = sd.FindMyState(BoolData) ?? BoolData;
            
            if (pbd.activated)
                return;

            pbd.activated = true;

            OnHit();
            
            sd.SaveMyState(pbd);
        }
    }
}