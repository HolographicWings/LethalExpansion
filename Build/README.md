# LethalExpansion

 This mod is an expansion project to add several settings and features, it come with an SDK to add new Scraps and Moons yourself.  
 [Github repository](https://github.com/HolographicWings/LethalExpansion)

 Features :
 - A mod menu with 28 settings to customize your game.
 - A support to load modules made with [my SDK](https://thunderstore.io/c/lethal-company/p/HolographicWings/LethalSDK/) to add new Scraps and Moons. (More soon)
 - A new Moon Catalogue ready for modded moons.
 - A Landmine extension to finally allow players to drop heavy items on them to avoid explosion, based on the weight of the items. (Disabled by default)
 - An optional automatic deadline system that increase the lenght of the expedition as much as the quota increase. (Disabled by default)
 - Added a space light to see the orbited planet though the ship camera.
 - A network system to synchronize your settings with the host.
 - Compatible with vanilla players. (Follow the "Vanilla Compatibility" section)
 - Settings to display the clock in 24H format and carried weight in KG.

# Looking for a successor !

### Looking for a successor to keep LethalExpansion and LethalSDK alife.

Hello, making this mod was an awesome adventure, but today i want to work on other projects and explore some game concept ideas to see if i can make my own game.
But i want deprecating this mod to be the last resort.
It's why i'm looking for someone interesting into continuing this mod and make it in his name.
If you are ever interested, invite me on discord with my username HolographicWings or write an issue either on LE or LSDK repositories.

Thanks you for supporting me during this project, i hope the LC modding will become even greater.

## Download and description :

If you have any issue, please read the "Report a bug" section

![Mod Menu](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/ModSettings.webp "Mod Menu")
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
- ShowMoonWeatherInCatalogue: Display the current weather of Moons in the Terminal's Moon Catalogue.
- ShowMoonRankInCatalogue: Display the rank of Moons in the Terminal's Moon Catalogue.
- ShowMoonPriceInCatalogue: Display the route price of Moons in the Terminal's Moon Catalogue.
- QuotaIncreaseSteepness: Change the Quota Increase Steepness. (Highter = less steep exponential increase)
- QuotaBaseIncrease: Change the Quota Base Increase.
- KickPlayerWithoutMod: Kick the players without Lethal Expansion installer. (Will be kicked anyway if LoadModules is True)
- BrutalCompanyPlusCompatibility: Leave Brutal Company Plus control the Quota settings.

## Vanilla Compatibility :
To make the mod compatible with vanilla players, keep default every setting that have "Mod required by client: Yes" in their description. (Keep mouse on a setting to see it's description)
![Mod Menu 2](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/ModSettings2.webp "Mod Menu 2")

## Recommended Mods :
[LandmineFix](https://thunderstore.io/c/lethal-company/p/TheBeeTeam/LandmineFix/) by TheBeeTeam is recommended in order to use the PreventMineToExplodeWithItems setting.

## Report a bug :
I will maybe open a Discord for support if the mod gets a lot of users, meanwhile you can use the [github's issue tab](https://github.com/HolographicWings/LethalExpansion/issues).
Please enable the fellowing settings in the "BepInEx.cfg" setting file from the "Lethal Company\BepInEx\config\" folder:  
  
[Logging]  
UnityLogListening = true  
[Logging.Console]  
Enabled = true  
[Logging.Disk]  
WriteUnityLog = true  
  
Then send the "LogOutput.log" file from the "Lethal Company\BepInEx\" folder in the bug report.  

## Known issues :
- Hoarding bug killed when grabbed an item from a mine don't drop the item that make it irrecoverable.
- When deadline remaining days is over 3 days, outside monsters spawns more.
- Landmines stills bip if a player walk on them when theres already an item placed on it.
- Landmine instand explode when a player walk on them (Vanilla bug) use [LandmineFix](https://thunderstore.io/c/lethal-company/p/TheBeeTeam/LandmineFix/) by TheBeeTeam.

## Changes :
- 1.3.30
    - Whitelisted a new terrain helper for the SDK.
- 1.3.29
    - Tiny hotfix for retro-compatibility with moons made with post-1.3.28 SDK.
- 1.3.28
    - Finally fixed the Coil-Head that don't attack clients.
- 1.3.27
    - Possibility to make a custom animation on the moon orbit prefabs.
    - Test to fix the Coil head who don't attack non-host players without changing the entrances.
- 1.3.26
    - Added a Hide ModSettings Menu setting and press O to open the ModSettings menu in main menu.
    - Tiny tweak in Terrains.
    - Added a compatibility patch for ExtraDaysToDeadline by Ustaalon.
    - Tweaks in the setting sync for compatibility with other mods.
- 1.3.25
    - Fixed README link to default modules.
- 1.3.24
    - Allow custom scripts from a DLL.
	- Moved Wateridge and default scraps to an [independent mod](https://thunderstore.io/c/lethal-company/p/HolographicWings/LEDefaultModules/).
- 1.3.23
    - Fixed WhoopieCushion.
- 1.3.22
	- Fixed issue where Noisemakers wouldn't play for other players.
	- Added a lot more support for contributing.
- 1.3.21
	- Fixed a missing field in custom Noisemakers scraps.
- 1.3.20
	- Added DamagePlayer and AudioOutputInterface to scrap components whitelist.
- 1.3.19
	- Added a blacklist field for custom scraps and moons. (Need testing)
	- Added a security against embed modules overwriting.
	- Fixed custom Whoopie Cushions.
- 1.3.18
	- Finally fixed shovels.
- 1.3.17
	- Added several experimental animations support for scraps made with SDK (may fix two handed animations).
	- Fixed some issues with new Scrap types.
	- Checked compatibility with v47.
	- Added a locker to don't go on the Challenge moon with LethalExpansion which can be cheaty (Please do this gamemode on Vanilla to respect the Leaderboard).
- 1.3.16
	- Reversed networking change with Entrances that causing issues.
- 1.3.15
	- New experimental method to ensure the proper loading of the custom moons.
	- Small networking fix with Entrances.
	- Used the experimental recursive scrap patch to apply the template scraps to every moons (testing goal).
	- Added very experimental Shovel support with SDK.
	- Added very experimental Flashlight support with SDK.
	- Added very experimental Noisemaker support with SDK.
	- Added very experimental WhoopieCushion support with SDK.
- 1.3.14
	- Changed Boombox Controller compatibility note from critical to good.
	- Added an experimental recursive scrap patch to add custom scraps on custom moons of other modules without dependency.
- 1.3.13
	- Fixed tiny issue with compatibility patches.
- 1.3.12
	- Added a compatibility patch for MoreMoneyStart.
	- Removed Christmas village and moved it to an [independent mod](https://thunderstore.io/c/lethal-company/p/HolographicWings/ChristmasVillage_Legacy).
	- Added a setting to roll back to Synchronous custom moon loading.
- 1.3.11
	- Fixed multiple firexists wasn't working since the moon loading became asynchronous.
- 1.3.10
	- Added incompatibility note about Boombox Controller Mod.
	- Reduced spawn luck of Christmas Star in Christmas Village.
- 1.3.9
	- Attempt to fix a crash that happen on certain setups when loading a custom moon.
	- Increased AutomaticDeadlineStage cap to 3000 (from 1000)
	- Slighty improved Compatibility logging.
- 1.3.8
	- Fixed a critical bug with SDK's character validator.
- 1.3.7
	- Added Coomfy Dungeon Compatibility.
- 1.3.6
	- Added Lethal Adjustments Compatibility.
- 1.3.5
	- Fixed missing saplin star in Christmas Village. (This is extremely important right ? :P Thanks to MegaPiggy to noticed it was missing)
- 1.3.4
	- Tiny improvements and additions to Christmas Village.
- 1.3.3
	- Fixed Item Drop Ship dummy don't dispawn.
	- Fixed mistake that concider the dungeons as outside.
- 1.3.2
	- Fixed new moon's Item Drop Ship doesn't unloads and keep it's position on the next moons.
- 1.3.1
	- Added Entrance ScanNode to Christmas village.
- 1.3.0
	- Fixed Item Drop Ship networking of new moons.
	- Added Christmas Village Moon.
	- Fixed clock doesn't showing when leaving dungeon.
	- Fixed water sound don't stopping when entering dungeon.
	- Fixed terrain crash with a lot of GPU Instanced details.
	- Fixed terrain holes support.
	- Improved water surface support.
	- Fixed issue with host/client validation when no module are loaded.
	- Fixed material issue with Eclipsed weather of new moons.
	- Fixed error when looking at an opened Item Dropship.
	- Fixed description error in QuotaSteepness setting.
	- Added experimental Ladder support for new moons.
<details>
  <summary>Old updates:</summary>
	<ul>
	    <li>1.2.16
	        <ul>
	            <li>Compatibility patch for MoonOfTheDay mod.</li>
	        </ul>
	    </li>
	    <li>1.2.15
	        <ul>
	            <li>Removed a debugging test I forgot that made the seed always same. (thanks to @MaxWasUnavailable to noticed it)</li>
	        </ul>
	    </li>
	    <li>1.2.14
	        <ul>
	            <li>Attempt to fix generation desync once and for all! (Thanks to Olskor to helped me with this issue)</li>
	        </ul>
	    </li>
	    <li>1.2.13
	        <ul>
	            <li>Attempt to fix issue preventing to join someone already orbiting a modded moon.</li>
	            <li>Temporarily disabled the Version checker popup that was appearing sometimes outside of the Main Menu.</li>
	        </ul>
	    </li>
	    <li>1.2.12
	        <ul>
	            <li>Attempt to fix Weather desync.</li>
	        </ul>
	    </li>
	    <li>1.2.11
	        <ul>
	            <li>Fixed inside monsters spawning outside in custom moons.</li>
	        </ul>
	    </li>
	    <li>1.2.10
	        <ul>
	            <li>Fixed broken quota settings from 1.2.7.</li>
	        </ul>
	    </li>
	    <li>1.2.9
	        <ul>
	            <li>Reworked the assetbundles loading (again).</li>
	            <li>Fixed audio file registered with another name don't register properly.</li>
	        </ul>
	    </li>
	    <li>1.2.8
	        <ul>
	            <li>Ajusted default spawn weight for new scraps.</li>
	            <li>Ajusted version checker.</li>
	        </ul>
	    </li>
	    <li>1.2.7
	        <ul>
	            <li>Overall micro optimizations of assets made with SDK.</li>
	            <li>Wateridge optimization first pass.</li>
	            <li>Added Brutal Company Plus Compatibility.</li>
	            <li>Several improvement in scrap and moon loaders.</li>
	            <li>Security against template module overwrite.</li>
	            <li>Added a timeout before kick clients who don't answer to network sync packets (Not working).</li>
	            <li>Improved Configurable Popups focus.</li>
	            <li>Fixed external scan nodes.</li>
	            <li>Fixed custom audio files importation issues.</li>
	        </ul>
	    </li>
	    <li>1.2.6
	        <ul>
	            <li>More retrocompatibility with outdated modules.</li>
	            <li>Fixing some exceptions when missing ScanNode on new scraps.</li>
	        </ul>
	    </li>
	    <li>1.2.5
	        <ul>
	            <li>Better sound loader for new scraps and SDK asset banks.</li>
	        </ul>
	    </li>
	    <li>1.2.4
	        <ul>
	            <li>Added two settings to configure the quota increment.</li>
	            <li>Finished the Workaround for moons made with old versions of the SDK.</li>
	        </ul>
	    </li>
	    <li>1.2.3
	        <ul>
	            <li>Added a Workaround to keep minimal compatibility with moons made with old versions of the SDK and avoid crashing.</li>
	        </ul>
	    </li>
	    <li>1.2.2
	        <ul>
	            <li>Forgot to change the version number, occurring to always tell the mod is outdated.</li>
	            <li>Added a second fire exit to Wateridge to test the Fire Exit Amount implementation of the SDK.</li>
	        </ul>
	    </li>
	    <li>1.2.1
	        <ul>
	            <li>Removed a debug message.</li>
	            <li>Nerfed Wateridge (less scraps, enemies spawn sooner).</li>
	        </ul>
	    </li>
	    <li>1.2.0
	        <ul>
	            <li>Network sync Rework (should fix the map generation desync).</li>
	            <li>Added a workaround when playing with HDLethalCompany and using new moons with a missing Volume Profile.</li>
	            <li>Added support for more Fire Exits in maps done with SDK.</li>
	            <li>Added Item Drop Ship support for moons done with SDK.</li>
	            <li>Fixed an issue that could break the new moons loading when missing modules.</li>
	            <li>Increased the mods setting menu size to be able to read the last settings' description.</li>
	            <li>Added settings to show or hide the Moons Current Weather, Dangeer Rank and Route Price.</li>
	            <li>Removed the Labyrinth added the Company Building, it was a test.</li>
	            <li>Added mod version to the Main Menu (compatible with MoreCompany).</li>
	            <li>Added a Configurable Popup hud for Contextual Notifications.</li>
	            <li>Added a Version Checker.</li>
	            <li>Fixed Moon Route prices getting wrong after returned to Main Menu then joined a new Lobby.</li>
	        </ul>
	    </li>
	    <li>1.1.9 :
	        <ul>
	            <li>Fixed terrain shader making the game crash when loading a moon with a terrain.</li>
	        </ul>
	    </li>
	    <li>1.1.8 :
	        <ul>
	            <li>Fixed urgent bug with the new Modules loader.</li>
	        </ul>
	    </li>
	    <li>1.1.7 :
	        <ul>
	            <li>Reworked the Modules loader to make it compatible with LC_API and R2Modman.</li>
	            <li>New file extension for Modules.</li>
	        </ul>
	    </li>
	    <li>1.1.6 :
	        <ul>
	            <li>Renamed the Old Sea Port moon into Wateridge.</li>
	            <li>Added Orbit prefab and description for Wateridge.</li>
	            <li>Edited Wateridge scraps and monsters.</li>
	            <li>Fixed network desync with global time speed.</li>
	            <li>Added auto scroll in ship main monitor to see read text.</li>
	            <li>Fixed an issue that prevent the new moons to load after returned from lobby to main menu.</li>
	        </ul>
	    </li>
	    <li>1.1.5 :
	        <ul>
	            <li>Support for 1.1.5 version of SDK.</li>
	            <li>Minor fixes.</li>
	        </ul>
	    </li>
	    <li>1.1.4 :
	        <ul>
	            <li>Game Version 45 Ready.</li>
	            <li>Added some error catches for SDK.</li>
	        </ul>
	    </li>
	    <li>1.1.3 :
	        <ul>
	            <li>Fixed bundle loading issue.</li>
	        </ul>
	    </li>
	    <li>1.1.2 :
	        <ul>
	            <li>Fixed new landmine system wasn't working.</li>
	            <li>Fixed orbited moons stopping to show after leaving the Company Building.</li>
	        </ul>
	    </li>
	    <li>1.1.1 :
	        <ul>
	            <li>Fixed critical issue that prevent the mod to load.</li>
	        </ul>
	    </li>
	    <li>1.1.0 :
	        <ul>
	            <li>Support for 1.1.0 version of SDK (Full custom moons support).</li>
	            <li>Removed useless assets and compressed the main skybox.</li>
	        </ul>
	    </li>
	    <li>1.0.1 : Removed useless patches.</li>
	</ul>
</details>

## Planned features :
- New landmine system network rework
- Mod version compatibility checker

## Credits :
	- Template Scraps :
		- https://assetstore.unity.com/packages/3d/props/tools/survival-game-tools-139872
	- Wateridge Moon :
		- https://assetstore.unity.com/packages/3d/environments/old-sea-port-environment-36897

## More screenshots :
![Scraps2](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/Scraps2.webp "Scraps2")
![Scraps1](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/Scraps1.webp "Scraps1")
![MoonCatalogue](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/MoonCatalogue.webp "MoonCatalogue")
![NewMoon1](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/NewMoon1.webp "NewMoon1")
![NewMoon2](https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/Screenshots/NewMoon2.webp "NewMoon2")

## Contribution
If you would like to contribute to the project, please take a look at [CONTRIBUTING.md](https://github.com/HolographicWings/LethalExpansion/blob/main/CONTRIBUTING.md) for details!
Thank you to user [Panthr75](https://github.com/Panthr75) for writing most of this very helpful document.