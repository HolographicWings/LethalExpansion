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

namespace LethalExpansion
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class LethalExpansion : BaseUnityPlugin
    {
        private const string MyGUID = "LethalExpansion";
        private const string PluginName = "LethalExpansion";
        private const string VersionString = "1.1.3";
        private readonly Version ModVersion = new Version(VersionString);
        private readonly Version[] CompatibleModVersions = {
            new Version(1, 1, 3)
        };
        public static readonly int[] CompatibleGameVersions = {40};

        public static bool sessionWaiting = true;
        public static bool hostDataWaiting = true;
        public static bool ishost = false;
        public static bool alreadypatched = false;
        public static bool isInGame = false;

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        public static ConfigFile config;

        public static NetworkManager networkManager;

        public GameObject SpaceLight;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");

            config = Config;

            ConfigManager.Instance.AddItem(new ConfigItem("GlobalTimeSpeedMultiplier", 1.4f, "Time", "Change the global time speed", 0.1f, 3f, sync: true, optional: true));
            ConfigManager.Instance.AddItem(new ConfigItem("LengthOfHours", 60, "Time", "Change amount of seconds in one hour", 1, 300));
            ConfigManager.Instance.AddItem(new ConfigItem("NumberOfHours", 18, "Time", "Max lenght of an Expedition in hours. (Begin at 6 AM | 18 = Midnight)", 6, 20));
            ConfigManager.Instance.AddItem(new ConfigItem("DeadlineDaysAmount", 3, "Expeditions", "Change amount of days for the Quota.", 1, 9, sync: false));
            ConfigManager.Instance.AddItem(new ConfigItem("StartingCredits", 60, "Expeditions", "Change amount of starting Credit.", 0, 1000, sync: false));
            ConfigManager.Instance.AddItem(new ConfigItem("MoonsRoutePricesMultiplier", 1f, "Moons", "Change the Cost of the Moon Routes.", 0f, 5f));
            ConfigManager.Instance.AddItem(new ConfigItem("StartingQuota", 130, "Expeditions", "Change the starting Quota.", 0, 1000, sync: false, optional:true));
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
            ConfigManager.Instance.AddItem(new ConfigItem("AutomaticDeadlineStage", 300, "Expeditions", "Increase the quota deadline of one day each time the quota exceeds this value.", 100, 1000));
            ConfigManager.Instance.AddItem(new ConfigItem("LoadModules", true, "Modules", "Load SDK Modules that add new content to the game. Disable it to play with Vanilla players. (RESTART REQUIRED)", sync:false, optional: false, requireRestart:true));
            ConfigManager.Instance.AddItem(new ConfigItem("MaxItemsInShip", 45, "Expeditions", "Change the Items cap can be kept in the ship.", 10, 500));

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

                isInGame = false;

                AssetGather.Instance.AddAudioMixer(GameObject.Find("Canvas/MenuManager").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer);

                SettingsMenu.Instance.InitSettingsMenu();
            }
            if (scene.name == "CompanyBuilding")
            {
                GameObject Labyrinth = Instantiate(AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/labyrinth.prefab"));
                SceneManager.MoveGameObjectToScene(Labyrinth, scene);
                waitForSession().GetAwaiter();

                SpaceLight.SetActive(false);
            }
            if (scene.name == "SampleSceneRelay")
            {
                SpaceLight = Instantiate(AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/SpaceLight.prefab"));
                SceneManager.MoveGameObjectToScene(SpaceLight, scene);

                Mesh FixedMonitorWallMesh = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Meshes/MonitorWall.fbx").GetComponent<MeshFilter>().mesh;
                GameObject MonitorWall = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube");
                MonitorWall.GetComponent<MeshFilter>().mesh = FixedMonitorWallMesh;

                MeshRenderer MonitorWallMeshRenderer = MonitorWall.GetComponent<MeshRenderer>();

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

                /*MonitorWall.transform.Find("Canvas (1)/MainContainer/BG").gameObject.SetActive(false);
                MonitorWall.transform.Find("Canvas (1)/MainContainer/BG (1)").gameObject.SetActive(false);*/

                AssetGather.Instance.AddAudioMixer(GameObject.Find("Systems/Audios/DiageticBackground").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer);

                waitForSession().GetAwaiter();

                isInGame = true;
            }
            if (scene.name.StartsWith("Level"))
            {
                SpaceLight.SetActive(false);
            }
            if (scene.name == "InitSceneLaunchOptions" && isInGame)
            {
                SpaceLight.SetActive(false);
                foreach (GameObject obj in scene.GetRootGameObjects())
                {
                    obj.SetActive(false);
                }

                GameObject mainPrefab = GameObject.Instantiate(Terminal_Patch.newMoons[StartOfRound.Instance.currentLevelID].MainPrefab);
                if (mainPrefab != null)
                {
                    SceneManager.MoveGameObjectToScene(mainPrefab, scene);
                }

                String[] _tmp = { "MapPropsContainer", "OutsideAINode", "SpawnDenialPoint", "ItemShipLandingNode", "OutsideLevelNavMesh" };
                foreach(string s in _tmp)
                {
                    if (GameObject.FindGameObjectWithTag(s) == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = s;
                        obj.tag = s;
                        obj.transform.position = new Vector3(0, -200, 0);
                        SceneManager.MoveGameObjectToScene(obj, scene);
                    }
                }
                if (GameObject.FindObjectOfType<RuntimeDungeon>(false) == null)
                {
                    GameObject dungeonGenerator = new GameObject();
                    dungeonGenerator.name = "DungeonGenerator";
                    dungeonGenerator.tag = "DungeonGenerator";
                    dungeonGenerator.transform.position = new Vector3(0, -200, 0);
                    RuntimeDungeon runtimeDungeon = dungeonGenerator.AddComponent<RuntimeDungeon>();
                    runtimeDungeon.Generator.DungeonFlow = RoundManager.Instance.dungeonFlowTypes[0];
                    runtimeDungeon.Generator.LengthMultiplier = 0.8f;
                    runtimeDungeon.Generator.PauseBetweenRooms = 0.2f;
                    runtimeDungeon.GenerateOnStart = false;
                    runtimeDungeon.Root = dungeonGenerator;
                    UnityNavMeshAdapter dungeonNavMesh = dungeonGenerator.AddComponent<UnityNavMeshAdapter>();
                    dungeonNavMesh.BakeMode = UnityNavMeshAdapter.RuntimeNavMeshBakeMode.FullDungeonBake;
                    dungeonNavMesh.LayerMask = 35072; //256 + 2048 + 32768 = 35072
                    SceneManager.MoveGameObjectToScene(dungeonGenerator, scene);
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
                while (hostDataWaiting)
                {
                    NetworkPacketManager.Instance.sendPacket("command:requestConfig");
                    await Task.Delay(1000);
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

            TimeOfDay.Instance.globalTimeSpeedMultiplier = ConfigManager.Instance.FindItemValue<float>("GlobalTimeSpeedMultiplier");
            TimeOfDay.Instance.lengthOfHours = ConfigManager.Instance.FindItemValue<int>("LengthOfHours");
            TimeOfDay.Instance.numberOfHours = ConfigManager.Instance.FindItemValue<int>("NumberOfHours");
            TimeOfDay.Instance.quotaVariables.deadlineDaysAmount = ConfigManager.Instance.FindItemValue<int>("DeadlineDaysAmount");
            TimeOfDay.Instance.quotaVariables.startingQuota = ConfigManager.Instance.FindItemValue<int>("StartingQuota");
            TimeOfDay.Instance.quotaVariables.startingCredits = ConfigManager.Instance.FindItemValue<int>("StartingCredits");
            RoundManager.Instance.scrapAmountMultiplier = ConfigManager.Instance.FindItemValue<float>("ScrapAmountMultiplier");
            RoundManager.Instance.scrapValueMultiplier = ConfigManager.Instance.FindItemValue<float>("ScrapValueMultiplier");
            RoundManager.Instance.mapSizeMultiplier = ConfigManager.Instance.FindItemValue<float>("MapSizeMultiplier");
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
