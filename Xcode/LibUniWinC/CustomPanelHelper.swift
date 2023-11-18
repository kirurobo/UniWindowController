//
//  CustomPanelHelper.swift
//  LibUniWinC
//
//  Created by owner on 2021/11/28.
//  Copyright © 2021 kirurobo. All rights reserved.
//

import Cocoa
import AppKit
import UniformTypeIdentifiers

class CustomPanelHelper {
    public let panel : NSSavePanel
    let customAccessoryView = NSView(frame: NSRect(x:0, y:0, width:400, height:40))
    var popup = NSPopUpButton(frame: NSRect(x:80, y:5, width:310, height:25))
    var label = NSTextField(frame: NSRect(x: 10, y:3, width: 70, height:25))
    var hasSubView : Bool = false
    var extArray : [[String]?] = []
    var extUTTypes : [[UTType]?] = []
    
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
        center.addObserver(self, selector: #selector(_willPanelCloseObserver(notification:)), name: NSWindow.willCloseNotification, object: panel)
    }
                               
    @objc func _willPanelCloseObserver(notification: Notification) {
        // パネルを閉じた後に accessoryView が画面に残ってしまっていたため、削除を試みる
        customAccessoryView.removeFromSuperview()
        label.removeFromSuperview()
        popup.removeFromSuperview()
        panel.accessoryView = nil
        
        let center = NotificationCenter.default
        center.removeObserver(self, name: NSSavePanel.willCloseNotification, object: panel)
    }

    /// 文字列で指定された拡張子の組みを候補に追加
    public func addFileType(title: String, extensions: [String]?) -> Void {
        popup.addItem(withTitle: title)
        extArray.append(extensions)
        extUTTypes.append(createContentType(extensitons: extensions))
        
        // 初回ならばデフォルトとして選択し、subView 追加
        if (!hasSubView) {
            popup.selectItem(at: 0)
            if #available(macOS 11.0, *) {
                panel.allowedContentTypes = [.jpeg, .png]
            } else {
                panel.allowedFileTypes = extArray.first ?? nil
            }
            
            customAccessoryView.addSubview(label)
            customAccessoryView.addSubview(popup)
            panel.accessoryView = customAccessoryView
            
            hasSubView = true;
        }
    }
    
    /// 文字列で渡された拡張子群をUTTypeに変換
    /// allowedFileTypes ではなく allowedContentTypes を利用するため。
    ///  - Parameters:
    ///      - ext: 拡張子文字列
    ///  - Returns:
    ///      - UTTypeに変換後の配列。nilなら任意を意味する
    private func createContentType(extensitons: [String]?) -> [UTType]? {
        if (extensitons == nil) {
            return nil
        }
        
        var result: [UTType] = []
        for ext in extensitons! {
            if (ext != "") {
                let type = UTType(tag: ext, tagClass: .filenameExtension, conformingTo: nil)
                if (type != nil) {
                    result.append(type!)
                }
            }
        }
        if (result.count > 0) {
            return result
        } else {
            return nil
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
                    extensions: (array.contains("*") ? nil : Array(array.dropFirst()))
                )
            }
        }
    }
    
    /// Apply a file type filter
    @objc func onFileTypeChanged(_ sender: Any?) {
        if #available(macOS 11.0, *) {
            let type = extUTTypes[popup.indexOfSelectedItem]
            if (type == nil) {
                // ファイルタイプ指定が nil なら、任意の種類を許可する
                panel.allowsOtherFileTypes = true
                panel.allowedContentTypes = []
            } else {
                // ファイルタイプ指定があればそれのみ許可とする
                panel.allowsOtherFileTypes = false
                panel.allowedContentTypes = type!
            }
        } else {
            panel.allowedFileTypes = extArray[popup.indexOfSelectedItem]
        }
    }
}
