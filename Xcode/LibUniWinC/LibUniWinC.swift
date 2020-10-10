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
    
    // MARK: - Static variables
    
    /// 操作対象となるウィンドウ。nilだと未指定
    private static var targetWindow: NSWindow? = nil
    
    /// 元々のStyleMaskをここに記憶
    private static var originalStyleMask: NSWindow.StyleMask = []
    
    /// 元々のCollectionBehavior
    private static var originalCollectionBehavior: NSWindow.CollectionBehavior = []

    /// 元々のウィンドウLevel
    private static var originalLevel: NSWindow.Level = NSWindow.Level.normal
    
    /// 現在の設定を保持する
    private static var state: State = State()
    
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
    
    @objc public static func isZoomed() -> Bool {
        return (targetWindow?.isZoomed ?? false)
    }

    @objc public static func detachWindow() -> Void {
        // 別のウィンドウが選択済みだったら、元に戻す
        if (targetWindow != nil) {
            let center = NotificationCenter.default
            center.removeObserver(self)
            
            targetWindow!.collectionBehavior = originalCollectionBehavior
            targetWindow!.styleMask = originalStyleMask
            targetWindow!.level = originalLevel
            
            targetWindow = nil
        }
    }
    /// ウィンドウを取得して準備
    @objc public static func attachMyWindow() -> Bool {
        // 自分のウィンドウを取得して利用開始
        let window: NSWindow = findMyWindow()
        setTargetWindow(window: window)
        
        return true
    }
    
    private static func updateScreenSize() -> Void {
        // 参考 https://stackoverrun.com/ja/q/1746184
        primaryMonitorHeight = NSScreen.screens.map {$0.frame.origin.y + $0.frame.height}.max()!
    }
    
    
    /// 初期化処理
    private static func setup() -> Void {
        // 画面の高さを取得
        updateScreenSize()
        
        // 解像度変化時に画面の高さを再取得
        NotificationCenter.default.addObserver(
            forName: NSApplication.didChangeScreenParametersNotification,
            object: NSApplication.shared,
            queue: OperationQueue.main
        ) {
            notification -> Void in
            updateScreenSize()
        }
        
        state.isReady = true
    }
    
    /// 自分自身のウィンドウを取得
    private static func findMyWindow() -> NSWindow {
        let myWindow: NSWindow = NSApp.orderedWindows[0]
        return myWindow
    }

    /// 対象のウィンドウを指定。それ以前にもし指定があればそれは元に戻す
    private static func setTargetWindow(window: NSWindow) -> Void {
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
        originalCollectionBehavior = window.collectionBehavior
        originalStyleMask = window.styleMask
        originalLevel = window.level
        
        // 設定を適用
        setTransparent(isTransparent: state.isTransparent)
        setBorderless(isBorderless: state.isBorderless)
        setTopmost(isTopmost: state.isTopmost)
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
            _setBorderAppearance(window: window, isShown: !isTransparent)
        }
        state.isTransparent = isTransparent
    }

    /// ウィンドウ枠を消去／復帰
    /// - Parameter isBorderless: trueなら透過ウィンドウにする
    @objc public static func setBorderless(isBorderless: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            _setBorderAppearance(window: window, isShown: !isBorderless)
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
                window.collectionBehavior = originalCollectionBehavior
                window.level = originalLevel
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
    @objc public static func setZoomed(isZoomed: Bool) -> Void {
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
            window.styleMask = originalStyleMask
            window.backgroundColor = NSColor.clear
            window.isOpaque = true
            window.hasShadow = true
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
                view.wantsLayer = false
                view.layer?.backgroundColor = CGColor.clear
                view.layer?.isOpaque = true
            }
        }
    }
    
    /// Hide or show the border of the given window.
    ///
    /// - Parameters:
    ///   - window: a window to show/hide
    ///   - isShow: boolean value to indicate show or hide
    private static func _setBorderAppearance(window: NSWindow, isShown: Bool) -> Void {
        window.styleMask = isShown ? transparentStyleMask : [.borderless]
        window.titlebarAppearsTransparent = !isShown
        window.titleVisibility = isShown ? .visible : .hidden
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
    /// - Returns: スクリーン座標
    private static func _getCursorPosition() -> NSPoint {
        return NSEvent.mouseLocation
    }

    /// カーソル位置を設定
    /// - Parameters:
    ///   - x: X座標
    ///   - y: Y座標
    /// - Returns: 成功すれば true
    @objc public static func SetCursorPosition(x: Float32, y: Float32) -> Bool {
        let position = NSMakePoint(CGFloat(x), CGFloat(y))
        let moveEvent = CGEvent(mouseEventSource: nil, mouseType: .mouseMoved,
                                mouseCursorPosition: position, mouseButton: .left)
        
        moveEvent?.post(tap: .cgSessionEventTap)
        return true
    }
}
