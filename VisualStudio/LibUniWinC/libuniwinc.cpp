// LibUniWinC.cpp

#include "pch.h"
#include "libuniwinc.h"


static HWND hTargetWnd_ = NULL;
static WINDOWINFO originalWindowInfo;
static WINDOWPLACEMENT originalWindowPlacement;
static SIZE originaiBorderSize;
static POINT ptVirtualScreen;
static SIZE szVirtualScreen;
static int nPrimaryMonitorHeight;
static BOOL bIsTransparent = FALSE;
static BOOL bIsBorderless= FALSE;
static BOOL bIsTopmost = FALSE;
static BOOL bIsClickThrough = FALSE;
static COLORREF dwKeyColor = 0x00000000;		// AABBGGRR
static TransparentType nTransparentType = TransparentType::Alpha;
static TransparentType nCurrentTransparentType = TransparentType::Alpha;


void attachWindow(const HWND hWnd);
void detachWindow();
void refreshWindow();
void updateScreenSize();


/// <summary>
/// 既にウィンドウが選択済みなら、元の状態に戻して選択を解除
/// </summary>
void detachWindow()
{
	if (hTargetWnd_) {
		SetTransparent(false);
		
		SetWindowLong(hTargetWnd_, GWL_STYLE, originalWindowInfo.dwStyle);
		SetWindowLong(hTargetWnd_, GWL_EXSTYLE, originalWindowInfo.dwExStyle);

		SetWindowPlacement(hTargetWnd_, &originalWindowPlacement);

		refreshWindow();
	}
	hTargetWnd_ = NULL;
}

/// <summary>
/// 指定ハンドルのウィンドウを今後使うようにする
/// </summary>
/// <param name="hWnd"></param>
void attachWindow(const HWND hWnd) {
	// 選択済みウィンドウが異なるものであれば、元に戻す
	if (hTargetWnd_ != hWnd) {
		detachWindow();
	}

	// とりあえずこのタイミングで画面サイズも更新
	updateScreenSize();

	// ウィンドウを選択
	hTargetWnd_ = hWnd;

	if (hWnd) {
		// 初期状態を記憶しておく
		GetWindowInfo(hWnd, &originalWindowInfo);
		GetWindowPlacement(hWnd, &originalWindowPlacement);


		// 既に設定があれば適用
		SetTransparent(bIsTransparent);
		SetBorderless(bIsBorderless);
		SetTopmost(bIsTopmost);
		SetClickThrough(bIsClickThrough);
	}
}

/// <summary>
/// オーナーウィンドウハンドルを探す際のコールバック
/// </summary>
/// <param name="hWnd"></param>
/// <param name="lParam"></param>
/// <returns></returns>
BOOL CALLBACK findOwnerWindowProc(const HWND hWnd, const LPARAM lParam)
{
	DWORD currentPid = (DWORD)lParam;
	DWORD pid;
	GetWindowThreadProcessId(hWnd, &pid);

	// プロセスIDが一致すれば自分のウィンドウとする
	if (pid == currentPid) {

		// オーナーウィンドウを探す
		// Unityエディタだと本体が選ばれて独立Gameビューが選ばれない…
		HWND hOwner = GetWindow(hWnd, GW_OWNER);
		if (hOwner) {
			// あればオーナーを選択
			attachWindow(hOwner);
		}
		else {
			// オーナーが無ければこのウィンドウを選択
			attachWindow(hWnd);
		}
		return FALSE;

		//// 同じプロセスIDでも、表示されているウィンドウのみを選択
		//LONG style = GetWindowLong(hWnd, GWL_STYLE);
		//if (style & WS_VISIBLE) {
		//	hTargetWnd_ = hWnd;
		//	return FALSE;
		//}
	}

	return TRUE;
}

void enableTransparentByDWM()
{
	if (!hTargetWnd_) return;

	// 全面をGlassにする
	MARGINS margins = { -1 };
	DwmExtendFrameIntoClientArea(hTargetWnd_, &margins);
}

void disableTransparentByDWM()
{
	if (!hTargetWnd_) return;

	// 枠のみGlassにする
	//	※ 本来のウィンドウが何らかの範囲指定でGlassにしていた場合は、残念ながら表示が戻りません
	MARGINS margins = { 0, 0, 0, 0 };
	DwmExtendFrameIntoClientArea(hTargetWnd_, &margins);
}

