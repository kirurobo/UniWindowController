# Changelog

UniWindowController (UniWinC)
https://github.com/kirurobo/UniWindowController

<!---
How to write the changelog.
https://keepachangelog.com/ja/1.0.0/
--->

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

