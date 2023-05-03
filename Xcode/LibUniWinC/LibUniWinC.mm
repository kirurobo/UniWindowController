//
//  LibUniWinC on macOS
//
//  Author: Kirurobo
//  License: MIT
//  Created: 2019/06/22
//  Copyright © 2019-2020 kirurobo. All rights reserved.
//

#define UNIWINC_EXPORT extern "C"

#import <Foundation/Foundation.h>
#import "LibUniWinC-Swift.h"

// Actually the argument type is wchar_t*
using StringCallback = void(* _Nonnull)(const void* _Nonnull);

// Callback for display changed
using IntCallback = void(* _Nonnull)(const SInt32);


UNIWINC_EXPORT BOOL IsActive() {
    return [LibUniWinC isActive];
}

UNIWINC_EXPORT BOOL IsTransparent() {
    return [LibUniWinC isTransparent];
}

UNIWINC_EXPORT BOOL IsBorderless() {
    return [LibUniWinC isBorderless];
}

UNIWINC_EXPORT BOOL IsTopmost() {
    return [LibUniWinC isTopmost];
}

UNIWINC_EXPORT BOOL IsBottommost() {
    return [LibUniWinC isBottommost];
}

UNIWINC_EXPORT BOOL IsMaximized() {
    return [LibUniWinC isMaximized];
}

UNIWINC_EXPORT BOOL IsMinimized() {
    return [LibUniWinC isMinimized];
}

UNIWINC_EXPORT BOOL DetachWindow() {
    [LibUniWinC detachWindow];
    return true;
}

UNIWINC_EXPORT BOOL AttachMyWindow() {
    return [LibUniWinC attachMyWindow];
}

UNIWINC_EXPORT BOOL AttachMyOwnerWindow() {
    return [LibUniWinC attachMyWindow];
}

UNIWINC_EXPORT BOOL AttachMyActiveWindow() {
    return [LibUniWinC attachMyWindow];
}

UNIWINC_EXPORT void SetTransparent(BOOL isTransparent) {
    [LibUniWinC setTransparentWithIsTransparent:isTransparent];
}

UNIWINC_EXPORT void SetBorderless(BOOL isBorderless) {
    [LibUniWinC setBorderlessWithIsBorderless:isBorderless];
}

UNIWINC_EXPORT void SetAlphaValue(Float32 alpha) {
    [LibUniWinC setAlphaValueWithAlpha: alpha];
}

UNIWINC_EXPORT void SetTopmost(BOOL isTopmost) {
    [LibUniWinC setTopmostWithIsTopmost:isTopmost];
}

UNIWINC_EXPORT void SetBottommost(BOOL isBottommost) {
    [LibUniWinC setBottommostWithIsBottommost:isBottommost];
}

UNIWINC_EXPORT void SetMaximized(BOOL isZoomed) {
    [LibUniWinC setMaximizedWithIsZoomed:isZoomed];
}

UNIWINC_EXPORT void SetClickThrough(BOOL isTransparent) {
    [LibUniWinC setClickThroughWithIsTransparent:isTransparent];
}

UNIWINC_EXPORT BOOL SetPosition(Float32 x, Float32 y) {
    return [LibUniWinC setPositionWithX:x y:y];
}

UNIWINC_EXPORT BOOL GetPosition(Float32* x, Float32* y) {
    return [LibUniWinC getPositionWithX:x y:y];
}

UNIWINC_EXPORT BOOL SetSize(Float32 width, Float32 height) {
    return [LibUniWinC setSizeWithWidth:width height:height];
}

UNIWINC_EXPORT BOOL GetSize(Float32* width, Float32* height) {
    return [LibUniWinC getSizeWithWidth:width height:height];
}

UNIWINC_EXPORT BOOL GetClientSize(Float32* width, Float32* height) {
    return [LibUniWinC getClientSizeWithWidth:width height:height];
}

UNIWINC_EXPORT SInt32 GetCurrentMonitor() {
    return [LibUniWinC getCurrentMonitor];
}

UNIWINC_EXPORT SInt32 GetMonitorCount() {
    return [LibUniWinC getMonitorCount];
}

UNIWINC_EXPORT BOOL GetMonitorRectangle(SInt32 monitorIndex, Float32* x, Float32* y, Float32* width, Float32* height) {
    return [LibUniWinC getMonitorRectangleWithMonitorIndex:monitorIndex x:x y:y width:width height:height];
}

UNIWINC_EXPORT BOOL RegisterMonitorChangedCallback(IntCallback callback) {
    return [LibUniWinC registerMonitorChangedCallbackWithCallback: callback];
}

UNIWINC_EXPORT BOOL UnregisterMonitorChangedCallback() {
    return [LibUniWinC unregisterMonitorChangedCallback];
}

UNIWINC_EXPORT BOOL RegisterWindowStyleChangedCallback(IntCallback callback) {
    return [LibUniWinC registerWindowStyleChangedCallbackWithCallback: callback];
}

UNIWINC_EXPORT BOOL UnregisterWindowStyleChangedCallback() {
    return [LibUniWinC unregisterWindowStyleChangedCallback];
}

// コールバックにファイルはダブルクオーテーションで囲まれ改行区切りとなった文字列で渡ります。
//  e.g. "/Dir/File1.txt"\n"/Dir/File2.txt"\n"/Dir/File""3"".txt"\n
UNIWINC_EXPORT BOOL RegisterDropFilesCallback(StringCallback callback) {
    return [LibUniWinC registerDropFilesCallbackWithCallback: callback];
}

UNIWINC_EXPORT BOOL UnregisterDropFilesCallback() {
    return [LibUniWinC unregisterDropFilesCallback];
}

UNIWINC_EXPORT BOOL SetAllowDrop(BOOL enabled) {
    return [LibUniWinC setAllowDropWithEnabled: enabled];
}

UNIWINC_EXPORT BOOL SetCursorPosition(Float32 x, Float32 y) {
    return [LibUniWinC setCursorPositionWithX:x y:y];
}

UNIWINC_EXPORT BOOL GetCursorPosition(Float32* x, Float32* y) {
    return [LibUniWinC getCursorPositionWithX:x y:y];
}

// Call periodically to maintain window state.
UNIWINC_EXPORT void Update() {
    [LibUniWinC update];
}

UNIWINC_EXPORT BOOL OpenFilePanel(const void* lpPanelSettings, UniChar* lpwsBuffer, UInt32 bufferSize) {
    return [LibUniWinC openFilePanelWithLpSettings: lpPanelSettings lpBuffer:lpwsBuffer bufferSize: bufferSize];
}

UNIWINC_EXPORT BOOL OpenSavePanel(const void* lpPanelSettings, UniChar* lpwsBuffer, UInt32 bufferSize) {
    return [LibUniWinC openSavePanelWithLpSettings: lpPanelSettings lpBuffer: lpwsBuffer bufferSize: bufferSize];
}


// For Windows only (Nothing to do on Mac)
UNIWINC_EXPORT void SetTransparentType(SInt32 type) {
    [LibUniWinC setTransparentTypeWithType: type];
}

// For Windows only (Nothing to do on Mac)
UNIWINC_EXPORT void SetKeyColor(SInt32 color) {
    [LibUniWinC setKeyColorWithColor: color];
}

// For Windows only (Nothing to do on Mac)
UNIWINC_EXPORT BOOL AttachWindowHandle(UInt64 hwnd) {
    return [LibUniWinC attachWindowHandleWithHwnd: hwnd];
}


// For debugging
UNIWINC_EXPORT SInt32 GetDebugInfo() {
    return [LibUniWinC getDebugInfo];
}
