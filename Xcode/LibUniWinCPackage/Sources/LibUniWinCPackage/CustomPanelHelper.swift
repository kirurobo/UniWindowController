//
//  CustomPanelHelper.swift
//  LibUniWinC
//
//  Created by owner on 2021/11/28.
//  Copyright © 2021 kirurobo. All rights reserved.
//

import Cocoa
import AppKit

class CustomPanelHelper {
    public let panel : NSSavePanel
    let customAccessoryView = NSView(frame: NSRect(x:0, y:0, width:400, height:40))
    var popup = NSPopUpButton(frame: NSRect(x:80, y:5, width:310, height:25))
    var label = NSTextField(frame: NSRect(x: 10, y:3, width: 70, height:25))
    var hasSubView : Bool = false
    var extArray : [[String]?] = []
    
    init(panel: NSSavePanel)
    {
        self.panel = panel
        
        label.stringValue = "File type : "
        label.isBordered = false
        label.isSelectable = false
        label.isEditable = false
        label.backgroundColor = NSColor.clear
        
        popup.pullsDown = false
        popup.action = #selector(onFileTypeChanged(_:))
        popup.target = self
        
        let center = NotificationCenter.default
//        center.addObserver(self, selector: #selector(_didPanelExposeObserver(notification:)), name:NSWindow.didExposeNotification, object: panel)
        center.addObserver(self, selector: #selector(_willPanelCloseObserver(notification:)), name: NSWindow.willCloseNotification, object: panel)
    }
    
//    @objc func _didPanelExposeObserver(notification: Notification) {
//        //panel.accessoryView = customAccessoryView
//    }
                           
    @objc func _willPanelCloseObserver(notification: Notification) {
        // パネルを閉じた後に accessoryView が画面に残ってしまっていたため、削除を試みる
        customAccessoryView.removeFromSuperview()
        label.removeFromSuperview()
        popup.removeFromSuperview()
        panel.accessoryView = nil
        
        let center = NotificationCenter.default
        center.removeObserver(self, name: NSSavePanel.willCloseNotification, object: panel)
    }

    public func addFileType(title: String, ext: [String]?) -> Void {
        popup.addItem(withTitle: title)
        extArray.append(ext)
        
        // 初回ならばデフォルトとして選択し、subView 追加
        if (!hasSubView) {
            popup.selectItem(at: 0)
            panel.allowedFileTypes = extArray.first ?? nil
            
            customAccessoryView.addSubview(label)
            customAccessoryView.addSubview(popup)
            panel.accessoryView = customAccessoryView
            
            hasSubView = true;
        }
    }
    
    /// Add  filters for  allowedFileTypes
    ///  - Parameters:
    ///     - panel: Custom panel
    ///     - text: text = "TitleA(TAB)textA1(TAB)extA2(TAB)...extAn(LF)TitleB(TAB)extB1(TAB)extB2...extBn(LF)"
    public func addFileTypes(text: String) -> Void {
        let items = text.components(separatedBy: "\n")
        for item in items {
            let array = Array(item.components(separatedBy: "\t"))
            
            // タイトルと拡張子で要素は２以上必要
            if (array.count > 1) {
                // "*" があれば拡張子指定なし（全てのファイル）とする
                self.addFileType(
                    title: array.first!,
                    ext: (array.contains("*") ? nil : Array(array.dropFirst()))
                )
            }
        }
    }
    
    /// Apply a file type filter
    @objc func onFileTypeChanged(_ sender: Any?) {
        panel.allowedFileTypes = extArray[popup.indexOfSelectedItem]
    }
}
