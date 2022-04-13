![Unity Settings Framework Logo](/.GitHubResources/Banner.png)

# UnitySettingsFramework

This package aims to provide a comprehensive, simple and expandable way of creating in-game settings for any Unity game.
\
To do so, it uses [Scriptable Objects](https://docs.unity3d.com/Manual/class-ScriptableObject.html) and [generics](https://docs.microsoft.com/en-us/dotnet/standard/generics/), the latter of which Unity can serialize since version [2020.1](https://forum.unity.com/threads/generics-serialization.746300/).
\
**The package will not work properly in pre-2020.1 versions!**

## Using the Package

### 1. Setup
After installing the Package into your project via the UPM (and making sure the assembly definitions are recognized by Unity), create a SettingsAsset through _Create > Scriptable Objects > Zenvin > Settings Asset_. This will serve as a collection of all settings you may create.
\
The system currently is set up to allow only for a single SettingsAsset object to be created in your project. (You will still be able to duplicate an existing asset, but I would advise against it).
\
Once you have created the SettingsAsset, you can use the **Settings Editor Window** (_Window > Zenvin > Settings Asset Editor_) to add, delete, move and group settings.


## 2. Settings
Settings are represented by Scriptable Object instances that inherit `Zenvin.Settings.Framework.SettingBase<T>`. They each contain a number of properties by default (see below), but can be extended to contain further fields by creating own classes inheriting the aforementioned base class.

### 2.0 GUID
The `GUID` provides a unique identifier for each setting. By default, it will be assigned a pseudo-random, unique hexadecimal string which can be changed through the Setting Editor window.
\
However the value cannot be empty, and it must be unique among **all** settings. Should that not be the case, the system will reset the `GUID` to its previous value.

### 2.1 Name
The `Name` is supposed to be the default label for the setting on UI. Is also will show up in the hierarchy of the Settings Editor window.
\
Setting names do not need to be unique.

### 2.2 Localization Key
The `NameLocalizationKey` can be used to add localization to your settings menus. Just as the `Name`, it can have any value.

### 2.3 Default Value
The `DefaultValue` is a generically typed value, and will be assigned to the Setting by default during runtime initialization.


## 3 Settings Groups
As the name suggests, Settings Groups can be used to group settings together. Just like Settings, they are Scriptable Objects (inheriting `Zenvin.Settings.Framework.SettingsGroup`) that contain several values (see below).
Other than settings however, groups are not meant to be expanded in functionality.
\
When creating UI for your settings, groups can be used to separate them into tabs or sub-headers.

### 3.0 GUID
The `GUID` provides a unique identifier for each group. By default, it will be assigned a pseudo-random, unique hexadecimal string which can be changed through the Setting Editor window.
\
However the value cannot be empty, and it must be unique among **all** groups. Should that not be the case, the system will reset the `GUID` to its previous value.

### 3.2 Name
The `Name` is supposed to be the default label for the group on UI. Is also will show up in the hierarchy of the Settings Editor window.
\
Group names do not need to be unique.

### 3.2 Localization Key
The `NameLocalizationKey` can be used to add localization to your settings menus. Just as the `Name`, it can have any value.

### 3.3 Icon
The `Icon` is a sprite that can be used to represent the group on UI, either instead or along with the name.


## 4. Creating Groups and Settings
Creating new groups and settings happens through the Settings Editor window. Simply right-click any existing group, and you will have the option to create a new group or setting inside it, or delete the group in question.
Settings on the other hand will only allow you to duplicate or delete them.
\
Note that changing the settings hierarchy is only permitted during edit-time. The popup menus will not appear during runtime.


## 5. Using Settings
Settings can be either referenced directly as a `SettingBase<T>` in the editor, or accessed by its `GUID`, assuming that the SettingsAsset has been initialized.

### 5.0 Initialization
Upon runtime start, the SettingsAsset needs to be initialized. This happens with a call to the _non-static_ `SettingsAsset.Initialize()` method.
\
During initialization, external settings can be loaded (see **7. Loading external Settings**).

### 5.1 Getting a Setting's value
Each setting object contains 2 essential values that are managed by the system:
* `CurrentValue`: The value that is currently **applied** to the setting.
* `CachedValue`: The value that the setting will have once applied.

To get a setting's value, you would usually poll `CurrentValue`. This can be done either continuously, or every time `SettingBase<T>.OnValueApplied` is invoked.

### 5.2 Processing Setting values
In some cases, it is necessary that your setting value adheres to specific rules. For example, when your setting refers to an array using its value.
\
`ProcessValue(ref T)` in `SettingBase<T>` is a `virtual void` method allows to process a given value, before the setting is updated with it. By default, this method does nothing, so it does not _have_ to be overridden if you just want to use the value as-is.

### 5.3 Setting, Applying, Reverting and Resetting values
* When `SetValue(T)` is called on a given `SettingBase<T>`, it will update the setting's `CachedValue` and mark the setting as _dirty_.
* Dirty settings can be applied with a call to `ApplyValue()`. This will remove the _dirty_ flag, update the setting's `CurrentValue` with whatever `CachedValue` was set to, and invoke `OnValueApplied`.
* Reverting a _dirty_ setting with a call to `RevertValue()` will set its `CachedValue` to its `CurrentValue`.
* A setting can be reset by calling `ResetValue()`. This will set its value to the `DefaultValue` it was given in the editor.
* Reverting or resetting will invoke `OnValueReverted` or `OnValueReset`, respectively. They will also both mark the setting as _dirty_.


## 6. Saving and Loading Setting values
The system allows you to save and load the `CurrentValue` of all registered settings to and from any given [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=net-6.0).
Each setting will be stored as a byte array, preceded by its `GUID`. Because of this, any custom implementation of the `SettingBase<T>` class needs to provide overrides for converting its `CurrentValue` from and to `byte[]`.
\
Settings can be loaded with `SettingsAsset.LoadAllSettings(Stream)` and saved with `SettingsAsset.SaveAllSettings(Stream)`.
\
Note that there are no `try-catch`es in there, to keep it lightweight. This means that you need to make sure that whatever `Stream` you give the system for loading is positioned at the same `Position` where it was when you saved.


## 7. Loading external Settings
The system allows loading one or multiple settings and setting groups from one or several JSON strings.
\
Loading will ignore all settings and groups whose `GUID`s already exist. However, it is possible to load external settings and groups as children of existing groups, as well as loading external settings as children of external groups from the same or a previous load process.
\
\
However, loading is only possible during the initialization process of the SettingsAsset. To load, first subscribe to `SettingsAsset.OnInitialize`, then call `SettingsAsset.Initialze()`.

### 7.0 Setting Factories
In order to load external settings, the system needs to know how to translate the `string` values it gets from the parsed JSON objects into `SettingBase<T>` instances.
\
That is where the `ISettingFactory` interface comes into play:
* `ISettingFactory.GetDefaultValidType()`: This method returns the default type `string` which this factory can translate into a setting object. This value can be overridden while calling `RuntimeSettingLoader.LoadSettingsIntoAsset`.
* `ISettingFactory.CreateSettingFromType(string, StringValuePair[])`: Should return an instance of your desired setting class.

# Examples
## 1. Creating a Setting based on an `int` to serve as a dropdown
Make a new script and paste the below code into it:
```csharp
using System;
using Zenvin.Settings.Framework;

public class DropdownSetting : SettingBase<int>     // declare custom setting type
{
    [SerializeField] private string[] values;       // expose field in the editor to allow assigning dropdown values

    protected override byte[] OnSerialize ()
    {
        return BitConverter.GetBytes (CurrentValue);    // serialize the current value
    }
    
    protected override int OnDeserialize (byte[] data)
    {
      return BitConverter.ToInt32 (data, 0);            // deserialize the current value
    }
}
```
After you let Unity recompile, the new setting type should show up in your create setting context menu.

## 2. Clamping a Setting's value
Assume you have the class from **Example 1**, and now you would like to make sure your setting's `CurrentValue` does not go out of the `values`' bounds.
\
All you need to do to achieve this, is `override` the `ProcessValue` method like this:
```csharp
protected void ProcessValue(ref int value)
{
    value = Mathf.Clamp (value, 0, values.Length - 1);
}
```

## 3. Loading external settings on runtime
First, create factories for your custom setting types. For the class from **Example 1**, this could look somewhat as follows:
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
Note that the factory uses `SettingBase<T>.CreateInstanceWithValues`, rather than [`ScriptableObject.CreateInstance`](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html). This is necessary, because the setting needs to be initialized. Setting instances created with the latter method will not be considered during loading.
\
\
Next, make a `MonoBehaviour` to initialize your Settings Asset and load settings using the `Zenvin.Settings.Loading.RuntimeSettingLoader` class:
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
        RuntimeSettingLoader.LoadSettingsIntoAsset(
            asset,                        // the target asset
            json,                         // the json string to load from
            null,                         // an IGroupIconLoader object to load group icons
            ("bool", new BoolSettingFactory()),         // factories provided by the package. 
            (null, new IntSettingFactory()),            // override for default type string is null, resulting in the interface value being used.
            ("float", new FloatSettingFactory()),       // override for default type string is "float", meaning this setting will be used wherever the json type is "float"
            
            ("dropdown", new DropdownSettingFactory()), // the factory we created earlier, along with an override for the type string
            ...                                         // potential further custom settings factories
        );
    }
}
```
Note that the `LoadSettingsIntoAsset` method can override individual factories' target types. This allows using the same factory for multiple types of settings.
\
Last, create a JSON string for the settings you want to load and assign it to that `MonoBehaviour`, along with the Settings Asset.
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
This would first try to create a new group called `Graphics` with GUID `_graphics`, and then to make a setting from a hypothetical `dropdown` factory, give it a default value of `2` and try to set it up with an array of values. Assuming both were successful, the new setting would become a child of the new group.
\
If there already was a group with GUID `_graphics`, no new group will be created, and the setting will be added to the existing one instead.

## 4. Setting up a dynamic Setting
Sometimes, a default value alone is not enough to set up an external setting with. Because of that, the `SettingBase<T>` class allows you to override its `OnCreateWithValues(StringValuePair[])` method. Sticking with the class from the previous examples, you could use the `Key`s of all the `StringValuePair`s you get, to populate your `values`.
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
The Framewok does not provide a way to do this, but it has a tool to help:
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
    [SerializeField] private SettingsAsset asset;   // the asset to source the settings from
    [SerializeField] private SettingControlCollection prefabs;  // a collection of available setting control prefabs
    [SerializeField] private LayoutGroup parent;    // the layout group to parent the spawned setting controls to


    private void Start ()
    {
        asset.Initialize();    // initializing settings could be done somewhere else as well.
        SpawnSettings();
    }

    private void SpawnSettings ()
    {
        var settings = asset.GetAllSettings();
        foreach (var setting in settings)   // iterate over all registered settings
        {
            if (prefabs.TryGetControl(setting.GetType(), out SettingControl prefab))    // try get a SettingControl prefab matching the current setting
            {
                if (prefab.TryInstantiateWith (setting, out SettingControl control))     // try instantiating the found prefab with the given setting. If successful, this will automatically spawn and initialize the prefab.
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