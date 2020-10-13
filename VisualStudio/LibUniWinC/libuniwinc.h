#pragma once

#ifdef LIBUNIWINC_EXPORTS
#define UNIWINC_API __stdcall
#define UNIWINC_EXPORT extern "C" __declspec(dllexport)
#else
#define UNIWINC_API __stdcall
#define UNIWINC_EXPORT extern "C" __declspec(dllimport)
#endif

enum TransparentType : int {
	None = 0,
	Alpha = 1,
	ColorKey = 2
};

UNIWINC_EXPORT BOOL UNIWINC_API IsActive();
UNIWINC_EXPORT BOOL UNIWINC_API IsTransparent();
UNIWINC_EXPORT BOOL UNIWINC_API IsBorderless();
UNIWINC_EXPORT BOOL UNIWINC_API IsTopmost();
UNIWINC_EXPORT BOOL UNIWINC_API IsMaximized();

UNIWINC_EXPORT BOOL UNIWINC_API AttachMyWindow();
UNIWINC_EXPORT BOOL UNIWINC_API AttachMyOwnerWindow();
UNIWINC_EXPORT BOOL UNIWINC_API AttachMyActiveWindow();
UNIWINC_EXPORT BOOL UNIWINC_API DetachWindow();

UNIWINC_EXPORT void UNIWINC_API SetTransparent(const BOOL isTransparent);
UNIWINC_EXPORT void UNIWINC_API SetBorderless(const BOOL isBorderless);
UNIWINC_EXPORT void UNIWINC_API SetTopmost(const BOOL isTopmost);
UNIWINC_EXPORT void UNIWINC_API SetClickThrough(const BOOL isTransparent);
UNIWINC_EXPORT void UNIWINC_API SetMaximized(const BOOL isZoomed);
UNIWINC_EXPORT BOOL UNIWINC_API SetPosition(const float x, const float y);
UNIWINC_EXPORT BOOL UNIWINC_API GetPosition(float* x, float* y);
UNIWINC_EXPORT BOOL UNIWINC_API SetSize(const float width, const float height);
UNIWINC_EXPORT BOOL UNIWINC_API GetSize(float* width, float* height);

// Mouse pointer
UNIWINC_EXPORT BOOL UNIWINC_API SetCursorPosition(const float x, const float y);
UNIWINC_EXPORT BOOL UNIWINC_API GetCursorPosition(float* x, float* y);

// Windows only
UNIWINC_EXPORT void UNIWINC_API SetTransparentType(const TransparentType type);
UNIWINC_EXPORT void UNIWINC_API SetKeyColor(const COLORREF color);

UNIWINC_EXPORT HWND UNIWINC_API GetWindowHandle();
UNIWINC_EXPORT DWORD UNIWINC_API GetMyProcessId();
