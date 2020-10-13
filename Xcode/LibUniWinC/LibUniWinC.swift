//
// Unified Window Controller macOS plugin
//
// Author: Kirurobo
// License: MIT
//
// Acknowledgement:
//  This code is based on transparent.swift created by kriver1 on 2018/05/23.
//  https://qiita.com/KRiver1/items/9ecf65759cf1349f56af
//
// References:
// - https://qiita.com/KRiver1/items/9ecf65759cf1349f56af
// - http://tatsudoya.blog.fc2.com/blog-entry-244.html
// - https://qiita.com/mybdesign/items/fe3e390741799c1814ad
// - https://blog.fenrir-inc.com/jp/2011/07/nsview_uiview.html
//

import Foundation
import Cocoa

/// Window controller main logic
@objcMembers
public class LibUniWinC : NSObject {
    
    // 現在の状態を保持する構造体
    private struct State {
        public var isReady: Bool = false
        public var isTopmost: Bool = false
        public var isBorderless: Bool = false
        public var isTransparent: Bool = false
    }
    
    private class OriginalWindowInfo {
        /// 元々のStyleMaskをここに記憶
        public var StyleMask: NSWindow.StyleMask = []
        
        /// 元々のCollectionBehavior
        public var CollectionBehavior: NSWindow.CollectionBehavior = []

        /// 元々のウィンドウLevel
        public var Level: NSWindow.Level = NSWindow.Level.normal
        
        public var titlebarAppearsTransparent: Bool = false
        public var titleVisibility: NSWindow.TitleVisibility = NSWindow.TitleVisibility.visible
        public var backgroundColor: NSColor = NSColor.clear
        public var isOpaque: Bool = true
        public var hasShadow: Bool = true
        public var contentViewWantsLayer: Bool = true
        public var contentViewLayerIsOpaque: Bool = true
        public var contentViewLayerBackgroundColor: CGColor? = CGColor.clear

//        public init() {
//            self.StyleMask = []
//            self.CollectionBehavior = []
//            self.Level = NSWindow.Level.normal
//            self.titlebarAppearsTransparent = false
//            self.titleVisibility = NSWindow.TitleVisibility.visible
//
//            window.backgroundColor = NSColor.clear
//            window.isOpaque = true
//            window.hasShadow = true
//        }
        
        /// 指定ウィンドウの初期値を記憶
        public func Store(window: NSWindow) -> Void {
            self.CollectionBehavior = window.collectionBehavior
            self.StyleMask = window.styleMask
            self.Level = window.level
            self.titlebarAppearsTransparent = window.titlebarAppearsTransparent
            self.titleVisibility = window.titleVisibility
            self.backgroundColor = window.backgroundColor
            self.isOpaque = window.isOpaque
            self.hasShadow = window.hasShadow
            
            if let view = window.contentView {
                self.contentViewWantsLayer = view.wantsLayer
                if let layer = view.layer {
                    self.contentViewLayerIsOpaque = layer.isOpaque
                    self.contentViewLayerBackgroundColor = layer.backgroundColor
                }
            }
        }
        
        /// 指定ウィンドウの状態を初期値に戻す
        public func Restore(window: NSWindow) -> Void {
            window.collectionBehavior = self.CollectionBehavior
            window.styleMask = self.StyleMask
            window.level = self.Level
            window.titlebarAppearsTransparent = self.titlebarAppearsTransparent
            window.titleVisibility = self.titleVisibility
            window.backgroundColor = self.backgroundColor
            window.isOpaque = self.isOpaque
            window.hasShadow = self.hasShadow
            
            window.contentView?.wantsLayer = self.contentViewWantsLayer
            window.contentView?.layer?.isOpaque = self.contentViewLayerIsOpaque
            window.contentView?.layer?.backgroundColor = self.contentViewLayerBackgroundColor
        }
    }
    
    // MARK: - Static variables
    
    /// 操作対象となるウィンドウ。nilだと未指定
    private static var targetWindow: NSWindow? = nil
    
