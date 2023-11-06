![# Unity Settings Framework Logo](/.GitHubResources/Banner.png)

This package aims to provide a comprehensible and expandable way of creating in-game settings for any Unity game.
\
To do so, it uses [Scriptable Objects](https://docs.unity3d.com/Manual/class-ScriptableObject.html) and [generics](https://docs.microsoft.com/en-us/dotnet/standard/generics/), the latter of which Unity can serialize since version [2020.1](https://forum.unity.com/threads/generics-serialization.746300/).
\
**The package will not work properly in pre-2020.1 versions!**


**NOTICE** \
As of package version 2.5.0, an alternative syntax for loading settings at runtime has been introduced. \
The example below, as well as the relevant sample script have been updated accordingly.


## Using the Package

### 1. Setup
After installing the Package into your project via the UPM (and making sure the assembly definitions are recognized by Unity), create a SettingsAsset through _Create > Scriptable Objects > Zenvin > Settings Asset_. This will serve as a collection of all Settings you may create.
\
Once you have created the `SettingsAsset`, you can use the **Settings Editor Window** (_Window > Zenvin > Settings Asset Editor_) to add, delete, move and group Settings. Double-clicking `SettingsAsset`s will open the editor.
\
If there is no asset being edited currently, the editor will show a list of available instances.


## 2. Settings and Setting Groups
Settings are represented by Scriptable Object instances that inherit `Zenvin.Settings.Framework.SettingBase<T>`. They each contain a number of properties by default, but can be extended to contain further members by creating own classes inheriting the aforementioned base class.

Settings Groups can be used to group Settings together. Just like Settings, they are Scriptable Objects (inheriting `Zenvin.Settings.Framework.SettingsGroup`), and it is possible to extend the built-in `SettingsGroup` type by inheriting from it.
\
When creating UI for your Settings, Groups can be used to separate them into tabs or sub-headers.

### Shared serialized Properties
Below is a list of properties shared between `Zenvin.Settings.Framework.SettingBase` and `Zenvin.Settings.Framework.SettingsGroup`, that show up in the inspector.

| Property | Type | Description |
|-|-|-|
| `GUID` | string | The `GUID` provides a unique identifier for each Setting or Group. By default, it will be assigned a pseudo-random, unique hexadecimal string which can be changed through the Setting Editor window.<br>However, the value cannot be empty and it must be unique among **all** Settings/Groups within the same `SettingsAsset`. Should that not be the case, the system will reset the `GUID` to its previous value. |
| `Name` | string | The `Name` is supposed to be the default label for the Setting on UI. Is also will show up in the hierarchy of the Settings Editor window.<br>Setting names do not need to be unique. |
| `NameLocalizationKey` | string | The `NameLocalizationKey` can be used to add localization to your Settings menus. Just as the `Name`, it can have an arbitrary value. |


### SettingBase serialized properties
Below is a list of properties unique to `Zenvin.Settings.Framework.SettingBase<T>`, that show up in the inspector.

| Property | Type | Description |
|-|-|-|
| `DefaultValue` | T | The `DefaultValue` is a generically typed value, and will be assigned to the Setting by default during runtime initialization. |


### SettingsGroup serialized properties
Below is a list of properties unique to `Zenvin.Settings.Framework.SettingsGroup`, that show up in the inspector.

| Property | Type | Description |
|-|-|-|
| `Icon` | Sprite | The `Icon` is a sprite that can be used to represent the Group on UI, either instead or along with the name. |


## 3. Creating Groups and Settings
Creating new Groups and Settings happens through the Settings Editor window. Simply right-click any existing Group, and you will have the option to create a Group or Setting inside it, or delete the Group in question.
Settings on the other hand will only allow you to duplicate or delete them.
\
Note that changing the Settings' hierarchy is only permitted during edit-time. The popup menus will not appear during runtime.


## 4. Using Settings
Generally speaking, there are 3 different ways of referencing Settings:
- As a `SettingBase<T>` reference that is assigned in the editor or through code.
- Wrapped in a `SettingReference<T>` instance, whose value is assigned in the editor. This has the advantage that a default value can be given, even if the Setting is `null`.
- Accessed by its `GUID` using `SettingsAsset.TryGetSettingByGUID(string, out SettingBase)`, assuming that the containing `SettingsAsset` has been initialized.

### 4.0 Initialization
`SettingsAsset`s needs to be initialized before their Settings can be used properly. Initialization must happen **at runtime**, with a call to the _non-static_ `SettingsAsset.Initialize()` method. 
\
For performance reasons, it is recommended to load external Settings (see [Loading external Settings](#6-loading-external-settings)) during initialization, rather than after the fact.

### 4.1 Getting a Setting's value
Each Setting object contains 2 essential values that are managed by the Framework:
* `CurrentValue`: The value that is currently **applied** to the Setting.
* `CachedValue`: The value that the Setting will have once applied.

To get a Setting's value, you would usually poll `CurrentValue`. This can be done either continuously, or every time `SettingBase<T>.OnValueApplied` is invoked.

### 4.2 Processing Setting values
In some cases, it is necessary that your Setting value adheres to specific rules. For example, when your Setting refers to an array using its value.
\
`ProcessValue(ref T)` in `SettingBase<T>` is a `virtual void` method allows to process a given value, before the Setting is updated with it. By default, this method does nothing, so it does not _have_ to be overridden if you just want to use the value as-is.

### 4.3 Setting, Applying, Reverting and Resetting values
* When `SetValue(T)` is called on a given `SettingBase<T>`, it will update the Setting's `CachedValue` and mark the Setting as _dirty_.
* Dirty Settings can be applied with a call to `ApplyValue()`. This will remove the _dirty_ flag, update the Setting's `CurrentValue` with whatever `CachedValue` was set to, and invoke `OnValueApplied`.
* Reverting a _dirty_ Setting with a call to `RevertValue()` will set its `CachedValue` to its `CurrentValue`.
* A Setting can be reset by calling `ResetValue()`. This will set its value to the `DefaultValue` it was given in the editor.
* Reverting or resetting will invoke `OnValueReverted` or `OnValueReset`, respectively. They will also both mark the Setting as _dirty_.


## 5. Saving and Loading Setting values
Settings can be serialized in any number of ways. The Framework uses the `ISerializer<T>` and `ISerializable<T>` interfaces to facilitate a most abstract approach to this. You can have a look at the included [JSON](./Runtime/Framework/Serialization/JSON/) and [binary](./Runtime/Framework/Serialization/Binary/) serializers to get an idea of how to implement `ISerializer<T>`, and check out any of the [built-in Setting types](./Runtime/Framework/Settings/) to see, how `ISerializable<T>` may be implemented. \
See below for a more in-depth explanation.

### 5.0 Serialization
Serializing Settings starts with a call to `SettingsAsset.SerializeSettings`. This method needs to be passed an `ISerializer<T>` instance. \
That instance is responsible for managing save data for all Settings that implement `ISerializable` with the same `T`. From this follows, that **any Settings that do not implement `ISerializable` of the required type will be ignored during the serialization process.** \
When Settings are serialized, a new instance of `T` will be created for each Setting and passed to the Setting's implementation of `ISerializable<T>.OnSerialize` to be manipulated (i.e. receive information about the Setting's state).
After that, it is passed on to the given `ISerializer<T>` instance, along with the currently serialized Setting's `GUID` via `ISerializer<T>.Serialize(string, T)`. \
From that point on, it is the serializer's task to store this information.

### 5.1 Deserialization
Deserializing Settings starts with a call to `SettingsAsset.DeserializeSettings`. This method needs to be passed an `ISerializer<T>` instance. \
That instance then has to provide a collection of all saved `GUID`s, along with their associated data in the form of an instance of `T`, via `ISerializer<T>.GetSerializedData()`. \
Subsequently, that collection will be iterated and any Setting whose GUID was found in the data and that implements a fitting `ISerializable<T>` will receive the respective data to read from. This again means that **any Settings that do not implement `ISerializable` of the required type will be ignored during the deserialization process.** \
Just with serialization, the specific way of deserialization will have to be implemented by the consumer, using `ISerializable<T>.OnDeserialize`. \
\
**Important** \
Due to the generic nature of the deserialization process, Settings' values cannot automatically be assigned to the loaded data. This means that calling `SettingBase<T>.SetValue` manually is required at the end of the `ISerializable<T>.OnDeserialize` method, if that method is supposed to fulfil its function. \
The Framework will however automatically call `ApplyValue` on each deserialized Setting.

### 5.2 Hooking into the Serialization/Deserialization process
Sometimes, the serialized or deserialized data might need to be processed, such as loading data from a file before deserialization, or writing it to a file after serialization. \
For this purpose, any class implementing `ISerializer<T>` may also implement `ISerializerCallbackReceiver` (not to be confused with Unity's [`ISerializationCallbackReceiver`](https://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.html)!). \
This interface implements the following four methods, which can be used to respond to specific steps of the serialization process: 
* `InitializeSerialization()` will be called before serialization starts.
* `FinalizeSerialization()` will be called after serialization has finished.
* `InitializeDeserialization()` will be called before deserialization starts.
* `FinalizeDeserialization()` will be called after deserialization has finished.

See [JSON File Serializer](./Runtime/Framework/Serialization/JSON/JsonFileSerializer.cs) or [Binary File Serializer](./Runtime/Framework/Serialization/Binary/BinaryFileSerializer.cs) for examples on how to implement the interface.

Note that none of the above methods will be invoked, if the serialization/deserialization fails instantly, due to the executing `SettingsAsset` not being initialized.

## 6. Loading external Settings
The Framework allows loading one or multiple Settings and Setting Groups from one or several JSON strings.
\
Loading will ignore all Settings and Groups whose `GUID`s already exist. However, it is possible to load external Settings and Groups as children of existing Groups, as well as loading external Settings as children of external Groups from the same or a previous load process.
\
\
However, loading is only possible during the initialization process of the SettingsAsset. To load, first subscribe to `SettingsAsset.OnInitialize`, then call `SettingsAsset.Initialize()`.

### 6.0 Setting Factories
In order to load external Settings, the `RuntimeSettingLoader` needs to know how to translate the `string` values it gets from the parsed JSON objects into `SettingBase<T>` instances.
\
That is where the `ISettingFactory` interface comes into play:
* `ISettingFactory.GetDefaultValidType()`: This method returns the default type `string` which this factory can translate into a Setting object. This value can be overridden while calling `RuntimeSettingLoader.LoadSettingsIntoAsset`.
* `ISettingFactory.CreateSettingFromType(string, StringValuePair[])`: Should return an instance of your desired Setting class.

### 6.1 Group Factories
Loading external Groups happens similarly to the way external Settings are loaded, with the main difference being that the `RuntimeSettingLoader` will fall back to the built-in `SettingsGroup` type, if it cannot translate the JSON values into Group instances.
\
Translating the values again, is where an interface is used - respectively `IGroupFactory`:
* `IGroupFactory.GetDefaultValidType()`: This method returns the default type `string` which this factory can translate into a Group object. This value can be overridden while calling `RuntimeSettingLoader.LoadSettingsIntoAsset`.
* `IGroupFactory.CreateGroupFromType(StringValuePair[])`: Should return an instance of your desired Group class.


# Examples
## 1. Creating a Setting based on an `int` to serve as a dropdown
Make a new script and paste the below code into it:
```csharp
using System;
using Zenvin.Settings.Framework;

public class DropdownSetting : SettingBase<int>     // declare custom Setting type
{
    [SerializeField] private string[] values;       // expose field in the editor to allow assigning dropdown values
}
```
After you let Unity recompile, the new Setting type should show up in your create setting context menu.

## 2. Clamping a Setting's value
Assume you have the class from **Example 1**, and now you would like to make sure your Setting's `CurrentValue` does not go out of the `values`' bounds.
\
All you need to do to achieve this, is `override` the `ProcessValue` method like this:
```csharp
protected override void ProcessValue(ref int value)
{
    value = Mathf.Clamp (value, 0, values.Length - 1);
}
```

## 3. Loading external Settings on runtime
First, create factories for your custom Setting types. For the class from **Example 1**, this could look somewhat as follows:
```csharp
public class DropdownSettingFactory : ISettingFactory
{
    string ISettingFactory.GetDefaultValidType() => "dropdown";     // make default type string "dropdown"
    
    SettingBase ISettingFactory.CreateSettingFromValue(string defaultValue, StringValuePair[] values)
    {
        if (!int.TryParse(defaultValue, out int val)) {     // try parsing the json default value to an int
          val = 0;                                              // if not possible, use 0 as default
        }
        return DropdownSetting.CreateInstanceWithValues<DropdownSetting>(val, values);  // create a new instance.
    }
}
```
The above factory can create an instance of `DropdownSetting`. By default, it will respond to the json type string `"dropdown"`.
Note that the factory uses `SettingBase<T>.CreateInstanceWithValues`, rather than [`ScriptableObject.CreateInstance`](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html). This is necessary, because the Setting needs to be initialized. Setting instances created with the latter method will not be considered during loading.
\
\
Next, make a `MonoBehaviour` to initialize your Settings Asset and load Settings using the `Zenvin.Settings.Loading.RuntimeSettingLoader` class:
```csharp
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Loading;

public class SettingsInitializer : MonoBehaviour
{
    [SerializeField] private SettingsAsset asset;
    [SerializeField, TextArea (10, 10)] private string json;  // this is for the JSON string you want to load
    
    private void Start ()
    {
        SettingsAsset.OnInitialize += OnInitialize; // static event, hence the SettingAsset parameter
        asset.Initialize();
    }
    
    private void OnInitialize (SettingsAsset asset)
    {
        var options = new SettingLoaderOptions (asset)                          // initialize options object with the target asset
            .WithData (json)                                                    // set the options' data from the json string 
            .WithSettingFactory ("bool", new BoolSettingFactory ())             // add factories provided by the package
            .WithSettingFactory ("int", new IntSettingFactory ())               // for the Int and Bool factories, the "type" they are used for will be overwritten
            .WithSettingFactory (new FloatSettingFactory ())                    // for the Float factory, its default "type" value will be used

            .WithSettingFactory ("dropdown", new DropdownSettingFactory ());    // add the factory we created earlier

        RuntimeSettingLoader.LoadSettingsIntoAsset (options);   // load settings into the asset, using the options created above
    }
}
```
Note that the `LoadSettingsIntoAsset` method can override individual factories' target types. This allows using the same factory for multiple types of Settings.
\
Last, create a JSON string for the Settings you want to load and assign it to that `MonoBehaviour`, along with the Settings Asset.
Such a JSON string could look like this:
```json
{
    "Groups": [
        {
            "GUID": "_graphics",
            "Name": "Graphics",
            "LocalizationKey": "",
            "ParentGroupGUID": "",
            "IconResource": ""
        }
    ],
    "Settings": [
        {
            "GUID": "anti_aliasing",
            "Name": "Anti-Aliasing",
            "LocalizationKey": "",
            "ParentGroupGUID": "_graphics",
            "Type": "dropdown",
            "DefaultValue": "2",
            "Values": [
                {
                    "Key": "",
                    "Value": "Disabled"
                },
                {
                    "Key": "",
                    "Value": "2x Multisampling"
                },
                {
                    "Key": "",
                    "Value": "4x Multisampling"
                },
                {
                    "Key": "",
                    "Value": "8x Multisampling"
                }
            ]
        }
    ]
}
```
This would first try to create a new Group called `Graphics` with GUID `_graphics`, and then to make a Setting from a hypothetical `dropdown` factory, give it a default value of `2` and try to set it up with an array of values. Assuming both were successful, the new Setting would become a child of the new Group.
\
If there already was a Group with GUID `_graphics`, no new Group will be created, and the Setting will be added to the existing one instead.

## 4. Setting up a dynamic Setting
Sometimes, a default value alone is not enough to set up an external Setting with. Because of that, the `SettingBase<T>` class allows you to override its `OnCreateWithValues(StringValuePair[])` method. Sticking with the class from the previous examples, you could use the `Key`s of all the `StringValuePair`s you get, to populate your `values`.
\
In code, this can look like this:
```csharp
protected override void OnCreateWithValues (StringValuePair[] _values)
{
    this.values = new string[_values.Length]; // assign a new array with appropriate length to values
    for (int i = 0; i < values.Length; i++)   // iterate through either array
    {
        values[i] = _values[i].Value;         // set content of values
    }
}
```

## 5. Automatically spawning UI for registered Settings
The Framework does not provide a way to do this, but it has a tool to help:
\
`Zenvin.Settings.UI.SettingControlCollection`
\
This class provides a way to reference `SettingControl` prefabs and get the fitting prefab for any given `SettingBase` sub-class, as long as there is one referenced.
\
Below is a simple example for how that can be used in a dynamically created settings menu:
```csharp
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private SettingsAsset asset;   // the asset to source the Settings from
    [SerializeField] private SettingControlCollection prefabs;  // a collection of available Setting control prefabs
    [SerializeField] private LayoutGroup parent;    // the layout group to parent the spawned Setting controls to


    private void Start ()
    {
        asset.Initialize();    // initializing Settings could be done somewhere else as well.
        SpawnSettings();
    }

    private void SpawnSettings ()
    {
        var settings = asset.GetAllSettings();
        foreach (var setting in settings)   // iterate over all registered Settings
        {
            if (prefabs.TryGetControl(setting.GetType(), out SettingControl prefab))    // try get a SettingControl prefab matching the current Setting
            {
                if (prefab.TryInstantiateWith (setting, out SettingControl control))     // try instantiating the found prefab with the given Setting. If successful, this will automatically spawn and initialize the prefab.
                {
                    control.transform.SetParent (parent.transform); // make instance a child of the layout group
                    control.transform.localScale = Vector3.one; // reset instance scale, because parenting UI elements likes to mess that up
                }
            }
        }
    }
}
```
This could be expanded to utilize the Settings' group structure to implement tabs and/or headers in the menu as well. Have a look into [SettingsMenu.cs](https://github.com/xZenvin/UnitySettingsFramework/blob/main/Samples/Settings%20Menu/Scripts/SettingsMenu.cs) to see how that might work.

## 6. Creating a SettingControl for a specific Setting type
In **Example 1**, we implemented a Setting to represent a dropdown. In order to properly display it on UI, a SettingControl is required that can take that Setting's values and represent them.
\
To achieve this, we first need a way of accessing the `DropdownSetting`'s `values`. A simple way to do this is by adding the following property:
```csharp
public string[] Options => values; // return the values
```
With that, creating a control for that Setting type is simply a matter of making a new class that inherits `Zenvin.Settings.UI.SettingControl<TControlType, THandledType>` and using either `UnityEngine.UI` or `TMPro.TMP_Dropdown` to actually display the values:
```csharp
public class DropdownControl : SettingControl<DropdownSetting, int> // DropdownSetting is the SettingBase this Control is meant for, and int is the value type ultimately managed by the Setting
{
    [SerializeField] private TMP_Dropdown dropdown;

    protected override void OnSetup () {    // OnSetup is called when the Control is spawned via TryInstantiateWith()
        dropdown.ClearOptions ();
        dropdown.AddOptions (new List<string> (Setting.Options));   // Setting is provided by the base class. It will have the type given in the class declaration, so DropdownSetting in this case.
        dropdown.SetValueWithoutNotify (Setting.CurrentValue);
	}

    protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {  // called whenever the assigned Setting's value changes
	    dropdown?.SetValueWithoutNotify (Setting.CachedValue);  // make sure the dropdown's selection is "in sync" with the Setting's value
	}
}
```
To have the dropdown update the Setting's value, you can hook it up to the `SetValue(THandledType)` method provided by `SettingControl<TControlType, THandledType>` - which simply calls `Setting.SetValue(THandledType)` - or implement your own way.
