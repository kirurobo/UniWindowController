# UniWindowController
Unified window controller for macOS and Windows  
Abbreviation：UniWinC

[![license](https://img.shields.io/badge/license-MIT-green.svg?style=flat)](https://github.com/kirurobo/UniWindowController/blob/master/LICENSE)

### README
- [Japanese (日本語での説明)](README-ja.md)
- [English](README.md)



## Overview
This is a library for apps built for Windows / macOS standalone with Unity.  
This library controls transparency, borderless, position, size, etc. of its own window.  
And it allows also accept file and folder drops.

![uniwinc](https://user-images.githubusercontent.com/1019117/96070514-5284e580-0edb-11eb-8a4d-d990a0a028a8.gif)  
https://twitter.com/i/status/1314440790945361920


## Demo
You can find some sample builts on the [Release page](https://github.com/kirurobo/UniWindowController/releases).


## Installation
If you use the UPM, you can also perform version upgrades from UPM.

A. Using the Unity Package Manager (UPM)
1. Open [Package Manager] from the [Window] menu of the Unity Editor.
2. Select [+] and then select [Add package from git URL...].  
    ![image](https://user-images.githubusercontent.com/1019117/234160406-f041bda9-262c-4d3f-b41c-45e11c3a94ce.png)
3. Enter https://github.com/kirurobo/UniWindowController.git#upm and [Add].  
    ![image](https://user-images.githubusercontent.com/1019117/234160520-35447b67-dd44-4af6-9c7c-ab71577a4c17.png)


B. Using an UnityPackage
1. Download an .unitypackage file from the [Release page](https://github.com/kirurobo/UniWindowController/releases).
2. Import the asset in the Unity Editor.


## Use in your Unity project
1. Add the `UniWindowController` prefab in the Runtime/Prefabs to your scene.
2. Select the `UniWindowController` placed scene, and watch the inspector.
  - Fix the Player Settings appropriately (the green button will change all settings at once)
  - Adjust the settings such as `IsTransparent` to your liking
3. Add `DragMoveCanvas` prefab in the Runtime/Prefabs if you want to move the window by mouse dragging.
  - An EventSystem is required for this to work. If it is not present in your scene, add UI → Event System.
4. Build for PC / Mac standalone
5. Launch the build


## Limitations
- Transparency is not available on the Unity Editor. Please build and try it.
  - It works for topmost, moving windows, etc., but I do not recommend closing the game view or changing the docking arrangement while it is running. In the meantime, the window will reacquire when the focus is shifted to the game view.
- The proper support for touch operations has not yet been determined.
  - On Windows, if you change the `TransparentType` from Alpha to ColorKey, you will lose the beautiful translucency, but the touch operation will be natural.
- Multiple windows are not supported.
- This has not been fully tested and there may be unstable behavior.

See also [Issues](https://github.com/kirurobo/UniWindowController/issues) for known issues.


## System requirements
- Unity: 2019 4.31f1 or later
  - Scripting Runtime: .NET 4.x or later
- OS: Windows 10, Windows 11 or macOS

Development environment is Unity 2020.4.30f1, Windows 11 / macOS 13.3


## Additional information

### About the hit test
When the window is successfully made transparent, it looks as if it is a non-rectangular window.  
However, this is only an appearance, and the window actually exists as a rectangular window.  
Therefore, by looking directly under the mouse cursor, if it is transparent, the mouse operation is passed to the window below (click-through), and if it is opaque, the mouse operation is returned to normal,
If it is opaque, it returns to normal.

Two types of hit tests are available. (You can also choose to disable the automatic hit test and control it yourself or not.)

| Name | Method | Note |
|:-----|:-----|:------------|
|Opacity|Check transparency|Matches appearance and is natural, but heavy processing|
|Raycast|Check colider|Lightweight, but requires coliders|

The Raycast method is recommended in terms of performance, but if you forget colider, you will not be able to touch the screen, so the default is Opacity.

Also, note that touch operation may feel uncomfortable because you cannot see the color under your finger in advance.  
Since we have not found the best solution for this, I'm sorry to say that touch support has been put on the back burner.


### About the transparency method (Seceltable only on Windows)
One way to support touch operation is to select monochromatic transparency for layered windows.  
If this is selected, semi-transparency cannot be expressed and performance will be reduced, but since the hit test is left to Windows, it should match your senses for touch operation.  

| Name | Description | Note |
|:-----|:-----|:------------|
|Alpha|Reflects transparency of rendering results|This is standard|
|ColorKey|Only one color with matching RGB is transparent|Poor performance, but touch is natural|


### C# scripting
This is something that can be manipulated from other scripts in Unity.  
Specifications are not finalized and are subject to change.


#### UniWindowController.cs
This is the main script.
The following properties can be manipulated from other scripts. (Other properties may be added.)

| Name | Type | Description |
|:-----|:-----|:------------|
|isTransparent|bool| Set/unset for transparent (non-rectangular) windows|
|isTopmost |bool| Always set/unset to topmost|
|isZoomed |bool| Maximize/unmaximize the window. Also, get the current state |
|isHitTestEnabled|bool| Enables/disables the automatic hit test. If enabled, isClickThrough will automatically change depending on the mouse cursor position. |
|isClickThrough|bool| Sets/unset the click-through state.|
|windowPosition|Vector2| Allows you to get/set the window position. The lower left corner of the main monitor is the origin and the coordinate system is positive upward, and the lower left corner of the window is the coordinate system.|
|windowSize|Vector2| You can get/set the window size.|


#### UniWindowMoveHandler.cs
If you attach this script to a UI element (which will be the Raycast Target), you can move the window by dragging that UI element.
For example, it is assumed to be attached to an image with a handle that says, "You can move by grabbing here.

Within the prefab called DragMoveCanvas, we use a Panel that covers the entire transparent screen. 
By setting the Layer to "Ignore Raycast", the automatic hit test will be excluded even if the panel is a Raycast.  
This allows dragging anywhere on the screen.  
However, other UI operations will take precedence over dragging. (This is due to the smaller Sort Order in DragMoveCanvas.)


### Source folder hierarchy
If you just want to use this library, you can download .unitypackage in Release and do not need to clone this repository.  
If you want to see/build the source, please refer to this.

- UniWinC
  - This is a Unity project.
  - It already contains built DLLs and bundles.
  - The contents of this project are in the release as the .unitypackage.
- VisualStudio
  - There is a solution to generate LibUniWinC.dll for Windows x86 and x64.
  - Building with Release will overwrite the DLL under the Unity folder.
  - A Windows Forms app project for testing is also included.
- Xcode
  - There is a project to generate LibUniWinC.bundle for macOS.
  - Building it will overwrite the .bundle under the Unity folder.


## Acknowledgements
- The macOS code is based on [Unity + Mac + Swift で透過最前面ウィンドウを作る](https://qiita.com/KRiver1/items/9ecf65759cf1349f56af) by かりばぁ.  
- I used hecomi's [Unity で .unitypackage で配布していたアセットを Package Manager 対応してみた](https://tips.hecomi.com/entry/2021/10/29/001304) for generating UPM branches in GitHub Actions.

I would like to thank them.