    /// 現在の設定を保持する構造体
    private static var state: State = State()
    
    /// ウィンドウの初期状態を記憶するインスタンス
    private static var orgWindowInfo: OriginalWindowInfo = OriginalWindowInfo()
    
    /// プライマリーモニターの高さ
    private static var primaryMonitorHeight: CGFloat = 0
    
    /// ウィンドウ透過時の StyleMask
    /// see: https://developer.apple.com/documentation/appkit/nswindow.stylemask
    private static let transparentStyleMask: NSWindow.StyleMask = [.closable, .titled, .resizable]
    
    
    // MARK: - Methods

    
    /// 準備完了かどうかを返す
    /// - Returns: 準備完了ならtrue
    @objc public static func isActive() -> Bool {
        if (state.isReady && targetWindow == nil) {
            return false
        }
        return true
    }
    
    @objc public static func isTransparent() -> Bool {
        return state.isTransparent
    }
    
    @objc public static func isBorderless() -> Bool {
        return state.isBorderless
    }
    
    @objc public static func isTopmost() -> Bool {
        return state.isTopmost
    }
    
    @objc public static func isMaximized() -> Bool {
        return (targetWindow?.isZoomed ?? false)
    }

    @objc public static func detachWindow() -> Void {
        // 別のウィンドウが選択済みだったら、元に戻す
        if (targetWindow != nil) {
            let center = NotificationCenter.default
            center.removeObserver(self)
            
            // スタイルを初期状態に戻す
            orgWindowInfo.Restore(window: targetWindow!)
                        
            targetWindow = nil
        }
    }
    /// ウィンドウを取得して準備
    @objc public static func attachMyWindow() -> Bool {
        // 自分のウィンドウを取得して利用開始
        let window: NSWindow = _findMyWindow()
        _attachWindow(window: window)
        
        return true
    }
    
    /// ウィンドウに設定された内容を再適用
    private static func _reapplyWindowStyles() -> Void {
        if (targetWindow != nil) {
            setTopmost(isTopmost: state.isTopmost)
            setBorderless(isBorderless: state.isBorderless)
            setTransparent(isTransparent: state.isTransparent)
        }
    }
    
    private static func _updateScreenSize() -> Void {
        // 参考 https://stackoverrun.com/ja/q/1746184
        primaryMonitorHeight = NSScreen.screens.map {$0.frame.origin.y + $0.frame.height}.max()!
    }
    
    
    /// 初期化処理
    private static func setup() -> Void {
        // 画面の高さを取得
        _updateScreenSize()
        
        // 解像度変化時に画面の高さを再取得
        NotificationCenter.default.addObserver(
            forName: NSApplication.didChangeScreenParametersNotification,
            object: NSApplication.shared,
            queue: OperationQueue.main
        ) {
            notification -> Void in
            _updateScreenSize()
        }

        
        state.isReady = true
    }
    
    /// 自分自身のウィンドウを取得
    private static func _findMyWindow() -> NSWindow {
        let myWindow: NSWindow = NSApp.orderedWindows[0]
        return myWindow
    }

    /// 対象のウィンドウを指定。それ以前にもし指定があればそれは元に戻す
    private static func _attachWindow(window: NSWindow) -> Void {
        // すでに同じウィンドウが選択されていれば、何もしない
        if (targetWindow == window) {
            return
        }
        
        // 過去に対象だったウィンドウを解除
        detachWindow()
        
        // 初期処理
        if (!state.isReady) {
            setup()
        }

        // 対象を設定
        targetWindow = window
        
        // 初期の状態を記録
        orgWindowInfo.Store(window: window)
        
        // 設定を適用
        _reapplyWindowStyles()
    }
    
    /// ウィンドウ透過の方法を設定
    /// 現在はWindowsでのみ実装
    /// - Parameter type: 0:None, 1:Alpha, 2:ColorKey
    @objc public static func setTransparentType(type: Int32) -> Void {
    }
    
