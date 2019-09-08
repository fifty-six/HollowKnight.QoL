using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modding;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL
{
    [UsedImplicitly]
    public class DreamRespawner : Mod, ITogglableMod
    {
        private static readonly Dictionary<string, Vector3> SCENE_TRANSFORMS = new Dictionary<string, Vector3>
        {
            ["Dream_03_Infected_Knight"] = new Vector3(13.4f, 28.4f),
            ["Grimm_Nightmare"] = new Vector3(91, 6.4f),
            ["Dream_01_False_Knight"] = new Vector3(40.6f, 27.4f),
            ["Dream_02_Mage_Lord"] = new Vector3(29.4f, 29.5f),
            ["Dream_Mighty_Zote"] = new Vector3(25, 30),
            ["Dream_04_White_Defender"] = new Vector3(70.6f, 7.4f)
        };

        private static readonly Dictionary<string, string> VAR_SCENES = new Dictionary<string, string>
        {
            ["infectedKnightDreamDefeated"] = "Abyss_19",
            ["killedNightmareGrimm"] = "Town",
            ["falseKnightDreamDefeated"] = "Crossroads_10",
            ["mageLordDreamDefeated"] = "Ruins1_24",
            ["greyPrinceDefeats"] = "Room_Bretta_Basement",
            ["whiteDefenderDefeats"] = "Waterways_15"
        };

        public override void Initialize()
        {
            ModHooks.Instance.SetPlayerBoolHook += SetBool;
            ModHooks.Instance.SetPlayerIntHook += SetInt;

            USceneManager.activeSceneChanged += SceneChanged;

            On.GameManager.EnterHero += OnEnterHero;
        }

        public void Unload()
        {
            ModHooks.Instance.SetPlayerBoolHook -= SetBool;
            ModHooks.Instance.SetPlayerIntHook -= SetInt;

            USceneManager.activeSceneChanged -= SceneChanged;

            On.GameManager.EnterHero -= OnEnterHero;
        }

        private static void SetInt(string intname, int value)
        {
            if (VAR_SCENES.ContainsKey(intname))
            {
                GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
                {
                    SceneName = VAR_SCENES[intname],
                    EntryGateName = "top1",
                    HeroLeaveDirection = GlobalEnums.GatePosition.door,
                    Visualization = GameManager.SceneLoadVisualizations.GrimmDream,
                    AlwaysUnloadUnusedAssets = false
                });
            }

            PlayerData.instance.SetIntInternal(intname, value);
        }

        private static void SetBool(string originalset, bool value)
        {
            if (VAR_SCENES.ContainsKey(originalset) && value)
            {
                PlayerData.instance.dreamReturnScene = VAR_SCENES[originalset];
            }

            PlayerData.instance.SetBoolInternal(originalset, value);
        }

        private static void SceneChanged(Scene arg0, Scene arg1)
        {
            if (arg0.name == null || arg1.name == null) return;
            
            if (SCENE_TRANSFORMS.Keys.Contains(arg1.name))
            {
                PlayerData.instance.dreamReturnScene = arg1.name;
            }

            bool? hasDreamDoor = WorldNavigation.Scenes.FirstOrDefault(i => i.Name == arg1.name)?.Transitions
                .Any(x => x.Name == "door_dreamReturn");

            if (SCENE_TRANSFORMS.Keys.Contains(arg0.name) && VAR_SCENES.ContainsValue(arg1.name) ||
                !VAR_SCENES.ContainsValue(arg1.name) && hasDreamDoor != null && (bool) hasDreamDoor)
            {
                IEnumerator H()
                {
                    yield return null;
                    yield return null;

                    foreach (GameObject go in UObject.FindObjectsOfType<GameObject>())
                    {
                        if (go.name.Contains("Blanker"))
                        {
                            go.GetComponent<SpriteRenderer>().enabled = false;
                        }

                        if (go.name == "White Flash")
                        {
                            UObject.Destroy(go);
                        }
                    }

                    yield return new WaitForSecondsRealtime(1.4f);

                    HeroController.instance.AcceptInput();
                }

                GameManager.instance.StartCoroutine(H());
            }

            if (GameManager.instance == null || arg1.name != "Dream_Mighty_Zote") return;
            // gpz platform cause falling for 8 years is annoying

            GameObject plane = new GameObject("Plane")
            {
                tag = "HeroWalkable",
                layer = 8
            };

            MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateMesh(new[]
            {
                new Vector3(20, 28),
                new Vector3(20, 28.5f),
                new Vector3(30, 28),
                new Vector3(30, 28.5f)
            });

            MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Particles/Multiply");

            // Texture
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.black);
            tex.Apply();

            // Renderer
            renderer.material.mainTexture = tex;
            renderer.material.color = Color.white;

            // Collider
            BoxCollider2D a = plane.AddComponent<BoxCollider2D>();
            a.isTrigger = false;

            // Make it exist.
            plane.SetActive(true);
        }

        private static void OnEnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additivegatesearch)
        {
            self.UpdateSceneName();

            if (SCENE_TRANSFORMS.ContainsKey(self.sceneName))
            {
                GameObject go = new GameObject("door_dreamReturn");
                {
                    TransitionPoint tp = go.AddComponent<TransitionPoint>();
                    tp.respawnMarker = go.AddComponent<HazardRespawnMarker>();
                    tp.isADoor = true;
                    tp.name = "door_dreamReturn";
                }
                go.transform.position = SCENE_TRANSFORMS[self.sceneName];

                orig(self, false);
            }
            else
            {
                orig(self, additivegatesearch);
            }
        }

        private static Mesh CreateMesh(Vector3[] vertices)
        {
            Mesh m = new Mesh
            {
                name = "ScriptedMesh",
                vertices = vertices,
                uv = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(1, 0)
                },
                triangles = new int[] {0, 1, 2, 1, 2, 3}
            };
            m.RecalculateNormals();
            return m;
        }
    }
}