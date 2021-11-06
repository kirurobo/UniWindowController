//
//  DebugUniWinCApp.swift
//  DebugUniWinC
//
//  Created by Kirurobo on 2021/11/05.
//  Copyright © 2021 kirurobo. All rights reserved.
//

import SwiftUI

@main
struct DebugUniWinCApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate
    
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}
