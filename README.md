# LethalExpansion

 This mod is an expansion project to add several settings and features, it come with an SDK to add new Scraps and Moons yourself.  
 [Thunderstore.io link](https://thunderstore.io/c/lethal-company/p/HolographicWings/LethalExpansion/)

 Features :
 - A mod menu with 21 settings to customize your game.
 - A support to load modules made with [my SDK](https://github.com/HolographicWings/LethalSDK-Unity-Project) to add new Scraps and Moons. (More soon)
 - A new Moon Catalogue ready for modded moons.
 - A Landmine extension to finally allow players to drop heavy items on them to avoid explosion, based on the weight of the items. (Disabled by default)
 - An optional automatic deadline system that increase the lenght of the expedition as much as the quota increase. (Disabled by default)
 - Added a space light to see the orbited planet though the ship camera.
 - A network system to synchronize your settings with the host.
 - Compatible with vanilla players. (Follow the "Vanilla Compatibility" section)
 - Settings to display the clock in 24H format and carried weight in KG.

[![Downloads badge](https://img.shields.io/github/downloads/HolographicWings/LethalExpansion/total.svg?style=for-the-badge)](https://github.com/HolographicWings/LethalExpansion/releases)

If you have any issue, please read the "Report a bug" section

![Mod Menu](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/ModSettings.png "Mod Menu")
Settings list :
- GlobalTimeSpeedMultiplier: Change the global time speed.
- LengthOfHours: Change amount of seconds in one hour.
- NumberOfHours: Max lenght of an Expedition in hours. (Begin at 6 AM | 18 = Midnight)
- DeadlineDaysAmount: Change amount of days for the Quota.
- StartingCredits: Change amount of starting Credit.
- MoonsRoutePricesMultiplier: Change the Cost of the Moon Routes.
- StartingQuota: Change the starting Quota.
- ScrapAmountMultiplier: Change the amount of Scraps in dungeons.
- ScrapValueMultiplier: Change the value of Scraps.
- MapSizeMultiplier: Change the size of the Dungeons. (Can crash when under 1.0)
- PreventMineToExplodeWithItems: Prevent Landmines to explode by dropping items on them.
- MineActivationWeight: Set the minimal weight to prevent Landmine's explosion (0.15 = 16 lb, Player = 2.0)
- WeightUnit: Change the carried Weight unit : 0 = Pounds (lb), 1 = Kilograms (kg) and 2 = Both
- ConvertPoundsToKilograms: Convert Pounds into Kilograms (16 lb = 7 kg) (Only effective if WeightUnit = 1)
- PreventScrapWipeWhenAllPlayersDie: Prevent the Scraps Wipe when all players die.
- 24HoursClock: Display a 24h clock instead of 12h.
- ClockAlwaysVisible: Display clock while inside of the Ship.
- AutomaticDeadline: Automatically increase the Deadline depending of the required quota.
- AutomaticDeadlineStage: Increase the quota deadline of one day each time the quota exceeds this value.
- LoadModules: Load SDK Modules that add new content to the game. Disable it to play with Vanilla players. (RESTART REQUIRED)
- MaxItemsInShip: Change the Items cap can be kept in the ship.

## Vanilla Compatibility :
To make the mod compatible with vanilla players, keep default every setting that have "Mod required by client: Yes" in their description. (Keep mouse on a setting to see it's description)
![Mod Menu 2](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/ModSettings2.png "Mod Menu 2")

## Report a bug :
I will maybe open a Discord for support if the mod gets a lot of users, meanwhile you can use the [github's issue tab](https://github.com/HolographicWings/LethalSDK-Unity-Project/issues).
Please enable the fellowing settings in the "BepInEx.cfg" setting file from the "Lethal Company\BepInEx\config\" folder:  
  
[Logging]  
UnityLogListening = true  
[Logging.Console]  
Enabled = true  
[Logging.Disk]  
WriteUnityLog = true  
  
Then send the "LogOutput.log" file from the "Lethal Company\BepInEx\" folder in the bug report.  

## Known issues :
- hoarding big killed when grabbed an item from a mine don't drop the item that make it irrecoverable
- when deadline remaining days is over 3 days, outside monsters spawn more
- Landmines stills bip if a player walk on them when theres already an item placed on it
- A recent change broke the entire new Landmines system.

## Changes :
- 1.1.7 :
	- Reworked the Modules loader to make it compatible with LC_API and R2Modman.
	- New file extension for Modules.
- 1.1.6 :
	- Renamed the Old Sea Port moon into Wateridge
	- Added Orbit prefab and description for Wateridge
	- Edited Wateridge scraps and monsters.
	- Fixed network desync with global time speed.
	- Added auto scroll in ship main monitor to see read text.
	- Fixed an issue that prevent the new moons to load after returned from lobby to main menu.
- 1.1.5 :
	- Support for 1.1.5 version of SDK.
	- Minor fixes.
- 1.1.4 :
	- Game Version 45 Ready.
	- Added some error catches for SDK.
- 1.1.3 :
	- Fixed bundle loading issue.
- 1.1.2 :
	- Fixed new landmine system wasn't working.
	- Fixed orbited moons stopping to show after leaving the Company Building.
- 1.1.1 :
	- Fixed critical issue that prevent the mod to load.
- 1.1.0 :
	- Support for 1.1.0 version of SDK (Full custom moons support)
	- Removed useless assets and compressed the main skybox
- 1.0.1 : Removed useless patches

## Planned features :
- New landmine system network rework
- Update checked
- Mod version compatibility checker

## More screenshots :
![Scraps2](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/Scraps2.png "Scraps2")
![Scraps1](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/Scraps1.png "Scraps1")
![MoonCatalogue](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/MoonCatalogue.png "MoonCatalogue")
![NewMoon1](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/NewMoon1.png "NewMoon1")
![NewMoon2](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/NewMoon2.png "NewMoon2")
