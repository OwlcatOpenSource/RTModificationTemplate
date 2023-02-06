Getting started
===============

1. Open the project using **Unity 2021.3.3f1**
    * Unity console will show many compiler errors, but **don't panic**!
	* Click **Modification Tools -> Setup project with Steam** (it will try to find your game installation using Steam app information on your computer. 
	* If automatic Steam-based set up fails it'll fall back to ask you to choose **Warhammer 40,000 Rogue Trader** installation folder_ in the dialog that will appear
    * If Unity shows you **API Update Required** dialog click **No Thanks**
    * Close and reopen project
	* If Unity console will still show some errors, just try to hit "Clear" button in the console. If after thar concole is error-free, everything is alright.
    * Project is now ready

2. Setup your modification (take a look at _Assets/Modifications/ExampleModification_ for examples)
    * Create a folder for your modification in **Assets/Modifications**
    * Create **Modification** scriptable object in this folder _(right click on folder -> Create -> Modification)_
        * Specify **Unique Name** in **Modification** scriptable object
    * Create **Scripts** folder and **_your-modification-name_.Scripts.asmdef** file inside it _(right click on folder -> Create -> Assembly Definition)_
    * Create **Content**, **Blueprints** and **Localization** folders as needed

3. Create your modification (Owlcat way)

3.1 Build your modification (use **Modification Tools -> Build** menu entry)

3.2 Test your modification
    * Copy **Build/your-modification-name** folder to **_user-folder_/AppData/LocalLow/Owlcat Games/WH 40000 RT/Modifications**
    * Add your modification to **_user-folder_/AppData/LocalLow/Owlcat Games/WH 40000 RT/OwlcatModificationManagerSettings.json**
        ```json5
        {
            "EnabledModifications": ["your-modification-name"] // use name from the manifest(!), not folder name
        }
        ```
    * Run Warhammer 40,000 Rogue Trader

3.3 Publish build results from **Build** folder
    * _TODO_
	
4. Create your modification (UnityModManager way)
	* This game has UMM built-in. That means that if you want to create or use UMM mod you don't need to download UMM and patch the game.
	* Create your modification as if you were using UMM.
	* You should use UMM dll and it's dependancies from WhRtModificationTemplate\Extra\UnityModManagerDlls.zip.
	* Do not copy UMM dll's from UnityModManagerDlls.zip into your mod, just reference them. All the dll's are already build-in the game.
	* In two words - use ide (Visual Studio or Jetbrains Rider) to create a new Dll project. Reference dll's from UnityModManagerDlls.zip.
	* Add Info.json file to the root folder of your mod. It should look similar to this one:
	
	```{
		"Id": "VmcharModMaker",
		"DisplayName": "Vmchar Test Mod",
		"Author": "vmchar",
		"Version": "1.0.0",
		"ManagerVersion": "0.25.0",
		"GameVersion": "0.1.0",
		"AssemblyName": "VmcharModMaker.dll",
		"EntryMethod": "VmcharModMaker.VladTestMod.Load"
	}```
	* This game uses some kind of cut UMM version. So don't change "Version", "ManagerVersion" and "GameVersion" fields values.
	* As an exapmle of UMM-made mod you can take a look at **WhRtModificationTemplate\Extra\VmcharModMaker (source project).zip**

4.1 To build your modification you should use your ide. Just perform as you are using normal verions of UMM. 

4.2 Test the modification by moving your mod folder inside **_user-folder_/AppData/LocalLow/Owlcat Games/WH 40000 RT/UnityModManager**

4.3 UMM tutorial. It's better to take a look at UMM documentation in case yourn't familiar with it. You can find it at https://wiki.nexusmods.com/index.php/How_to_create_mod_for_unity_game

Features (non UMM)
========

### Settings
	* Press Shift+F10 key to open OwlcatModificationManager control panel.
	* Press Ctrl+F10 to open UMM control panel.

Control panels let you enable/disable mods and check information about installed mods.

All content of your modification must be placed in folder with **Modification** scriptable object or it's subfolders.

### Scripts

All of your scripts must be placed in assemblies (in folder with ***.asmdef** files or it's subfolders). **Never** put your scripts (except Editor scripts) in other places.

### Content

All of your content (assets, prefabs, scenes, sprites, etc) must be placed in **_your-modification-name_/Content** folder.

### Blueprints

Blueprints are JSON files which represent serialized version of static game data (classes inherited from **SimpleBlueprint**).

* Blueprints must have file extension ***.jbp** and must be situated in **_your-modification-name_/Blueprints** folder.
    * _example: Examples/Basics/Blueprints/TestBuff.jbp_

    ```json5
    // *.jbp file format
    {
        "AssetId": "unity-file-guid-from-meta", // "42ea8fe3618449a5b09561d8207c50ab" for example
        "Data": {
            "$type": "type-id, type-name", // "618a7e0d54149064ab3ffa5d9057362c, BlueprintBuff" for example
            
            // type-specific data
        }
    }
    ```

    * if you specify **AssetId** of an existing blueprint (built-in or from another modification) then the existing blueprint will be replaced

* For access to metadata of all built-in blueprints use this method
    ```C#
    // read data from <WotR-installation-path>/Bundles/cheatdata.json
    // returns object {Entries: [{Name, Guid, TypeFullName}]}
    BlueprintList Kingmaker.Cheats.Utilities.GetAllBlueprints();
    ```

* You can write patches for existing blueprints: to do so, create a ***.patch** JSON file in **_your-modification-name_/Blueprints** folder. Instead of creating a new blueprint, these files will modify existing ones by changing only fields that are specified in the patch and retaining everything else as-is.

    * _Example 1: Examples/Basics/Blueprints/ChargeAbility.patch_
    * 
    * _Example 2: Examples/Basics/Blueprints/InvisibilityBuff.patch_ 

    * Connection between the existing blueprint and the patch must be specified in **BlueprintPatches** scriptable object _(right click in folder -> Create -> Blueprints' Patches)_

        * _example: Examples/Basics/BlueprintPatches.asset_
      
    * **OLD**: Newtonsoft.Json's Populate is used for patching (_#ArrayMergeSettings and _#Entries isn't supported)
  
      * https://www.newtonsoft.com/json/help/html/PopulateObject.htm 
  
    * **NEW** (game version 1.1.1): Newtonsoft.Json's Merge is used for patching
  
      * https://www.newtonsoft.com/json/help/html/MergeJson.htm

    ```json5
    // *.patch file format: change icon in BlueprintBuff and disable first component
    {
      "_#ArrayMergeSettings": "Merge", // "Union"/"Concat"/"Replace"
      "m_Icon": {"guid": "b937cb64288636b4c8fd4ba7bea337ea", "fileid": 21300000},
      "Components": [
        {
          "m_Flags": 1
        }
      ]
    }
    ```
  _OR_

    ```json5
    {
      "_#Entries": [
        {
          "_#ArrayMergeSettings": "Merge", // "Union"/"Concat"/"Replace"
          "m_Icon": {"guid": "b937cb64288636b4c8fd4ba7bea337ea", "fileid": 21300000},
          "Components": [
            {
              "m_Flags": 1
            }
          ]
        }
      ]
    }
    ```

### Localization

You can add localized strings to the game or replace existing strings. Create **enGB|ruRU|deDE|frFR|zhCN|esES.json** file(s) in **_your-modification-name_/Localization** folder.

* _example: Examples/Basics/Localizations/enGB.json_

* You shouldn't copy enGB locale with different names if creating only enGB strings: enGB locale will be used if modification doesn't contains required locale.

* The files should be in UTF-8 format (no fancy regional encodings, please!)

```json5
// localization file fromat
{
    "strings": [
        {
            "Key": "guid", // "15edb451-dc5b-4def-807c-a451743eb3a6" for example
            "Value": "whatever-you-want"
        }
    ]
}
```

### Assembly entry point

You can mark static method with **OwlcatModificationEnterPoint** attribute and the game will invoke this method with corresponding _OwlcatModification_ parameter once on game start. Only one entry point per assembly is allowed.

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

```C#
[OwlcatModificationEnterPoint]
public static void EnterPoint(OwlcatModification modification)
{
    ...
}
```

### GUI

Use **OwlcatModification.OnGUI** for inserting GUI to the game. It will be accessible from modifications' window (_ctrl+M_ to open). GUI should be implemented with **IMGUI** (root layout is **vertical**).

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

### Harmony Patching

Harmony lib is included in the game and you can use it for patching code at runtime.

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method) and Examples/Basics/Scripts/Tests/HarmonyPatch.cs_

* [Harmony Documentation](https://harmony.pardeike.net/articles/intro.html)

```C#
OwlcatModification modification = ...;
modification.OnGUI = () => GUILayout.Label("Hello world!");
```

### Storing data

* You can save/load global modification's data or settings with methods _OwlcatModification_.**LoadData** and  _OwlcatModification_.**SaveData**. Unity Serializer will be used for saving this data.

    * _Example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.TestData method)_

    ```C#
    [Serialzable]
    public class ModificationData
    {
        public int IntValue;
    }
    ...
    OwlcatModification modification = ...;
    var data = modification.LoadData<ModificationData>();
    data.IntValue = 42;
    modification.SaveData(data);
    ```

* You can save/load per-save modification's data or settings by adding **EntityPartKeyValueStorage** to **Game.Instance.Player**.

    * _Example: Examples/Basics/Scripts/Tests/PerSaveDataTest.cs_

    ```C#
    var data = Game.Instance.Player.Ensure<EntityPartKeyValueStorage>().GetStorage("storage-name");
    data["IntValue"] = 42.ToString();
    ```

### EventBus

You can subscribe to game events with **EventBus.Subscribe** or raise your own event using **EventBus.RaiseEvent**.

* _Example (subscribe): Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

* Raise your own event:

    ```C#
    interface IModificationEvent : IGlobalSubscriber
    {
        void HandleModificationEvent(int intValue);
    }
    ...
    EventBus.RaiseEvent<IModificationEvent>(h => h.HandleModificationEvent(42))
    ```

### Rulebook Events

* **IBeforeRulebookEventTriggerHandler** and **IAfterRulebookEventTriggerHandler** exists specifically for modifications. These events are raised before _OnEventAboutToTrigger_ and _OnEventDidTigger_ correspondingly.
* Use _RulebookEvent_.**SetCustomData** and _RulebookEvent_.**TryGetCustomData** to store and read your custom RulebookEvent data.

### Resources

_OwlcatModification_.**LoadResourceCallbacks** is invoked every time when a resource (asset, prefab or blueprint) is loaded.

### Game Modes and Controllers

A **Controller** is a class that implements a particular set of game mechanics. It must implementi _IController_ interface.

**Game Modes** (objects of class _GameMode_) are logical groupings of **Controllers** which all must be active at the same time. Only one **Game Mode** can be active at any moment. Each frame the game calls **Tick** method for every **Controller** in active **Game Mode**. You can add your own logic to Pathfinder's main loop or extend/replace existing logic using **OwlcatModificationGameModeHelper**.

* _Example (subscribe): Examples/Basics/Scripts/Tests/ControllersTest.cs_

### Using Pathfinder shaders

Default Unity shaders doesn't work in Pathfinder. Use shaders from **Owlcat** namespace in your materials. If you don't know what you need it's probably **Owlcat/Lit** shader.

### Scenes

You can create scenes for modifications but there is a couple limitations:

* if you want to use Owlcat's MonoBehaviours (i.e. UnitSpawner) you must inherit from it and use child class defined in your assembly

* place an object with component **OwlcatModificationMaterialsInSceneFixer** in every scene which contains Renderers

### Helpers

* Copy guid and file id as json string: _right-click-on-asset -> Modification Tools -> Copy guid and file id_

* Copy blueprint's guid: _right-click-on-blueprint -> Modification Tools -> Copy blueprint's guid_
    
* Create blueprint: _right-click-in-folder -> Modification Tools -> Create Blueprint_

* Find blueprint's type: _Modification Tools -> Blueprints' Types_

### Interactions and dependencies between modifications

Work in progress. Please note that users will be able to change order of mods in the manager. We're planning to provide the ability to specify a list of dependencies for your modification, but it will only work as a hint: the user will be responsible for arranging a correct order of mods in the end.
 
### Testing

* Command line argument **-start_from=_area-name/area-preset-name_** allows you to start game from the specified area without loading main menu.
* Cheat **reload_modifications_data** allows you to reload content, blueprints and localizations. All instantiated objects (prefab instances, for example) stays unchanged.