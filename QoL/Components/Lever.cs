using System;
using GlobalEnums;
using UnityEngine;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL.Components
{
    public class Lever : MonoBehaviour
    {
        private readonly PersistentBoolData _bool;
        
        private string _id;
        
        public Action OnHit { get; set; }
        
        public string Id
        {
            set => _bool.id = _id = value;
        }

        public Lever()
        {
            _bool = new PersistentBoolData
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
            
            PersistentBoolData pbd = sd.FindMyState(_bool) ?? _bool;
            
            if (pbd.activated)
                return;

            pbd.activated = true;

            OnHit();
            
            sd.SaveMyState(pbd);
        }
    }
}