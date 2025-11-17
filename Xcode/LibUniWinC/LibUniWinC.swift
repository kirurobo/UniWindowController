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
// - https://qiita.com/fuziki/items/974f70b663ebfadfb136
// - https://qiita.com/KRiver1/items/9ecf65759cf1349f56af
// - http://tatsudoya.blog.fc2.com/blog-entry-244.html
// - https://qiita.com/mybdesign/items/fe3e390741799c1814ad
// - https://blog.fenrir-inc.com/jp/2011/07/nsview_uiview.html
//

import Foundation
import Cocoa
import ObjectiveC

extension NSWindow {
    /// swizzleによってconstrainFrameRectを無効にする
    func swizzleConstrainFrameRect() {
        let originalSelector = #selector(constrainFrameRect(_:to:))
        let swizzledSelector = #selector(disabled_constrainFrameRect(_:to:))

        guard let originalMethod = class_getInstanceMethod(NSWindow.self, originalSelector),
              let swizzledMethod = class_getInstanceMethod(NSWindow.self, swizzledSelector) else {
            return
        }

        // constrainFrameRectを元のものと入れ替える
        method_exchangeImplementations(originalMethod, swizzledMethod)
    }

    /// 制限なしとした constrainFrameRect
   @objc func disabled_constrainFrameRect(_ frameRect: NSRect, to screen: NSScreen?) -> NSRect {
       return frameRect
   }
}


/// Window controller main logic
@objcMembers
public class LibUniWinC {
    
    // MARK: - Internal structs and classes
    
    /// 現在の設定を保持する構造体
    private struct State {
        public var isReady: Bool = false
        public var isTopmost: Bool = false
        public var isBottommost: Bool = false
        public var isBorderless: Bool = false
        public var isTransparent: Bool = false
        public var alphaValue: Float32 = 1
        
        // サイズ変更がなされると不正確となる。透過時にこれを使う
        public var isZoomed: Bool = false
        
        // Keep unzoomed size for the borderless window
        public var normalWindowRect: NSRect = NSRect(x: 0, y: 0, width: 0, height: 0)
        
        // メニューバーより上にも自由配置を許すか
        public var isFreePositioningEnabled: Bool = false
        // 自由移動のため実際にconstrainFrameRectを無効化されたか
        public var isConstrainFrameRectDisabled: Bool = false
    }
    
    /// Event types for WindowStyleChanged
    private enum EventType : Int32 {
        case None = 0
        case Style = 1
        case Size = 2
        case Order = 4
    }
    
    /// Flag constants for file dialog
    public enum PanelFlag : Int32 {
        case None = 0
        case FileMustExist = 1
        case FolderMustExist = 2
        case AllowMultipleSelection = 4
        //case CanCreateDirectories = 16
        case OverwritePrompt = 256
        case CreatePrompt = 512
        case ShowHidden = 4096
        case RetrieveLink = 8192
        
        public func containedIn(value: Int32) -> Bool {
            return (self.rawValue & value > 0)
        }
    }
    
    public struct PanelSettings {
        public var structSize: Int32 = 0;
        public var flags: Int32 = 0;
        public var titleText: UnsafePointer<UniChar>?;
        public var filterText: UnsafePointer<UniChar>?;
        public var initialFile: UnsafePointer<UniChar>?;
        public var initialDirectory: UnsafePointer<UniChar>?;
        public var defaultExt: UnsafePointer<UniChar>?;
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
        public var isKeyWindow: Bool = true
        public var alphaValue: CGFloat = 1
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
            self.isKeyWindow = window.isKeyWindow
            self.alphaValue = window.alphaValue
            
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
            window.alphaValue = self.alphaValue
            
            window.contentView?.wantsLayer = self.contentViewWantsLayer
            window.contentView?.layer?.isOpaque = self.contentViewLayerIsOpaque
            window.contentView?.layer?.backgroundColor = self.contentViewLayerBackgroundColor
            