/// <summary>
/// SetLayeredWindowsAttributes によって指定色を透過させる
/// </summary>
void enableTransparentBySetLayered()
{
	if (!hTargetWnd_) return;

	LONG exstyle = GetWindowLong(hTargetWnd_, GWL_EXSTYLE);
	exstyle |= WS_EX_LAYERED;
	SetWindowLong(hTargetWnd_, GWL_EXSTYLE, exstyle);
	SetLayeredWindowAttributes(hTargetWnd_, dwKeyColor, 0xFF, LWA_COLORKEY);
}

/// <summary>
/// SetLayeredWindowsAttributes による指定色透過を解除
/// </summary>
void disableTransparentBySetLayered()
{
	COLORREF cref = { 0 };
	SetLayeredWindowAttributes(hTargetWnd_, cref, 0xFF, LWA_ALPHA);

	LONG exstyle = originalWindowInfo.dwExStyle;
	//exstyle &= ~WinApi.WS_EX_LAYERED;
	SetWindowLong(hTargetWnd_, GWL_EXSTYLE, exstyle);
}

/// <summary>
/// 枠を消した際に描画サイズが合わなくなることに対応するため、ウィンドウを強制リサイズして更新
/// </summary>
void refreshWindow() {
	if (!hTargetWnd_) return;

	if (IsZoomed(hTargetWnd_)) {
		// 最大化されていた場合は、ウィンドウサイズ変更の代わりに一度最小化して再度最大化
		ShowWindow(hTargetWnd_, SW_MINIMIZE);
		ShowWindow(hTargetWnd_, SW_MAXIMIZE);
	}
	else if (IsIconic(hTargetWnd_)) {
		// 最小化されていた場合は、次に表示されるときにこうしんされるものとして、何もしない
	}
	else if (IsWindowVisible(hTargetWnd_)) {
		// 通常のウィンドウだった場合は、ウィンドウサイズを1px変えることで再描画

		// 現在のウィンドウサイズを取得
		RECT rect;
		GetWindowRect(hTargetWnd_, &rect);

		// 1px横幅を広げて、リサイズイベントを強制的に起こす
		SetWindowPos(
			hTargetWnd_,
			NULL,
			0, 0, (rect.right - rect.left + 1), (rect.bottom - rect.top + 1),
			SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS
		);

		// 元のサイズに戻す。この時もリサイズイベントは発生するはず
		SetWindowPos(
			hTargetWnd_,
			NULL,
			0, 0, (rect.right - rect.left), (rect.bottom - rect.top),
			SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS
		);

		ShowWindow(hTargetWnd_, SW_SHOW);
	}
}

BOOL compareRect(const RECT rcA, const RECT rcB) {
	return ((rcA.left == rcB.left) && (rcA.right == rcB.right) && (rcA.top == rcB.top) && (rcA.bottom == rcB.bottom));
}

/// <summary>
/// 現在の画面サイズを取得
/// </summary>
/// <returns></returns>
void updateScreenSize() {
	ptVirtualScreen.x = GetSystemMetrics(SM_XVIRTUALSCREEN);
	ptVirtualScreen.y = GetSystemMetrics(SM_YVIRTUALSCREEN);
	szVirtualScreen.cx = GetSystemMetrics(SM_CXVIRTUALSCREEN);
	szVirtualScreen.cy = GetSystemMetrics(SM_CYVIRTUALSCREEN);
	nPrimaryMonitorHeight = GetSystemMetrics(SM_CYSCREEN);
}

/// <summary>
/// Cocoaと同様のY座標に変換
/// 事前にプライマリーモニターの高さが取得できていることとする
/// </summary>
/// <param name="y"></param>
/// <returns></returns>
LONG calcFlippedY(LONG y) {
	return (nPrimaryMonitorHeight - y);
}

// Windows only

/// <summary>
/// 現在選択されているウィンドウハンドルを取得
/// </summary>
/// <returns></returns>
HWND UNIWINC_API GetWindowHandle() {
	return hTargetWnd_;
}

