
# Contributing
LethalExpansion currently targets `.netstandard 2.1` and uses a `Dependencies.props` + `Paths.props` project structure, as the paths to certain dlls can be different depending on where `Lethal Company` is installed and how your development environment is set up.

## Paths.props
When first opening up this project in an IDE, you may get the following error:

```
No 'Paths.props' file detected. Please add one to the directory 'C:/FilePath/To/Solution/Folder'. For information on how to setup a 'Paths.props' file, look at CONTRIBUTING.md.
```

To fix this, go to the folder that this solution is in and create a new file named `Paths.props`. Inside that file should look something like this:

```xml
<Project>
  <PropertyGroup>
    <GameAssembliesPath>C:/SteamLibrary/steamapps/common/Lethal Company/Lethal Company_Data/Managed</GameAssembliesPath>
    <BepInExCorePath>C:/SteamLibrary/steamapps/common/Lethal Company/BepInEx/core</BepInExCorePath>
    <LethalSDKPath>C:/SteamLibrary/steamapps/common/Lethal Company/BepInEx/plugins/HolographicWings-LethalExpansion/LethalSDK.dll</LethalSDKPath>
    <MoreCompanyPath>C:/SteamLibrary/steamapps/common/Lethal Company/BepInEx/plugins/notnotnotswipez-MoreCompany/MoreCompany.dll</MoreCompanyPath>
    <DebugDirectory>C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\BepInEx\plugins</DebugDirectory>
	<LethalModulesPath>C:\Users\fearf\AppData\Roaming\Thunderstore Mod Manager\DataFolder\LethalCompany\profiles\More\BepInEx\plugins\HolographicWings-LethalExpansion</LethalModulesPath>
  </PropertyGroup>
</Project>
```

Note you most likely will not be able to copy and paste the above text exactly, as some of the paths will be different depending on where you have `Lethal Company` installed, where `BepInEx` is installed, where `LethalSDK.dll` is, and where `MoreCompany.dll` is.

Here's an empty `Paths.props` template:
```xml
<Project>
  <PropertyGroup>
    <GameAssembliesPath></GameAssembliesPath>
    <BepInExCorePath></BepInExCorePath>
    <LethalSDKPath></LethalSDKPath>
    <MoreCompanyPath></MoreCompanyPath>
    <DebugDirectory></DebugDirectory>
    <LethalModulesPath></LethalModulesPath>
  </PropertyGroup>
</Project>
```

> [!NOTE]
> Keep in mind that `Paths.props` won't be included in commits.

Below are the properties that are required inside `Paths.props`.

#### GameAssembliesPath
This is the path to the Lethal Company game assemblies directory.

You can find this by doing the following
- Go to Steam
- Right click on Lethal Company
- Click on `Properties`
- Click on `Installed Files`
- Click on `Browse`
- Navigate to the `LethalCompany_Data` folder
- Navigate to the `Managed` folder

Copy that path and paste it in `Paths.props` in-between `<GameAssembliesPath>` and `</GameAssembliesPath>`.

#### BepInExCorePath
This is the path to the `BepInEx/core` directory.

Finding this will depend on how you are installing mods. Follow the instructions below depending on how you are installing mods to get the path of the `BepInEx/core` directory.

Paste that path in `Paths.props` in-between `<BepInExCorePath>` and `</BepInExCorePath>`.

##### Manual Installation
- Go to Steam
- Right click on Lethal Company
- Click on `Properties`
- Click on `Installed Files`
- Click on `Browse`
- Navigate to the `BepInEx` folder
- Navigate to the `core` folder
- Copy that path

##### R2ModMan
- Select a profile for development of Lethal Expansion
  - In that profile, make sure `BepInEx` is installed
- Navigate to `Settings`
- Click `Browse profile folder`
- Navigate to the `BepInEx` folder
- Navigate to the `core` folder
- Copy that path

#### LethalSDKPath
This is the path to the `LethalSDK.dll` file.

Since `LethalSDK` is not included in this solution, referencing it is a little weird. It requires installing `LethalExpansion` just to get `LethalSDK.dll`. The path of this will depend on how you are installing mods. Follow the instructions below depending on how you are installing mods to get the path of the `LethalSDK.dll` file.

Paste that path in `Paths.props` in-between `<LethalSDKPath>` and `</LethalSDKPath>`.

##### Manual Installation
- Go to Steam
- Right click on Lethal Company
- Click on `Properties`
- Click on `Installed Files`
- Click on `Browse`
- Navigate to the `BepInEx` folder
- Navigate to the `plugins` folder
- Find `LethalSDK.dll` and copy the path to it

##### R2ModMan
- Select a profile for development of Lethal Expansion
  - In that profile, make sure `LethalExpansion` is installed
- Navigate to `Settings`
- Click `Browse profile folder`
- Navigate to the `BepInEx` folder
- Navigate to the `plugins` folder
- Navigate to the `HolographicWings-LethalExpansion` folder
- Find `LethalSDK.dll` and copy the path to it

#### MoreCompanyPath
This is the path to the `MoreCompany.dll` file.

Finding this will depend on how you are installing mods. Follow the instructions below depending on how you are installing mods to get the path of the `MoreCompany.dll` file.

Paste that path in `Paths.props` in-between `<MoreCompanyPath>` and `</MoreCompanyPath>`.

##### Manual Installation
- Go to Steam
- Right click on Lethal Company
- Click on `Properties`
- Click on `Installed Files`
- Click on `Browse`
- Navigate to the `BepInEx` folder
- Navigate to the `plugins` folder
- Find `MoreCompany.dll` and copy the path to it

##### R2ModMan
- Select a profile for development of Lethal Expansion
  - In that profile, make sure `MoreCompany` is installed
- Navigate to `Settings`
- Click `Browse profile folder`
- Navigate to the `BepInEx` folder
- Navigate to the `plugins` folder
- Navigate to the `notnotnotswipez-MoreCompany` folder
- Find `MoreCompany.dll` and copy the path to it

### DebugDirectory
This is the directory that all of your built files will be put in after the project is done compiling. This should point to your BepInEx\plugins directory where you intend to test the mod.

For example, if you want to have your built version of the mod automatically loaded into your modded client that is in Steam's default location, this path should be:
`C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\BepInEx\plugins`
When building, the project will automatically tag `LethalExpansion` to the end of the above directory.


### LethalModulesPath
This is the location where the default asset bundle modules for LethalExpansion are located. This path should be a directory that includes both `oldseaport.lem` and `templatemod.lem`.

Please note that all of the `.lem` files in this repository are symbolic links, which will not work correctly, so try to avoid using those when considering the correct path.

If you have LethalExpansion installed already in Steam's default location, then this path should look something like:
`C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\BepInEx\plugins\HolographicWings-LethalExpansion`
