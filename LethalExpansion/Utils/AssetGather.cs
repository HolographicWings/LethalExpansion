using LethalSDK.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace LethalExpansion.Utils
{
    public class AssetGather
    {
        private static AssetGather _instance;
        public static AssetGather Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssetGather();
                }
                return _instance;
            }
        }
        //Audio Clips
        public Dictionary<String, AudioClip> audioClips = new Dictionary<String, AudioClip>();
        //Audio Mixers
        public Dictionary<String, (AudioMixer, AudioMixerGroup[])> audioMixers = new Dictionary<String, (AudioMixer, AudioMixerGroup[])>();
        //Planet Prefabs
        public Dictionary<String, GameObject> planetPrefabs = new Dictionary<String, GameObject>();
        //Map Objects
        public Dictionary<String, GameObject> mapObjects = new Dictionary<String, GameObject>();
        //Outside Objects
        public Dictionary<String, SpawnableOutsideObject> outsideObjects = new Dictionary<String, SpawnableOutsideObject>();
        //Scraps
        public Dictionary<String, Item> scraps = new Dictionary<String, Item>();
        //Level Ambiances
        public Dictionary<String, LevelAmbienceLibrary> levelAmbiances = new Dictionary<String, LevelAmbienceLibrary>();
        //Enemies
        public Dictionary<String, EnemyType> enemies = new Dictionary<String, EnemyType>();

        #region Audio Clips
        public void AddAudioClip(AudioClip clip)
        {
            if (clip != null && !audioClips.ContainsKey(clip.name) && !audioClips.ContainsValue(clip))
            {
                LethalExpansion.Log.LogInfo(clip.name);
                audioClips.Add(clip.name, clip);
            }
        }
        #endregion
        #region Audio Mixers
        public void AddAudioMixer(AudioMixer mixer)
        {
            if (mixer != null && !audioMixers.ContainsKey(mixer.name))
            {
                List<AudioMixerGroup> tmp = new List<AudioMixerGroup>();
                foreach (var group in mixer.FindMatchingGroups(string.Empty))
                {
                    if (group != null && !tmp.Contains(group))
                    {
                        tmp.Add(group);
                    }
                }
                LethalExpansion.Log.LogInfo(mixer.name);
                audioMixers.Add(mixer.name, (mixer, tmp.ToArray()));
            }
        }
        #endregion
        #region Planet Prefabs
        public void AddPlanetPrefabs(GameObject prefab)
        {
            if (prefab != null && !planetPrefabs.ContainsKey(prefab.name) && !planetPrefabs.ContainsValue(prefab))
            {
                LethalExpansion.Log.LogInfo(prefab.name);
                planetPrefabs.Add(prefab.name, prefab);
            }
        }
        #endregion
        #region Map Objects
        public void AddMapObjects(GameObject mapObject)
        {
            if (mapObject != null && !mapObjects.ContainsKey(mapObject.name) && !mapObjects.ContainsValue(mapObject))
            {
                LethalExpansion.Log.LogInfo(mapObject.name);
                mapObjects.Add(mapObject.name, mapObject);
            }
        }
        #endregion
        #region Outside Objects
        public void AddOutsideObject(SpawnableOutsideObject outsideObject)
        {
            if (outsideObject != null && !outsideObjects.ContainsKey(outsideObject.name) && !outsideObjects.ContainsValue(outsideObject))
            {
                LethalExpansion.Log.LogInfo(outsideObject.name);
                outsideObjects.Add(outsideObject.name, outsideObject);
            }
        }
        #endregion
        #region Scraps
        public void AddScrap(Item scrap)
        {
            if (scrap != null && !scraps.ContainsKey(scrap.name) && !scraps.ContainsValue(scrap))
            {
                LethalExpansion.Log.LogInfo(scrap.name);
                scraps.Add(scrap.name, scrap);
            }
        }
        #endregion
        #region Level Ambiances
        public void AddLevelAmbiances(LevelAmbienceLibrary levelAmbiance)
        {
            if (levelAmbiance != null && !levelAmbiances.ContainsKey(levelAmbiance.name) && !levelAmbiances.ContainsValue(levelAmbiance))
            {
                LethalExpansion.Log.LogInfo(levelAmbiance.name);
                levelAmbiances.Add(levelAmbiance.name, levelAmbiance);
            }
        }
        #endregion
        #region Enemies
        public void AddEnemies(EnemyType enemie)
        {
            if (enemie != null && !enemies.ContainsKey(enemie.name) && !enemies.ContainsValue(enemie))
            {
                LethalExpansion.Log.LogInfo(enemie.name);
                enemies.Add(enemie.name, enemie);
            }
        }
        #endregion
    }
}
