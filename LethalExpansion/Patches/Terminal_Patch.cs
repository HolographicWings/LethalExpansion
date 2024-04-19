using DunGen;
using HarmonyLib;
using LethalExpansion.Extenders;
using LethalExpansion.Utils;
using LethalSDK.Utils;
using LethalSDK.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    public class Terminal_Patch
    {
        private static int[] defaultMoonRoutePrices = null;
        public static bool scrapsPatched = false;
        public static bool scrapsRecursivelyPatched = false;
        public static bool moonsPatched = false;
        public static bool assetsGotten = false;
        public static bool flowFireExitSaved = false;

        public static List<int> fireExitAmounts = new List<int>();

        public static TerminalKeyword routeKeyword;
        public static TerminalKeyword infoKeyword;
        public static TerminalKeyword confirmKeyword;
        public static TerminalKeyword denyKeyword;

        public static List<string> newScrapsNames = new List<string>();
        public static List<string> newMoonsNames = new List<string>();
        public static Dictionary<int, Moon> newMoons = new Dictionary<int, Moon>();

        public static void MainPatch(Terminal __instance)
        {
            //reset flags
            scrapsPatched = false;
            scrapsRecursivelyPatched = false;
            moonsPatched = false;
            //get the route terminal keyword for ulterior usage
            routeKeyword = __instance.terminalNodes.allKeywords.First(k => k.word == "route");
            infoKeyword = __instance.terminalNodes.allKeywords.First(k => k.word == "info");
            confirmKeyword = __instance.terminalNodes.allKeywords.First(k => k.word == "confirm");
            denyKeyword = __instance.terminalNodes.allKeywords.First(k => k.word == "deny");
            //RemoveMoon(__instance, "Experimentation");
            //remove duplicated moon routes
            Hotfix_DoubleRoutes();
            //get a plenty of assets references for ulterior usage
            GatherAssets();
            AddScraps(__instance);
            ResetTerminalKeywords(__instance);
            AddMoons(__instance);
            AddScrapsRecursive(__instance);
            ResetMoonsRoutePrices();
            UpdateMoonsRoutePrices();
            UpdateMoonsCatalogue(__instance);
            //keep the default fire exits amount of every dungeon flow
            SaveFireExitAmounts();
            //if the moon description and orbit loading is delayed when trying to join a lobby already orbitting a modded moon
            if (LethalExpansion.delayedLevelChange != -1)
            {
                //retry to load it's description and orbit since the new moons have been added
                StartOfRound.Instance.ChangeLevel(LethalExpansion.delayedLevelChange);
                StartOfRound.Instance.ChangePlanet();
            }
            //request weathers to host
            NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.request, "hostweathers", string.Empty, 0);
            LethalExpansion.Log.LogInfo("Terminal Main Patch.");
            //get the list of gathered asset references if in debug mode
            if (ConfigManager.Instance.FindItemValue<bool>("SettingsDebug"))
            {
                AssetGather.Instance.GetList();
            }
        }
        public static void MainPatchPostConfig(Terminal __instance)
        {
            ResetMoonsRoutePrices();
            UpdateMoonsRoutePrices();
            UpdateMoonsCatalogue(__instance);
        }
        private static void GatherAssets()
        {
            if (!assetsGotten)
            {
                foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
                {
                    AssetGather.Instance.AddAudioClip(item.grabSFX);
                    AssetGather.Instance.AddAudioClip(item.dropSFX);
                    AssetGather.Instance.AddAudioClip(item.pocketSFX);
                    AssetGather.Instance.AddAudioClip(item.throwSFX);
                    if(item.spawnPrefab != null)
                    {
                        List<Type> componentTypes = new List<Type>
                        {
                            typeof(Shovel),
                            typeof(FlashlightItem),
                            typeof(WalkieTalkie),
                            typeof(ExtensionLadderItem),
                            typeof(NoisemakerProp),
                            typeof(PatcherTool),
                            typeof(WhoopieCushionItem),
                            typeof(ShotgunItem),
                            typeof(RemoteProp),
                            typeof(AudioSource)
                        };
                        List<System.Object> foundComponents = new List<System.Object>();
                        foreach (Type type in componentTypes)
                        {
                            Component component = item.spawnPrefab.GetComponent(type);

                            if (component != null)
                            {
                                foundComponents.Add(component);
                            }
                        }
                        foreach (System.Object component in foundComponents)
                        {
                            switch (component.GetType().Name)
                            {
                                case "Shovel":
                                    AssetGather.Instance.AddAudioClip(((Shovel)component).reelUp);
                                    AssetGather.Instance.AddAudioClip(((Shovel)component).swing);
                                    AssetGather.Instance.AddAudioClip(((Shovel)component).hitSFX);
                                    break;
                                case "FlashlightItem":
                                    AssetGather.Instance.AddAudioClip(((FlashlightItem)component).flashlightClips);
                                    AssetGather.Instance.AddAudioClip(((FlashlightItem)component).outOfBatteriesClip);
                                    AssetGather.Instance.AddAudioClip(((FlashlightItem)component).flashlightFlicker);
                                    break;
                                case "WalkieTalkie":
                                    AssetGather.Instance.AddAudioClip(((WalkieTalkie)component).stopTransmissionSFX);
                                    AssetGather.Instance.AddAudioClip(((WalkieTalkie)component).startTransmissionSFX);
                                    AssetGather.Instance.AddAudioClip(((WalkieTalkie)component).switchWalkieTalkiePowerOff);
                                    AssetGather.Instance.AddAudioClip(((WalkieTalkie)component).switchWalkieTalkiePowerOn);
                                    AssetGather.Instance.AddAudioClip(((WalkieTalkie)component).talkingOnWalkieTalkieNotHeldSFX);
                                    AssetGather.Instance.AddAudioClip(((WalkieTalkie)component).playerDieOnWalkieTalkieSFX);
                                    break;
                                case "ExtensionLadderItem":
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).hitRoof);
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).fullExtend);
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).hitWall);
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).ladderExtendSFX);
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).ladderFallSFX);
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).ladderShrinkSFX);
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).blinkWarningSFX);
                                    AssetGather.Instance.AddAudioClip(((ExtensionLadderItem)component).lidOpenSFX);
                                    break;
                                case "NoisemakerProp":
                                    AssetGather.Instance.AddAudioClip(((NoisemakerProp)component).noiseSFX);
                                    AssetGather.Instance.AddAudioClip(((NoisemakerProp)component).noiseSFXFar);
                                    break;
                                case "PatcherTool":
                                    AssetGather.Instance.AddAudioClip(((PatcherTool)component).activateClips);
                                    AssetGather.Instance.AddAudioClip(((PatcherTool)component).beginShockClips);
                                    AssetGather.Instance.AddAudioClip(((PatcherTool)component).overheatClips);
                                    AssetGather.Instance.AddAudioClip(((PatcherTool)component).finishShockClips);
                                    AssetGather.Instance.AddAudioClip(((PatcherTool)component).outOfBatteriesClip);
                                    AssetGather.Instance.AddAudioClip(((PatcherTool)component).detectAnomaly);
                                    AssetGather.Instance.AddAudioClip(((PatcherTool)component).scanAnomaly);
                                    break;
                                case "WhoopieCushionItem":
                                    AssetGather.Instance.AddAudioClip(((WhoopieCushionItem)component).fartAudios);
                                    break;
                                case "ShotgunItem":
                                    AssetGather.Instance.AddAudioClip(((ShotgunItem)component).gunShootSFX);
                                    AssetGather.Instance.AddAudioClip(((ShotgunItem)component).gunReloadSFX);
                                    AssetGather.Instance.AddAudioClip(((ShotgunItem)component).gunReloadFinishSFX);
                                    AssetGather.Instance.AddAudioClip(((ShotgunItem)component).noAmmoSFX);
                                    AssetGather.Instance.AddAudioClip(((ShotgunItem)component).gunSafetySFX);
                                    AssetGather.Instance.AddAudioClip(((ShotgunItem)component).switchSafetyOnSFX);
                                    AssetGather.Instance.AddAudioClip(((ShotgunItem)component).switchSafetyOffSFX);
                                    break;
                                case "AudioSource":
                                    AssetGather.Instance.AddAudioClip(((AudioSource)component).clip);
                                    break;
                            }
                        }
                    }
                }
                AssetGather.Instance.AddSprites(GameObject.Find("Environment/HangarShip/StartGameLever").GetComponent<InteractTrigger>().hoverIcon);
                AssetGather.Instance.AddSprites(GameObject.Find("Environment/HangarShip/Terminal/TerminalTrigger/TerminalScript").GetComponent<InteractTrigger>().hoverIcon);
                AssetGather.Instance.AddSprites(GameObject.Find("Environment/HangarShip/OutsideShipRoom/Ladder/LadderTrigger").GetComponent<InteractTrigger>().hoverIcon);
                foreach (SelectableLevel level in StartOfRound.Instance.levels)
                {
                    AssetGather.Instance.AddPlanetPrefabs(level.planetPrefab);
                    level.spawnableMapObjects.ToList().ForEach(e => AssetGather.Instance.AddMapObjects(e.prefabToSpawn));
                    level.spawnableOutsideObjects.ToList().ForEach(e => AssetGather.Instance.AddOutsideObject(e.spawnableObject));
                    level.spawnableScrap.ForEach(e => AssetGather.Instance.AddScrap(e.spawnableItem));
                    AssetGather.Instance.AddLevelAmbiances(level.levelAmbienceClips);
                    level.Enemies.ForEach(e => AssetGather.Instance.AddEnemies(e.enemyType));
                    level.OutsideEnemies.ForEach(e => AssetGather.Instance.AddEnemies(e.enemyType));
                    level.DaytimeEnemies.ForEach(e => AssetGather.Instance.AddEnemies(e.enemyType));
                }
                foreach (KeyValuePair<String, (AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                {
                    (AssetBundle, ModManifest) _tmp = AssetBundlesManager.Instance.Load(bundle.Key);
                    if (_tmp.Item2.assetBank != null)
                    {
                        try
                        {
                            foreach (var a in _tmp.Item2.assetBank.AudioClips())
                            {
                                AssetGather.Instance.AddAudioClip(a.AudioClipName, bundle.Value.Item1.LoadAsset<AudioClip>(a.AudioClipPath));
                            }
                            foreach (var p in _tmp.Item2.assetBank.PlanetPrefabs())
                            {
                                var prefab = bundle.Value.Item1.LoadAsset<GameObject>(p.PlanetPrefabPath);
                                if(prefab != null)
                                {
                                    Animator animator;
                                    if (prefab.GetComponent<Animator>() == null)
                                    {
                                        animator = prefab.AddComponent<Animator>();
                                        animator = AssetGather.Instance.planetPrefabs.First().Value.GetComponent<Animator>();
                                    }
                                    AssetGather.Instance.AddPlanetPrefabs(prefab);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LethalExpansion.Log.LogError(ex.Message);
                        }
                    }
                }
                //Interfacing LE and SDK Asset Gathers
                AssetGatherDialog.audioClips = AssetGather.Instance.audioClips;
                AssetGatherDialog.audioMixers = AssetGather.Instance.audioMixers;
                AssetGatherDialog.sprites = AssetGather.Instance.sprites;

                assetsGotten = true;
            }
        }
        public static void AddScraps(Terminal __instance)
        {
            if (ConfigManager.Instance.FindItemValue<bool>("LoadModules") && !scrapsPatched)
            {
                AudioClip defaultDropSound = AssetGather.Instance.audioClips["DropCan"];
                AudioClip defaultGrabSound = AssetGather.Instance.audioClips["ShovelPickUp"];

                AudioClip defaultShovelReelUp = AssetGather.Instance.audioClips["ShovelReelUp"];
                AudioClip defaultShovelSwing = AssetGather.Instance.audioClips["ShovelSwing"];
                AudioClip[] defaultShovelHitSFXs = new AudioClip[] { AssetGather.Instance.audioClips["ShovelHitDefault"], AssetGather.Instance.audioClips["ShovelHitDefault2"] };

                AudioClip[] defaultFlashlightClips = new AudioClip[] { AssetGather.Instance.audioClips["FlashlightClick"] };
                AudioClip defaultFlashlightOutOfBatteriesClip = AssetGather.Instance.audioClips["FlashlightOutOfBatteries"];
                AudioClip defaultFlashlightFlicker = AssetGather.Instance.audioClips["FlashlightFlicker"];

                AudioClip[] defaultNoisemakerNoiseSFX = new AudioClip[] { AssetGather.Instance.audioClips["ClownHorn1"] };
                AudioClip[] defaultNoisemakerNoiseSFXFar = new AudioClip[] { AssetGather.Instance.audioClips["ClownHornFar"] };

                AudioClip[] defaultWhoopieCushionAudios = new AudioClip[] { AssetGather.Instance.audioClips["Fart1"],AssetGather.Instance.audioClips["Fart2"],AssetGather.Instance.audioClips["Fart3"],AssetGather.Instance.audioClips["Fart5"] };

                foreach (KeyValuePair<String, (AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                {
                    (AssetBundle, ModManifest) _tmp = AssetBundlesManager.Instance.Load(bundle.Key);

                    if (_tmp.Item1 != null && _tmp.Item2 != null)
                    {
                        if (_tmp.Item2.scraps != null)
                        {
                            foreach (var newScrap in _tmp.Item2.scraps)
                            {
                                if (newScrap != null && newScrap.prefab != null && (newScrap.RequiredBundles == null || AssetBundlesManager.Instance.BundlesLoaded(newScrap.RequiredBundles)) && (newScrap.IncompatibleBundles == null || !AssetBundlesManager.Instance.IncompatibleBundlesLoaded(newScrap.IncompatibleBundles)))
                                {
                                    if (!newScrapsNames.Contains(newScrap.itemName))
                                    {
                                        try
                                        {
                                            Item tmpItem = null;
                                            object physicsProp = null;
                                            switch (newScrap.scrapType)
                                            {
                                                case ScrapType.Normal:
                                                    PhysicsProp pp = newScrap.prefab.GetComponent<PhysicsProp>();
                                                    if (pp != null)
                                                    {
                                                        tmpItem = pp.itemProperties;
                                                    }
                                                    break;
                                                case ScrapType.Shovel:
                                                    Shovel s = newScrap.prefab.GetComponent<Shovel>();
                                                    if (s != null)
                                                    {
                                                        tmpItem = s.itemProperties;
                                                        physicsProp = newScrap.prefab.GetComponent<Shovel>();
                                                        if (physicsProp != null)
                                                        {
                                                            AudioClip reelUp = null;
                                                            if (newScrap.reelUp.Length > 0 && AssetGather.Instance.audioClips.ContainsKey(newScrap.reelUp))
                                                            {
                                                                reelUp = AssetGather.Instance.audioClips[newScrap.reelUp];
                                                            }
                                                            AudioClip swing = null;
                                                            if (newScrap.swing.Length > 0 && AssetGather.Instance.audioClips.ContainsKey(newScrap.swing))
                                                            {
                                                                swing = AssetGather.Instance.audioClips[newScrap.swing];
                                                            }
                                                            List<AudioClip> hitSFX = new List<AudioClip>();
                                                            if (newScrap.hitSFX != null && newScrap.hitSFX.Length > 0)
                                                            {
                                                                foreach (string clip in newScrap.hitSFX)
                                                                {
                                                                    if (AssetGather.Instance.audioClips.ContainsKey(clip))
                                                                    {
                                                                        hitSFX.Add(AssetGather.Instance.audioClips[clip]);
                                                                    }
                                                                }
                                                            }
                                                            ((Shovel)physicsProp).reelUp = reelUp != null ? reelUp : defaultShovelReelUp;
                                                            ((Shovel)physicsProp).swing = swing != null ? swing : defaultShovelSwing;
                                                            ((Shovel)physicsProp).hitSFX = hitSFX != null && hitSFX.Count > 0 ? hitSFX.ToArray() : defaultShovelHitSFXs;
                                                        }
                                                    }
                                                    break;
                                                case ScrapType.Flashlight:
                                                    FlashlightItem fi = newScrap.prefab.GetComponent<FlashlightItem>();
                                                    if (fi != null)
                                                    {
                                                        tmpItem = fi.itemProperties;
                                                        physicsProp = newScrap.prefab.GetComponent<FlashlightItem>();
                                                        if (physicsProp != null)
                                                        {
                                                            AudioClip outOfBatteriesClip = null;
                                                            if (newScrap.outOfBatteriesClip.Length > 0 && AssetGather.Instance.audioClips.ContainsKey(newScrap.outOfBatteriesClip))
                                                            {
                                                                outOfBatteriesClip = AssetGather.Instance.audioClips[newScrap.outOfBatteriesClip];
                                                            }
                                                            AudioClip flashlightFlicker = null;
                                                            if (newScrap.flashlightFlicker.Length > 0 && AssetGather.Instance.audioClips.ContainsKey(newScrap.flashlightFlicker))
                                                            {
                                                                flashlightFlicker = AssetGather.Instance.audioClips[newScrap.flashlightFlicker];
                                                            }
                                                            List<AudioClip> flashlightClips = new List<AudioClip>();
                                                            if (newScrap.flashlightClips != null && newScrap.flashlightClips.Length > 0)
                                                            {
                                                                foreach (string clip in newScrap.flashlightClips)
                                                                {
                                                                    if (AssetGather.Instance.audioClips.ContainsKey(clip))
                                                                    {
                                                                        flashlightClips.Add(AssetGather.Instance.audioClips[clip]);
                                                                    }
                                                                }
                                                            }
                                                            ((FlashlightItem)physicsProp).outOfBatteriesClip = outOfBatteriesClip != null ? outOfBatteriesClip : defaultFlashlightOutOfBatteriesClip;
                                                            ((FlashlightItem)physicsProp).flashlightFlicker = flashlightFlicker != null ? flashlightFlicker : defaultFlashlightFlicker;
                                                            ((FlashlightItem)physicsProp).flashlightClips = flashlightClips != null && flashlightClips.Count > 0 ? flashlightClips.ToArray() : defaultFlashlightClips;
                                                        }
                                                    }
                                                    break;
                                                case ScrapType.Noisemaker:
                                                    NoisemakerProp np = newScrap.prefab.GetComponent<NoisemakerProp>();
                                                    if(np != null)
                                                    {
                                                        tmpItem = np.itemProperties;
                                                        physicsProp = newScrap.prefab.GetComponent<NoisemakerProp>();
                                                        if (physicsProp != null)
                                                        {
                                                            List<AudioClip> noiseSFX = new List<AudioClip>();
                                                            if (newScrap.noiseSFX != null && newScrap.noiseSFX.Length > 0)
                                                            {
                                                                foreach (string clip in newScrap.noiseSFX)
                                                                {
                                                                    if (AssetGather.Instance.audioClips.ContainsKey(clip))
                                                                    {
                                                                        noiseSFX.Add(AssetGather.Instance.audioClips[clip]);
                                                                    }
                                                                }
                                                            }
                                                            List<AudioClip> noiseSFXFar = new List<AudioClip>();
                                                            if (newScrap.noiseSFXFar != null && newScrap.noiseSFXFar.Length > 0)
                                                            {
                                                                foreach (string clip in newScrap.noiseSFXFar)
                                                                {
                                                                    if (AssetGather.Instance.audioClips.ContainsKey(clip))
                                                                    {
                                                                        noiseSFXFar.Add(AssetGather.Instance.audioClips[clip]);
                                                                    }
                                                                }
                                                            }
                                                            ((NoisemakerProp)physicsProp).noiseSFX = noiseSFX != null && noiseSFX.Count > 0 ? noiseSFX.ToArray() : defaultNoisemakerNoiseSFX;
                                                            ((NoisemakerProp)physicsProp).noiseSFXFar = noiseSFXFar != null && noiseSFXFar.Count > 0 ? noiseSFXFar.ToArray() : defaultNoisemakerNoiseSFXFar;
                                                        }
                                                    }
                                                    break;
                                                case ScrapType.WhoopieCushion:
                                                    WhoopieCushionItem wci = newScrap.prefab.GetComponent<WhoopieCushionItem>();
                                                    if(wci != null)
                                                    {
                                                        tmpItem = wci.itemProperties;
                                                        physicsProp = newScrap.prefab.GetComponent<WhoopieCushionItem>();
                                                        if (physicsProp != null)
                                                        {
                                                            List<AudioClip> fartAudios = new List<AudioClip>();
                                                            if (newScrap.fartAudios != null && newScrap.fartAudios.Length > 0)
                                                            {
                                                                foreach (string clip in newScrap.fartAudios)
                                                                {
                                                                    if (AssetGather.Instance.audioClips.ContainsKey(clip))
                                                                    {
                                                                        fartAudios.Add(AssetGather.Instance.audioClips[clip]);
                                                                    }
                                                                }
                                                            }
                                                            ((WhoopieCushionItem)physicsProp).fartAudios = fartAudios != null && fartAudios.Count > 0 ? fartAudios.ToArray() : defaultWhoopieCushionAudios;
                                                        }
                                                    }
                                                    break;
                                            }

                                            if (tmpItem != null)
                                            {

                                                AudioSource audioSource = newScrap.prefab.GetComponent<AudioSource>();
                                                if (audioSource != null)
                                                {
                                                    audioSource.outputAudioMixerGroup = AssetGather.Instance.audioMixers.ContainsKey("Diagetic") ? AssetGather.Instance.audioMixers["Diagetic"].Item2.First(a => a.name == "Master") : null;
                                                }

                                                if (newScrap.noiseAudio != null && newScrap.scrapType == ScrapType.Noisemaker)
                                                {
                                                    newScrap.noiseAudio.outputAudioMixerGroup = AssetGather.Instance.audioMixers.ContainsKey("Diagetic") ? AssetGather.Instance.audioMixers["Diagetic"].Item2.First(a => a.name == "Master") : null;
                                                }

                                                AudioClip _tpmGrabSFX = null;
                                                if (newScrap.grabSFX.Length > 0 && AssetGather.Instance.audioClips.ContainsKey(newScrap.grabSFX))
                                                {
                                                    _tpmGrabSFX = AssetGather.Instance.audioClips[newScrap.grabSFX];
                                                }
                                                tmpItem.grabSFX = _tpmGrabSFX != null ? _tpmGrabSFX : defaultGrabSound;
                                                AudioClip _tpmDropSFX = null;
                                                if (newScrap.grabSFX.Length > 0 && AssetGather.Instance.audioClips.ContainsKey(newScrap.dropSFX))
                                                {
                                                    _tpmDropSFX = AssetGather.Instance.audioClips[newScrap.dropSFX];
                                                }
                                                tmpItem.dropSFX = _tpmDropSFX != null ? _tpmDropSFX : defaultDropSound;

                                                StartOfRound.Instance.allItemsList.itemsList.Add(tmpItem);
                                                if (newScrap.useGlobalSpawnWeight)
                                                {
                                                    SpawnableItemWithRarity itemRarity = new SpawnableItemWithRarity();
                                                    itemRarity.spawnableItem = tmpItem;
                                                    itemRarity.rarity = newScrap.globalSpawnWeight;
                                                    foreach (SelectableLevel level in __instance.moonsCatalogueList)
                                                    {
                                                        if(!newScrap.planetSpawnBlacklist.Any(l => l == level.PlanetName))
                                                        {
                                                            level.spawnableScrap.Add(itemRarity);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ScrapSpawnChancePerScene[] chances = newScrap.perPlanetSpawnWeight();
                                                    foreach (SelectableLevel level in __instance.moonsCatalogueList)
                                                    {
                                                        try
                                                        {
                                                            if ((chances.Any(l => l.SceneName == level.PlanetName) || chances.Any(l => l.SceneName == "Others")) && !newScrap.planetSpawnBlacklist.Any(l => l == level.PlanetName))
                                                            {
                                                                ScrapSpawnChancePerScene chance = new ScrapSpawnChancePerScene(string.Empty, 0);
                                                                try
                                                                {
                                                                    chance = chances.First(l => l.SceneName == level.PlanetName);
                                                                }
                                                                catch
                                                                {
                                                                    try
                                                                    {
                                                                        chance = chances.First(l => l.SceneName == "Others");
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        LethalExpansion.Log.LogError(ex);
                                                                    }
                                                                }

                                                                if (chance.SceneName != string.Empty)
                                                                {
                                                                    SpawnableItemWithRarity itemRarity = new SpawnableItemWithRarity();
                                                                    itemRarity.spawnableItem = tmpItem;
                                                                    itemRarity.rarity = chance.SpawnWeight;

                                                                    level.spawnableScrap.Add(itemRarity);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LethalExpansion.Log.LogError(ex.Message);
                                                        }
                                                    }
                                                }
                                                newScrapsNames.Add(tmpItem.itemName);
                                                AssetGather.Instance.AddScrap(tmpItem);
                                                LethalExpansion.Log.LogInfo($"{newScrap.itemName} Scrap added.");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LethalExpansion.Log.LogError(ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        LethalExpansion.Log.LogWarning($"{newScrap.itemName} Scrap already added.");
                                    }
                                }
                            }
                        }
                    }
                }
                scrapsPatched = true;
            }
        }
        public static void AddScrapsRecursive(Terminal __instance)
        {
            if (ConfigManager.Instance.FindItemValue<bool>("LoadModules") && !scrapsRecursivelyPatched)
            {
                foreach (KeyValuePair<String, (AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                {
                    (AssetBundle, ModManifest) _tmp = AssetBundlesManager.Instance.Load(bundle.Key);

                    if (_tmp.Item1 != null && _tmp.Item2 != null)
                    {
                        if (_tmp.Item2.scraps != null)
                        {
                            foreach (var newScrap in _tmp.Item2.scraps)
                            {
                                if (newScrap != null && newScrap.prefab != null && (newScrap.RequiredBundles == null || AssetBundlesManager.Instance.BundlesLoaded(newScrap.RequiredBundles)) && (newScrap.IncompatibleBundles == null || !AssetBundlesManager.Instance.IncompatibleBundlesLoaded(newScrap.IncompatibleBundles)))
                                {
                                    if (newScrapsNames.Contains(newScrap.itemName))
                                    {
                                        try
                                        {
                                            string[] vanillaBlacklist = new string[] { "41 Experimentation", "220 Assurance", "56 Vow", "21 Offense", "61 March", "85 Rend", "7 Dine", "8 Titan"};

                                            Item tmpItem = null;
                                            switch (newScrap.scrapType)
                                            {
                                                case ScrapType.Normal:
                                                    PhysicsProp pp = newScrap.prefab.GetComponent<PhysicsProp>();
                                                    if (pp != null)
                                                    {
                                                        tmpItem = pp.itemProperties;
                                                    }
                                                    break;
                                                case ScrapType.Shovel:
                                                    Shovel s = newScrap.prefab.GetComponent<Shovel>();
                                                    if (s != null)
                                                    {
                                                        tmpItem = s.itemProperties;
                                                    }
                                                    break;
                                                case ScrapType.Flashlight:
                                                    FlashlightItem fi = newScrap.prefab.GetComponent<FlashlightItem>();
                                                    if (fi != null)
                                                    {
                                                        tmpItem = fi.itemProperties;
                                                    }
                                                    break;
                                                case ScrapType.Noisemaker:
                                                    NoisemakerProp np = newScrap.prefab.GetComponent<NoisemakerProp>();
                                                    if (np != null)
                                                    {
                                                        tmpItem = np.itemProperties;
                                                    }
                                                    break;
                                                case ScrapType.WhoopieCushion:
                                                    WhoopieCushionItem wci = newScrap.prefab.GetComponent<WhoopieCushionItem>();
                                                    if (wci != null)
                                                    {
                                                        tmpItem = wci.itemProperties;
                                                    }
                                                    break;
                                            }
                                            if (tmpItem != null)
                                            {
                                                if (newScrap.useGlobalSpawnWeight)
                                                {
                                                    SpawnableItemWithRarity itemRarity = new SpawnableItemWithRarity();
                                                    itemRarity.spawnableItem = tmpItem;
                                                    itemRarity.rarity = newScrap.globalSpawnWeight;
                                                    foreach (SelectableLevel level in __instance.moonsCatalogueList)
                                                    {
                                                        string[] moonScrapBlacklist = new string[0];
                                                        if (newMoons.Any(m => m.Value.PlanetName == level.PlanetName))
                                                        {
                                                            moonScrapBlacklist = newMoons.First(m => m.Value.PlanetName == level.PlanetName).Value.spawnableScrapBlacklist;
                                                        }
                                                        if (!vanillaBlacklist.Contains(level.PlanetName) && !newScrap.planetSpawnBlacklist.Any(l => l == level.PlanetName) && !moonScrapBlacklist.Any(s => s == newScrap.itemName))
                                                        {
                                                            level.spawnableScrap.Add(itemRarity);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ScrapSpawnChancePerScene[] chances = newScrap.perPlanetSpawnWeight();
                                                    foreach (SelectableLevel level in __instance.moonsCatalogueList)
                                                    {
                                                        try
                                                        {
                                                            string[] moonScrapBlacklist = new string[0];
                                                            if (newMoons.Any(m => m.Value.PlanetName == level.PlanetName))
                                                            {
                                                                moonScrapBlacklist = newMoons.First(m => m.Value.PlanetName == level.PlanetName).Value.spawnableScrapBlacklist;
                                                            }
                                                            if (!vanillaBlacklist.Contains(level.PlanetName) && !newScrap.planetSpawnBlacklist.Any(l => l == level.PlanetName) && !moonScrapBlacklist.Any(s => s == newScrap.itemName))
                                                            {
                                                                if (chances.Any(l => l.SceneName == level.PlanetName) || chances.Any(l => l.SceneName == "Others"))
                                                                {
                                                                    ScrapSpawnChancePerScene chance = new ScrapSpawnChancePerScene(string.Empty, 0);
                                                                    try
                                                                    {
                                                                        chance = chances.First(l => l.SceneName == level.PlanetName);
                                                                    }
                                                                    catch
                                                                    {
                                                                        try
                                                                        {
                                                                            chance = chances.First(l => l.SceneName == "Others");
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            LethalExpansion.Log.LogError(ex);
                                                                        }
                                                                    }

                                                                    if (chance.SceneName != string.Empty)
                                                                    {
                                                                        SpawnableItemWithRarity itemRarity = new SpawnableItemWithRarity();
                                                                        itemRarity.spawnableItem = tmpItem;
                                                                        itemRarity.rarity = chance.SpawnWeight;

                                                                        level.spawnableScrap.Add(itemRarity);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            LethalExpansion.Log.LogError(ex.Message);
                                                        }
                                                    }
                                                }
                                                LethalExpansion.Log.LogInfo($"{newScrap.itemName} Scrap patched recursively.");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LethalExpansion.Log.LogError(ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        LethalExpansion.Log.LogWarning($"{newScrap.itemName} Scrap already added.");
                                    }
                                }
                            }
                        }
                    }
                }
                scrapsRecursivelyPatched = true;
            }
        }
        public static void AddMoons(Terminal __instance)
        {
            newMoons = new Dictionary<int, Moon>();
            if (ConfigManager.Instance.FindItemValue<bool>("LoadModules") && !moonsPatched)
            {
                foreach (KeyValuePair<String, (AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                {
                    (AssetBundle, ModManifest) _tmp = AssetBundlesManager.Instance.Load(bundle.Key);

                    if (_tmp.Item1 != null && _tmp.Item2 != null)
                    {
                        if (_tmp.Item2.moons != null)
                        {
                            foreach (Moon newMoon in _tmp.Item2.moons)
                            {
                                if (newMoon != null && newMoon.IsEnabled)
                                {
                                    if (!newMoonsNames.Contains(newMoon.MoonName))
                                    {
                                        try
                                        {
                                            if ((newMoon.RequiredBundles != null && !AssetBundlesManager.Instance.BundlesLoaded(newMoon.RequiredBundles)) || (newMoon.IncompatibleBundles != null && AssetBundlesManager.Instance.IncompatibleBundlesLoaded(newMoon.IncompatibleBundles)))
                                            {
                                                LethalExpansion.Log.LogWarning($"{newMoon.MoonName} can't be added, missing or incompatible bundles.");
                                            }
                                            else
                                            {
                                                TerminalNode cancelRouteNode = null;
                                                foreach (CompatibleNoun option in routeKeyword.compatibleNouns[0].result.terminalOptions)
                                                {
                                                    if (option.result.name == "CancelRoute")
                                                    {
                                                        cancelRouteNode = option.result;
                                                        break;
                                                    }
                                                }

                                                SelectableLevel newLevel = ScriptableObject.CreateInstance<SelectableLevel>();

                                                newLevel.SetIsFromLE(true);

                                                newLevel.name = newMoon.PlanetName;
                                                newLevel.PlanetName = newMoon.PlanetName;
                                                newLevel.sceneName = "InitSceneLaunchOptions";
                                                newLevel.levelID = StartOfRound.Instance.levels.Length;
                                                if (newMoon.OrbitPrefabName != null && newMoon.OrbitPrefabName.Length > 0 && AssetGather.Instance.planetPrefabs.ContainsKey(newMoon.OrbitPrefabName))
                                                {
                                                    newLevel.planetPrefab = AssetGather.Instance.planetPrefabs[newMoon.OrbitPrefabName];
                                                }
                                                else
                                                {
                                                    newLevel.planetPrefab = AssetGather.Instance.planetPrefabs.First().Value;
                                                }
                                                newLevel.lockedForDemo = true;
                                                newLevel.spawnEnemiesAndScrap = newMoon.SpawnEnemiesAndScrap;
                                                if (!string.IsNullOrWhiteSpace(newMoon.PlanetDescription))
                                                {
                                                    newLevel.LevelDescription = newMoon.PlanetDescription;
                                                }
                                                else
                                                {
                                                    newLevel.LevelDescription = string.Empty;
                                                }
                                                newLevel.videoReel = newMoon.PlanetVideo;
                                                if (newMoon.RiskLevel != null && newMoon.RiskLevel.Length > 0)
                                                {
                                                    newLevel.riskLevel = newMoon.RiskLevel;
                                                }
                                                else
                                                {
                                                    newLevel.riskLevel = string.Empty;
                                                }
                                                newLevel.timeToArrive = newMoon.TimeToArrive;
                                                newLevel.DaySpeedMultiplier = newMoon.DaySpeedMultiplier;
                                                newLevel.planetHasTime = newMoon.PlanetHasTime;
                                                newLevel.factorySizeMultiplier = newMoon.FactorySizeMultiplier;

                                                newLevel.overrideWeather = newMoon.OverwriteWeather;
                                                newLevel.overrideWeatherType = (LevelWeatherType)(int)newMoon.OverwriteWeatherType;
                                                newLevel.currentWeather = LevelWeatherType.None;

                                                RandomWeatherPair[] tmpRandomWeatherTypes1 = newMoon.RandomWeatherTypes();
                                                List<RandomWeatherWithVariables> tmpRandomWeatherTypes2 = new List<RandomWeatherWithVariables>();
                                                foreach (RandomWeatherPair item in tmpRandomWeatherTypes1)
                                                {
                                                    tmpRandomWeatherTypes2.Add(new RandomWeatherWithVariables() { weatherType = (LevelWeatherType)(int)item.Weather, weatherVariable = item.WeatherVariable1, weatherVariable2 = item.WeatherVariable2 });
                                                }
                                                newLevel.randomWeathers = tmpRandomWeatherTypes2.ToArray();

                                                DungeonFlowPair[] tmpDungeonFlowTypes1 = newMoon.DungeonFlowTypes();
                                                List<IntWithRarity> tmpDungeonFlowTypes2 = new List<IntWithRarity>();
                                                foreach (DungeonFlowPair item in tmpDungeonFlowTypes1)
                                                {
                                                    tmpDungeonFlowTypes2.Add(new IntWithRarity() { id = item.ID, rarity = item.Rarity });
                                                }
                                                newLevel.dungeonFlowTypes = tmpDungeonFlowTypes2.ToArray();

                                                SpawnableScrapPair[] tmpSpawnableScrap1 = newMoon.SpawnableScrap();
                                                List<SpawnableItemWithRarity> tmpSpawnableScrap2 = new List<SpawnableItemWithRarity>();
                                                foreach (SpawnableScrapPair item in tmpSpawnableScrap1)
                                                {
                                                    try
                                                    {
                                                        tmpSpawnableScrap2.Add(new SpawnableItemWithRarity() { spawnableItem = AssetGather.Instance.scraps[item.ObjectName], rarity = item.SpawnWeight });
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        LethalExpansion.Log.LogWarning(ex.Message);
                                                    }
                                                }
                                                newLevel.spawnableScrap = tmpSpawnableScrap2;

                                                newLevel.minScrap = newMoon.MinScrap;
                                                newLevel.maxScrap = newMoon.MaxScrap;

                                                if (newMoon.LevelAmbienceClips != null && newMoon.LevelAmbienceClips.Length > 0 && AssetGather.Instance.levelAmbiances.ContainsKey(newMoon.LevelAmbienceClips))
                                                {
                                                    newLevel.levelAmbienceClips = AssetGather.Instance.levelAmbiances[newMoon.LevelAmbienceClips];
                                                }
                                                else
                                                {
                                                    newLevel.levelAmbienceClips = AssetGather.Instance.levelAmbiances.First().Value;
                                                }

                                                newLevel.maxEnemyPowerCount = newMoon.MaxEnemyPowerCount;

                                                SpawnableEnemiesPair[] tmpEnemies1 = newMoon.Enemies();
                                                List<SpawnableEnemyWithRarity> tmpEnemies2 = new List<SpawnableEnemyWithRarity>();
                                                foreach (SpawnableEnemiesPair item in tmpEnemies1)
                                                {
                                                    tmpEnemies2.Add(new SpawnableEnemyWithRarity() { enemyType = AssetGather.Instance.enemies[item.EnemyName], rarity = item.SpawnWeight });
                                                }
                                                newLevel.Enemies = tmpEnemies2;

                                                newLevel.enemySpawnChanceThroughoutDay = newMoon.EnemySpawnChanceThroughoutDay;
                                                newLevel.spawnProbabilityRange = newMoon.SpawnProbabilityRange;

                                                SpawnableMapObjectPair[] tmpSpawnableMapObjects1 = newMoon.SpawnableMapObjects();
                                                List<SpawnableMapObject> tmpSpawnableMapObjects2 = new List<SpawnableMapObject>();
                                                foreach (SpawnableMapObjectPair item in tmpSpawnableMapObjects1)
                                                {
                                                    tmpSpawnableMapObjects2.Add(new SpawnableMapObject() { prefabToSpawn = AssetGather.Instance.mapObjects[item.ObjectName], spawnFacingAwayFromWall = item.SpawnFacingAwayFromWall, numberToSpawn = item.SpawnRate });
                                                }
                                                newLevel.spawnableMapObjects = tmpSpawnableMapObjects2.ToArray();

                                                SpawnableOutsideObjectPair[] tmpSpawnableOutsideObjects1 = newMoon.SpawnableOutsideObjects();
                                                List<SpawnableOutsideObjectWithRarity> tmpSpawnableOutsideObjects2 = new List<SpawnableOutsideObjectWithRarity>();
                                                foreach (SpawnableOutsideObjectPair item in tmpSpawnableOutsideObjects1)
                                                {
                                                    tmpSpawnableOutsideObjects2.Add(new SpawnableOutsideObjectWithRarity() { spawnableObject = AssetGather.Instance.outsideObjects[item.ObjectName], randomAmount = item.SpawnRate });
                                                }
                                                newLevel.spawnableOutsideObjects = tmpSpawnableOutsideObjects2.ToArray();

                                                newLevel.maxOutsideEnemyPowerCount = newMoon.MaxOutsideEnemyPowerCount;
                                                newLevel.maxDaytimeEnemyPowerCount = newMoon.MaxDaytimeEnemyPowerCount;

                                                SpawnableEnemiesPair[] tmpOutsideEnemies1 = newMoon.OutsideEnemies();
                                                List<SpawnableEnemyWithRarity> tmpOutsideEnemies2 = new List<SpawnableEnemyWithRarity>();
                                                foreach (SpawnableEnemiesPair item in tmpOutsideEnemies1)
                                                {
                                                    tmpOutsideEnemies2.Add(new SpawnableEnemyWithRarity() { enemyType = AssetGather.Instance.enemies[item.EnemyName], rarity = item.SpawnWeight });
                                                }
                                                newLevel.OutsideEnemies = tmpOutsideEnemies2;

                                                SpawnableEnemiesPair[] tmpDaytimeEnemies1 = newMoon.DaytimeEnemies();
                                                List<SpawnableEnemyWithRarity> tmpDaytimeEnemies2 = new List<SpawnableEnemyWithRarity>();
                                                foreach (SpawnableEnemiesPair item in tmpDaytimeEnemies1)
                                                {
                                                    tmpDaytimeEnemies2.Add(new SpawnableEnemyWithRarity() { enemyType = AssetGather.Instance.enemies[item.EnemyName], rarity = item.SpawnWeight });
                                                }
                                                newLevel.DaytimeEnemies = tmpDaytimeEnemies2;

                                                newLevel.outsideEnemySpawnChanceThroughDay = newMoon.OutsideEnemySpawnChanceThroughDay;
                                                newLevel.daytimeEnemySpawnChanceThroughDay = newMoon.DaytimeEnemySpawnChanceThroughDay;
                                                newLevel.daytimeEnemiesProbabilityRange = newMoon.DaytimeEnemiesProbabilityRange;
                                                newLevel.levelIncludesSnowFootprints = newMoon.LevelIncludesSnowFootprints;

                                                newLevel.SetFireExitAmountOverwrite(newMoon.FireExitsAmountOverwrite);

                                                __instance.moonsCatalogueList = __instance.moonsCatalogueList.AddItem(newLevel).ToArray();

                                                TerminalKeyword moonKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                                                moonKeyword.SetIsFromLE(true);
                                                moonKeyword.word = newMoon.RouteWord != null || newMoon.RouteWord.Length >= 3 ? newMoon.RouteWord.ToLower() : Regex.Replace(newMoon.MoonName, @"\s", "").ToLower();
                                                moonKeyword.name = newMoon.MoonName;
                                                moonKeyword.defaultVerb = routeKeyword;
                                                __instance.terminalNodes.allKeywords = __instance.terminalNodes.allKeywords.AddItem(moonKeyword).ToArray();

                                                TerminalNode moonRouteConfirm = ScriptableObject.CreateInstance<TerminalNode>();
                                                moonRouteConfirm.SetIsFromLE(true);
                                                moonRouteConfirm.name = newMoon.MoonName.ToLower() + "RouteConfirm";
                                                moonRouteConfirm.displayText = $"Routing autopilot to {newMoon.PlanetName}.\r\nYour new balance is [playerCredits].\r\n\r\n{newMoon.BoughtComment}\r\n\r\n";
                                                moonRouteConfirm.clearPreviousText = true;
                                                moonRouteConfirm.buyRerouteToMoon = StartOfRound.Instance.levels.Length;
                                                moonRouteConfirm.lockedInDemo = true;
                                                moonRouteConfirm.itemCost = newMoon.RoutePrice;

                                                TerminalNode moonRoute = ScriptableObject.CreateInstance<TerminalNode>();
                                                moonRoute.SetIsFromLE(true);
                                                moonRoute.name = newMoon.MoonName.ToLower() + "Route";
                                                moonRoute.displayText = $"The cost to route to {newMoon.PlanetName} is [totalCost]. It is \r\ncurrently [currentPlanetTime] on this moon.\r\n\r\nPlease CONFIRM or DENY.\r\n\r\n\r\n";
                                                moonRoute.clearPreviousText = true;
                                                moonRoute.buyRerouteToMoon = -2;
                                                moonRoute.displayPlanetInfo = StartOfRound.Instance.levels.Length;
                                                moonRoute.lockedInDemo = true;
                                                moonRoute.overrideOptions = true;
                                                moonRoute.itemCost = newMoon.RoutePrice;
                                                moonRoute.terminalOptions = new CompatibleNoun[]
                                                {
                                                    new CompatibleNoun(){noun = denyKeyword, result = cancelRouteNode != null ? cancelRouteNode : new TerminalNode()},
                                                    new CompatibleNoun(){noun = confirmKeyword, result = moonRouteConfirm},
                                                };

                                                CompatibleNoun moonRouteNoun = new CompatibleNoun();

                                                moonRouteNoun.noun = moonKeyword;
                                                moonRouteNoun.result = moonRoute;
                                                routeKeyword.compatibleNouns = routeKeyword.compatibleNouns.AddItem(moonRouteNoun).ToArray();

                                                TerminalNode moonInfo = ScriptableObject.CreateInstance<TerminalNode>();
                                                moonInfo.name = newMoon.MoonName.ToLower() + "Info";
                                                moonInfo.displayText = $"{newMoon.PlanetName}\r\n----------------------\r\n\r\n";
                                                if (!string.IsNullOrWhiteSpace(newMoon.PlanetLore))
                                                {
                                                    moonInfo.displayText += $"{newMoon.PlanetLore}\r\n";
                                                }
                                                else
                                                {
                                                    moonInfo.displayText += "No info about this moon can be found.\r\n";
                                                }
                                                moonInfo.clearPreviousText = true;
                                                moonInfo.maxCharactersToType = 35;

                                                CompatibleNoun moonInfoNoun = new CompatibleNoun();

                                                moonInfoNoun.noun = moonKeyword;
                                                moonInfoNoun.result = moonInfo;
                                                infoKeyword.compatibleNouns = infoKeyword.compatibleNouns.AddItem(moonInfoNoun).ToArray();

                                                StartOfRound.Instance.levels = StartOfRound.Instance.levels.AddItem(newLevel).ToArray();

                                                newMoons.Add(newLevel.levelID, newMoon);

                                                LethalExpansion.Log.LogInfo(newMoon.MoonName + " Moon added.");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LethalExpansion.Log.LogError(ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        LethalExpansion.Log.LogWarning(newMoon.MoonName + " Moon already added.");
                                    }
                                }
                            }
                        }
                    }
                }
                moonsPatched = true;
            }
        }
        private static void Hotfix_DoubleRoutes()
        {
            try
            {
                LethalExpansion.Log.LogDebug("Hotfix: Removing duplicated routes");
                HashSet<string> uniqueNames = new HashSet<string>();
                List<CompatibleNoun> uniqueNouns = new List<CompatibleNoun>();

                int duplicateCount = 0;

                foreach (CompatibleNoun noun in routeKeyword.compatibleNouns)
                {
                    if (!uniqueNames.Contains(noun.result.name) || noun.result.name == "Daily Moon" /* MoonOfTheDay 1.0.4 compatibility workaround*/)
                    {
                        uniqueNames.Add(noun.result.name);
                        uniqueNouns.Add(noun);
                    }
                    else
                    {
                        LethalExpansion.Log.LogDebug($"{noun.result.name} duplicated route removed.");
                        duplicateCount++;
                    }
                }

                routeKeyword.compatibleNouns = uniqueNouns.ToArray();

                LethalExpansion.Log.LogDebug($"Hotfix: {duplicateCount} duplicated route(s) removed");
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        private static void UpdateMoonsRoutePrices()
        {
            try
            {
                bool patchMoonsRoutePricesMultiplier = true;
                if (ConfigManager.Instance.FindItemValue<bool>("AdvancedCompanyCompatibility") && LethalExpansion.loadedPlugins.Any(p => p.Metadata.GUID == "AdvancedCompany"))
                {
                    patchMoonsRoutePricesMultiplier = false;
                }
                if (patchMoonsRoutePricesMultiplier)
                {
                    for (int i = 0; i < routeKeyword.compatibleNouns.Length; i++)
                    {
                        TerminalNode routeConfirmNode = routeKeyword.compatibleNouns[i].result.terminalOptions.First(t => t.noun.name == "Confirm").result;
                        int nounPrompt = routeKeyword.compatibleNouns[i].result.itemCost;
                        int nounPromptConfirm = routeConfirmNode.itemCost;

                        routeKeyword.compatibleNouns[i].result.itemCost = (int)(nounPrompt * ConfigManager.Instance.FindItemValue<float>("MoonsRoutePricesMultiplier"));
                        routeConfirmNode.itemCost = (int)(nounPromptConfirm * ConfigManager.Instance.FindItemValue<float>("MoonsRoutePricesMultiplier"));
                    }
                    LethalExpansion.Log.LogInfo("Moon route price updated.");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        private static void ResetMoonsRoutePrices()
        {
            try
            {
                if (defaultMoonRoutePrices != null)
                {
                    for (int i = 0; i < defaultMoonRoutePrices.Length; i++)
                    {
                        routeKeyword.compatibleNouns[i].result.itemCost = defaultMoonRoutePrices[i];
                        routeKeyword.compatibleNouns[i].result.terminalOptions.First(t => t.noun.name == "Confirm").result.itemCost = defaultMoonRoutePrices[i];
                    }
                    LethalExpansion.Log.LogInfo("Moon route price reset.");
                }
                else
                {
                    defaultMoonRoutePrices = new int[routeKeyword.compatibleNouns.Length];
                    for (int i = 0; i < routeKeyword.compatibleNouns.Length; i++)
                    {
                        defaultMoonRoutePrices[i] = routeKeyword.compatibleNouns[i].result.itemCost;
                    }
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        private static void ResetTerminalKeywords(Terminal __instance)
        {
            try
            {
                foreach(var keyword in __instance.terminalNodes.allKeywords)
                {
                    if (keyword.GetIsFromLE())
                    {
                        __instance.terminalNodes.allKeywords = __instance.terminalNodes.allKeywords.Remove(keyword);
                    }
                }
                LethalExpansion.Log.LogInfo("Terminal reset.");
            }
            catch(Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }

        private static void UpdateMoonsCatalogue(Terminal __instance)
        {
            try
            {
                string text = "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company building   //   Buying at [companyBuyingPercent].\r\n\r\n";

                foreach (SelectableLevel moon in __instance.moonsCatalogueList)
                {
                    bool isHidden = newMoons.ContainsKey(moon.levelID) && newMoons[moon.levelID].IsHidden;
                    text += ($"{(isHidden ? "[hidden]" : string.Empty)}* {moon.PlanetName}{(ConfigManager.Instance.FindItemValue<bool>("ShowMoonWeatherInCatalogue") ? $" [planetTime]" : string.Empty)}{(ConfigManager.Instance.FindItemValue<bool>("ShowMoonRankInCatalogue") ? $" ({moon.riskLevel})" : string.Empty)}{((ConfigManager.Instance.FindItemValue<bool>("ShowMoonPriceInCatalogue") && routeKeyword.compatibleNouns.Any(n => n.result.displayPlanetInfo == moon.levelID)) ? $" ({routeKeyword.compatibleNouns.First(n => n.result.displayPlanetInfo == moon.levelID).result.itemCost})" : string.Empty)}\r\n");
                }
                text += "\r\n";

                __instance.terminalNodes.allKeywords.First(node => node.name == "Moons").specialKeywordResult.displayText = text;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        [HarmonyPatch("TextPostProcess")]
        [HarmonyPostfix]
        private static void TextPostProcess_Postfix(Terminal __instance, ref string __result)
        {
            __result = TextHidder(__result);
        }
        private static string TextHidder(string inputText)
        {
            try
            {
                string pattern = @"^\[hidden\].*\r\n";
                string result = Regex.Replace(inputText, pattern, "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                return result;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
            return inputText;
        }
        public static void RemoveMoon(Terminal __instance, string moonName)
        {
            try
            {
                if (moonName == null)
                {
                    return;
                }
                CompatibleNoun[] nouns = routeKeyword.compatibleNouns;
                if (!__instance.moonsCatalogueList.Any(level => level.name.Contains(moonName)) && !nouns.Any(level => level.noun.name.Contains(moonName)))
                {
                    LethalExpansion.Log.LogInfo($"{moonName} moon not exist.");
                    return;
                }
                for (int i = 0; i < __instance.moonsCatalogueList.Length; i++)
                {
                    if (__instance.moonsCatalogueList[i].name.Contains(moonName))
                    {
                        __instance.moonsCatalogueList = ModUtils.RemoveElementFromArray(__instance.moonsCatalogueList, i);
                    }
                }
                for (int i = 0; i < nouns.Length; i++)
                {
                    if (nouns[i].noun.name.Contains(moonName))
                    {
                        routeKeyword.compatibleNouns = ModUtils.RemoveElementFromArray(nouns, i);
                    }
                }
                if (!__instance.moonsCatalogueList.Any(level => level.name.Contains(moonName)) &&
                    !routeKeyword.compatibleNouns.Any(level => level.noun.name.Contains(moonName)))
                {
                    LethalExpansion.Log.LogInfo($"{moonName} moon removed.");
                }
                else
                {
                    LethalExpansion.Log.LogInfo($"{moonName} moon failed to remove.");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        private static void SaveFireExitAmounts()
        {
            try
            {
                if (!flowFireExitSaved)
                {
                    foreach (var flow in RoundManager.Instance.dungeonFlowTypes.Select(t=>t.dungeonFlow))
                    {
                        flow.dungeonFlow.SetDefaultFireExitAmount(flow.dungeonFlow.GlobalProps.First(p => p.ID == 1231).Count.Min);
                    }
                    flowFireExitSaved = true;
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        public static void ResetFireExitAmounts()
        {
            try
            {
                if (flowFireExitSaved)
                {
                    foreach (var flow in RoundManager.Instance.dungeonFlowTypes.Select(t=>t.dungeonFlow))
                    {
                        flow.dungeonFlow.GlobalProps.First(p => p.ID == 1231).Count = new DunGen.IntRange(flow.dungeonFlow.GetDefaultFireExitAmount(), flow.dungeonFlow.GetDefaultFireExitAmount());
                    }
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
    }
}
