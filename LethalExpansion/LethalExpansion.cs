using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalExpansion.Patches;
using LethalExpansion.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using LethalExpansion.Patches.Monsters;
using LethalExpansion.Utils.HUD;
using UnityEngine.Audio;
using DunGen;
using UnityEngine.UIElements;
using DunGen.Adapters;
using LethalSDK.Component;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Video;
using Unity.Netcode.Components;
using LethalSDK.Utils;
using BepInEx.Bootstrap;
using System.Collections;
using UnityEngine.Events;

namespace LethalExpansion
{
    [BepInPlugin(PluginGUID, PluginName, VersionString)]
    [BepInDependency("me.swipez.melonloader.morecompany", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("BrutalCompanyPlus", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("MoonOfTheDay", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("beeisyou.LandmineFix", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("LethalAdjustments", BepInDependency.DependencyFlags.SoftDependency)]
    public class LethalExpansion : BaseUnityPlugin
    {
        private const string PluginGUID = "LethalExpansion";
        private const string PluginName = "LethalExpansion";
        private const string VersionString = "1.3.12";
        public static readonly Version ModVersion = new Version(VersionString);
        /*private readonly Version[] CompatibleModVersions = {
            new Version(1, 3, 11)
        };*/
        private readonly Dictionary<string, compatibility> CompatibleMods = new Dictionary<string, compatibility>
        {
            { "com.sinai.unityexplorer",compatibility.medium },
            { "HDLethalCompany",compatibility.good },
            { "LC_API",compatibility.good },
            { "me.swipez.melonloader.morecompany",compatibility.medium },
            { "BrutalCompanyPlus",compatibility.medium },
            { "MoonOfTheDay",compatibility.good },
            { "Television_Controller",compatibility.bad },
            { "beeisyou.LandmineFix",compatibility.perfect },
            { "LethalAdjustments",compatibility.good },
            { "CoomfyDungeon", compatibility.bad },
            { "BiggerLobby", compatibility.critical },
            { "KoderTech.BoomboxController", compatibility.critical },
            { "299792458.MoreMoneyStart", compatibility.good }
        };
        private enum compatibility
        {
            unknown = 0,
            perfect = 1,
            good = 2,
            medium = 3,
            bad = 4,
            critical = 5
        }
        List<PluginInfo> loadedPlugins = new List<PluginInfo>();

        public static readonly int[] CompatibleGameVersions = {45};

        public static bool sessionWaiting = true;
        public static bool hostDataWaiting = true;
        public static bool ishost = false;
        public static bool alreadypatched = false;
        public static bool weathersReadyToShare = false;
        public static bool isInGame = false;

        public static int delayedLevelChange = -1;

        public static string lastKickReason = string.Empty;

        private static readonly Harmony Harmony = new Harmony(PluginGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        public static ConfigFile config;

        public static NetworkManager networkManager;

        public GameObject SpaceLight;
        public GameObject terrainfixer;
        public static Transform currentWaterSurface;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");


            Logger.LogInfo("Getting other plugins list");
            loadedPlugins = GetLoadedPlugins();
            foreach (var plugin in loadedPlugins)
            {
                if (plugin.Metadata.GUID != PluginGUID)
                {
                    if (CompatibleMods.ContainsKey(plugin.Metadata.GUID))
                    {
                        switch (CompatibleMods[plugin.Metadata.GUID])
                        {
                            case compatibility.unknown:
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Logger.LogInfo("                              ");
                                Console.ResetColor();
                                Logger.LogInfo($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {CompatibleMods[plugin.Metadata.GUID]}");
                                break;
                            case compatibility.perfect:
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Logger.LogInfo("                              ");
                                Console.ResetColor();
                                Logger.LogInfo($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {CompatibleMods[plugin.Metadata.GUID]}");
                                break;
                            case compatibility.good:
                                Console.BackgroundColor = ConsoleColor.Green;
                                Logger.LogInfo("                              ");
                                Console.ResetColor();
                                Logger.LogInfo($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {CompatibleMods[plugin.Metadata.GUID]}");
                                break;
                            case compatibility.medium:
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                Logger.LogInfo("                              ");
                                Console.ResetColor();
                                Logger.LogWarning($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {CompatibleMods[plugin.Metadata.GUID]}");
                                break;
                            case compatibility.bad:
                                Console.BackgroundColor = ConsoleColor.Red;
                                Logger.LogInfo("                              ");
                                Console.ResetColor();
                                Logger.LogError($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {CompatibleMods[plugin.Metadata.GUID]}");
                                break;
                            case compatibility.critical:
                                Console.BackgroundColor = ConsoleColor.Magenta;
                                Logger.LogInfo("                              ");
                                Console.ResetColor();
                                Logger.LogFatal($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {CompatibleMods[plugin.Metadata.GUID]}");
                                break;
                            default:
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Logger.LogInfo("                              ");
                                Console.ResetColor();
                                Logger.LogInfo($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {CompatibleMods[plugin.Metadata.GUID]}");
                                break;
                        }
                        Logger.LogInfo("------------------------------");
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Logger.LogInfo("                              ");
                        Console.ResetColor();
                        Logger.LogInfo($"Plugin: {plugin.Metadata.Name} - Version: {plugin.Metadata.Version} - Compatibility: {compatibility.unknown}");
                        Logger.LogInfo("------------------------------");
                    }
                }
            }

            config = Config;

            ConfigManager.Instance.AddItem(new ConfigItem("GlobalTimeSpeedMultiplier", 1.4f, "Time", "Change the global time speed", 0.1f, 3f, sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("NumberOfHours", 18, "Time", "Max lenght of an Expedition in hours. (Begin at 6 AM | 18 = Midnight)", 6, 20));
            ConfigManager.Instance.AddItem(new ConfigItem("DeadlineDaysAmount", 3, "Expeditions", "Change amount of days for the Quota.", 1, 9, sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("StartingCredits", 60, "Expeditions", "Change amount of starting Credit.", 0, 1000, sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("MoonsRoutePricesMultiplier", 1f, "Moons", "Change the Cost of the Moon Routes.", 0f, 5f));
            ConfigManager.Instance.AddItem(new ConfigItem("StartingQuota", 130, "Expeditions", "Change the starting Quota.", 0, 1000, sync: true, optional:true));
            ConfigManager.Instance.AddItem(new ConfigItem("ScrapAmountMultiplier", 1f, "Dungeons", "Change the amount of Scraps in dungeons.", 0f, 10f));
            ConfigManager.Instance.AddItem(new ConfigItem("ScrapValueMultiplier", 0.4f, "Dungeons", "Change the value of Scraps.", 0f, 10f));
            ConfigManager.Instance.AddItem(new ConfigItem("MapSizeMultiplier", 1.5f, "Dungeons", "Change the size of the Dungeons. (Can crash when under 1.0)", 0.8f, 10f));
            ConfigManager.Instance.AddItem(new ConfigItem("PreventMineToExplodeWithItems", false, "Dungeons", "Prevent Landmines to explode by dropping items on them"));
            ConfigManager.Instance.AddItem(new ConfigItem("MineActivationWeight", 0.15f, "Dungeons", "Set the minimal weight to prevent Landmine's explosion (0.15 = 16 lb, Player = 2.0)", 0.01f, 5f));
            ConfigManager.Instance.AddItem(new ConfigItem("WeightUnit", 0, "HUD", "Change the carried Weight unit : 0 = Pounds (lb), 1 = Kilograms (kg) and 2 = Both", 0, 2, sync: false));
            ConfigManager.Instance.AddItem(new ConfigItem("ConvertPoundsToKilograms", true, "HUD", "Convert Pounds into Kilograms (16 lb = 7 kg) (Only effective if WeightUnit = 1)", sync: false));
            ConfigManager.Instance.AddItem(new ConfigItem("PreventScrapWipeWhenAllPlayersDie", false, "Expeditions", "Prevent the Scraps Wipe when all players die."));
            ConfigManager.Instance.AddItem(new ConfigItem("24HoursClock", false, "HUD", "Display a 24h clock instead of 12h.", sync: false));
            ConfigManager.Instance.AddItem(new ConfigItem("ClockAlwaysVisible", false, "HUD", "Display clock while inside of the Ship."));
            ConfigManager.Instance.AddItem(new ConfigItem("AutomaticDeadline", false, "Expeditions", "Automatically increase the Deadline depending of the required quota."));
            ConfigManager.Instance.AddItem(new ConfigItem("AutomaticDeadlineStage", 300, "Expeditions", "Increase the quota deadline of one day each time the quota exceeds this value.", 100, 3000));
            ConfigManager.Instance.AddItem(new ConfigItem("LoadModules", true, "Modules", "Load SDK Modules that add new content to the game. Disable it to play with Vanilla players. (RESTART REQUIRED)", sync:false, optional: false, requireRestart:true));
            ConfigManager.Instance.AddItem(new ConfigItem("MaxItemsInShip", 45, "Expeditions", "Change the Items cap can be kept in the ship.", 10, 500));
            ConfigManager.Instance.AddItem(new ConfigItem("ShowMoonWeatherInCatalogue", true, "HUD", "Display the current weather of Moons in the Terminal's Moon Catalogue.", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("ShowMoonRankInCatalogue", false, "HUD", "Display the rank of Moons in the Terminal's Moon Catalogue.", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("ShowMoonPriceInCatalogue", false, "HUD", "Display the route price of Moons in the Terminal's Moon Catalogue.", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("QuotaIncreaseSteepness", 16, "Expeditions", "Change the Quota Increase Steepness. (Highter = less steep exponential increase)", 0, 32, sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("QuotaBaseIncrease", 100, "Expeditions", "Change the Quota Base Increase.", 0, 300, sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("KickPlayerWithoutMod", false, "Lobby", "Kick the players without Lethal Expansion installer. (Will be kicked anyway if LoadModules is True)", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("BrutalCompanyPlusCompatibility", false, "Compatibility", "Leave Brutal Company Plus control the Quota settings.", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("LethalAdjustmentsCompatibility", false, "Compatibility", "Leave Lethal Adjustments control the Dungeon settings.", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("CoomfyDungeonCompatibility", false, "Compatibility", "Let Coomfy Dungeons control the Dungeon size & scrap settings.", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("MoreMoneyStartCompatibility", false, "Compatibility", "Let MoreMoneyStart control the Starting credits amount.", sync: true));
            ConfigManager.Instance.AddItem(new ConfigItem("SettingsDebug", false, "Debug", "Show an output of every settings in the Console. (The Console must listen Info messages)", sync: false));
            ConfigManager.Instance.AddItem(new ConfigItem("LegacyMoonLoading", false, "Modules", "Roll back to Synchronous moon loading. (Freeze the game longer and highter chance of crash)", sync: true));

            ConfigManager.Instance.ReadConfig();

            Config.SettingChanged += ConfigSettingChanged;

            AssetBundlesManager.Instance.LoadAllAssetBundles();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Harmony.PatchAll(typeof(GameNetworkManager_Patch));
            Harmony.PatchAll(typeof(Terminal_Patch));
            Harmony.PatchAll(typeof(MenuManager_Patch));
            Harmony.PatchAll(typeof(GrabbableObject_Patch));
            Harmony.PatchAll(typeof(RoundManager_Patch));
            Harmony.PatchAll(typeof(TimeOfDay_Patch));
            Harmony.PatchAll(typeof(HUDManager_Patch));
            Harmony.PatchAll(typeof(StartOfRound_Patch));
            Harmony.PatchAll(typeof(EntranceTeleport_Patch));
            Harmony.PatchAll(typeof(Landmine_Patch));
            Harmony.PatchAll(typeof(AudioReverbTrigger_Patch));
            Harmony.PatchAll(typeof(InteractTrigger_Patch));
            Harmony.PatchAll(typeof(RuntimeDungeon));
            Harmony harmony = new Harmony("LethalExpansion");
            MethodInfo BaboonBirdAI_GrabScrap_Method = AccessTools.Method(typeof(BaboonBirdAI), "GrabScrap", null, null);
            MethodInfo HoarderBugAI_GrabItem_Method = AccessTools.Method(typeof(HoarderBugAI), "GrabItem", null, null);
            MethodInfo MonsterGrabItem_Method = AccessTools.Method(typeof(MonsterGrabItem_Patch), "MonsterGrabItem", null, null);
            harmony.Patch(BaboonBirdAI_GrabScrap_Method, null, new HarmonyMethod(MonsterGrabItem_Method), null, null, null);
            harmony.Patch(HoarderBugAI_GrabItem_Method, null, new HarmonyMethod(MonsterGrabItem_Method), null, null, null);
            MethodInfo HoarderBugAI_DropItem_Method = AccessTools.Method(typeof(HoarderBugAI), "DropItem", null, null);
            MethodInfo MonsterDropItem_Patch_Method = AccessTools.Method(typeof(MonsterGrabItem_Patch), "MonsterDropItem_Patch", null, null);
            MethodInfo HoarderBugAI_KillEnemy_Method = AccessTools.Method(typeof(HoarderBugAI), "KillEnemy", null, null);
            MethodInfo KillEnemy_Patch_Method = AccessTools.Method(typeof(MonsterGrabItem_Patch), "KillEnemy_Patch", null, null);
            harmony.Patch(HoarderBugAI_DropItem_Method, null, new HarmonyMethod(MonsterDropItem_Patch_Method), null, null, null);
            harmony.Patch(HoarderBugAI_KillEnemy_Method, null, new HarmonyMethod(KillEnemy_Patch_Method), null, null, null);

            HDRenderPipelineAsset hdAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
            if (hdAsset != null)
            {
                var clonedSettings = hdAsset.currentPlatformRenderPipelineSettings;
                clonedSettings.supportWater = true;
                hdAsset.currentPlatformRenderPipelineSettings = clonedSettings;
                Logger.LogInfo("Water support applied to the HDRenderPipelineAsset.");
            }
            else
            {
                Logger.LogError("HDRenderPipelineAsset not found.");
            }

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
        }
        List<PluginInfo> GetLoadedPlugins()
        {
            return Chainloader.PluginInfos.Values.ToList();
        }
        private int width = 256;
        private int height = 256;
        private int depth = 20;
        private float scale = 20f;
        float[,] GenerateHeights()
        {
            float[,] heights = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heights[x, y] = CalculateHeight(x, y);
                }
            }
            return heights;
        }

        float CalculateHeight(int x, int y)
        {
            float xCoord = (float)x / width * scale;
            float yCoord = (float)y / height * scale;

            return Mathf.PerlinNoise(xCoord, yCoord);
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogInfo("Loading scene: " + scene.name);
            if (scene.name == "InitScene")
            {
                networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            }
            if (scene.name == "MainMenu")
            {
                sessionWaiting = true;
                hostDataWaiting = true;
                ishost = false;
                alreadypatched = false;

                LethalExpansion.delayedLevelChange = -1;

                isInGame = false;

                AssetGather.Instance.AddAudioMixer(GameObject.Find("Canvas/MenuManager").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer);

                SettingsMenu.Instance.InitSettingsMenu();

                //VersionChecker.CheckVersion().GetAwaiter();

                if(lastKickReason != null && lastKickReason.Length > 0)
                {
                    PopupManager.Instance.InstantiatePopup(scene, "Kicked from Lobby", $"You have been kicked\r\nReason: {lastKickReason}", button2: "Ignore");
                }
                if(!ConfigManager.Instance.FindEntryValue<bool>("CoomfyDungeonCompatibility") && loadedPlugins.Any(p => p.Metadata.GUID == "CoomfyDungeon"))
                {
                    PopupManager.Instance.InstantiatePopup(scene,
                        "CoomfyDungeon mod found",
                        "Warning: CoomfyDungeon is incompatible with LethalExpansion, Would you like to enable the compatibility mode? Otherwise dungeon generation Desync may occurs!",
                        "Yes",
                        "No",
                        new UnityAction(() => { ConfigManager.Instance.SetItemValue<bool>("CoomfyDungeonCompatibility", true); ConfigManager.Instance.SetEntryValue<bool>("CoomfyDungeonCompatibility", true); }),
                        titlesize:20,
                        contentsize:18
                        );
                }
                if(loadedPlugins.Any(p => p.Metadata.GUID == "BiggerLobby"))
                {
                    PopupManager.Instance.InstantiatePopup(scene,
                        "BiggerLobby mod found",
                        "Warning: BiggerLobby is incompatible with LethalExpansion, host/client synchronization will break and dungeon generation Desync may occurs!",
                        button2: "Ignore",
                        titlesize:20,
                        contentsize:18
                        );
                }
                if(loadedPlugins.Any(p => p.Metadata.GUID == "KoderTech.BoomboxController"))
                {
                    PopupManager.Instance.InstantiatePopup(scene,
                        "BoomboxController mod found",
                        "Warning: BoomboxController is incompatible with LethalExpansion, host/client synchronization will break and dungeon generation Desync may occurs!",
                        button2: "Ignore",
                        titlesize:20,
                        contentsize:18
                        );
                }
            }
            if (scene.name == "CompanyBuilding")
            {
                /*GameObject Labyrinth = Instantiate(AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/labyrinth.prefab"));
                SceneManager.MoveGameObjectToScene(Labyrinth, scene);*/

                SpaceLight.SetActive(false);
                terrainfixer.SetActive(false);
            }
            if (scene.name == "SampleSceneRelay")
            {
                SpaceLight = Instantiate(AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/SpaceLight.prefab"));
                SceneManager.MoveGameObjectToScene(SpaceLight, scene);

                Mesh FixedMonitorWallMesh = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Meshes/MonitorWall.fbx").GetComponent<MeshFilter>().mesh;
                GameObject MonitorWall = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube");
                MonitorWall.GetComponent<MeshFilter>().mesh = FixedMonitorWallMesh;

                MeshRenderer MonitorWallMeshRenderer = MonitorWall.GetComponent<MeshRenderer>();

                GameObject waterSurface = GameObject.Instantiate(GameObject.Find("Systems/GameSystems/TimeAndWeather/Flooding"));
                Destroy(waterSurface.GetComponent<FloodWeather>());
                waterSurface.name = "WaterSurface";
                waterSurface.transform.position = Vector3.zero;
                waterSurface.transform.Find("Water").GetComponent<MeshFilter>().sharedMesh = null;
                SpawnPrefab.Instance.waterSurface = waterSurface;

                /*Material BlueScreenMaterial = new Material(MonitorWallMeshRenderer.materials[1]);
                BlueScreenMaterial.SetColor("_BaseColor", new Color32(0,0,80, 255));*/

                Material[] materialArray = new Material[9];
                materialArray[0] = MonitorWallMeshRenderer.materials[0];
                materialArray[1] = MonitorWallMeshRenderer.materials[1];
                materialArray[2] = MonitorWallMeshRenderer.materials[1];
                //materialArray[2] = BlueScreenMaterial;
                materialArray[3] = MonitorWallMeshRenderer.materials[1];
                materialArray[4] = MonitorWallMeshRenderer.materials[1];
                //materialArray[4] = BlueScreenMaterial;
                materialArray[5] = MonitorWallMeshRenderer.materials[1];
                materialArray[6] = MonitorWallMeshRenderer.materials[1];
                materialArray[7] = MonitorWallMeshRenderer.materials[1];
                materialArray[8] = MonitorWallMeshRenderer.materials[2];

                MonitorWallMeshRenderer.materials = materialArray;

                StartOfRound.Instance.screenLevelDescription.gameObject.AddComponent<AutoScrollText>();

                /*MonitorWall.transform.Find("Canvas (1)/MainContainer/BG").gameObject.SetActive(false);
                MonitorWall.transform.Find("Canvas (1)/MainContainer/BG (1)").gameObject.SetActive(false);*/

                AssetGather.Instance.AddAudioMixer(GameObject.Find("Systems/Audios/DiageticBackground").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer);

                terrainfixer = new GameObject();
                terrainfixer.name = "terrainfixer";
                terrainfixer.transform.position = new Vector3(0, -500, 0);
                Terrain terrain = terrainfixer.AddComponent<Terrain>();
                TerrainData terrainData = new TerrainData();
                terrainData.heightmapResolution = width + 1;
                terrainData.size = new Vector3(width, depth, height);
                terrainData.SetHeights(0, 0, GenerateHeights());
                terrain.terrainData = terrainData;

                Terminal_Patch.ResetFireExitAmounts();

                UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(Volume));

                for (int i = 0; i < array.Length; i++)
                {
                    if((array[i] as Volume).sharedProfile == null)
                    {
                        (array[i] as Volume).sharedProfile = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<VolumeProfile>("Assets/Mods/LethalExpansion/Sky and Fog Global Volume Profile.asset");
                    }
                }

                waitForSession().GetAwaiter();

                isInGame = true;
            }
            if (scene.name.StartsWith("Level"))
            {
                SpaceLight.SetActive(false);
                terrainfixer.SetActive(false);
                if (ConfigManager.Instance.FindItemValue<bool>("SettingsDebug"))
                {
                    foreach (var entry in ConfigManager.Instance.GetAll())
                    {
                        Log.LogInfo("==========");
                        Log.LogInfo(entry.Key);
                        Log.LogInfo(entry.Value);
                        Log.LogInfo(entry.DefaultValue);
                        Log.LogInfo(entry.Sync);
                    }
                }
            }
            if (scene.name == "InitSceneLaunchOptions" && isInGame)
            {
                SpaceLight.SetActive(false);
                terrainfixer.SetActive(false);
                foreach (GameObject obj in scene.GetRootGameObjects())
                {
                    obj.SetActive(false);
                }
                if (ConfigManager.Instance.FindItemValue<bool>("SettingsDebug"))
                {
                    foreach(var entry in ConfigManager.Instance.GetAll())
                    {
                        Log.LogInfo("==========");
                        Log.LogInfo(entry.Key);
                        Log.LogInfo(entry.Value);
                        Log.LogInfo(entry.DefaultValue);
                        Log.LogInfo(entry.Sync);
                    }
                }
                if (ConfigManager.Instance.FindItemValue<bool>("LegacyMoonLoading"))
                {
                    LoadCustomMoonAsync(scene).RunSynchronously();
                }
                else
                {
                    LoadCustomMoonAsync(scene).GetAwaiter();
                }
            }
        }
        async Task LoadCustomMoonAsync(Scene scene)
        {
            await Task.Delay(400);
            try
            {
                if (Terminal_Patch.newMoons[StartOfRound.Instance.currentLevelID].MainPrefab != null)
                {
                    if (Terminal_Patch.newMoons[StartOfRound.Instance.currentLevelID].MainPrefab.transform != null)
                    {
                        CheckAndRemoveIllegalComponents(Terminal_Patch.newMoons[StartOfRound.Instance.currentLevelID].MainPrefab.transform);
                        GameObject mainPrefab = GameObject.Instantiate(Terminal_Patch.newMoons[StartOfRound.Instance.currentLevelID].MainPrefab);
                        currentWaterSurface = mainPrefab.transform.Find("Environment/Water");
                        if (mainPrefab != null)
                        {
                            SceneManager.MoveGameObjectToScene(mainPrefab, scene);
                            var DiageticBackground = mainPrefab.transform.Find("Systems/Audio/DiageticBackground");
                            if (DiageticBackground != null)
                            {
                                DiageticBackground.GetComponent<AudioSource>().outputAudioMixerGroup = AssetGather.Instance.audioMixers.ContainsKey("Diagetic") ? AssetGather.Instance.audioMixers["Diagetic"].Item2.First(a => a.name == "Master") : null;
                            }
                            Terrain[] Terrains = mainPrefab.GetComponentsInChildren<Terrain>();
                            if (Terrains != null && Terrains.Length > 0)
                            {
                                foreach (Terrain terrain in Terrains)
                                {
                                    terrain.drawInstanced = true;
                                }
                            }
                        }
                    }
                }
                String[] _tmp = { "MapPropsContainer", "OutsideAINode", "SpawnDenialPoint", "ItemShipLandingNode", "OutsideLevelNavMesh" };
                foreach (string s in _tmp)
                {
                    if (GameObject.FindGameObjectWithTag(s) == null || GameObject.FindGameObjectsWithTag(s).Any(o => o.scene.name != "InitSceneLaunchOptions"))
                    {
                        GameObject obj = new GameObject();
                        obj.name = s;
                        obj.tag = s;
                        obj.transform.position = new Vector3(0, -200, 0);
                        SceneManager.MoveGameObjectToScene(obj, scene);
                    }
                }
                await Task.Delay(200);
                GameObject DropShip = GameObject.Find("ItemShipAnimContainer");
                if (DropShip != null)
                {
                    var ItemShip = DropShip.transform.Find("ItemShip");
                    if (ItemShip != null)
                    {
                        ItemShip.GetComponent<AudioSource>().outputAudioMixerGroup = AssetGather.Instance.audioMixers.ContainsKey("Diagetic") ? AssetGather.Instance.audioMixers["Diagetic"].Item2.First(a => a.name == "Master") : null;
                    }
                    var ItemShipMusicClose = DropShip.transform.Find("ItemShip/Music");
                    if (ItemShipMusicClose != null)
                    {
                        ItemShipMusicClose.GetComponent<AudioSource>().outputAudioMixerGroup = AssetGather.Instance.audioMixers.ContainsKey("Diagetic") ? AssetGather.Instance.audioMixers["Diagetic"].Item2.First(a => a.name == "Master") : null;
                    }
                    var ItemShipMusicFar = DropShip.transform.Find("ItemShip/Music/Music (1)");
                    if (ItemShipMusicFar != null)
                    {
                        ItemShipMusicFar.GetComponent<AudioSource>().outputAudioMixerGroup = AssetGather.Instance.audioMixers.ContainsKey("Diagetic") ? AssetGather.Instance.audioMixers["Diagetic"].Item2.First(a => a.name == "Master") : null;
                    }
                }
                await Task.Delay(200);
                RuntimeDungeon runtimeDungeon = GameObject.FindObjectOfType<RuntimeDungeon>(false);
                if (runtimeDungeon == null)
                {
                    GameObject dungeonGenerator = new GameObject();
                    dungeonGenerator.name = "DungeonGenerator";
                    dungeonGenerator.tag = "DungeonGenerator";
                    dungeonGenerator.transform.position = new Vector3(0, -200, 0);
                    runtimeDungeon = dungeonGenerator.AddComponent<RuntimeDungeon>();
                    runtimeDungeon.Generator.DungeonFlow = RoundManager.Instance.dungeonFlowTypes[0];
                    runtimeDungeon.Generator.LengthMultiplier = 0.8f;
                    runtimeDungeon.Generator.PauseBetweenRooms = 0.2f;
                    runtimeDungeon.GenerateOnStart = false;
                    runtimeDungeon.Root = dungeonGenerator;
                    runtimeDungeon.Generator.DungeonFlow = RoundManager.Instance.dungeonFlowTypes[0];
                    UnityNavMeshAdapter dungeonNavMesh = dungeonGenerator.AddComponent<UnityNavMeshAdapter>();
                    dungeonNavMesh.BakeMode = UnityNavMeshAdapter.RuntimeNavMeshBakeMode.FullDungeonBake;
                    dungeonNavMesh.LayerMask = 35072; //256 + 2048 + 32768 = 35072
                    SceneManager.MoveGameObjectToScene(dungeonGenerator, scene);
                }
                else
                {
                    if (runtimeDungeon.Generator.DungeonFlow == null)
                    {
                        runtimeDungeon.Generator.DungeonFlow = RoundManager.Instance.dungeonFlowTypes[0];
                    }
                }

                GameObject OutOfBounds = GameObject.CreatePrimitive(PrimitiveType.Cube);
                OutOfBounds.name = "OutOfBounds";
                OutOfBounds.layer = 13;
                OutOfBounds.transform.position = new Vector3(0, -300, 0);
                OutOfBounds.transform.localScale = new Vector3(1000, 5, 1000);
                BoxCollider boxCollider = OutOfBounds.GetComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                OutOfBounds.AddComponent<OutOfBoundsTrigger>();
                Rigidbody rigidbody = OutOfBounds.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                SceneManager.MoveGameObjectToScene(OutOfBounds, scene);

                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
            }
        }
        

        void CheckAndRemoveIllegalComponents(Transform root)
        {
            try
            {
                var components = root.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (!ComponentWhitelists.moonPrefabWhitelist.Any(whitelistType => component.GetType() == whitelistType))
                    {
                        LethalExpansion.Log.LogWarning($"Removed illegal {component.GetType().Name} component.");
                        GameObject.Destroy(component);
                    }
                }

                foreach (Transform child in root)
                {
                    CheckAndRemoveIllegalComponents(child);
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        private void OnSceneUnloaded(Scene scene)
        {
            if(scene.name.Length > 0)
            {
                Logger.LogInfo("Unloading scene: " + scene.name);
            }
            if (scene.name.StartsWith("Level") || scene.name == "CompanyBuilding" || (scene.name == "InitSceneLaunchOptions" && isInGame))
            {
                if(SpaceLight != null)
                {
                    SpaceLight.SetActive(true);
                }
                if(currentWaterSurface != null)
                {
                    currentWaterSurface = null;
                }
                Terminal_Patch.ResetFireExitAmounts();
            }
        }
        private async Task waitForSession()
        {
            while (sessionWaiting)
            {
                await Task.Delay(1000);
            }

            if (!ishost)
            {
                while (!sessionWaiting && hostDataWaiting)
                {
                    NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.request, "hostconfig", string.Empty, 0);
                    await Task.Delay(3000);
                }
            }
            else
            {
                for (int i = 0; i < ConfigManager.Instance.GetAll().Count; i++)
                {
                    if (ConfigManager.Instance.MustBeSync(i))
                    {
                        ConfigManager.Instance.SetItemValue(i, ConfigManager.Instance.FindEntryValue(i));
                    }
                }
            }

            bool patchGlobalTimeSpeedMultiplier = true;
            bool patchNumberOfHours = true;
            bool patchDeadlineDaysAmount = true;
            bool patchStartingQuota = true;
            bool patchStartingCredits = true;
            bool patchBaseIncrease = true;
            bool patchIncreaseSteepness = true;
            bool patchScrapValueMultiplier = true;
            bool patchScrapAmountMultiplier = true;
            bool patchMapSizeMultiplier = true;
            bool patchMaxShipItemCapacity = true;

            if (!ConfigManager.Instance.FindItemValue<bool>("BrutalCompanyPlusCompatibility") || !loadedPlugins.Any(p => p.Metadata.GUID == "BrutalCompanyPlus"))
            {
                patchDeadlineDaysAmount = false;
                patchStartingQuota = false;
                patchStartingCredits = false;
                patchBaseIncrease = false;
                patchIncreaseSteepness = false;
            }
            if (!ConfigManager.Instance.FindItemValue<bool>("LethalAdjustmentsCompatibility") || !loadedPlugins.Any(p => p.Metadata.GUID == "LethalAdjustments"))
            {
                patchScrapValueMultiplier = false;
                patchScrapAmountMultiplier = false;
                patchMapSizeMultiplier = false;
            }
            if (!ConfigManager.Instance.FindItemValue<bool>("CoomfyDungeonCompatibility") || !loadedPlugins.Any(p => p.Metadata.GUID == "CoomfyDungeon"))
            {
                patchScrapAmountMultiplier = false;
                patchMapSizeMultiplier = false;
            }
            if (!ConfigManager.Instance.FindItemValue<bool>("MoreMoneyStartCompatibility") || !loadedPlugins.Any(p => p.Metadata.GUID == "299792458.MoreMoneyStart"))
            {
                patchStartingCredits = false;
            }

            if(patchGlobalTimeSpeedMultiplier)
                TimeOfDay.Instance.globalTimeSpeedMultiplier = ConfigManager.Instance.FindItemValue<float>("GlobalTimeSpeedMultiplier");
            if (patchNumberOfHours)
                TimeOfDay.Instance.numberOfHours = ConfigManager.Instance.FindItemValue<int>("NumberOfHours");
            if (patchDeadlineDaysAmount)
                TimeOfDay.Instance.quotaVariables.deadlineDaysAmount = ConfigManager.Instance.FindItemValue<int>("DeadlineDaysAmount");
            if (patchStartingQuota)
                TimeOfDay.Instance.quotaVariables.startingQuota = ConfigManager.Instance.FindItemValue<int>("StartingQuota");
            if (patchStartingCredits)
                TimeOfDay.Instance.quotaVariables.startingCredits = ConfigManager.Instance.FindItemValue<int>("StartingCredits");
            if (patchBaseIncrease)
                TimeOfDay.Instance.quotaVariables.baseIncrease = ConfigManager.Instance.FindItemValue<int>("QuotaBaseIncrease");
            if (patchIncreaseSteepness)
                TimeOfDay.Instance.quotaVariables.increaseSteepness = ConfigManager.Instance.FindItemValue<int>("QuotaIncreaseSteepness");
            if (patchScrapValueMultiplier)
                RoundManager.Instance.scrapValueMultiplier = ConfigManager.Instance.FindItemValue<float>("ScrapValueMultiplier");
            if (patchScrapAmountMultiplier)
                RoundManager.Instance.scrapAmountMultiplier = ConfigManager.Instance.FindItemValue<float>("ScrapAmountMultiplier");
            if (patchMapSizeMultiplier)
                RoundManager.Instance.mapSizeMultiplier = ConfigManager.Instance.FindItemValue<float>("MapSizeMultiplier");
            if (patchMaxShipItemCapacity)
                StartOfRound.Instance.maxShipItemCapacity = ConfigManager.Instance.FindItemValue<int>("MaxItemsInShip");

            if (!alreadypatched)
            {
                Terminal_Patch.MainPatch(GameObject.Find("TerminalScript").GetComponent<Terminal>());
                alreadypatched = true;
            }
        }
        private void ConfigSettingChanged(object sender, EventArgs e)
        {
            SettingChangedEventArgs settingChangedEventArgs = e as SettingChangedEventArgs;

            if (settingChangedEventArgs == null)
            {
                return;
            }
            Log.LogInfo(string.Format("{0} Changed to {1}", settingChangedEventArgs.ChangedSetting.Definition.Key, settingChangedEventArgs.ChangedSetting.BoxedValue));
        }
    }
}
