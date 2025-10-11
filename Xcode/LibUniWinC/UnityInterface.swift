//
//  UnityInterface.swift
//
//  Author: Kirurobo
//  License: MIT
//  Created: 2023/09/19
//  Copyright © 2023 kirurobo.
//


// Actually the argument type is wchar_t*
//using StringCallback = void(* _Nonnull)(const void* _Nonnull);

// Callback for display changed
//using IntCallback = void(* _Nonnull)(const SInt32);

@_cdecl("IsActive")
public func IsActive() -> Bool {
    return LibUniWinC.isActive()
}


@_cdecl("IsTransparent")
public func IsTransparent() -> Bool {
    return LibUniWinC.isTransparent()
}

@_cdecl("IsBorderless")
public func IsBorderless() -> Bool {
    return LibUniWinC.isBorderless()
}

@_cdecl("IsTopmost")
public func IsTopmost() -> Bool {
    return LibUniWinC.isTopmost()
}

@_cdecl("IsBottommost")
public func IsBottommost() -> Bool {
    return LibUniWinC.isBottommost()
}

@_cdecl("IsMaximized")
public func IsMaximized() -> Bool {
    return LibUniWinC.isMaximized()
}

@_cdecl("IsMinimized")
public func IsMinimized() -> Bool {
    return LibUniWinC.isMinimized()
}

@_cdecl("IsFreePositioningEnabled")
public func IsFreePositioningEnabled() -> Bool {
    return LibUniWinC.isFreePositioningEnabled()
}

@_cdecl("DetachWindow")
public func DetachWindow() -> Bool {
    LibUniWinC.detachWindow()
    return true;
}

@_cdecl("AttachMyWindow")
public func AttachMyWindow() -> Bool {
    return LibUniWinC.attachMyWindow()
}

@_cdecl("AttachMyOwnerWindow")
public func AttachMyOwnerWindow() -> Bool {
    return LibUniWinC.attachMyWindow()
}

@_cdecl("AttachMyActiveWindow")
public func AttachMyActiveWindow() -> Bool {
    return LibUniWinC.attachMyWindow()
}

@_cdecl("SetTransparent")
public func SetTransparent(isTransparent: Bool) -> Void {
    return LibUniWinC.setTransparent(isTransparent: isTransparent)
}

@_cdecl("SetBorderless")
public func SetBorderless(isBorderless: Bool) -> Void {
    return LibUniWinC.setBorderless(isBorderless: isBorderless)
}

@_cdecl("SetAlphaValue")
public func SetAlphaValue(alpha: Float32) -> Void {
    LibUniWinC.setAlphaValue(alpha: alpha)
}

@_cdecl("SetTopmost")
public func SetTopmost(isTopmost: Bool) -> Void {
    LibUniWinC.setTopmost(isTopmost: isTopmost)
}

@_cdecl("SetBottommost")
public func SetBottommost(isBottommost: Bool) -> Void {
    LibUniWinC.setBottommost(isBottommost: isBottommost)
}

@_cdecl("SetMaximized")
public func SetMaximized(isZoomed: Bool) -> Void {
    LibUniWinC.setMaximized(isZoomed: isZoomed)
}

@_cdecl("SetClickThrough")
public func SetClickThrough(isTransparent: Bool) -> Void {
    LibUniWinC.setClickThrough(isTransparent: isTransparent)
}

@_cdecl("EnableFreePositioning")
public func EnableFreePositioning(isFree: Bool) -> Void {
    LibUniWinC.enableFreePositioning(enabled: isFree)
}

@_cdecl("SetPosition")
public func SetPosition(x: Float32, y: Float32) -> Bool {
    return LibUniWinC.setPosition(x: x, y: y)
}

@_cdecl("GetPosition")
public func GetPosition(x: UnsafeMutablePointer<Float32>, y: UnsafeMutablePointer<Float32>) -> Bool {
    return LibUniWinC.getPosition(x: x, y: y)
}

@_cdecl("SetSize")
public func SetSize(width: Float32, height: Float32) -> Bool {
    return LibUniWinC.setSize(width: width, height: height)
}

@_cdecl("GetSize")
public func GetSize(width: UnsafeMutablePointer<Float32>, height: UnsafeMutablePointer<Float32>) -> Bool {
    return LibUniWinC.getSize(width:width, height:height)
}

@_cdecl("GetClientSize")
public func GetClientSize(width: UnsafeMutablePointer<Float32>, height: UnsafeMutablePointer<Float32>) -> Bool {
    return LibUniWinC.getClientSize(width:width, height:height)
}

@_cdecl("GetClientRectangle")
public func GetClientRectangle(x: UnsafeMutablePointer<Float32>, y: UnsafeMutablePointer<Float32>, width: UnsafeMutablePointer<Float32>, height: UnsafeMutablePointer<Float32>) -> Bool {
    return LibUniWinC.getClientRectangle(x: x, y: y, width:width, height:height)
}

