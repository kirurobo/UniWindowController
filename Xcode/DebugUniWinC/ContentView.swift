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

struct ContentView: View {
    @State private var messageText = "Window"
    @State private var outputText = ""
    @State private var window: NSWindow?
    
    var body: some View {
        Text("Window info").padding()
        
        Button(action: {
//            if (LibUniWinC.attachMyWindow()) {
//                //LibUniWinC.setTransparent(isTransparent: true)
//                messageText = "IsActive: " + String(LibUniWinC.isActive())
//            } else {
//                messageText = "Can't attach"
//            }
            
            let dict = getWindowList()
            
            outputText = String(findWindowNumber(dict: dict, name: "DebugUniWinC"))
            window = findWindow(dict: dict, name: "DebugUniWinC")
            if (window != nil) {
                outputText = "Class Name: " + window!.className
                LibUniWinC._attachWindow(window: window!)
            } else {
                outputText = "Window is nil"
            }
            messageText = outputText
            outputText = toString(dict: dict) + getAllWindows()
        }){ Text(messageText) }
        
        ScrollView([.vertical, .horizontal]) {
            Text(outputText)
        }
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
