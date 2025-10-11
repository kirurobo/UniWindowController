#pragma once

#ifdef LIBUNIWINC_EXPORTS
#define UNIWINC_API __stdcall
#define UNIWINC_EXPORT extern "C" __declspec(dllexport)
#else
#define UNIWINC_API __stdcall
#define UNIWINC_EXPORT extern "C" __declspec(dllimport)
#endif


// Maximum monitor number that this library could be handle
#define UNIWINC_MAX_MONITORCOUNT 32

// Maximum length for a classname
#define UNIWINC_MAX_CLASSNAME 32


// Methods to transparent the window
enum class TransparentType : int {
	None = 0,
	Alpha = 1,
	ColorKey = 2,
};

// State changed event type (Experimental)
enum class WindowStateEventType : int {
	None = 0,
	StyleChanged = 1,
	Resized = 2,

	// 以下はStyleChangedに加えて詳細情報を伝えるために用意（開発時のデバッグ用途が主のため今後の仕様変更はありえる）
	//   1: StyleChanged flag, 8: Enabled flag
	TopMostEnabled = 16 + 1 + 8,
	TopMostDisabled = 16 + 1,
	BottomMostEnabled = 32 + 1 + 8,
	BottomMostDisabled = 32 + 1,
	WallpaperModeEnabled = 64 + 1 + 8,
	WallpaperModeDisabled = 64 + 1,
};

enum class PanelFlag : int {
	None = 0,
	FileMustExist = 1,
	FolderMustExist = 2,
	AllowMultiSelect = 4,
	OverwritePrompt = 256,
	CreatePrompt = 512,
	ShowHidden = 4096,
	ReferLink = 8192,
};

// Struct to transmit file panel settings
#pragma pack(push, 1)
typedef struct tagPANELSETTINGS {
	INT32 nStructSize;
	INT32 nFlags;
	LPWSTR lpszTitle;
	LPWSTR lpszFilter;
	LPWSTR lpszInitialFile;
	LPWSTR lpszInitialDir;
	LPWSTR lpszDefaultExt;

} PANELSETTINGS, *PPANELSETTINGS;
#pragma pack(pop)

// Function called when window style (e.g. maximized, transparetize, etc.)
//   param: The argument is indicate the kind of event
using WindowStyleChangedCallback =  void(UNIWINC_API *)(INT32);

// Function called when files have selected
//   param: The argument is a \0 ended  UTF-16 string with each path separated by \n
using FilesCallback = void(UNIWINC_API *)(WCHAR*);

// Function called when displays have changed
//   param: The argument is the numbers of monitors
using MonitorChangedCallback = void(UNIWINC_API *)(INT32);


// Winodow state functions
UNIWINC_EXPORT BOOL UNIWINC_API IsActive();
UNIWINC_EXPORT BOOL UNIWINC_API IsTransparent();
UNIWINC_EXPORT BOOL UNIWINC_API IsBorderless();
UNIWINC_EXPORT BOOL UNIWINC_API IsTopmost();
UNIWINC_EXPORT BOOL UNIWINC_API IsBottommost();
UNIWINC_EXPORT BOOL UNIWINC_API IsBackground();
UNIWINC_EXPORT BOOL UNIWINC_API IsMaximized();
UNIWINC_EXPORT BOOL UNIWINC_API IsMinimized();
UNIWINC_EXPORT BOOL UNIWINC_API IsFreePositioningEnabled();
UNIWINC_EXPORT void UNIWINC_API Update();

UNIWINC_EXPORT BOOL UNIWINC_API AttachMyWindow();
UNIWINC_EXPORT BOOL UNIWINC_API AttachMyOwnerWindow();
UNIWINC_EXPORT BOOL UNIWINC_API AttachMyActiveWindow();
UNIWINC_EXPORT BOOL UNIWINC_API DetachWindow();

