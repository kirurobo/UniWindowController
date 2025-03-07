//
//  ContentView.swift
//  DebugUniWinC
//
//  Created by Kirurobo on 2021/11/05.
//  Copyright © 2021 kirurobo. All rights reserved.
//

import SwiftUI

// 参考 https://qiita.com/usagimaru/items/6ffd09c5b27042281108
func getWindowList() -> [NSDictionary]? {
    guard let windowList: NSArray = CGWindowListCopyWindowInfo(.optionOnScreenOnly, kCGNullWindowID) else {
        return nil
    }
    let swiftWindowList = windowList as! [NSDictionary]
    return swiftWindowList
}

func toString(dict: [NSDictionary]?) -> String {
    guard let dic = dict else {
        return ""
    }
    return (dic.compactMap({ (d) -> String in
        (d.compactMap({ (key, val) -> String in
            return "\(key)=\(val)"
        }) as Array).joined(separator: "\n") })).joined(separator: "\n\n")
}

func findWindow(dict: [NSDictionary]?, name: String) -> NSWindow? {
    if (dict == nil) {
        return nil
    }
    
    var result:NSWindow? = nil
    for dic in dict! {
        if ((dic[kCGWindowOwnerName] as! String) == name) {
            let num = (dic[kCGWindowNumber] as! Int)
            result = NSApp.window(withWindowNumber: num)
            break
        }
    }
    return result
}

func findWindowNumber(dict: [NSDictionary]?, name: String) -> Int {
    if (dict == nil) {
        return 0
    }
    
    var result:Int = 0
    for dic in dict! {
        if ((dic[kCGWindowOwnerName] as! String) == name) {
            let num = (dic[kCGWindowNumber] as! Int)
            result = num
            break
        }
    }
    return result
}

func getAllWindows() -> String {
    var text = ""
    let windows = NSWindow.windowNumbers(options: NSWindow.NumberListOptions.allApplications)
    for num in windows! {
        text = text + num.stringValue + ", "
    }
    return text
}

func getOpenFileNames() -> String {
    let bufferSize = 1024
    let buffer = UnsafeMutablePointer<UniChar>.allocate(capacity: bufferSize)
//    let settings = LibUniWinC.PanelSettings()
//    let lpSettings = UnsafeRawPointer<Void>(&settings)
//    
//    LibUniWinC.openSavePanel(lpSettings: lpSettings, lpBuffer: buffer, bufferSize: UInt32(bufferSize))
    buffer.deallocate()
    return ""
}

struct ContentView: View {
    @State private var buttonText = "Get all windows"
    @State private var outputText = ""
    @State private var modifiersText = "None"
    @State private var window: NSWindow?

    var body: some View {
        Text("File dialog test")
        Button(action: {
            let title = Array("Select file\0".utf16)
            let filter = Array("All files\t*\nImages (png, jpg, tiff)\tpng\tjpg\tjpeg\ttiff\n\0".utf16)
            title.withUnsafeBufferPointer { (titlePtr: UnsafeBufferPointer<UInt16>) in
                filter.withUnsafeBufferPointer { (filterPtr: UnsafeBufferPointer<UInt16>) in
                    var settings = LibUniWinC.PanelSettings(
                        structSize: Int32(MemoryLayout<LibUniWinC.PanelSettings>.size),
                        flags: 0,
                        titleText: titlePtr.baseAddress,
                        filterText: filterPtr.baseAddress,
                        initialFile: nil,
                        initialDirectory: nil,
                        defaultExt: nil
                    )
                    let bufferSize = 2048
                    let buffer = UnsafeMutablePointer<UniChar>.allocate(capacity: bufferSize)
                    buffer.initialize(repeating: UniChar.zero, count: bufferSize)
                    _ = LibUniWinC.openFilePanel(lpSettings: &settings, lpBuffer: buffer, bufferSize: UInt32(bufferSize))
                }
            }
        }){ Text("Open") }

        Button(action: {
            let title = Array("No save is actually performed\0".utf16)
            let filter = Array("Text file (txt)\ttxt\nImages　(png, jpg, tiff)\tpng\tjpg\tjpeg\ttiff\nAll files\t*\n\0".utf16)

            title.withUnsafeBufferPointer { (titlePtr: UnsafeBufferPointer<UInt16>) in
                filter.withUnsafeBufferPointer { (filterPtr: UnsafeBufferPointer<UInt16>) in
                    var settings = LibUniWinC.PanelSettings(
                        structSize: Int32(MemoryLayout<LibUniWinC.PanelSettings>.size),
                        flags: 0,
                        titleText: titlePtr.baseAddress,
                        filterText: filterPtr.baseAddress,                        
                        initialFile: nil,
                        initialDirectory: nil,
                        defaultExt: nil
                    )
                    let bufferSize = 2048
                    let buffer = UnsafeMutablePointer<UniChar>.allocate(capacity: bufferSize)
                    buffer.initialize(repeating: UniChar.zero, count: bufferSize)
                    _ = LibUniWinC.openSavePanel(lpSettings: &settings, lpBuffer: buffer, bufferSize: UInt32(bufferSize))
                }
            }
        }){ Text("Save") }
        
        // v0.9.7- 追加されたGetModifierKeys()のテスト
        Button(action: {
            let keys = LibUniWinC.getModifierKeys()
            if (keys == 0) {
                modifiersText = "None"
            } else {
                modifiersText = (keys & 1 != 0 ? "Option " : "") + (keys & 2 != 0 ? "Control " : "") + (keys & 4 != 0 ? "Shift " : "") + (keys & 8 != 0 ? "Command " : "")
            }
        }) { Text("Show modifier keys when clicked") }
        Text(modifiersText)

    
        Text("Window Info.").padding()
        
        Button(action: {
            let dict = getWindowList()
            
            buttonText = String(findWindowNumber(dict: dict, name: "DebugUniWinC"))
            window = findWindow(dict: dict, name: "DebugUniWinC")
            if (window != nil) {
                buttonText = "Attached class: " + window!.className
                LibUniWinC._attachWindow(window: window!)
                
                outputText = "Title : " + window!.title
                + "\nStyleMask : " +  window!.styleMask.rawValue.description
                + "\nFrame : " + window!.frame.debugDescription
                + "\nIsKeyWindow : " + window!.isKeyWindow.description
                + "\nIsZoomed : " + window!.isZoomed.description
                + "\nCanHide : " + window!.canHide.description
                + "\nIsOpaque : " + window!.isOpaque.description
                + "\nHasShadow : " + window!.hasShadow.description
                + "\nIsSheet : " + window!.isSheet.description
                + "\nOcclusionState : " + window!.occlusionState.rawValue.description
                + "\n\n"
                
                window!.hasShadow = false

            } else {
                buttonText = "Current window is nil"
                outputText = ""
            }

            outputText += toString(dict: dict) + getAllWindows()
        }){ Text(buttonText) }
        
        ScrollView([.vertical, .horizontal]) {
            Text(outputText)
        }
    }
}

#Preview {
    ContentView()
}