/// <summary>
/// 自分のプロセスIDを取得
/// </summary>
/// <returns></returns>
DWORD UNIWINC_API GetMyProcessId() {
	return GetCurrentProcessId();
}



// Public functions

/// <summary>
/// 利用可能な状態ならtrueを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsActive() {
	if (hTargetWnd_ && IsWindow(hTargetWnd_)) {
		return TRUE;
	}
	return FALSE;
}

/// <summary>
/// 透過にしているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsTransparent() {
	return bIsTransparent;
}

/// <summary>
/// 枠を消去しているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsBorderless() {
	return bIsBorderless;
}

/// <summary>
/// 最前面にしているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsTopmost() {
	return bIsTopmost;
}

/// <summary>
/// 最大化しているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsMaximized() {
	return (hTargetWnd_ && IsZoomed(hTargetWnd_));
}

/// <summary>
/// ウィンドウを元に戻して対象から解除
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API DetachWindow() {
	detachWindow();
	return true;
}

/// <summary>
/// 自分のウィンドウを探して選択（オーナーと同じ）
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API AttachMyWindow() {
	return AttachMyOwnerWindow();
}

/// <summary>
/// 現在のプロセスIDを持つオーナーウィンドウを探して選択
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API AttachMyOwnerWindow() {
	DWORD currentPid = GetCurrentProcessId();
	return EnumWindows(findOwnerWindowProc, (LPARAM)currentPid);
}

/// <summary>
/// 現在アクティブ、かつプロセスIDが一致するウィンドウを選択
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API AttachMyActiveWindow() {
	DWORD currentPid = GetCurrentProcessId();
	HWND hWnd = GetActiveWindow();
	DWORD pid;

	GetWindowThreadProcessId(hWnd, &pid);
	if (pid == currentPid) {
		attachWindow(hWnd);
		return TRUE;
	}
	return FALSE;
}

/// <summary>
/// 透明化方法の選択を変更
/// </summary>
/// <param name="type"></param>
/// <returns></returns>
void UNIWINC_API SetTransparentType(const TransparentType type) {
	if (bIsTransparent) {
		// 透明化状態であれば、一度解除してから設定
		SetTransparent(FALSE);
		nTransparentType = type;
		SetTransparent(TRUE);
	}
	else {
		// 透明化状態でなければ、そのまま設定
		nTransparentType = type;
	}
}

/// <summary>
/// 単色透過時に透過とする色を指定
/// </summary>
/// <param name="color">透過する色</param>
/// <returns></returns>
void UNIWINC_API SetKeyColor(const COLORREF color) {
	if (bIsTransparent && (nTransparentType == TransparentType::ColorKey)) {
		// 透明化状態であれば、一度解除してから設定
		SetTransparent(FALSE);
		dwKeyColor = color;
		SetTransparent(TRUE);
	}
	else {
		// 透明化状態でなければ、そのまま設定
		dwKeyColor = color;
	}
}

/// <summary>
/// 透過および枠消しを設定／解除
/// </summary>
/// <param name="bTransparent"></param>
/// <returns></returns>
void UNIWINC_API SetTransparent(const BOOL bTransparent) {
	if (hTargetWnd_) {
		if (bTransparent) {
			switch (nTransparentType)
			{
			case Alpha:
				enableTransparentByDWM();
				break;
			case ColorKey:
				enableTransparentBySetLayered();
				break;
			default:
				break;
			}
		}
		else {
			switch (nCurrentTransparentType)
			{
			case Alpha:
				disableTransparentByDWM();
				break;
			case ColorKey:
				disableTransparentBySetLayered();
				break;
			default:
				break;
			}
		}

		// 戻す方法を決めるため、透明化が変更された時のタイプを記憶
		nCurrentTransparentType = nTransparentType;
	}

	// 透明化状態を記憶
	bIsTransparent = bTransparent;
}