@_cdecl("GetCurrentMonitor")
public func GetCurrentMonitor() -> Int32 {
    return LibUniWinC.getCurrentMonitor()
}

@_cdecl("GetMonitorCount")
public func GetMonitorCount() -> Int32 {
    return LibUniWinC.getMonitorCount()
}

@_cdecl("GetMonitorRectangle")
public func GetMonitorRectangle(
    monitorIndex: Int32,
    x: UnsafeMutablePointer<Float32>, y: UnsafeMutablePointer<Float32>,
    width: UnsafeMutablePointer<Float32>, height: UnsafeMutablePointer<Float32>
) -> Bool {
    return LibUniWinC.getMonitorRectangle(monitorIndex:monitorIndex, x:x, y:y, width:width, height:height)
}

@_cdecl("RegisterMonitorChangedCallback")
public func RegisterMonitorChangedCallback(callback: LibUniWinC.intCallback) -> Bool {
    return LibUniWinC.registerMonitorChangedCallback(callback: callback)
}

@_cdecl("UnregisterMonitorChangedCallback")
public func UnregisterMonitorChangedCallback() -> Bool {
    return LibUniWinC.unregisterMonitorChangedCallback()
}

@_cdecl("RegisterWindowStyleChangedCallback")
public func RegisterWindowStyleChangedCallback(callback: LibUniWinC.intCallback) -> Bool {
    return LibUniWinC.registerWindowStyleChangedCallback(callback: callback)
}

@_cdecl("UnregisterWindowStyleChangedCallback")
public func UnregisterWindowStyleChangedCallback() -> Bool {
    return LibUniWinC.unregisterWindowStyleChangedCallback()
}

// コールバックにファイルはダブルクオーテーションで囲まれ改行区切りとなった文字列で渡ります。
//  e.g. "/Dir/File1.txt"\n"/Dir/File2.txt"\n"/Dir/File""3"".txt"\n
@_cdecl("RegisterDropFilesCallback")
public func RegisterDropFilesCallback(callback: LibUniWinC.stringCallback) -> Bool {
    return LibUniWinC.registerDropFilesCallback(callback: callback)
}

@_cdecl("UnregisterDropFilesCallback")
public func UnregisterDropFilesCallback() -> Bool {
    return LibUniWinC.unregisterDropFilesCallback()
}

@_cdecl("SetAllowDrop")
public func SetAllowDrop(enabled: Bool) -> Bool {
    return LibUniWinC.setAllowDrop(enabled: enabled)
}

@_cdecl("SetCursorPosition")
public func SetCursorPosition(x: Float32, y: Float32) -> Bool {
    return LibUniWinC.setCursorPosition(x:x, y:y)
}

@_cdecl("GetCursorPosition")
public func GetCursorPosition(x: UnsafeMutablePointer<Float32>, y: UnsafeMutablePointer<Float32>) -> Bool {
    return LibUniWinC.getCursorPosition(x:x, y:y)
}

@_cdecl("GetMouseButtons")
public func GetMouseButtons() -> Int32 {
    return LibUniWinC.getMouseButtons()
}

@_cdecl("GetModifierKeys")
public func GetModifierKeys() -> Int32 {
    return LibUniWinC.getModifierKeys()
}

// Call periodically to maintain window state.
@_cdecl("Update")
public func Update() -> Void {
    LibUniWinC.update()
}

@_cdecl("OpenFilePanel")
public func OpenFilePanel(lpSettings: UnsafeRawPointer, lpBuffer: UnsafeMutablePointer<UInt16>?, bufferSize: UInt32) -> Bool {
    return LibUniWinC.openFilePanel(lpSettings: lpSettings, lpBuffer:lpBuffer, bufferSize: bufferSize)
}

@_cdecl("OpenSavePanel")
public func OpenSavePanel(lpSettings: UnsafeRawPointer, lpBuffer: UnsafeMutablePointer<UInt16>?, bufferSize: UInt32) -> Bool {
    return LibUniWinC.openSavePanel(lpSettings: lpSettings, lpBuffer: lpBuffer, bufferSize: bufferSize)
}


// For Windows only (Nothing to do on Mac)
@_cdecl("SetTransparentType")
public func SetTransparentType(type: Int32) -> Void {
    LibUniWinC.setTransparentType(type: type)
}

// For Windows only (Nothing to do on Mac)
@_cdecl("SetKeyColor")
public func SetKeyColor(color: Int32) -> Void {
    return LibUniWinC.setKeyColor(color: color)
}

// For Windows only (Nothing to do on Mac)
@_cdecl("AttachWindowHandle")
public func AttachWindowHandle(hwnd: UInt64) -> Bool {
    return LibUniWinC.attachWindowHandle(hwnd: hwnd)
}


// For debugging
@_cdecl("GetDebugInfo")
public func GetDebugInfo() -> Int32 {
    return LibUniWinC.getDebugInfo()
}
