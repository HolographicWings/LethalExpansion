using HarmonyLib;
using LethalExpansion.Utils;
using LethalSDK.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class Terminal_Patch
    {
        private static int[] defaultMoonRoutePrices;
        public static bool scrapsPatched = false;
        public static bool moonsPatched = false;

        public static Dictionary<int, Moon> newMoons = new Dictionary<int, Moon>();

        public static void MainPatch(Terminal __instance)
        {
            scrapsPatched = false;
            moonsPatched = false;
            //RemoveMoon(__instance, "Experimentation");
            Hotfix_DoubleRoutes(__instance);
            //TestBlankScene(__instance);
            GatherAssets(__instance);
            AddScraps(__instance);
            AddMoons(__instance);
            UpdateMoonsCatalogue(__instance);
            ResetMoonsRoutePrices(__instance);
            UpdateMoonsRoutePrices(__instance);
            LethalExpansion.Log.LogInfo("Terminal Main Patch.");
        }
        private static void GatherAssets(Terminal __instance)
        {
            foreach(Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                AssetGather.Instance.AddAudioClip(item.grabSFX);
                AssetGather.Instance.AddAudioClip(item.dropSFX);
                AssetGather.Instance.AddAudioClip(item.pocketSFX);
                AssetGather.Instance.AddAudioClip(item.throwSFX);
            }
            foreach(SelectableLevel level in StartOfRound.Instance.levels)
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
        }
        public static void AddScraps(Terminal __instance)
        {
            if (ConfigManager.Instance.FindItemValue<bool>("LoadModules") && !scrapsPatched)
            {
                AudioClip defaultDropSound = AssetGather.Instance.audioClips["DropCan"];
                AudioClip defaultGrabSound = AssetGather.Instance.audioClips["ShovelPickUp"];
                foreach (KeyValuePair<String, (AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                {
                    (AssetBundle, ModManifest) _tmp = AssetBundlesManager.Instance.Load(bundle.Key);

                    foreach (var newScrap in _tmp.Item2.scraps)
                    {
                        if (newScrap == null)
                        {
                            return;
                        }

                        Item tmpItem = newScrap.prefab.GetComponent<PhysicsProp>().itemProperties;

                        AudioSource audioSource = newScrap.prefab.GetComponent<AudioSource>();
                        audioSource.outputAudioMixerGroup = AssetGather.Instance.audioMixers["Diagetic"].Item2.First(a => a.name == "Master");

                        AudioClip _tpmGrabSFX = null;
                        if (newScrap.grabSFX.Length > 0)
                        {
                            if (bundle.Value.Item2.assetBank != null)
                            {
                                if (bundle.Value.Item2.assetBank.HaveAudioClip(newScrap.grabSFX))
                                {
                                    _tpmGrabSFX = _tmp.Item1.LoadAsset<AudioClip>(bundle.Value.Item2.assetBank.AudioClipPath(newScrap.grabSFX));
                                }
                                else
                                {
                                    _tpmGrabSFX = AssetGather.Instance.audioClips[newScrap.grabSFX];
                                }
                            }
                            else
                            {
                                _tpmGrabSFX = AssetGather.Instance.audioClips[newScrap.grabSFX];
                            }
                        }
                        tmpItem.grabSFX = _tpmGrabSFX != null ? _tpmGrabSFX : defaultGrabSound;
                        AudioClip _tpmDropSFX = null;
                        if (newScrap.dropSFX.Length > 0)
                        {
                            if(bundle.Value.Item2.assetBank != null)
                            {
                                if (bundle.Value.Item2.assetBank.HaveAudioClip(newScrap.dropSFX))
                                {
                                    _tpmDropSFX = _tmp.Item1.LoadAsset<AudioClip>(bundle.Value.Item2.assetBank.AudioClipPath(newScrap.dropSFX));
                                }
                                else
                                {
                                    _tpmDropSFX = AssetGather.Instance.audioClips[newScrap.dropSFX];
                                }
                            }
                            else
                            {
                                _tpmDropSFX = AssetGather.Instance.audioClips[newScrap.dropSFX];
                            }
                        }
                        tmpItem.dropSFX = _tpmDropSFX != null ? _tpmDropSFX : defaultDropSound;

                        StartOfRound.Instance.allItemsList.itemsList.Add(tmpItem);


                        if (newScrap.useGlobalSpawnRate)
                        {
                            SpawnableItemWithRarity itemRarity = new SpawnableItemWithRarity();
                            itemRarity.spawnableItem = tmpItem;
                            itemRarity.rarity = newScrap.globalSpawnWeight;
                            foreach (SelectableLevel level in __instance.moonsCatalogueList)
                            {
                                level.spawnableScrap.Add(itemRarity);
                            }
                        }
                        else
                        {
                            var ddqdz = newScrap.perPlanetSpawnWeight();
                            foreach (SelectableLevel level in __instance.moonsCatalogueList)
                            {
                                try
                                {
                                    if (ddqdz.Any(l => l.SceneName == level.PlanetName))
                                    {
                                        var tmp = ddqdz.First(l => l.SceneName == level.PlanetName);

                                        SpawnableItemWithRarity itemRarity = new SpawnableItemWithRarity();
                                        itemRarity.spawnableItem = tmpItem;
                                        itemRarity.rarity = tmp.SpawnWeight;

                                        level.spawnableScrap.Add(itemRarity);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LethalExpansion.Log.LogError(ex.Message);
                                }
                            }
                        }
                        LethalExpansion.Log.LogInfo(newScrap.itemName + " Scrap added.");
                    }
                }
                scrapsPatched = true;
            }
        }
        public static void AddMoons(Terminal __instance)
        {
            if (ConfigManager.Instance.FindItemValue<bool>("LoadModules") && !moonsPatched)
            {
                foreach (KeyValuePair<String, (AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                {
                    (AssetBundle, ModManifest) _tmp = AssetBundlesManager.Instance.Load(bundle.Key);

                    foreach (Moon newMoon in _tmp.Item2.moons)
                    {
                        if (newMoon == null)
                        {
                            return;
                        }

                        TerminalKeyword confirmKeyword = __instance.terminalNodes.allKeywords.First(k => k.word == "confirm");
                        TerminalKeyword denyKeyword = __instance.terminalNodes.allKeywords.First(k => k.word == "deny");
                        TerminalKeyword routeKeyword = __instance.terminalNodes.allKeywords.First(k => k.word == "route");
                        TerminalNode cancelRouteNode = null;
                        foreach (var noun in routeKeyword.compatibleNouns)
                        {
                            foreach (var option in noun.result.terminalOptions)
                            {
                                if (option.result.name == "CancelRoute")
                                {
                                    cancelRouteNode = option.result;
                                    break;
                                }
                            }
                            break;
                        }

                        SelectableLevel newLevel = (SelectableLevel)ScriptableObject.CreateInstance(typeof(SelectableLevel));

                        newLevel.name = newMoon.PlanetName;
                        newLevel.PlanetName = newMoon.PlanetName;
                        newLevel.sceneName = "InitSceneLaunchOptions";
                        newLevel.levelID = StartOfRound.Instance.levels.Length;
                        newLevel.planetPrefab = AssetGather.Instance.planetPrefabs[newMoon.OrbitPrefabName];
                        newLevel.lockedForDemo = true;
                        newLevel.spawnEnemiesAndScrap = newMoon.SpawnEnemiesAndScrap;
                        newLevel.LevelDescription = newMoon.PlanetDescription;
                        newLevel.videoReel = newMoon.PlanetVideo;
                        newLevel.riskLevel = newMoon.RiskLevel;
                        newLevel.timeToArrive = newMoon.TimeToArrive;
                        newLevel.DaySpeedMultiplier = newMoon.DaySpeedMultiplier;
                        newLevel.planetHasTime = newMoon.PlanetHasTime;
                        newLevel.factorySizeMultiplier = 1f;
                        newLevel.dungeonFlowTypes = new IntWithRarity[]
                        {
                            new IntWithRarity(){id = 0, rarity = 300}
                        };

                        __instance.moonsCatalogueList = __instance.moonsCatalogueList.AddItem(newLevel).ToArray();

                        TerminalKeyword moonKeyword = (TerminalKeyword)ScriptableObject.CreateInstance(typeof(TerminalKeyword));
                        moonKeyword.word = newMoon.RouteWord != null || newMoon.RouteWord.Length >= 3 ? newMoon.RouteWord.ToLower() : Regex.Replace(newMoon.MoonName, @"\s", "").ToLower();
                        moonKeyword.defaultVerb = routeKeyword;
                        __instance.terminalNodes.allKeywords = __instance.terminalNodes.allKeywords.AddItem(moonKeyword).ToArray();

                        TerminalNode moonRouteConfirm = (TerminalNode)ScriptableObject.CreateInstance(typeof(TerminalNode));
                        moonRouteConfirm.displayText = $"Routing autopilot to {newMoon.PlanetName}.\r\nYour new balance is [playerCredits].\r\n\r\n{newMoon.BoughtComment}\r\n\r\n";
                        moonRouteConfirm.clearPreviousText = true;
                        moonRouteConfirm.buyRerouteToMoon = StartOfRound.Instance.levels.Length;
                        moonRouteConfirm.lockedInDemo = true;
                        moonRouteConfirm.itemCost = newMoon.RoutePrice;

                        TerminalNode moonRoute = (TerminalNode)ScriptableObject.CreateInstance(typeof(TerminalNode));
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

                        CompatibleNoun moonNoun = new CompatibleNoun();
                        moonNoun.noun = moonKeyword;
                        moonNoun.result = moonRoute;
                        routeKeyword.compatibleNouns = routeKeyword.compatibleNouns.AddItem(moonNoun).ToArray();

                        StartOfRound.Instance.levels = StartOfRound.Instance.levels.AddItem(newLevel).ToArray();

                        LethalExpansion.Log.LogInfo(newLevel.levelID);
                        newMoons.Add(newLevel.levelID, newMoon);

                        LethalExpansion.Log.LogInfo(newMoon.MoonName + " Moon added.");
                    }
                }
                moonsPatched = true;
            }
        }
        private static void Hotfix_DoubleRoutes(Terminal __instance)
        {
            LethalExpansion.Log.LogDebug("Hotfix: Removing duplicated routes");
            HashSet<string> uniqueNames = new HashSet<string>();
            List<CompatibleNoun> uniqueNouns = new List<CompatibleNoun>();

            int duplicateCount = 0;

            foreach (CompatibleNoun noun in __instance.terminalNodes.allKeywords.First(node => node.name == "Route").compatibleNouns)
            {
                if (!uniqueNames.Contains(noun.result.name))
                {
                    uniqueNames.Add(noun.result.name);
                    uniqueNouns.Add(noun);
                }
                else
                {
                    duplicateCount++;
                }
            }

            __instance.terminalNodes.allKeywords.First(node => node.name == "Route").compatibleNouns = uniqueNouns.ToArray();

            LethalExpansion.Log.LogDebug("Hotfix: " + duplicateCount + " duplicated route(s) removed");
        }
        private static void TestBlankScene(Terminal __instance)
        {
            foreach (var test in __instance.moonsCatalogueList)
            {
                test.sceneName = "InitSceneLaunchOptions";
            }
        }
        private static void UpdateMoonsRoutePrices(Terminal __instance)
        {
            TerminalKeyword routeKeyword = __instance.terminalNodes.allKeywords.First(node => node.name == "Route");
            defaultMoonRoutePrices = new int[routeKeyword.compatibleNouns.Length];
            for (int i = 0; i < routeKeyword.compatibleNouns.Length; i++)
            {
                TerminalNode routeConfirmNode = routeKeyword.compatibleNouns[i].result.terminalOptions.First(t => t.noun.name == "Confirm").result;
                defaultMoonRoutePrices[i] = routeKeyword.compatibleNouns[i].result.itemCost;
                int nounPrompt = routeKeyword.compatibleNouns[i].result.itemCost;
                int nounPromptConfirm = routeConfirmNode.itemCost;
                routeKeyword.compatibleNouns[i].result.itemCost = (int)(nounPrompt * ConfigManager.Instance.FindItemValue<float>("MoonsRoutePricesMultiplier"));
                routeConfirmNode.itemCost = (int)(nounPromptConfirm * ConfigManager.Instance.FindItemValue<float>("MoonsRoutePricesMultiplier"));
            }
            LethalExpansion.Log.LogInfo("Moon route price updated.");
        }
        private static void ResetMoonsRoutePrices(Terminal __instance)
        {
            if(defaultMoonRoutePrices != null)
            {
                var routeKeyword = __instance.terminalNodes.allKeywords.First(node => node.name == "Route");
                for (int i = 0; i < defaultMoonRoutePrices.Length; i++)
                {
                    routeKeyword.compatibleNouns[i].result.itemCost = defaultMoonRoutePrices[i];
                    routeKeyword.compatibleNouns[i].result.terminalOptions.First(t => t.noun.name == "Confirm").result.itemCost = defaultMoonRoutePrices[i];
                }
                LethalExpansion.Log.LogInfo("Moon route price reset.");
            }
        }

        private static void UpdateMoonsCatalogue(Terminal __instance)
        {
            string text = "Welcome to the exomoons catalogue.\r\nTo route the autopilot to a moon, use the word ROUTE.\r\nTo learn about any moon, use the word INFO.\r\n____________________________\r\n\r\n* The Company building   //   Buying at [companyBuyingPercent].\r\n\r\n";

            foreach (SelectableLevel moon in __instance.moonsCatalogueList)
            {
                text += ("* " + moon.PlanetName + " [planetTime] (" + moon.riskLevel + ")\r\n");
            }

            text += "\r\n";

            __instance.terminalNodes.allKeywords.First(node => node.name == "Moons").specialKeywordResult.displayText = text;
        }
        public static void RemoveMoon(Terminal __instance, string moonName)
        {
            if (moonName == null)
            {
                return;
            }
            CompatibleNoun[] nouns = __instance.terminalNodes.allKeywords.First(node => node.name == "Route").compatibleNouns;
            if (!__instance.moonsCatalogueList.Any(level => level.name.Contains(moonName)) && !nouns.Any(level => level.noun.name.Contains(moonName)))
            {
                LethalExpansion.Log.LogInfo(moonName + " moon not exist.");
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
                    __instance.terminalNodes.allKeywords.First(node => node.name == "Route").compatibleNouns = ModUtils.RemoveElementFromArray(nouns, i);
                }
            }
            if (!__instance.moonsCatalogueList.Any(level => level.name.Contains(moonName)) &&
                !__instance.terminalNodes.allKeywords.First(node => node.name == "Route").compatibleNouns.Any(level => level.noun.name.Contains(moonName)))
            {
                LethalExpansion.Log.LogInfo(moonName + " moon removed.");
            }
            else
            {
                LethalExpansion.Log.LogInfo(moonName + " moon failed to remove.");
            }
        }
    }
}
