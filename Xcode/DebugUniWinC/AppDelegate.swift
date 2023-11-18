//
//  AppDelegate.swift
//  DebugUniWinC
//
//  Created by Kirurobo on 2021/11/06.
//  Copyright © 2021 kirurobo. All rights reserved.
//

import Cocoa

class AppDelegate: NSObject, NSApplicationDelegate {
    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        return true
    }
}
