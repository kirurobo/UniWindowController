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
    
    // 現在の設定を保持する構造体
    private struct State {
        public var isReady: Bool = false
        public var isTopmost: Bool = false
        public var isBorderless: Bool = false
        public var isTransparent: Bool = false
        
        // サイズ変更がなされると不正確となる。透過時にこれを使う
        public var isZoomed: Bool = false
    }
    
    /// ウィンドウの初期状態を保持するクラス
    private class OriginalWindowInfo {
        /// 元々のStyleMaskをここに記憶
        public var styleMask: NSWindow.StyleMask = []
        
        /// 元々のCollectionBehavior
        public var collectionBehavior: NSWindow.CollectionBehavior = []

        /// 元々のウィンドウLevel
        public var level: NSWindow.Level = NSWindow.Level.normal
        
        public var titlebarAppearsTransparent: Bool = false
        public var titleVisibility: NSWindow.TitleVisibility = NSWindow.TitleVisibility.visible
        public var backgroundColor: NSColor = NSColor.clear
        public var isOpaque: Bool = true
        public var hasShadow: Bool = true
        public var contentViewWantsLayer: Bool = true
        public var contentViewLayerIsOpaque: Bool = true
        public var contentViewLayerBackgroundColor: CGColor? = CGColor.clear

        
        /// 指定ウィンドウの初期値を記憶
        public func Store(window: NSWindow) -> Void {
            self.collectionBehavior = window.collectionBehavior
            self.styleMask = window.styleMask
            self.level = window.level
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
            window.collectionBehavior = self.collectionBehavior
            window.styleMask = self.styleMask
            window.level = self.level
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
    
    /// Callback function with wchar_t pointer
    public typealias stringCallback = (@convention(c) (UnsafeRawPointer) -> Void)

    
    // MARK: - Static variables
    
    /// 操作対象となるウィンドウ。nilだと未指定
    private static var targetWindow: NSWindow? = nil
    
    /// 現在の設定を保持する構造体
    private static var state: State = State()
    
    /// ウィンドウの初期状態を記憶するインスタンス
    private static var orgWindowInfo: OriginalWindowInfo = OriginalWindowInfo()
    
    /// Sub view to implement file dropping
    private static var overlayView: OverlayView? = nil
    
    public static var fileDropCallback: stringCallback? = nil
    
    /// プライマリーモニターの高さ
    private static var primaryMonitorHeight: CGFloat = 0
    
    private static var monitorCount: Int = 0
    private static var monitorRectangles: [CGRect] = []
    private static var monitorIndices: [Int] = []

    
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
        if (state.isTransparent) {
            return state.isZoomed
        } else {
            return (targetWindow?.isZoomed ?? false)
        }
    }

    @objc public static func detachWindow() -> Void {
        // 別のウィンドウが選択済みだったら、元に戻す
        if (targetWindow != nil) {
            let center = NotificationCenter.default
            center.removeObserver(self)
            
            // スタイルを初期状態に戻す
            orgWindowInfo.Restore(window: targetWindow!)
            
            // Remove the subview
            if (overlayView != nil) {
                targetWindow?.contentView?.willRemoveSubview(overlayView!)
                overlayView = nil
            }
                        
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
        // Reference: https://stackoverrun.com/ja/q/1746184
        primaryMonitorHeight = NSScreen.screens.map {$0.frame.origin.y + $0.frame.height}.max()!
        
        // モニタの状態も更新
        _updateMonitorRectangles()
    }
    
    private static func _updateMonitorRectangles() -> Void {
        // モニタ数
        monitorCount = NSScreen.screens.count
        
        // 一旦消去
        monitorRectangles.removeAll()
        monitorIndices.removeAll()
        
        // 現在のスクリーン情報を記憶
        for i in 0..<monitorCount {
            let screen = NSScreen.screens[i]
            monitorRectangles.append(screen.visibleFrame)
            monitorIndices.append(i)
        }
        
        // 左上の画面が0、右下が最大となるようソート
        monitorIndices = monitorIndices.sorted(by: {
            (monitorRectangles[$0].minX < monitorRectangles[$1].minX)
                || (monitorRectangles[$0].minX == monitorRectangles[$1].minX && monitorRectangles[$0].maxY < monitorRectangles[$1].maxY)
        })
    }
    
    /// 初期化処理
    private static func _setup() -> Void {
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
            _setup()
        }

        // 対象を設定
        targetWindow = window
        
        // 初期の状態を記録
        orgWindowInfo.Store(window: window)
        
        // 設定を適用
        _reapplyWindowStyles()
        
        // フルスクリーン移行時に再適用
        NotificationCenter.default.addObserver(
            forName: NSWindow.didEnterFullScreenNotification,
            object: nil,
            queue: OperationQueue.main)
        {
            notification -> Void in _reapplyWindowStyles()
        }
        
        // フルスクリーンから復帰時に再適用
        NotificationCenter.default.addObserver(
            forName: NSWindow.didExitFullScreenNotification,
            object: nil,
            queue: OperationQueue.main)
        {
            notification -> Void in _reapplyWindowStyles()
        }
        
        // Add a subview to handle file dropping
        overlayView = OverlayView(frame: window.frame)
        window.contentView?.addSubview(overlayView!)
        //overlayView?.fitToSuperView()
    }
    
    /// ウィンドウの透過／非透過設定
    /// - Parameters:
    ///   - window: 対象ウィンドウ
    ///   - isTransparent: trueなら透過、falseなら戻す
    private static func _setWindowTransparent(window: NSWindow, isTransparent: Bool) -> Void {
        if (isTransparent) {
            window.styleMask = []
            if (state.isBorderless) {
                window.styleMask.insert(.borderless)
            }
            window.backgroundColor = NSColor.clear
            window.isOpaque = false
            window.hasShadow = false
            
            //window.contentView?.wantsLayer = true
        } else {
            window.styleMask = orgWindowInfo.styleMask
            if (state.isBorderless) {
                window.styleMask.insert(.borderless)
            }
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
        if (isBorderless) {
            window.styleMask.insert(.borderless)
            window.titlebarAppearsTransparent = true
            window.titleVisibility = .hidden
        } else {
            if (!orgWindowInfo.styleMask.contains(.borderless)) {
                // 初期状態で.borderlessだったならばそれは残す
                window.styleMask.remove(.borderless)
            }
            window.titlebarAppearsTransparent = orgWindowInfo.titlebarAppearsTransparent
            window.titleVisibility = orgWindowInfo.titleVisibility
        }
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
        }
        state.isTransparent = isTransparent
    }

    /// ウィンドウ枠を消去／復帰
    /// - Parameter isBorderless: trueなら透過ウィンドウにする
    @objc public static func setBorderless(isBorderless: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            if (isMaximized()) {
                ////  最大化状態なら、一度解除して枠を変更して再度最大化
                ////   ↑枠の分の隙間をなくせるかと思ったが、枠なしで最大化しても意味ないようだった
                //setMaximized(isZoomed: false)
                _setWindowBorderless(window: window, isBorderless: isBorderless)
                //setMaximized(isZoomed: true)
            } else {
                // 最大化されていなければ、そのままウィンドウ枠を変更
                _setWindowBorderless(window: window, isBorderless: isBorderless)
            }
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
                window.collectionBehavior = orgWindowInfo.collectionBehavior
                window.level = orgWindowInfo.level
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
            if (state.isTransparent) {
                // 透過中なら、一度透過解除してから最大化を変更してみる
                //   最大化前のウィンドウサイズが記憶されるが、透過のままだと挙動が直感に反するため
                setTransparent(isTransparent: false)
                if (targetWindow!.isZoomed != isZoomed) {
                    targetWindow!.zoom(nil)
                }
                setTransparent(isTransparent: true)
                if (isZoomed) {
                    // 透明化状態で zoom() をしてもウィンドウ枠の分小さくなってしまっていたため画面サイズにリサイズ
                    let monitorIndex = getCurrentMonitor()
                    let rect = monitorRectangles[monitorIndices[Int(monitorIndex)]]
                    var frame = targetWindow!.frame
                    frame.size.width = CGFloat(rect.width)
                    frame.size.height = CGFloat(rect.height)
                    targetWindow?.setFrame(frame, display: true)
                }
            } else {
                // 透過していない場合の処理
                if (targetWindow!.isZoomed != isZoomed) {
                    // 挙動がトグルとなっている
                    targetWindow!.zoom(nil)
                }
            }
        }
        state.isZoomed = isZoomed
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
    
    /// 現在有効な画面数を取得
    /// - Returns: 画面数
    @objc public static func getCurrentMonitor() -> Int32 {
        var primaryMonitorIndex: Int = 0
        
        // ウィンドウ未取得ならプライマリモニタの番号を返す
        if (targetWindow == nil) {
            for i in 0..<monitorCount {
                let screen = NSScreen.screens[monitorIndices[i]]
                let sf = screen.visibleFrame
                
                //　原点にあるモニタはプライマリモニタと判定
                if (sf.minX == 0 && sf.minY == 0) {
                    primaryMonitorIndex = i
                    break;
                }
            }
            return Int32(primaryMonitorIndex)
        }
        
        // 現在のウィンドウの中心座標を取得
        let frame = targetWindow!.frame;
        let cx: CGFloat = (frame.minX + frame.maxX) / 2.0
        let cy: CGFloat = (frame.minY + frame.maxY) / 2.0
        
        for i in 0..<monitorCount {
            let screen = NSScreen.screens[monitorIndices[i]]
            let sf = screen.visibleFrame
            
            // ウィンドウ中心を含む画面があればその画面番号を返す
            if (sf.minX <= cx && cx <= sf.maxX && sf.minY <= cy && cy <= sf.maxY) {
                return Int32(i)
            }
            
            //　原点にあるモニタはプライマリモニタと判定
            if (sf.minX == 0 && sf.minY == 0) {
                primaryMonitorIndex = i
            }
        }
        return Int32(primaryMonitorIndex)
    }

    /// 現在有効な画面数を取得
    /// - Returns: 画面数
    @objc public static func getMonitorCount() -> Int32 {
        // NOTE: UnityにあるScreenやDisplayとは異なるため、Monitorという言葉にした
        return Int32(monitorCount)
    }
    
    /// 指定した画面の位置、サイズを取得
    /// - Parameters:
    ///   - monitorIndex: 画面の番号
    ///   - x: X座標
    ///   - y: Y座標
    ///   - width: ウィンドウ幅
    ///   - height: ウィンドウ高さ
    /// - Returns: 成功すれば true
    @objc public static func getMonitorRectangle(
        monitorIndex: Int32,
        x: UnsafeMutablePointer<Float32>, y: UnsafeMutablePointer<Float32>,
        width: UnsafeMutablePointer<Float32>, height: UnsafeMutablePointer<Float32>
    ) -> Bool {
        // 存在しないスクリーン番号ならば false で終了
        if (monitorIndex < 0 || monitorIndex >= monitorCount || monitorIndex >= NSScreen.screens.count) {
            return false
        }
        
        let frame = NSScreen.screens[monitorIndices[Int(monitorIndex)]].visibleFrame
        x.pointee = Float32(frame.minX)
        y.pointee = Float32(frame.minY)
        width.pointee = Float32(frame.width)
        //height.pointee = Float32(frame.height)
        height.pointee = Float32(fileDropCallback.debugDescription.count)
        return true
    }
    
    @objc public static func setAllowDrop(enabled: Bool) -> Bool {
        overlayView?.setEnabled(enabled: enabled)
        return true
    }

    @objc public static func registerFileDropCallback(callback: @escaping stringCallback) -> Bool {
        fileDropCallback = callback
        return true
    }
    
    @objc public static func unregisterFileDropCallback() -> Bool {
        fileDropCallback = nil
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