UNIWINC_EXPORT void UNIWINC_API SetTransparent(const BOOL isTransparent);
UNIWINC_EXPORT void UNIWINC_API SetBorderless(const BOOL isBorderless);
UNIWINC_EXPORT void UNIWINC_API SetAlphaValue(const float alpha);
UNIWINC_EXPORT void UNIWINC_API SetTopmost(const BOOL isTopmost);
UNIWINC_EXPORT void UNIWINC_API SetBottommost(const BOOL isBottommost);
UNIWINC_EXPORT void UNIWINC_API SetBackground(const BOOL isBackground);
UNIWINC_EXPORT void UNIWINC_API SetClickThrough(const BOOL isTransparent);
UNIWINC_EXPORT void UNIWINC_API SetMaximized(const BOOL isZoomed);
UNIWINC_EXPORT void UNIWINC_API EnableFreePositioning(const BOOL isFree);
UNIWINC_EXPORT BOOL UNIWINC_API SetPosition(const float x, const float y);
UNIWINC_EXPORT BOOL UNIWINC_API GetPosition(float* x, float* y);
UNIWINC_EXPORT BOOL UNIWINC_API SetSize(const float width, const float height);
UNIWINC_EXPORT BOOL UNIWINC_API GetSize(float* width, float* height);
UNIWINC_EXPORT BOOL UNIWINC_API GetClientSize(float* width, float* height);
UNIWINC_EXPORT BOOL UNIWINC_API GetClientRectangle(float* x, float* y, float* width, float* height);
UNIWINC_EXPORT INT32 UNIWINC_API GetCurrentMonitor();

// Event handling
UNIWINC_EXPORT BOOL UNIWINC_API RegisterWindowStyleChangedCallback(WindowStyleChangedCallback callback);
UNIWINC_EXPORT BOOL UNIWINC_API UnregisterWindowStyleChangedCallback();
UNIWINC_EXPORT BOOL UNIWINC_API RegisterMonitorChangedCallback(MonitorChangedCallback callback);
UNIWINC_EXPORT BOOL UNIWINC_API UnregisterMonitorChangedCallback();
UNIWINC_EXPORT BOOL UNIWINC_API RegisterDropFilesCallback(FilesCallback callback);
UNIWINC_EXPORT BOOL UNIWINC_API UnregisterDropFilesCallback();


// Monitor Info.
UNIWINC_EXPORT INT32 UNIWINC_API GetMonitorCount();
UNIWINC_EXPORT BOOL UNIWINC_API GetMonitorRectangle(const INT32 monitorIndex, float* x, float* y, float* width, float* height);

// Mouse pointer
UNIWINC_EXPORT BOOL UNIWINC_API SetCursorPosition(const float x, const float y);
UNIWINC_EXPORT BOOL UNIWINC_API GetCursorPosition(float* x, float* y);
UNIWINC_EXPORT INT32 UNIWINC_API GetMouseButtons();

// Keyboard
UNIWINC_EXPORT INT32 UNIWINC_API GetModifierKeys();

// File drop
UNIWINC_EXPORT BOOL UNIWINC_API SetAllowDrop(const BOOL bEnabled);

// File panels
UNIWINC_EXPORT BOOL UNIWINC_API OpenFilePanel(const PPANELSETTINGS pSettings, LPWSTR pResultBuffer, const UINT32 nBufferSize);
UNIWINC_EXPORT BOOL UNIWINC_API OpenSavePanel(const PPANELSETTINGS pSettings, LPWSTR pResultBuffer, const UINT32 nBufferSize);

// Debug function
UNIWINC_EXPORT INT32 UNIWINC_API GetDebugInfo();


// Windows only
UNIWINC_EXPORT void UNIWINC_API SetTransparentType(const TransparentType type);
UNIWINC_EXPORT void UNIWINC_API SetKeyColor(const COLORREF color);
UNIWINC_EXPORT HWND UNIWINC_API GetWindowHandle();
UNIWINC_EXPORT HWND UNIWINC_API GetDesktopWindowHandle();
UNIWINC_EXPORT DWORD UNIWINC_API GetMyProcessId();
UNIWINC_EXPORT BOOL UNIWINC_API AttachWindowHandle(const HWND);
