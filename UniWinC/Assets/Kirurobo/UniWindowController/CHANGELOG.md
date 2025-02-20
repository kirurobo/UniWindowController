# Changelog

UniWindowController (UniWinC)
https://github.com/kirurobo/UniWindowController

<!---
How to write the changelog.
https://keepachangelog.com/ja/1.0.0/
--->

## [v0.9.6] - 2025-02-20
### Changed
- Replaced FindObjectOfType with FindAnyObjectByType.
### Fixed
- Fixed coordinate misalignment with Retina support on macOS.

## [v0.9.5] - 2025-02-18
### Changed
- Renamed asmdef Unity.UniWindowController to Kirurobo.UniWindowController.
- Renamed asmdef Unity.UniWindowController.Editor to Kirurobo.UniWindowController.Editor.
  - Please remove if there are old files in the project.
### Fixed
- Fixed for support both of New Input System and Legacy Input Manager.
- Fixed wrong name "LeftCtrl" and "RightCtrl" in the code.
### Added
- Added GetClientRectangle() in the native plugins.

## [v0.9.4] - 2025-02-06
### Changed
- Support New Input System.
### Fixed
- Remember main camera's clear flags and background color before applying automatic camera background switch.
- To prevent errors on macOS, the save dialogue no longer displays a file type drop-down.
- Fixed a crash when setting the window to borderless on macOS if the screen was initially in full screen mode.

## [v0.9.3] - 2024-05-06
### Changed
- Rewrote the .bundle in Swift

## [v0.9.2] - 2023-09-18
### Fixed
- DllNotFoundException in the Unity Apple Silicon Editor
### Changed
- Added client size display to the UI sample

## [v0.9.1] - 2023-05-03
### Fixed
- Position of file type selection box on macOS
- GetClientSize() on macOS 

## [v0.9.0] - 2023-04-22
### Changed
- The development environment has been updated to Unity 2020.3.43
### Fixed
- Fixed size shift when window frame is hidden in Unity 2020 on Windows

## [v0.8.6] - 2022-06-18
### Fixed
- Window shadow in macOS

## [v0.8.5] - 2021-12-12
### Fixed
- File type selection in macOS

## [v0.8.4] - 2021-11-27
### Changed
- Made the class singleton.
- All samples are bundled for package manager.

### Added
- File type selection in macOS

## [v0.8.3] - 2021-11-27
### Added
- SetAlphaValue

## [v0.8.2] - 2021-10-15
### Added
- FilePanel.OpenFilePanel()
- FilePanel.SaveFilePanel

### Fixed
- M1 may also be supported in macOS. (Not tested)
- Minor improvements to an issue of lost keystrokes when the window is transparent.


## [v0.8.1] - 2021-09-13
### Changed
- ***Renamed "Unity" folder to "UniWinC".***

for macOS.
- Use screen.frame intead of screen.visibleFrame.
- Use NSWindow.Level.popUpMenu instead of Level.floating to bring the window to the front of the menu bar.


## [v0.8.0] - 2020-12-27
### Added
- Fullscreen demo.
- Set to bottommost. (Experimental)


## [v0.7.0] - 2020-12-07
### Added
- Support Unity Package Manager.

### Changed
- Restructured folders.


## [v0.6.0] - 2020-12-06
### Added
- Files dropping for Mac and Windows.
- Prepare fit to monitor property.

### Changed
- macOS 10.12 and below is no longer supported.
- "Maximized" keyword was renamed to "Zoomed".