/// <summary>
/// ウィンドウ枠を有効／無効にする
/// </summary>
/// <param name="bBorderless"></param>
void UNIWINC_API SetBorderless(const BOOL bBorderless) {
	if (hTargetWnd_) {
		int newW, newH, newX, newY;
		RECT rcWin, rcCli;
		GetWindowRect(hTargetWnd_, &rcWin);
		GetClientRect(hTargetWnd_, &rcCli);

		int w = rcWin.right - rcWin.left;
		int h = rcWin.bottom - rcWin.top;

		int bZoomed = IsZoomed(hTargetWnd_);
		int bIconic = IsIconic(hTargetWnd_);

		// 最大化されていたら、一度最大化は解除
		if (bZoomed) {
			ShowWindow(hTargetWnd_, SW_NORMAL);
		}

		if (bBorderless) {
			// 枠無しウィンドウにする
			LONG currentWS = (WS_VISIBLE | WS_POPUP);
			SetWindowLong(hTargetWnd_, GWL_STYLE, currentWS);

			newW = rcCli.right - rcCli.left;
			newH = rcCli.bottom - rcCli.top;

			int bw = (w - newW) / 2;	// 枠の片側幅 [px]
			newX = rcWin.left + bw;
			newY = rcWin.top + ((h - newH) - bw);	// 本来は枠の下側高さと左右の幅が同じ保証はないが、とりあえず同じとみなしている

		}
		else {
			// ウィンドウスタイルを戻す
			SetWindowLong(hTargetWnd_, GWL_STYLE, originalWindowInfo.dwStyle);

			int dx = (originalWindowInfo.rcWindow.right - originalWindowInfo.rcWindow.left) - (originalWindowInfo.rcClient.right - originalWindowInfo.rcClient.left);
			int dy = (originalWindowInfo.rcWindow.bottom - originalWindowInfo.rcWindow.top) - (originalWindowInfo.rcClient.bottom - originalWindowInfo.rcClient.top);
			int bw = dx / 2;	// 枠の片側幅 [px]

			newW = rcCli.right - rcCli.left + dx;
			newH = rcCli.bottom- rcCli.top + dy;

			newX = rcWin.left - bw;
			newY = rcWin.top - (dy - bw);	// 本来は枠の下側高さと左右の幅が同じ保証はないが、とりあえず同じとみなしている
		}

		// ウィンドウサイズが変化しないか、最大化や最小化状態なら標準のサイズ更新
		if (bZoomed) {
			// 最大化されていたら、ここで再度最大化
			ShowWindow(hTargetWnd_, SW_MAXIMIZE);
		} else if (bIconic) {
			// 最小化されていたら、次に表示されるときの再描画を期待して、何もしない
		} else if (newW == w && newH == h) {
			// ウィンドウ再描画
			refreshWindow();
		}
		else
		{
			// クライアント領域サイズを維持するようサイズと位置を調整
			SetWindowPos(
				hTargetWnd_,
				NULL,
				newX, newY, newW, newH,
				SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS
			);

			ShowWindow(hTargetWnd_, SW_SHOW);
		}
	}

	// 枠無しかを記憶
	bIsBorderless = bBorderless;
}

/// <summary>
/// 最前面化／解除
/// </summary>
/// <param name="bTopmost"></param>
/// <returns></returns>
void UNIWINC_API SetTopmost(const BOOL bTopmost) {
	if (hTargetWnd_) {
		SetWindowPos(
			hTargetWnd_,
			(bTopmost ? HWND_TOPMOST : HWND_NOTOPMOST),
			0, 0, 0, 0,
			SWP_NOSIZE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS // | SWP_FRAMECHANGED
		);
	}

	bIsTopmost = bTopmost;
}

/// <summary>
/// 最大化／解除
/// </summary>
/// <param name="bZoomed"></param>
/// <returns></returns>
void UNIWINC_API SetMaximized(const BOOL bZoomed) {
	if (hTargetWnd_) {
		if (bZoomed) {
			ShowWindow(hTargetWnd_, SW_MAXIMIZE);
		}
		else
		{
			ShowWindow(hTargetWnd_, SW_NORMAL);
		}
	}
}

void UNIWINC_API SetClickThrough(const BOOL bTransparent) {
	if (hTargetWnd_) {
		if (bTransparent) {
			LONG exstyle = GetWindowLong(hTargetWnd_, GWL_EXSTYLE);
			exstyle |= WS_EX_TRANSPARENT;
			exstyle |= WS_EX_LAYERED;
			SetWindowLong(hTargetWnd_, GWL_EXSTYLE, exstyle);
		}
		else
		{
			LONG exstyle = GetWindowLong(hTargetWnd_, GWL_EXSTYLE);
			exstyle &= ~WS_EX_TRANSPARENT;
			if (!bIsTransparent && !(originalWindowInfo.dwExStyle & WS_EX_LAYERED)) {
				exstyle &= ~WS_EX_LAYERED;
			}
			SetWindowLong(hTargetWnd_, GWL_EXSTYLE, exstyle);
		}
	}
	bIsClickThrough = bTransparent;
}

