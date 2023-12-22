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
        //Sprites
        public Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();

        public void GetList()
        {
            LethalExpansion.Log.LogInfo("Audio Clips");
            foreach (var item in audioClips)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Audio Mixers");
            foreach (var item in audioMixers)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Planet Prefabs");
            foreach (var item in planetPrefabs)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Map Objects");
            foreach (var item in mapObjects)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Outside Objects");
            foreach (var item in outsideObjects)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Scraps");
            foreach (var item in scraps)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Level Ambiances");
            foreach (var item in levelAmbiances)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Enemies");
            foreach (var item in enemies)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
            LethalExpansion.Log.LogInfo("Sprites");
            foreach (var item in sprites)
            {
                LethalExpansion.Log.LogInfo(item.Key);
            }
        }

        #region Audio Clips
        public void AddAudioClip(AudioClip clip)
        {
            if (clip != null && !audioClips.ContainsKey(clip.name) && !audioClips.ContainsValue(clip))
            {
                audioClips.Add(clip.name, clip);
            }
        }
        public void AddAudioClip(string name, AudioClip clip)
        {
            if (clip != null && !audioClips.ContainsKey(name) && !audioClips.ContainsValue(clip))
            {
                audioClips.Add(name, clip);
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
                audioMixers.Add(mixer.name, (mixer, tmp.ToArray()));
            }
        }
        #endregion
        #region Planet Prefabs
        public void AddPlanetPrefabs(GameObject prefab)
        {
            if (prefab != null && !planetPrefabs.ContainsKey(prefab.name) && !planetPrefabs.ContainsValue(prefab))
            {
                planetPrefabs.Add(prefab.name, prefab);
            }
        }
        public void AddPlanetPrefabs(string name, GameObject prefab)
        {
            if (prefab != null && !planetPrefabs.ContainsKey(name) && !planetPrefabs.ContainsValue(prefab))
            {
                planetPrefabs.Add(name, prefab);
            }
        }
        #endregion
        #region Map Objects
        public void AddMapObjects(GameObject mapObject)
        {
            if (mapObject != null && !mapObjects.ContainsKey(mapObject.name) && !mapObjects.ContainsValue(mapObject))
            {
                mapObjects.Add(mapObject.name, mapObject);
            }
        }
        #endregion
        #region Outside Objects
        public void AddOutsideObject(SpawnableOutsideObject outsideObject)
        {
            if (outsideObject != null && !outsideObjects.ContainsKey(outsideObject.name) && !outsideObjects.ContainsValue(outsideObject))
            {
                outsideObjects.Add(outsideObject.name, outsideObject);
            }
        }
        #endregion
        #region Scraps
        public void AddScrap(Item scrap)
        {
            if (scrap != null && !scraps.ContainsKey(scrap.name) && !scraps.ContainsValue(scrap))
            {
                scraps.Add(scrap.name, scrap);
            }
        }
        #endregion
        #region Level Ambiances
        public void AddLevelAmbiances(LevelAmbienceLibrary levelAmbiance)
        {
            if (levelAmbiance != null && !levelAmbiances.ContainsKey(levelAmbiance.name) && !levelAmbiances.ContainsValue(levelAmbiance))
            {
                levelAmbiances.Add(levelAmbiance.name, levelAmbiance);
            }
        }
        #endregion
        #region Enemies
        public void AddEnemies(EnemyType enemie)
        {
            if (enemie != null && !enemies.ContainsKey(enemie.name) && !enemies.ContainsValue(enemie))
            {
                enemies.Add(enemie.name, enemie);
            }
        }
        #endregion
        #region Sprites
        public void AddSprites(Sprite sprite)
        {
            if (sprite != null && !sprites.ContainsKey(sprite.name) && !sprites.ContainsValue(sprite))
            {
                sprites.Add(sprite.name, sprite);
            }
        }
        public void AddSprites(string name, Sprite sprite)
        {
            if (sprite != null && !sprites.ContainsKey(name) && !sprites.ContainsValue(sprite))
            {
                sprites.Add(name, sprite);
            }
        }
        #endregion
    }
}