            // Restore the constrainFrameRect()
            _enableFreePositioning(enabled: false)
        }
    }
    
    
    // MARK: - Static variables
    
    /// 操作対象となるウィンドウ。nilだと未指定
    private static var targetWindow: NSWindow? = nil
    
    /// 現在の設定を保持する構造体
    private static var state: State = State()
    
    /// ウィンドウの初期状態を記憶するインスタンス
    private static var orgWindowInfo: OriginalWindowInfo = OriginalWindowInfo()
    
    
    /// Callback function with wchar_t pointer
    public typealias stringCallback = (@convention(c) (UnsafeRawPointer) -> Void)
    public typealias intCallback = (@convention(c) (Int32) -> Void)
    public static var dropFilesCallback: stringCallback? = nil
    public static var openFilesCallback: stringCallback? = nil
    public static var saveFilesCallback: stringCallback? = nil
    public static var monitorChangedCallback: intCallback? = nil
    public static var windowStyleChangedCallback: intCallback? = nil
    private static var observerObject: Any? = nil
    
    /// Sub view to implement file dropping
    private static var overlayView: OverlayView? = nil
    
    /// プライマリーモニターの高さ
    private static var primaryMonitorHeight: CGFloat = 0
    
    private static var monitorCount: Int = 0
    private static var monitorRectangles: [CGRect] = []
    private static var monitorIndices: [Int] = []
    
    // MARK: - Properties
    
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
    
    @objc public static func isBottommost() -> Bool {
        return state.isBottommost
    }
    
    @objc public static func isMaximized() -> Bool {
        return state.isZoomed
        //return _isZoomedActually()
    }
    
    @objc public static func isMinimized() -> Bool {
        return (targetWindow?.isMiniaturized ?? false)
    }
    
    @objc public static func isFreePositioningEnabled() -> Bool {
        return state.isFreePositioningEnabled
    }
    
    private static func _isZoomedActually() -> Bool {
        if (targetWindow == nil) {
            return false
        } else if (targetWindow!.isMiniaturized) {
            return false
        } else if (state.isTransparent) {
            // When the window is transparent
            let monitorIndex = getCurrentMonitor()
            let rect = monitorRectangles[monitorIndices[Int(monitorIndex)]]
            let frame = targetWindow!.frame
            return (frame.size == rect.size) && (frame.origin == rect.origin)
        } else {
            // When the window is opaque
            return targetWindow!.isZoomed
        }
    }
    
    // MARK: - Initialize, window handling
    
    /// Initialize
    private static func _setup() -> Void {
        // Get the screen size
        _updateScreenInfo()
        
        // Prepare notification to refresh the screen size
        NotificationCenter.default.addObserver(
            forName: NSApplication.didChangeScreenParametersNotification,
            object: NSApplication.shared,
            queue: OperationQueue.main
        ) {
            notification -> Void in _onMonitorChanged()
        }
        
        // Flag as initialized
        state.isReady = true
    }
    
    /// Called when screen parameeters changed
    private static func _onMonitorChanged() -> Void {
        _updateScreenInfo()
        
        // Run callback
        let count = getMonitorCount()
        monitorChangedCallback?(count)
    }
    
    /// Retrieve current monitor settings
    private static func _updateScreenInfo() -> Void {
        // Reference: https://stackoverrun.com/ja/q/1746184
        primaryMonitorHeight = NSScreen.screens.map {$0.frame.origin.y + $0.frame.height}.max()!
        
        // Get the number of monitors
        monitorCount = NSScreen.screens.count
        
        // Clear the list
        monitorRectangles.removeAll()
        monitorIndices.removeAll()
        
        // Get each screen rectangle
        for i in 0..<monitorCount {
            let screen = NSScreen.screens[i]
            //monitorRectangles.append(screen.visibleFrame)
            monitorRectangles.append(screen.frame)
            monitorIndices.append(i)
        }
        
        // Sort the list so that the top left monitor is at the zero
        monitorIndices = monitorIndices.sorted(by: {
            (monitorRectangles[$0].minX < monitorRectangles[$1].minX)
            || (monitorRectangles[$0].minX == monitorRectangles[$1].minX && monitorRectangles[$0].maxY < monitorRectangles[$1].maxY)
        })
    }
    
    /// Find my own window
    private static func _findMyWindow() -> NSWindow {
        //var myWindow: NSWindow = NSApp.orderedWindows.first!
        //let myWindow: NSWindow = NSApp.mainWindow ?? NSApp.orderedWindows.first!
        
        for window in NSApp.orderedWindows {
            // キー操作を受け取るウィンドウが見つかれば、それだとする
            if (window.isKeyWindow) {
                return window
            }
            //            print("[DEBUG - orderedWindows]")
            //            print(window.title)
            //            print(window.isKeyWindow)
            //            print(window.isZoomed)
            //            print(window.contentLayoutRect)
        }
        // キーウィンドウが見つからなければ先頭とする
        return NSApp.orderedWindows.first!
    }
    
    /// Detach from the window
    @objc public static func detachWindow() -> Void {
        _detachWindow()
    }
    
    /// Attach to my main window
    @objc public static func attachMyWindow() -> Bool {
        let window: NSWindow = _findMyWindow()
        _attachWindow(window: window)
        
        return true
    }
    
    /// Set the target window
    /// Restore the former winodw if exist
    public static func _attachWindow(window: NSWindow) -> Void {
        // Do nothing if the same window is the target
        if (targetWindow == window) {
            return
        }
        
        // Release the former window if exist
        detachWindow()
        
        // Initialize when the first call
        if (!state.isReady) {
            _setup()
        }
        
        // Set to the target
        targetWindow = window
        
        // Store the original state
        orgWindowInfo.Store(window: window)
        
        // Apply the state
        _reapplyWindowStyles()
        
        // Add observers for window state changed callback and reapply styles
        let center = NotificationCenter.default
        center.addObserver(self, selector: #selector(_fullScreenChangedObserver(notification:)), name: NSWindow.didEnterFullScreenNotification, object: window)
        center.addObserver(self, selector: #selector(_fullScreenChangedObserver(notification:)), name: NSWindow.didExitFullScreenNotification, object: window)
        center.addObserver(self, selector: #selector(_windowStateChangedObserver(notification:)), name: NSWindow.didMiniaturizeNotification, object: window)
        center.addObserver(self, selector: #selector(_windowStateChangedObserver(notification:)), name: NSWindow.didDeminiaturizeNotification, object: window)
        //center.addObserver(self, selector: #selector(_resizedObserver(notification:)), name: NSWindow.didResizeNotification, object: window)
        center.addObserver(self, selector: #selector(_resizedObserver(notification:)), name: NSWindow.didEndLiveResizeNotification, object: window)
        //center.addObserver(self, selector: #selector(_keepKeyWindowObserver(notification:)), name: NSWindow.didExposeNotification, object: window)
        center.addObserver(self, selector: #selector(_keepKeyWindowObserver(notification:)), name: NSWindow.didResignKeyNotification, object: window)
        center.addObserver(self, selector: #selector(_keepBottommostObserver(notification:)), name: NSWindow.didBecomeKeyNotification, object: window)
    }
    
    private static func _detachWindow() -> Void {
        if (targetWindow != nil) {
            let center = NotificationCenter.default
            center.removeObserver(self, name: NSWindow.didEnterFullScreenNotification, object: targetWindow)
            center.removeObserver(self, name: NSWindow.didExitFullScreenNotification, object: targetWindow)
            center.removeObserver(self, name: NSWindow.didMiniaturizeNotification, object: targetWindow)
            center.removeObserver(self, name: NSWindow.didDeminiaturizeNotification, object: targetWindow)
            //center.removeObserver(self, name: NSWindow.didResizeNotification, object: targetWindow)
            center.removeObserver(self, name: NSWindow.didEndLiveResizeNotification, object: targetWindow)
            //center.removeObserver(self, name: NSWindow.didExposeNotification, object: targetWindow)
            center.removeObserver(self, name: NSWindow.didResignKeyNotification, object: targetWindow)
            center.removeObserver(self, name: NSWindow.didBecomeKeyNotification, object: targetWindow)
            
            //center.removeObserver(self)
            
            // Restore the original style
            orgWindowInfo.Restore(window: targetWindow!)
            
            // Remove the subview
            if (overlayView != nil) {
                overlayView?.removeFromSuperview()
                overlayView = nil
            }
            
            targetWindow = nil
        }
    }
    
    @objc static func _fullScreenChangedObserver(notification: Notification) {
        // Reapply the state at fullscreen
        _reapplyWindowStyles()
        _doWindowStyleChangedCallback(num: EventType.Size)
    }
    
    @objc static func _windowStateChangedObserver(notification: Notification) {
        _doWindowStyleChangedCallback(num: EventType.Size)
    }
    
    @objc static func _resizedObserver(notification: Notification) {
        if (targetWindow != nil) {
            let zoomed = _isZoomedActually()
            
            if (state.isZoomed != zoomed) {
                state.isZoomed = zoomed
            }
            _doWindowStyleChangedCallback(num: EventType.Size)
        }
    }
    
    @objc static func _keepKeyWindowObserver(notification: Notification) {
        if (targetWindow != nil && !state.isBottommost) {
            if (orgWindowInfo.isKeyWindow && !targetWindow!.isKeyWindow) {
                _makeKeyWindow()
                //_doWindowStyleChangedCallback(num: EventType.Order)
            }
        }
    }
    
    @objc static func _keepBottommostObserver(notification: Notification) {
        if ((targetWindow != nil) && state.isBottommost) {
            targetWindow!.level = orgWindowInfo.level
            targetWindow!.order(NSWindow.OrderingMode.below, relativeTo:0)
            _doWindowStyleChangedCallback(num: EventType.Order)
        }
    }
    
    /// Call this periodically to maintain window state.
    @objc public static func update() {
        if (targetWindow != nil) {
            if (state.isTransparent) {
                // Keep window transparent
                if (targetWindow!.isOpaque) {
                    _setWindowTransparent(window: targetWindow!, isTransparent: true)
                }
                
                // Keep contentView transparent
                if (targetWindow!.contentView?.layer?.isOpaque ?? false) {
                    _setContentViewTransparent(window: targetWindow!, isTransparent: true)
                }
            }
        }
    }
    
    private static func _makeKeyWindow() {
        guard let window = targetWindow else {
            return
        }
        
        if (state.isBorderless) {
            // Restore the key window state. NSWindow.canBecomeKeyWindow is false by default for borderless window, so makeKey() is unavailable...
            state.isBorderless = false;     // Suppress the callback
            setBorderless(isBorderless: false)
            window.makeKey()
            state.isBorderless = true;      // Suppress the callback
            setBorderless(isBorderless: true)
        } else {
            window.makeKey()
        }
    }
    
    private static func _doWindowStyleChangedCallback(num : EventType) -> Void {
        windowStyleChangedCallback?(num.rawValue)
    }
    
    /// Create an overlay view to handle file dropping
    private static func _setupOverlayView() -> Void {
        guard let window = targetWindow
        else {
            return
        }
        
        // Add a subview to handle file dropping
        overlayView = OverlayView(frame: window.frame)
        window.contentView?.addSubview(overlayView!)
        overlayView?.fitToSuperView()
    }
    
    /// Apply current window state
    private static func _reapplyWindowStyles() -> Void {
        if (targetWindow != nil) {
            if (state.isBottommost) {
                setBottommost(isBottommost: state.isBottommost)
            } else {
                setTopmost(isTopmost: state.isTopmost)
            }
            setTransparent(isTransparent: state.isTransparent)
            setBorderless(isBorderless: state.isBorderless)
            setMaximized(isZoomed: state.isZoomed)
            setAlphaValue(alpha: state.alphaValue)
            
            _enableFreePositioning(enabled: state.isFreePositioningEnabled)
        }
    }
    
    /// Copy UTF-16 string to uint16 buffer and add null for the end of the string
    private static func _copyUTF16ToBuffer(text: String.UTF16View, buffer: UnsafeMutablePointer<UTF16Char>) -> Bool {
        let count = text.count
        if (count <= 0) {
            return false
        }
        
        var i = 0
        for c in text {
            buffer[i] = c
            i += 1
        }
        buffer[count] = UTF16Char.zero     // End of the string
        return true
    }
    
    // MARK: - Functions to get or set the window state
    
    /// ウィンドウの透過／非透過設定
    /// - Parameters:
    ///   - window: 対象ウィンドウ
    ///   - isTransparent: trueなら透過、falseなら戻す
    private static func _setWindowTransparent(window: NSWindow, isTransparent: Bool) -> Void {
        if (isTransparent) {
            //            window.styleMask = orgWindowInfo.styleMask
            //            //window.styleMask = []
            //            if (state.isBorderless) {
            //                window.titlebarAppearsTransparent = true
            //                window.titleVisibility = .hidden
            //                window.styleMask.insert(.borderless)
            //            }
            //window.hasShadow = false      // _setWindowBorderless()に移動
            window.backgroundColor = NSColor.clear
            window.isOpaque = false
            
            //window.contentView?.wantsLayer = true
        } else {
            //            window.styleMask = orgWindowInfo.styleMask
            //            if (state.isBorderless) {
            //                window.styleMask.insert(.borderless)
            //            }
            window.backgroundColor = orgWindowInfo.backgroundColor
            window.isOpaque = orgWindowInfo.isOpaque
            //window.hasShadow = orgWindowInfo.hasShadow
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
            // 枠なしにした後も残っていたため、枠なしの場合は常に影はオフとする
            window.hasShadow = false
            
            // macOSのフルスクリーンでは、styleMask が 0 (== [.borderless]) であった。
            // そのため .fullscreen が含まれるかというフラグではフルスクリーンを判別できないよう。
            // その場合にクラッシュすることを防ぐため、すでに .borderless でないときのみ .borderless にすることにする。
            if (!window.styleMask.contains(.fullScreen) && (window.styleMask != [.borderless]))  {
                window.styleMask = [.borderless]
                
                if (window.hasTitleBar) {
                    window.titlebarAppearsTransparent = true
                    window.titleVisibility = .hidden
                }
            }
        } else {
            window.styleMask = orgWindowInfo.styleMask
            if (!orgWindowInfo.styleMask.contains(.borderless)) {
                // 初期状態で.borderlessだったならばそれは残し、そうでなければ枠なしを解除
                window.styleMask.remove(.borderless)
            }
            if (window.hasTitleBar) {
                window.titlebarAppearsTransparent = orgWindowInfo.titlebarAppearsTransparent
                window.titleVisibility = orgWindowInfo.titleVisibility
            }
            window.hasShadow = orgWindowInfo.hasShadow
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
    
    /// Sets window alpha value
    ///  - Parameter alpha: 0.0 - 1.0
    @objc public static func setAlphaValue(alpha: Float32) -> Void {
        if let window: NSWindow = targetWindow {
            window.alphaValue = CGFloat(alpha)
        }
        state.alphaValue = alpha
    }
    
    /// ウィンドウ透過を有効化／無効化
    /// - Parameter isTransparent: trueなら透過ウィンドウにする
    @objc public static func setTransparent(isTransparent: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            _setWindowTransparent(window: window, isTransparent: isTransparent)
            _setContentViewTransparent(window: window, isTransparent: isTransparent)
        }
        
        if (state.isTransparent != isTransparent) {
            _doWindowStyleChangedCallback(num: EventType.Style)
        }
        
        state.isTransparent = isTransparent
    }
    
    /// Hide or show the window border
    /// - Parameter isBorderless: true for borderless
    @objc public static func setBorderless(isBorderless: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            if (!state.isZoomed) {
                if (isBorderless != state.isBorderless) {
                    // Store the window size when the window become borderless
                    state.normalWindowRect = window.frame
                }
            }
            
            if (orgWindowInfo.isKeyWindow) {
                if (isBorderless) {
                    // 枠なしにする前に、キーウィンドウにしておく
                    window.makeKey()
                    _setWindowBorderless(window: window, isBorderless: isBorderless)
                } else {
                    // 枠ありにした後で、キーウィンドウにする
                    _setWindowBorderless(window: window, isBorderless: isBorderless)
                    window.makeKey()
                }
            } else {
                _setWindowBorderless(window: window, isBorderless: isBorderless)
            }
            
            // 透過切り替え直後にキー操作が効かなくなるためキーウインドウにしたい。だがうまくはいかないよう。透過だとキーにできないのは仕方がなさそう…
            //            window.makeMain()
            //            window.makeKey()
            
            if (state.isZoomed) {
                if (!window.isZoomed) {
                    window.zoom(nil)
                }
                if (isBorderless) {
                    // Stretch to the full-screen size
                    let monitorIndex = getCurrentMonitor()
                    let rect = monitorRectangles[monitorIndices[Int(monitorIndex)]]
                    window.setFrame(rect, display: true, animate: false)
                }
            } else {
                // 枠なしを切り替えるたびにウィンドウサイズが小さくなったので、これはコメントアウト
                //                if (!isBorderless && state.isBorderless) {
                //                    // Restore the window size when the window become bordered
                //                    if (state.normalWindowRect.width != 0 && state.normalWindowRect.height != 0) {
                //                        window.setFrame(state.normalWindowRect, display: true, animate: false)
                //                    }
                //                }
            }
        }
        
        if (state.isBorderless != isBorderless) {
            _doWindowStyleChangedCallback(num: EventType.Style)
        }
        
        state.isBorderless = isBorderless
    }
    
    /// 常に最前面を有効化／無効化
    /// - Parameter isTopmost: true for topmost (higher than the menu bar)
    @objc public static func setTopmost(isTopmost: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            if (isTopmost) {
                window.collectionBehavior = [.fullScreenAuxiliary]
                window.level = NSWindow.Level.popUpMenu
            } else {
                window.collectionBehavior = orgWindowInfo.collectionBehavior
                window.level = orgWindowInfo.level
            }
        }
        
        if (state.isTopmost != isTopmost) {
            _doWindowStyleChangedCallback(num: EventType.Style)
        }
        
        state.isTopmost = isTopmost
        state.isBottommost = false
    }
    
    /// 常に最背面を有効化／無効化
    /// - Parameter isBottommost: trueなら最背面
    @objc public static func setBottommost(isBottommost: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            if (isBottommost) {
                window.collectionBehavior = [.fullScreenAuxiliary]
                window.level = orgWindowInfo.level
                window.order(NSWindow.OrderingMode.below, relativeTo:0)
            } else {
                window.collectionBehavior = orgWindowInfo.collectionBehavior
                window.level = orgWindowInfo.level
            }
        }
        
        if (state.isBottommost != isBottommost) {
            _doWindowStyleChangedCallback(num: EventType.Style)
        }
        
        state.isBottommost = isBottommost
        state.isTopmost = false
    }
    
    /// 操作のクリックスルーを有効化／無効化
    @objc public static func setClickThrough(isTransparent: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            window.ignoresMouseEvents = isTransparent
            //window!.acceptsMouseMovedEvents = true      // 試しに付けてみたが不要なようだった
        }
    }
    
    /// macOSで通常は制限されているウィンドウ位置を許可する
    @objc public static func enableFreePositioning(enabled: Bool) -> Void {
        // 指示された内容を現在の設定値として覚える
        state.isFreePositioningEnabled = enabled
        
        // 実際の処理
        _enableFreePositioning(enabled: enabled)
    }
    
    /// constrainFrameRect による制限を解除／復帰
    private static func _enableFreePositioning(enabled: Bool) -> Void {
        // 自由位置のenabledは、constrainのdisabled。すでに一致していればメソッド交換は行わない
        if (enabled == state.isConstrainFrameRectDisabled) {
            return
        }
        
        if let window: NSWindow = targetWindow {
            // constrainFrameRect の交換を行う（無効化も、復帰も交換）
            window.swizzleConstrainFrameRect()
            state.isConstrainFrameRectDisabled.toggle()
        }
    }
    
    /// Maximize the window
    @objc public static func setMaximized(isZoomed: Bool) -> Void {
        if let window: NSWindow = targetWindow {
            if (state.isBorderless) {
                // window.zoom() is unavailable if the window is ransparent (borderless)
                
                if (isZoomed) {
                    // Store the window size when the window become zoomed
                    //if (!state.isZoomed && state.isBorderless && !_isZoomedActually()) {
                    if (!_isZoomedActually() && state.isBorderless) {
                        state.normalWindowRect = window.frame
                    }
                    
                    // The window couldn't be zoomed when it is borderless
                    let monitorIndex = getCurrentMonitor()
                    let rect = monitorRectangles[monitorIndices[Int(monitorIndex)]]
                    window.setFrame(rect, display: true, animate: false)
                } else {
                    if (state.normalWindowRect.width != 0 && state.normalWindowRect.height != 0) {
                        window.setFrame(state.normalWindowRect, display: true, animate: false)
                    }
                }
                state.isZoomed = isZoomed
            } else {
                // The window is opaque
                if (window.isZoomed != isZoomed) {
                    // Toggle
                    window.zoom(nil)
                    state.isZoomed = window.isZoomed
                }
            }
        } else {
            // Remember the state
            state.isZoomed = isZoomed
        }
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
        targetWindow?.setFrame(frame, display: true, animate: false)
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
    
    /// ウィンドウのクライアント領域サイズを取得
    /// - Parameters:
    ///   - width: 幅
    ///   - height: 高さ
    /// - Returns: 成功すれば true
    @objc public static func getClientSize(width: UnsafeMutablePointer<Float32>, height: UnsafeMutablePointer<Float32>) -> Bool {
        if (targetWindow == nil) {
            width.pointee = 0;
            height.pointee = 0;
            return false
        }
        let currentSize = targetWindow!.contentRect(forFrameRect: targetWindow!.frame).size
        width.pointee = Float32(currentSize.width)
        height.pointee = Float32(currentSize.height)
        return true
    }
    
    /// ウィンドウのクライアント領域位置・サイズを取得
    /// - Parameters:
    ///    - x: ウィンドウ左からのx座標
    ///    - y: ウィンドウ下からのy座標
    ///    - width: 幅
    ///    - height: 高さ
    /// - Returns: 成功すれば true
    @objc public static func getClientRectangle(
        x: UnsafeMutablePointer<Float32>,
        y: UnsafeMutablePointer<Float32>,
        width: UnsafeMutablePointer<Float32>,
        height: UnsafeMutablePointer<Float32>) -> Bool {
            if (targetWindow == nil) {
                x.pointee = 0;
                y.pointee = 0;
                width.pointee = 0;
                height.pointee = 0;
                return false
            }
            let winRect = targetWindow!.frame
            let rect = targetWindow!.contentRect(forFrameRect: targetWindow!.frame)
            x.pointee = Float32(rect.minX - winRect.minX)
            y.pointee = Float32(rect.minY - winRect.minY)
            width.pointee = Float32(rect.width)
            height.pointee = Float32(rect.height)
            return true
        }
    
    @objc public static func registerWindowStyleChangedCallback(callback: @escaping intCallback) -> Bool {
        windowStyleChangedCallback = callback
        return true
    }
    
    @objc public static func unregisterWindowStyleChangedCallback() -> Bool {
        windowStyleChangedCallback = nil
        return true
    }
    
    
    // MARK: - Monitor Info.
    
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
        height.pointee = Float32(frame.height)
        return true
    }
    
    @objc public static func registerMonitorChangedCallback(callback: @escaping intCallback) -> Bool {
        monitorChangedCallback = callback
        return true
    }
    
    @objc public static func unregisterMonitorChangedCallback() -> Bool {
        monitorChangedCallback = nil
        return true
    }
    
    
    // MARK: - File drop
    
    @objc public static func setAllowDrop(enabled: Bool) -> Bool {
        if (overlayView == nil) {
            _setupOverlayView()
        }
        
        overlayView?.setEnabled(enabled: enabled)
        return true
    }
    
    @objc public static func registerDropFilesCallback(callback: @escaping stringCallback) -> Bool {
        dropFilesCallback = callback
        return true
    }
    
    @objc public static func unregisterDropFilesCallback() -> Bool {
        dropFilesCallback = nil
        return true
    }
    
    
    // MARK: - Mouser curosor
    
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
    
    /// マウスのボタン押下状態を取得
    /// - Returns: マウスボタン押下状態を示すビットフラグ（1: Left, 2: Right, 4: Middle）
    @objc public static func getMouseButtons() -> Int32 {
        let buttons = NSEvent.pressedMouseButtons
        let result = buttons & (1 + 2 + 4)  // Middle ボタンまでは pressedMouseButtons の仕様と一致。それ以上は非対応。
        return Int32(result)
    }
    
    /// 修飾キー状態を取得
    /// 数値は Windows API のものがベース。NSEvent.ModifierFlagsのrawValueとは異なる。
    /// - Returns: 修飾キー押下を示すビットフラグ（0:None, 1:Option/Alt, 2:Control, 4:Shift, 8:Command/Win)
    @objc public static func getModifierKeys() -> Int32 {
        var result : Int32 = 0
        
        if let flags = NSApp.currentEvent?.modifierFlags {
            result += (flags.contains(.option)) ? 1 : 0
            result += (flags.contains(.control)) ? 2 : 0
            result += (flags.contains(.shift)) ? 4 : 0
            result += (flags.contains(.command)) ? 8 : 0
        }
        return result
    }
    
    
    // MARK: - File dialogs
    
    /// Open dialog
    /// - Parameters:
    ///     - lpSettings: Pointer of PanelSettings
    ///     - lpBuffer: Pointer of UTF-16 string for output
    ///     - bufferSize: Size of UTF-16 string buffer
    @objc public static func openFilePanel(lpSettings: UnsafeRawPointer, lpBuffer: UnsafeMutablePointer<UniChar>?, bufferSize: UInt32) -> Bool {
        let panel = NSOpenPanel()
        let panelHelper = CustomPanelHelper(panel: panel)
        
        let pPanelSettings = lpSettings.bindMemory(to: PanelSettings.self, capacity: MemoryLayout<PanelSettings>.size)
        let ps = pPanelSettings.pointee
        let initialDir = getStringFromUtf16Array(textPointer: ps.initialDirectory)
        let initialFile = getStringFromUtf16Array(textPointer: ps.initialFile) as NSString
        
        if (targetWindow != nil) {
            if (state.isTopmost) {
                // Temporarily disable always on top in order to show the dialog
                targetWindow?.level = NSWindow.Level.floating
            }
            // ↓　panel.parent を設定すると accessoryView が見えなくなってしまうためコメントアウト。問題なければ後日削除
            // Set attached window as the parent
            //panel.parent = targetWindow
        } else {
            // Find my window if the window is not attached
            //let myWindow: NSWindow? = NSApp.orderedWindows.first
            //panel.parent = myWindow
        }
        
        panel.allowsMultipleSelection = PanelFlag.AllowMultipleSelection.containedIn(value: ps.flags)
        panel.showsHiddenFiles = PanelFlag.ShowHidden.containedIn(value: ps.flags)
        //panel.allowedFileTypes = fileTypes
        panelHelper.addFileTypes(text: getStringFromUtf16Array(textPointer: ps.filterText))
        panel.isAccessoryViewDisclosed = true   // これをしないと Options ボタンを押すまでファイルタイプ選択が出ない
        
        panel.message = getStringFromUtf16Array(textPointer: ps.titleText)
        //panel.title = getStringFromUtf16Array(textPointer: ps.titleText)
        
        if (initialDir != "") {
            panel.directoryURL = URL(fileURLWithPath: initialDir, isDirectory: true)
        } else if (initialFile.deletingLastPathComponent != "") {
            panel.directoryURL = URL(fileURLWithPath: initialFile.deletingLastPathComponent, isDirectory: true)
        }
        panel.nameFieldStringValue = initialFile.lastPathComponent
        
        panel.canChooseFiles = true
        panel.canChooseDirectories = false
        panel.allowsOtherFileTypes = false
        panel.canCreateDirectories = true
        //panel.showsTagField = false
        panel.allowsOtherFileTypes = false
        panel.level = NSWindow.Level.popUpMenu
        panel.orderFrontRegardless()
        panel.center()
        
        let result = panel.runModal();
        
        var text: String = ""
        if (result == .OK) {
            if (panel.urls.count > 0) {
                // Make new-line separated string
                for url in panel.urls {
                    text += "\"" + url.path.replacingOccurrences(of: "\"", with: "\"\"") + "\"\n"
                }
            }
        }
        if (targetWindow != nil) {
            if (state.isTopmost) {
                // Re-enable always on top
                targetWindow?.level = NSWindow.Level.popUpMenu
            }
            if (state.isBorderless) {
                _makeKeyWindow()
                //                // Restore the key window state. NSWindow.canBecomeKeyWindow is false by default for borderless window, so makeKey() is unavailable...
                //                state.isBorderless = false;     // Suppress the callback
                //                setBorderless(isBorderless: false)
                //                state.isBorderless = true;      // Suppress the callback
                //                setBorderless(isBorderless: true)
            }
            targetWindow?.makeKeyAndOrderFront(nil)
        }
        
        return outputToStringBuffer(text: text, lpBuffer: lpBuffer, bufferSize: bufferSize)
    }
    
    /// Open file select dialog to save
    /// - Parameters:
    ///     - lpSettings: Pointer of PanelSettings
    ///     - lpBuffer: Pointer of UTF-16 string for output
    ///     - bufferSize: Size of UTF-16 string buffer
    @objc public static func openSavePanel(lpSettings: UnsafeRawPointer, lpBuffer: UnsafeMutablePointer<UniChar>?, bufferSize: UInt32) -> Bool {
        let panel = NSSavePanel()
        
        let pPanelSettings = lpSettings.bindMemory(to: PanelSettings.self, capacity: MemoryLayout<PanelSettings>.size)
        let ps = pPanelSettings.pointee;
        let initialDir = getStringFromUtf16Array(textPointer: ps.initialDirectory)
        let initialFile = getStringFromUtf16Array(textPointer: ps.initialFile) as NSString
        
        if (targetWindow != nil) {
            if (state.isTopmost) {
                // Temporarily disable always on top in order to show the dialog
                targetWindow?.level = NSWindow.Level.floating
            }
            // ↓　panel.parent を設定すると accessoryView が見えなくなってしまうためコメントアウト。問題なければ後日削除
            // Set attached window as the parent
            //panel.parent = targetWindow
        } else {
            // Find my window if the window is not attached
            //let myWindow: NSWindow = NSApp.orderedWindows[0]
            //panel.parent = myWindow
        }
        
        panel.showsHiddenFiles = PanelFlag.ShowHidden.containedIn(value: ps.flags)
        //panel.message = getStringFromUtf16Array(textPointer: ps.titleText)
        panel.title = getStringFromUtf16Array(textPointer: ps.titleText)
        if (initialDir != "") {
            panel.directoryURL = URL(fileURLWithPath: initialDir, isDirectory: true)
        } else if (initialFile.deletingLastPathComponent != "") {
            panel.directoryURL = URL(fileURLWithPath: initialFile.deletingLastPathComponent, isDirectory: true)
        }
        panel.nameFieldStringValue = initialFile.lastPathComponent
        panel.allowsOtherFileTypes = true
        
        panel.canCreateDirectories = true   //PanelFlag.CanCreateDirectories.containedIn(value: ps.flags)
        //panel.canSelectHiddenExtension = false
        //panel.showsTagField = false
        panel.level = NSWindow.Level.popUpMenu
        panel.orderFrontRegardless()
        panel.center()
        
        // ファイル種類選択欄を追加
        let panelHelper = CustomPanelHelper(panel: panel)
        panelHelper.addFileTypes(text: getStringFromUtf16Array(textPointer: ps.filterText))
        
        // ダイアログを開く
        let result = panel.runModal();
        
        var text: String = ""
        if (result == .OK && (panel.url != nil)) {
            let url: String = panel.url!.path
            text = "\"" + url.replacingOccurrences(of: "\"", with: "\"\"") + "\"\n"
        }
        if (targetWindow != nil) {
            if (state.isTopmost) {
                // Re-enable always on top
                targetWindow?.level = NSWindow.Level.popUpMenu
            }
            if (state.isBorderless) {
                _makeKeyWindow()
            }
            targetWindow?.makeKeyAndOrderFront(nil)
        }
        
        return outputToStringBuffer(text: text, lpBuffer: lpBuffer, bufferSize: bufferSize)
    }
    
    /// Parse an UTF-16 null terminated string pointer to String
    private static func getStringFromUtf16Array(textPointer: UnsafePointer<UniChar>?) -> String {
        if (textPointer == nil) {
            return ""
        }
        var len = 0
        while textPointer![len] != UniChar.zero {
            len += 1
        }
        return String(utf16CodeUnits: textPointer!, count: len)
    }
    
    /// Call a StringCallback with UTF-16 parameter
    /// - Parameters:
    ///   - callback: Registered callback function
    ///   - text: Parrameter as String
    /// - Returns: True if success
    public static func callStringCallback(callback: stringCallback?, text: String) -> Bool {
        if (callback == nil)
        {
            return false
        }
        
        let count = text.utf16.count
        if (count <= 0) {
            return false
        }
        
        let buffer = UnsafeMutablePointer<UniChar>.allocate(capacity: count + 1)
        var i = 0
        for c in text.utf16 {
            buffer[i] = c
            i += 1
        }
        buffer[count] = UniChar.zero     // End of the string
        
        // Do callback
        callback?(buffer)
        
        buffer.deallocate()
        return true
    }
    
    /// Return an UTF-16 string by using a pointer
    /// - Parameters:
    ///     - text: Parrameter as String
    ///     - lpBuffer: UTF-16 string buffer that allocated  by caller
    ///     - bufferSize: Size of the string buffer
    /// - Returns: True if success
    private static func outputToStringBuffer(text: String, lpBuffer: UnsafeMutablePointer<UniChar>?, bufferSize: UInt32) -> Bool {
        let size = Int(bufferSize)
        //let buffer = lpBuffer.bindMemory(to: UniChar.self, capacity: size)
        guard let buffer = lpBuffer else {
            return false
        }
        
        // Fill in zero
        for i in 0..<size {
            buffer[i] = UniChar.zero
        }
        
        let utf16text = text.utf16
        let count = utf16text.count
        if (count <= 0) {
            return false
        }
        
        var i = 0
        for c in utf16text {
            buffer[i] = c
            i += 1
        }
        return true
    }
    
    /// For Windows only
    @objc public static func attachWindowHandle(hwnd: UInt64) -> Bool {
        return true
    }
    
    /// Return some information for debugging
    @objc public static func getDebugInfo() -> Int32 {
        var result: Int32 = 0
        
        if (targetWindow != nil) {
            if (targetWindow!.canBecomeMain) { result += 1 }
            if (targetWindow!.canBecomeKey) { result += 2 }
            if (targetWindow!.isKeyWindow) { result += 4 }
            
            //            // styleMaskの値を調べる
            //            result = Int32(targetWindow!.styleMask.rawValue)
        }
        return result
    }
}
