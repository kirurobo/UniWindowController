//
//  OverlayView.swift
//  LibUniWinC
//
//  Created by Kirurobo on 2020/11/28.
//  Copyright © 2020 Kirurobo. All rights reserved.
//

import Cocoa

protocol FileDroppedDelegate {
    func complete(result: String)
}

class OverlayView: NSView {
    // Reference: https://stackoverflow.com/questions/31657523/get-file-path-using-drag-and-drop-swift-macos
    
    var pathsString: String = ""
    var enabled = true

    public func setEnabled(enabled: Bool) {
        self.enabled = enabled
    }
    
    private func setup() {
        self.registerForDraggedTypes(
            [NSPasteboard.PasteboardType.URL, NSPasteboard.PasteboardType.fileURL]
        )
    }
    
    public func fitToSuperView() -> Void {
        if (superview == nil) {
            return
        }
        
        // Fit to the parent frame
        let constraints = [
            self.centerXAnchor.constraint(equalTo: superview!.centerXAnchor),
            self.centerYAnchor.constraint(equalTo: superview!.centerYAnchor),
            self.widthAnchor.constraint(equalTo: superview!.widthAnchor),
            self.heightAnchor.constraint(equalTo: superview!.heightAnchor)
        ]
        NSLayoutConstraint.activate(constraints)
    }
    
    required init?(coder: NSCoder) {
        super.init(coder: coder)
        setup()
    }
    
    override init(frame: NSRect) {
        super.init(frame: frame)
        setup()
    }
    
    override func draggingEntered(_ sender: NSDraggingInfo) -> NSDragOperation {
        if (self.enabled) {
            return .copy
        } else {
            return .generic
        }
    }
    
    override func performDragOperation(_ sender: NSDraggingInfo) -> Bool {
        if (!self.enabled) {
            return false
        }
        
        guard
            let urls = sender.draggingPasteboard.propertyList(
                forType: NSPasteboard.PasteboardType(rawValue: "NSFilenamesPboardType")
            ) as? NSArray
        else {
            return false
        }
        
        // Make new-line separated string
        let files: String = urls.componentsJoined(by: "¥n")
        
        guard let ustr = files.data(using: .utf8)
        else {
            return false
        }
        
        let buffer = UnsafeMutablePointer<uint8>.allocate(capacity: ustr.count + 1)
        for i in 0..<ustr.count {
            buffer[i] = ustr[i]
            //buffer[i] = wchar_t.zero
        }
        buffer[ustr.count] = uint8.zero
        
        // Do callback
        LibUniWinC.fileDropCallback?(buffer)
        
        buffer.deallocate()
        
        // Store file paths
        self.pathsString = files
        return true
    }
    
    override func draw(_ dirtyRect: NSRect) {
        super.draw(dirtyRect)

        // Drawing code here.
        NSColor.red.set()
        let figure = NSBezierPath()
        figure.move(to: dirtyRect.origin)
        figure.line(to: NSMakePoint(dirtyRect.width, dirtyRect.height))
        figure.line(to: NSMakePoint(dirtyRect.width - 5, dirtyRect.height))
        figure.line(to: NSMakePoint(dirtyRect.width, dirtyRect.height - 5))
        figure.line(to: NSMakePoint(dirtyRect.width, dirtyRect.height))
        figure.lineWidth = 2
        figure.stroke()
    }
}

extension String {
    func withWideChars<Result>(_ body: (UnsafePointer<wchar_t>) -> Result) -> Result {
        let unicodestr = self.unicodeScalars.map { wchar_t(bitPattern: $0.value) } + [0]
        return unicodestr.withUnsafeBufferPointer { body($0.baseAddress!) }
    }
}