/// <summary>
/// ウィンドウ座標を設定
/// </summary>
/// <param name="x">ウィンドウ左端座標 [px]</param>
/// <param name="y">プライマリー画面下端を原点とし、上が正のY座標 [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API SetPosition(const float x, const float y) {
	if (hTargetWnd_ == NULL) return FALSE;

	// 現在のウィンドウ位置とサイズを取得
	RECT rect;
	GetWindowRect(hTargetWnd_, &rect);

	// 引数の y はCocoa相当の座標系でウィンドウ左下なので、変換
	int newY = (nPrimaryMonitorHeight - (int)y) - (rect.bottom - rect.top);

	return SetWindowPos(
		hTargetWnd_, NULL,
		(int)x, (int)newY,
		0, 0,
		SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOZORDER | SWP_ASYNCWINDOWPOS
		);
}

/// <summary>
/// ウィンドウ座標を取得
/// </summary>
/// <param name="x">ウィンドウ左端座標 [px]</param>
/// <param name="y">プライマリー画面下端を原点とし、上が正のY座標 [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API GetPosition(float* x, float* y) {
	*x = 0;
	*y = 0;

	if (hTargetWnd_ == NULL) return FALSE;

	RECT rect;
	if (GetWindowRect(hTargetWnd_, &rect)) {
		*x = (float)rect.left;
		*y = (float)(nPrimaryMonitorHeight - rect.bottom);	// 左下基準とする
		return TRUE;
	}
	return FALSE;
}

/// <summary>
/// ウィンドウサイズを設定
/// </summary>
/// <param name="width">幅 [px]</param>
/// <param name="height">高さ [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API SetSize(const float width, const float height) {
	if (hTargetWnd_ == NULL) return FALSE;

	// 現在のウィンドウ位置とサイズを取得
	RECT rect;
	GetWindowRect(hTargetWnd_, &rect);

	// 左下原点とするために調整した、新規Y座標
	int newY = rect.bottom - (int)height;

	return SetWindowPos(
		hTargetWnd_, NULL,
		rect.left, newY,
		(int)width, (int)height,
		SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_ASYNCWINDOWPOS
	);
}


/// <summary>
/// ウィンドウサイズを取得
/// </summary>
/// <param name="width">幅 [px]</param>
/// <param name="height">高さ [px]</param>
/// <returns>成功すれば true</returns>
BOOL  UNIWINC_API GetSize(float* width, float* height) {
	*width = 0;
	*height = 0;

	if (hTargetWnd_ == NULL) return FALSE;

	RECT rect;
	if (GetWindowRect(hTargetWnd_, &rect)) {
		*width = (float)(rect.right - rect.left);
		*height = (float)(rect.bottom - rect.top);
		return TRUE;
	}
	return FALSE;
}

/// <summary>
/// マウスカーソル座標を取得
/// </summary>
/// <param name="x">ウィンドウ左端座標 [px]</param>
/// <param name="y">プライマリー画面下端を原点とし、上が正のY座標 [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API GetCursorPosition(float* x, float* y) {
	*x = 0;
	*y = 0;

	POINT pos;
	if (GetCursorPos(&pos)) {
		*x = (float)pos.x;
		*y = (float)(nPrimaryMonitorHeight - pos.y - 1);	// 左下基準とする
		return TRUE;
	}
	return FALSE;

}

/// <summary>
/// マウスカーソル座標を設定
/// </summary>
/// <param name="x">ウィンドウ左端座標 [px]</param>
/// <param name="y">プライマリー画面下端を原点とし、上が正のY座標 [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API SetCursorPosition(const float x, const float y) {
	POINT pos;

	pos.x = (int)x;
	pos.y = nPrimaryMonitorHeight - (int)y - 1;

	return SetCursorPos(pos.x, pos.y);
}