    /// 単色マスクの場合の色設定
    /// 現在はWindowsでのみ実装
    /// - Parameter color: 透過する色
    @objc public static func setKeyColor(color: Int32) -> Void {
    }

    /// ウィンドウ透過を有効化／無効化
    /// - Parameter isTransparent: trueなら透過ウィンドウにする
    @objc public static func setTransparent(isTransparent: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            _setWindowTransparent(window: window, isTransparent: isTransparent)
            _setContentViewTransparent(window: window, isTransparent: isTransparent)
            _setWindowBorderless(window: window, isBorderless: !isTransparent)
        }
        state.isTransparent = isTransparent
    }

    /// ウィンドウ枠を消去／復帰
    /// - Parameter isBorderless: trueなら透過ウィンドウにする
    @objc public static func setBorderless(isBorderless: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            _setWindowBorderless(window: window, isBorderless: isBorderless)
        }
        state.isBorderless = isBorderless
    }

    /// 常に最前面を有効化／無効化
    /// - Parameter isTopmost: trueなら最前面
    @objc public static func setTopmost(isTopmost: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            if (isTopmost) {
                window.collectionBehavior = [.fullScreenAuxiliary]
                window.level = NSWindow.Level.floating
            } else {
                window.collectionBehavior = orgWindowInfo.CollectionBehavior
                window.level = orgWindowInfo.Level
            }
        }
        state.isTopmost = isTopmost
    }
    
    /// 操作のクリックスルーを有効化／無効化
    @objc public static func setClickThrough(isTransparent: Bool) -> Void {
        if (targetWindow != nil) {
            targetWindow!.ignoresMouseEvents = isTransparent
        }
    }

    /// ウィンドウを最大化
    @objc public static func setMaximized(isZoomed: Bool) -> Void {
        if (targetWindow != nil) {
            if (targetWindow!.isZoomed != isZoomed) {
                // 挙動がトグルとなっている
                targetWindow!.zoom(nil)
            }
        }
    }
    
    /// ウィンドウの透過／非透過設定
    /// - Parameters:
    ///   - window: 対象ウィンドウ
    ///   - isTransparent: trueなら透過、falseなら戻す
    private static func _setWindowTransparent(window: NSWindow, isTransparent: Bool) -> Void {
        if (isTransparent) {
            window.styleMask = transparentStyleMask
            window.backgroundColor = NSColor.clear
            window.isOpaque = false
            window.hasShadow = false
            
            //window.contentView?.wantsLayer = true
        } else {
            window.styleMask = orgWindowInfo.StyleMask
            window.backgroundColor = orgWindowInfo.backgroundColor
            window.isOpaque = orgWindowInfo.isOpaque
            window.hasShadow = orgWindowInfo.hasShadow
        }
    }
    
    /// ContentViewの透過／非透過設定
    /// - Parameters:
    ///   - window: 対象ウィンドウ
    ///   - isTransparent: trueなら透過、falseなら戻す
    private static func _setContentViewTransparent(window: NSWindow, isTransparent: Bool) -> Void {
        if let view: NSView = window.contentView {
            if (isTransparent) {
                view.wantsLayer = true
                view.layer?.backgroundColor = CGColor.clear
                view.layer?.isOpaque = false
            } else {
                view.wantsLayer = orgWindowInfo.contentViewWantsLayer
                view.layer?.backgroundColor = orgWindowInfo.contentViewLayerBackgroundColor
                view.layer?.isOpaque = orgWindowInfo.contentViewLayerIsOpaque
            }
        }
    }
    
    /// ウィンドウ枠の除去／復帰
    /// - Parameters:
    ///   - window: 対象ウィンドウ
    ///   - isBorderless: 枠なしにするか
    private static func _setWindowBorderless(window: NSWindow, isBorderless: Bool) -> Void {
        window.styleMask = isBorderless ? transparentStyleMask : orgWindowInfo.StyleMask
        window.titlebarAppearsTransparent = isBorderless || orgWindowInfo.titlebarAppearsTransparent
        window.titleVisibility = isBorderless ? .hidden : orgWindowInfo.titleVisibility
    }
    
    /// ウィンドウの位置を設定
    /// - Parameters:
    ///   - x: ウィンドウ左座標
    ///   - y: ウィンドウ下座標
    /// - Returns: 成功すれば true
    @objc public static func setPosition(x: Float32, y: Float32) -> Bool {
        if (targetWindow == nil) {
            return false
        }
        //// Windowsに合わせる場合。左下が原点なので画面の高さを用いて変換
        //let cocoaY = primaryMonitorHeight - CGFloat(y)
        //let position: NSPoint = NSMakePoint(CGFloat(x), cocoaY)
        //targetWindow?.setFrameTopLeftPoint(position)

        // ウィンドウ左下を基準としてセット
        let position: NSPoint = NSMakePoint(CGFloat(x), CGFloat(y))
        targetWindow?.setFrameOrigin(position)
        return true
    }
    
    /// ウィンドウの現在位置を取得
    ///   - x: ウィンドウ左座標
    ///   - y: ウィンドウ下座標
    /// - Returns: 成功すれば true
    @objc public static func getPosition(x: UnsafeMutablePointer<Float32>, y: UnsafeMutablePointer<Float32>) -> Bool {
        if (targetWindow == nil) {
            x.pointee = 0;
            y.pointee = 0;
            return false
        }
        
        let frame = targetWindow!.frame
        x.pointee = Float32(frame.minX)
        y.pointee = Float32(frame.minY)
        
        // Windowsに合わせる場合
        //y.pointee = Float32(primaryMonitorHeight - frame.maxY)
        
        return true
    }
    
    /// ウィンドウのサイズを設定
    /// - Parameters:
    ///   - width: ウィンドウ幅
    ///   - height: ウィンドウ高さ
    /// - Returns: 成功すれば true
    @objc public static func setSize(width: Float32, height:Float32) -> Bool {
        if (targetWindow == nil) {
            return false
        }
        var frame = targetWindow!.frame
        
        frame.size.width = CGFloat(width)
        frame.size.height = CGFloat(height)
        targetWindow?.setFrame(frame, display: true)
        return true
    }
    
    /// ウィンドウのサイズを取得
    /// - Parameters:
    ///   - width: ウィンドウ幅
    ///   - height: ウィンドウ高さ
    /// - Returns: 成功すれば true
    @objc public static func getSize(width: UnsafeMutablePointer<Float32>, height: UnsafeMutablePointer<Float32>) -> Bool {
        if (targetWindow == nil) {
            width.pointee = 0;
            height.pointee = 0;
            return false
        }
        let currentSize = targetWindow!.frame.size
        width.pointee = Float32(currentSize.width)
        height.pointee = Float32(currentSize.height)
        return true
    }
    
    /// 現在のカーソル座標を取得
    /// - Parameters:
    ///   - x: X座標
    ///   - y: Y座標
    /// - Returns: 成功すれば true
    @objc public static func getCursorPosition(x: UnsafeMutablePointer<Float32>, y: UnsafeMutablePointer<Float32>) -> Bool {
        let mousePos = NSEvent.mouseLocation
        x.pointee = Float32(mousePos.x)
        y.pointee = Float32(mousePos.y)
        return true
    }

    /// カーソル位置を設定
    /// - Parameters:
    ///   - x: X座標
    ///   - y: Y座標
    /// - Returns: 成功すれば true
    @objc public static func setCursorPosition(x: Float32, y: Float32) -> Bool {
        let position = NSMakePoint(CGFloat(x), CGFloat(y))
        let moveEvent = CGEvent(mouseEventSource: nil, mouseType: .mouseMoved,
                                mouseCursorPosition: position, mouseButton: .left)
        
        moveEvent?.post(tap: .cgSessionEventTap)
        return true
    }
}
