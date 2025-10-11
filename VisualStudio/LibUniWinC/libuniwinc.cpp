// LibUniWinC.cpp

#include "pch.h"
#include "libuniwinc.h"


static HWND hTargetWnd_ = NULL;
static HWND hPanelOwnerWnd_ = NULL;
static WINDOWINFO originalWindowInfo_;
static WINDOWPLACEMENT originalWindowPlacement_;
static HWND hParentWnd_ = NULL;
static BOOL bExpedtDesktopWnd = FALSE;
static HWND hDesktopWnd_ = NULL;
static SIZE szOriginaiBorder_;
static POINT ptVirtualScreen_;
static SIZE szVirtualScreen_;
static INT nPrimaryMonitorHeight_;
static BOOL bIsTransparent_ = FALSE;
static BOOL bIsBorderless_ = FALSE;
static BYTE byAlpha_ = 0xFF;							// ウィンドウ全体の透明度 0x00:透明 ～ 0xFF:不透明
static BOOL bIsTopmost_ = FALSE;
static BOOL bIsBottommost_ = FALSE;
static BOOL bIsBackground_ = FALSE;
static BOOL bIsFreePositioning_ = FALSE;				// macOSのみ有効。Windowsでは値の保持のみ
static BOOL bIsClickThrough_ = FALSE;
static BOOL bAllowDropFile_ = FALSE;
static COLORREF dwKeyColor_ = 0x00000000;				// AABBGGRR
static TransparentType nTransparentType_ = TransparentType::Alpha;
static TransparentType nCurrentTransparentType_ = TransparentType::Alpha;
static INT nMonitorCount_ = 0;							// モニタ数。モニタ解像度一覧取得時は一時的に0に戻る
static RECT pMonitorRect_[UNIWINC_MAX_MONITORCOUNT];	// EnumDisplayMonitorsの順番で保持した、各画面のRECT
static INT pMonitorIndices_[UNIWINC_MAX_MONITORCOUNT];	// このライブラリ独自のモニタ番号をキーとした、EnumDisplayMonitorsでの順番
static HMONITOR hMonitors_[UNIWINC_MAX_MONITORCOUNT];	// Monitor handles
static WNDPROC lpMyWndProc_ = NULL;
static WNDPROC lpOriginalWndProc_ = NULL;
//static HHOOK hHook_ = NULL;
static WindowStyleChangedCallback hWindowStyleChangedHandler_ = nullptr;
static MonitorChangedCallback hMonitorChangedHandler_ = nullptr;
static FilesCallback hDropFilesHandler_ = nullptr;


// ========================================================================
#pragma region Internal functions

void attachWindow(const HWND hWnd);
void detachWindow();
void refreshWindowRect();
void updateScreenSize();
void applyWindowAlphaValue();
//void beginHook();
//void endHook();
void createCustomWindowProcedure();
void destroyCustomWindowProcedure();


/// <summary>
/// 既にウィンドウが選択済みなら、元の状態に戻して選択を解除
/// </summary>
void detachWindow()
{
	if (hTargetWnd_) {
		// Restore the original window procedure
		destroyCustomWindowProcedure();

		//// Unhook if exist
		//endHook();

		if (IsWindow(hTargetWnd_)) {
			// 透明化は、起動時は無効であるものとして、戻すときは無効化
			SetTransparent(FALSE);

			//// 壁紙化が試みられていればウィンドウの親を戻す
			//if (hDesktopWnd_ != NULL) {
			//	SetParent(hTargetWnd_, hParentWnd_);
			//}

			//// 常に最前面は、起動時の状態に合わせるよう戻す	↓SetWindowLongで本来戻るはずで不要？
			//SetTopmost((originalWindowInfo.dwExStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST);

			// 最初のスタイルに戻す
			SetWindowLong(hTargetWnd_, GWL_STYLE, originalWindowInfo_.dwStyle);
			SetWindowLong(hTargetWnd_, GWL_EXSTYLE, originalWindowInfo_.dwExStyle);

			// ウィンドウ位置を戻す
			SetWindowPlacement(hTargetWnd_, &originalWindowPlacement_);

			// 表示を更新
			refreshWindowRect();
		}
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
	//   本来は画面解像度変更時に更新したい。ウィンドウプロシージャでどう？
	updateScreenSize();

	// Set the target
	hTargetWnd_ = hWnd;

	if (hWnd) {
		// Save the original state
		GetWindowInfo(hWnd, &originalWindowInfo_);
		GetWindowPlacement(hWnd, &originalWindowPlacement_);
		//hParentWnd_ = GetParent(hWnd);

		// Apply current settings
		applyWindowAlphaValue();
		SetTransparent(bIsTransparent_);
		SetBorderless(bIsBorderless_);
		SetTopmost(bIsTopmost_);
		SetBottommost(bIsBottommost_);
		//SetBackground(bIsBackground_);
		SetClickThrough(bIsClickThrough_);
		SetAllowDrop(bAllowDropFile_);

		// Replace the window procedure
		createCustomWindowProcedure();
	}
}


/// オーナーウィンドウハンドルを探してアタッチする際のコールバック
/// </summary>
/// <param name="hWnd"></param>
/// <param name="lParam"></param>
/// <returns></returns>
BOOL CALLBACK attachOwnerWindowProc(const HWND hWnd, const LPARAM lParam)
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
			hPanelOwnerWnd_ = hOwner;
		}
		else {
			// オーナーが無ければこのウィンドウを選択
			hPanelOwnerWnd_ = hWnd;
		}
		return FALSE;
	}

	return TRUE;
}

/// <summary>
/// デスクトップのウィンドウハンドルを探す際のコールバック
/// </summary>
/// <param name="hWnd"></param>
/// <param name="lParam"></param>
/// <returns></returns>
BOOL CALLBACK findDesktopWindowProc(const HWND hWnd, const LPARAM lParam)
{
	WCHAR className[UNIWINC_MAX_CLASSNAME];
	int len = GetClassName(hWnd, className, UNIWINC_MAX_CLASSNAME);

	if (len > 0) {
		// クラス名が取得でき、WorkerW または Progman ならその子で SHELLDLL_DefView を対象とする
		// 参考 http://www.orangemaker.sakura.ne.jp/labo/memo/sdk-mfc/win7Desktop.html
		if ((lstrcmp(TEXT("WorkerW"), className) == 0) || (lstrcmp(TEXT("Progman"), className) == 0)) {
			if (bExpedtDesktopWnd) {
				hDesktopWnd_ = hWnd;
				return FALSE;
			}

			HWND hChild = FindWindowEx(hWnd, NULL, TEXT("SHELLDLL_DefView"), NULL);
			if (hChild != NULL) {
				//hDesktopWnd_ = hChild;
				//return FALSE;

				bExpedtDesktopWnd = TRUE;
				return TRUE;
			}
		}
	}

	return TRUE;
}

/// <summary>
/// モニタ情報取得時のコールバック
/// EnumDisplayMonitors()で呼ばれる。その際は最初にnMonitorCountが0にセットされるものとする。
/// </summary>
/// <param name="hMon"></param>
/// <param name="hDc"></param>
/// <param name="lpRect"></param>
/// <param name="lParam"></param>
/// <returns></returns>
BOOL CALLBACK monitorEnumProc(HMONITOR hMon, HDC hDc, LPRECT lpRect, LPARAM lParam)
{
	// 最大取り扱いモニタ数に達したら探索終了
	if (nMonitorCount_ >= UNIWINC_MAX_MONITORCOUNT) return FALSE;

	// RECTを記憶
	pMonitorRect_[nMonitorCount_] = *lpRect;

	// プライマリモニタの高さを記憶
	if (lpRect->left == 0 && lpRect->top == 0) {
		// 原点に位置するモニタがプライマリモニタだと判断
		nPrimaryMonitorHeight_ = lpRect->bottom;
	}

	// インデックスを一旦登場順で保存
	pMonitorIndices_[nMonitorCount_] = nMonitorCount_;

	// Store the monitor handle
	hMonitors_[nMonitorCount_] = hMon;

	// モニタ数カウント
	nMonitorCount_++;

	return TRUE;
}

/// <summary>
/// 接続モニタ数とそれらのサイズ一覧を取得
/// </summary>
/// <returns>成功ならTRUE</returns>
BOOL updateMonitorRectangles() {
	//  カウントするため一時的に0に戻す
	nMonitorCount_ = 0;

	// モニタを列挙してRECTを保存
	if (!EnumDisplayMonitors(NULL, NULL, monitorEnumProc, NULL)) {
		return FALSE;
	}

	// モニタの位置を基準にバブルソート
	for (int i = 0; i < (nMonitorCount_ - 1); i++) {
		for (int j = (nMonitorCount_ - 1); j > i; j--) {
			RECT pr = pMonitorRect_[pMonitorIndices_[j - 1]];
			RECT cr = pMonitorRect_[pMonitorIndices_[j]];

			// 左にあるモニタが先、横が同じなら下にあるモニタが先となるようソート
			if (pr.left >  cr.left || ((pr.left == cr.left) && (pr.bottom < cr.bottom))) {
				int index = pMonitorIndices_[j - 1];
				pMonitorIndices_[j - 1] = pMonitorIndices_[j];
				pMonitorIndices_[j] = index;
			}
		}
	}

	return TRUE;
}

/// <summary>
/// 全面をGlassにする
/// </summary>
void enableTransparentByDWM()
{
	if (!hTargetWnd_) return;

	MARGINS margins = { -1 };
	DwmExtendFrameIntoClientArea(hTargetWnd_, &margins);
}

/// <summary>
/// 枠のみGlassにする
/// </summary>
void disableTransparentByDWM()
{
	if (!hTargetWnd_) return;

	// TODO: できれば決め打ちでは無くせるとよい
	//   本来のウィンドウが何らかの範囲指定でGlassにしていた場合は、残念ながら表示が戻りません
	MARGINS margins = { 0, 0, 0, 0 };
	DwmExtendFrameIntoClientArea(hTargetWnd_, &margins);
}

/// <summary>
/// DWM利用時またはウィンドウ非透過時の透明度設定
/// </summary>
void applyWindowAlphaValue() {
	if (!hTargetWnd_) return;

	// 半透明の場合、レイヤードウィンドウになっていなければ以降はレイヤードウィンドウにする
	if (byAlpha_ < 0xFF) {
		LONG exstyle = GetWindowLong(hTargetWnd_, GWL_EXSTYLE);

		// まだレイヤードウィンドウになっていなければ、設定
		if (!(exstyle & WS_EX_LAYERED)) {
			exstyle |= WS_EX_LAYERED;
			SetWindowLong(hTargetWnd_, GWL_EXSTYLE, exstyle);
		}
	}

	COLORREF cref = { 0 };
	SetLayeredWindowAttributes(hTargetWnd_, cref, byAlpha_, LWA_ALPHA);
}

/// <summary>
/// SetLayeredWindowsAttributes によって指定色を透過させる
/// </summary>
void enableTransparentBySetLayered()
{
	if (!hTargetWnd_) return;

	LONG exstyle = GetWindowLong(hTargetWnd_, GWL_EXSTYLE);

	// レイヤードウィンドウになっていなければ、設定
	if (!(exstyle & WS_EX_LAYERED)) {
		exstyle |= WS_EX_LAYERED;
		SetWindowLong(hTargetWnd_, GWL_EXSTYLE, exstyle);
	}

	SetLayeredWindowAttributes(hTargetWnd_, dwKeyColor_, byAlpha_, LWA_COLORKEY | LWA_ALPHA);
}

/// <summary>
/// SetLayeredWindowsAttributes による指定色透過を解除
/// </summary>
void disableTransparentBySetLayered()
{
	if (!hTargetWnd_) return;

	COLORREF cref = { 0 };
	SetLayeredWindowAttributes(hTargetWnd_, cref, byAlpha_, LWA_ALPHA);
}

/// <summary>
/// 壁紙の親となるウィンドウハンドルを取得
/// </summary>
void findDesktopWindow() {
	bExpedtDesktopWnd = FALSE;
	EnumWindows(findDesktopWindowProc, NULL);
}

/// <summary>
/// 枠を消した際に描画サイズが合わなくなることに対応するため、ウィンドウを強制リサイズして更新
/// </summary>
void refreshWindowRect() {
	if (!hTargetWnd_) return;

	if (IsZoomed(hTargetWnd_)) {
		// 最大化されていた場合は、ウィンドウサイズ変更の代わりに一度最小化して再度最大化
		ShowWindow(hTargetWnd_, SW_MINIMIZE);
		ShowWindow(hTargetWnd_, SW_MAXIMIZE);
	}
	else if (IsIconic(hTargetWnd_)) {
		// 最小化されていた場合は、次に表示されるときに更新されるものとして、何もしない
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
			SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS
		);

		// 元のサイズに戻す。この時もリサイズイベントは発生するはず
		SetWindowPos(
			hTargetWnd_,
			NULL,
			0, 0, (rect.right - rect.left), (rect.bottom - rect.top),
			SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS
		);

		ShowWindow(hTargetWnd_, SW_SHOW);
	}
}

BOOL compareRect(const RECT rcA, const RECT rcB) {
	return ((rcA.left == rcB.left) && (rcA.right == rcB.right) && (rcA.top == rcB.top) && (rcA.bottom == rcB.bottom));
}

/// <summary>
/// Update current monitor information
/// </summary>
/// <returns></returns>
void updateScreenSize() {
	//nPrimaryMonitorHeight_ = GetSystemMetrics(SM_CYSCREEN);	// 150% などの時は実解像度と一致しない

	// Update the monitor resolution list.
	//   To use the nPrimaryMonitorHeight, do this after its acquisition.
	updateMonitorRectangles();
}

/// <summary>
/// 現在、実際に常に最前面になっているかを調べる
/// </summary>
/// <returns></returns>
BOOL getTopMost() {
	if ((hTargetWnd_ == NULL) || !IsWindow(hTargetWnd_)) {
		return FALSE;
	}
	LONG ex = GetWindowLong(hTargetWnd_, GWL_EXSTYLE);
	return (ex & WS_EX_TOPMOST) == WS_EX_TOPMOST;
}

#pragma endregion Internal functions


// ========================================================================
#pragma region For window style

/// <summary>
/// ウィンドウ状態が置き換わっているか定期的に調べて、強制的に修正
/// </summary>
/// <returns></returns>
void UNIWINC_API Update() {
	// 今のところWindowsでは何もしない
	return;
}

/// <summary>
/// 利用可能な状態ならTRUEを返す
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
	return bIsTransparent_;
}

/// <summary>
/// 枠を消去しているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsBorderless() {
	return bIsBorderless_;
}

/// <summary>
/// 最前面にしているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsTopmost() {
	return bIsTopmost_;
}

/// <summary>
/// 最背面にしているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsBottommost() {
	return bIsBottommost_;
}

/// <summary>
/// 壁紙にしているか否かを返す
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsBackground() {
	return bIsBackground_;
}

/// <summary>
/// Return true if the window is zoomed
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsMaximized() {
	return (hTargetWnd_ && IsZoomed(hTargetWnd_));
}

/// <summary>
/// Return true if the window is iconic
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API IsMinimized() {
	return (hTargetWnd_ && IsIconic(hTargetWnd_));
}

/// <summary>
/// （macOSのみ有効）ウィンドウの自由配置機能が有効かどうかを返します
/// </summary>
/// <returns>Windowsでは特に制限なしのためTRUE</returns>
BOOL UNIWINC_API IsFreePositioningEnabled()
{
	return bIsFreePositioning_;
}

/// <summary>
/// Restore and release the target window
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API DetachWindow() {
	detachWindow();
	return TRUE;
}

/// <summary>
/// Find my own window and attach (Same as the AttachMyOwnerWindow)
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API AttachMyWindow() {
	return AttachMyOwnerWindow();
}

/// <summary>
/// Find and select the window with the current process ID
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API AttachMyOwnerWindow() {
	DWORD currentPid = GetCurrentProcessId();
	return EnumWindows(attachOwnerWindowProc, (LPARAM)currentPid);
}

/// <summary>
/// Find owner window handle
/// </summary>
/// <returns></returns>
HWND FindOwnerWindowHandle() {
	DWORD currentPid = GetCurrentProcessId();
	if (EnumWindows(attachOwnerWindowProc, (LPARAM)currentPid)) {
		return hPanelOwnerWnd_;
	}
	return NULL;
}

/// <summary>
/// Find and select the active window with the current process ID
///   (To attach the process with multiple windows)
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
/// Attach to the specified window
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API AttachWindowHandle(const HWND hWnd) {
	attachWindow(hWnd);
	return TRUE;
}

/// <summary>
/// Select the transparentize method
/// </summary>
/// <param name="type"></param>
/// <returns></returns>
void UNIWINC_API SetTransparentType(const TransparentType type) {
	if (bIsTransparent_) {
		// 透明化状態であれば、一度解除してから設定
		SetTransparent(FALSE);
		nTransparentType_ = type;
		SetTransparent(TRUE);
	}
	else {
		// 透明化状態でなければ、そのまま設定
		nTransparentType_ = type;
	}
}

/// <summary>
/// 単色透過時に透過とする色を指定
/// </summary>
/// <param name="color">透過する色</param>
/// <returns></returns>
void UNIWINC_API SetKeyColor(const COLORREF color) {
	if (bIsTransparent_ && (nTransparentType_ == TransparentType::ColorKey)) {
		// 透明化状態であれば、一度解除してから設定
		SetTransparent(FALSE);
		dwKeyColor_ = color;
		SetTransparent(TRUE);
	}
	else {
		// 透明化状態でなければ、そのまま設定
		dwKeyColor_ = color;
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
			switch (nTransparentType_)
			{
			case TransparentType::Alpha:
				enableTransparentByDWM();
				break;
			case TransparentType::ColorKey:
				enableTransparentBySetLayered();
				break;
			default:
				break;
			}
		}
		else {
			switch (nCurrentTransparentType_)
			{
			case TransparentType::Alpha:
				disableTransparentByDWM();
				break;
			case TransparentType::ColorKey:
				disableTransparentBySetLayered();
				break;
			default:
				break;
			}
		}

		// 戻す方法を決めるため、透明化が変更された時のタイプを記憶
		nCurrentTransparentType_ = nTransparentType_;
	}

	// 透明化状態を記憶
	bIsTransparent_ = bTransparent;
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

		newX = rcWin.left;
		newY = rcWin.top;
		int w = rcWin.right - rcWin.left;
		int h = rcWin.bottom - rcWin.top;

		bool hasMenu =  (GetMenu(hTargetWnd_) != NULL);		// ウィンドウがメニューを持っているか

		int bZoomed = IsZoomed(hTargetWnd_);
		int bIconic = IsIconic(hTargetWnd_);

		// 最大化されていたら、一度最大化は解除
		if (bZoomed) {
			ShowWindow(hTargetWnd_, SW_NORMAL);
		}

		int offset = 1;
		LONG newStyle;
		if (bBorderless) {
			// 枠無しウィンドウのスタイル
			newStyle = (WS_VISIBLE | WS_POPUP);
			offset = -1;
		} else {
			// 初期のウィンドウスタイル（必ずしも枠ありとは限らない）
			newStyle = originalWindowInfo_.dwStyle;
			offset = 1;
		}
		
		// 変更後のウィンドウサイズを計算
		AdjustWindowRect(&rcCli, newStyle, hasMenu);
		newW = rcCli.right - rcCli.left;
		newH = rcCli.bottom - rcCli.top;
			
		int dx = w - newW;	// 変更後に広がる幅（負もある） [px]
		int dy = h - newH;	// 変更後に広がる高さ（負もある） [px]
		int bw = dx / 2;	// 枠の片側幅 [px]
		int bh = bw;		// 本来は枠の下側高さと左右の幅が同じ保証はないが、とりあえず同じとみなしている
		newX = rcWin.left + bw;
		newY = rcWin.top + (dy - bh);

		// ウィンドウサイズが変化しないか、最大化や最小化状態なら標準のサイズ更新
		if (bZoomed) {
			// ウィンドウスタイルを適用
			SetWindowLong(hTargetWnd_, GWL_STYLE, newStyle);

			// 最大化されていたら、ここで再度最大化
			ShowWindow(hTargetWnd_, SW_MAXIMIZE);
		} else if (bIconic) {
			// ウィンドウスタイルを適用
			SetWindowLong(hTargetWnd_, GWL_STYLE, newStyle);
			// 最小化されていたら、次に表示されるときの再描画を期待して、SetWindowPosやShowWindowは省略
		} else {
			// クライアント領域サイズを維持するようサイズと位置を調整
			//    Unity2019までの手順ではUnity2020ではサイズが戻ってしまう。サイズ変更を繰り返したり、後でウィンドウスタイルを変更してみる。
			//    ウィンドウリサイズのタイミングがずれた場合の挙動が不安なため、SWP_ASYNCWINDOWPOSを外した。
			SetWindowPos(
				hTargetWnd_,
				NULL,
				newX, newY, newW + offset, newH,
				SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS
			);
			SetWindowPos(
				hTargetWnd_,
				NULL,
				newX, newY, newW, newH,
				SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS
			);

			// ウィンドウスタイルを適用
			SetWindowLong(hTargetWnd_, GWL_STYLE, newStyle);

			SetWindowPos(
				hTargetWnd_,
				NULL,
				newX, newY, newW + offset, newH,
				SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS
			);
			SetWindowPos(
				hTargetWnd_,
				NULL,
				newX, newY, newW, newH,
				SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS
			);
			ShowWindow(hTargetWnd_, SW_SHOW);
		}
	}

	// 枠無しか否かを記憶
	bIsBorderless_ = bBorderless;
}

/// <summary>
/// ウィンドウ全体の透明度を設定
/// </summary>
/// <param name=""></param>
/// <returns></returns>
void UNIWINC_API SetAlphaValue(const float alpha) {
	// 透明度指定値を記憶
	byAlpha_ = (BYTE)(0xFF * alpha);

	if (hTargetWnd_) {
		if (bIsTransparent_) {
			// 現在が透過時の処理

			switch (nTransparentType_)
			{
			case TransparentType::Alpha:
				applyWindowAlphaValue();
				break;
			case TransparentType::ColorKey:
				enableTransparentBySetLayered();	// 透過開始と同じ関数で設定
				break;
			default:
				applyWindowAlphaValue();
				break;
			}
		} else {
			// 現在が非透過での処理
			applyWindowAlphaValue();
		}
	}
}

/// <summary>
/// 最前面化／解除
/// </summary>
/// <param name="bTopmost"></param>
/// <returns></returns>
void UNIWINC_API SetTopmost(const BOOL bTopmost) {
	// 最背面化されていたら、解除
	bIsBottommost_ = FALSE;

	if (hTargetWnd_) {
		SetWindowPos(
			hTargetWnd_,
			(bTopmost ? HWND_TOPMOST : HWND_NOTOPMOST),
			0, 0, 0, 0,
			SWP_NOSIZE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS // | SWP_FRAMECHANGED
		);

		// Run callback if the topmost state changed
		if (bIsTopmost_ != bTopmost) {
			if (hWindowStyleChangedHandler_ != nullptr) {
				hWindowStyleChangedHandler_((INT32)(bTopmost ? WindowStateEventType::TopMostEnabled : WindowStateEventType::TopMostDisabled));
			}
		}
	}

	bIsTopmost_ = bTopmost;
}

/// <summary>
/// 最背面化／解除
/// </summary>
/// <param name="bBottommost"></param>
/// <returns></returns>
void UNIWINC_API SetBottommost(const BOOL bBottommost) {
	// 最前面化されていたら、解除
	bIsTopmost_ = FALSE;

	if (hTargetWnd_) {
		SetWindowPos(
			hTargetWnd_,
			(bBottommost ? HWND_BOTTOM : HWND_NOTOPMOST),
			0, 0, 0, 0,
			SWP_NOSIZE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOACTIVATE //| SWP_ASYNCWINDOWPOS // | SWP_FRAMECHANGED
		);

		// Run callback if the bottommost state changed
		if (bIsBottommost_ != bBottommost) {
			if (hWindowStyleChangedHandler_ != nullptr) {
				hWindowStyleChangedHandler_((INT32)(bBottommost ? WindowStateEventType::BottomMostEnabled : WindowStateEventType::BottomMostDisabled));
			}
		}
	}

	bIsBottommost_ = bBottommost;
}

/// <summary>
/// 壁紙化／解除
/// </summary>
/// <param name="bEnabled"></param>
/// <returns></returns>
void UNIWINC_API SetBackground(const BOOL bEnabled) {
	if (hTargetWnd_) {
		if (bEnabled) {
			// デスクトップにあたるウィンドウが未取得なら、ここで取得
			if (hDesktopWnd_ == NULL) {
				findDesktopWindow();
			}

			if (hDesktopWnd_ != NULL) {
				SetParent(hTargetWnd_, hDesktopWnd_);
				//SetBottommost(TRUE);
				//SetWindowPos(
				//	hTargetWnd_,
				//	HWND_BOTTOM,
				//	0, 0, 0, 0,
				//	SWP_NOSIZE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS // | SWP_FRAMECHANGED
				//);
				//refreshWindow();
			}
		}
		else
		{
			SetParent(hTargetWnd_, hParentWnd_);
			//SetBottommost(FALSE);
		}

		// Run callback if the bottommost state changed
		if (bIsBackground_!= bEnabled) {
			if (hWindowStyleChangedHandler_ != nullptr) {
				hWindowStyleChangedHandler_((INT32)(bEnabled ? WindowStateEventType::WallpaperModeEnabled : WindowStateEventType::WallpaperModeDisabled));
			}
		}
	}

	bIsBackground_= bEnabled;
}

/// <summary>
/// Zoom the window or normalize
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

/// <summary>
/// （macOSのみ）ウィンドウ配置制限を解除・復帰します
/// </summary>
/// <param name="isFree">Windowsでは動作しません</param>
/// <returns>なし</returns>
void UNIWINC_API EnableFreePositioning(const BOOL isFree)
{
	bIsFreePositioning_ = isFree;
	return;
}

/// <summary>
/// クリックスルー（マウス操作無効化）を設定／解除
/// </summary>
/// <param name="bTransparent"></param>
/// <returns></returns>
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

			// 半透明を維持するため、レイヤードウィンドウは戻さないようコメントアウト
			//if (!bIsTransparent_ && !(originalWindowInfo_.dwExStyle & WS_EX_LAYERED)) {
			//	exstyle &= ~WS_EX_LAYERED;
			//}
			SetWindowLong(hTargetWnd_, GWL_EXSTYLE, exstyle);
		}
	}
	bIsClickThrough_ = bTransparent;
}

/// <summary>
/// Set the window position
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
	int newY = (nPrimaryMonitorHeight_ - (int)y) - (rect.bottom - rect.top);
	int newX = (int)(x);

	return SetWindowPos(
		hTargetWnd_, NULL,
		newX, newY,
		0, 0,
		SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOZORDER //| SWP_ASYNCWINDOWPOS
		);
}

/// <summary>
/// Get the window position
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
		*x = (float)(rect.left);
		*y = (float)(nPrimaryMonitorHeight_- rect.bottom);	// 左下基準とする
		return TRUE;
	}
	return FALSE;
}

/// <summary>
/// Set the window size
/// </summary>
/// <param name="width">幅 [px]</param>
/// <param name="height">高さ [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API SetSize(const float width, const float height) {
	if (hTargetWnd_ == NULL) return FALSE;

	// 現在のウィンドウ位置とサイズを取得
	RECT rect;
	GetWindowRect(hTargetWnd_, &rect);

	int x = rect.left;
	int y = rect.bottom;
	int w = (int)(width);
	int h = (int)(height);

	// 左下原点とするために調整した、新規Y座標
	y = y - h;

	return SetWindowPos(
		hTargetWnd_, NULL,
		x, y, w, h,
		SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOZORDER | SWP_FRAMECHANGED //| SWP_ASYNCWINDOWPOS
	);
}

/// <summary>
/// Get the window size with the border
/// </summary>
/// <param name="width">幅 [px]</param>
/// <param name="height">高さ [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API GetSize(float* width, float* height) {
	*width = 0;
	*height = 0;

	if (hTargetWnd_ == NULL) return FALSE;
	RECT rect;
	if (GetWindowRect(hTargetWnd_, &rect)) {
		*width = (float)(rect.right - rect.left);	// +1 は不要なよう
		*height = (float)(rect.bottom - rect.top);	// +1 は不要なよう

		return TRUE;
	}
	return FALSE;
}

/// <summary>
/// Get the client area size of the window
/// </summary>
/// <param name="width">幅 [px]</param>
/// <param name="height">高さ [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API GetClientSize(float* width, float* height) {
	*width = 0;
	*height = 0;

	if (hTargetWnd_ == NULL) return FALSE;
	RECT rect;
	if (GetClientRect(hTargetWnd_, &rect)) {
		*width = (float)(rect.right - rect.left);
		*height = (float)(rect.bottom - rect.top);

		return TRUE;
	}
	return FALSE;
}

/// <summary>
/// Get the client area position and size of the window
/// </summary>
/// <param name="x">ウィンドウ左端からのx座標 [px]</param>
/// <param name="y">ウィンドウ下端からのy座標 [px]</param>
/// <param name="width">幅 [px]</param>
/// <param name="height">高さ [px]</param>
/// <returns>成功すれば true</returns>
BOOL UNIWINC_API GetClientRectangle(float* x, float* y, float* width, float* height) {
	*x = 0;
	*y = 0;
	*width = 0;
	*height = 0;

	if (hTargetWnd_ == NULL) return FALSE;
	RECT windowRect;
	if (GetWindowRect(hTargetWnd_, &windowRect)) {
		LONG windowHeight = (windowRect.bottom - windowRect.top);
		RECT rect;
		if (GetClientRect(hTargetWnd_, &rect)) {
			POINT originPoint;	// クライアント左下のスクリーン座標が入る
			originPoint.x = rect.left;
			originPoint.y = rect.bottom;
			if (ClientToScreen(hTargetWnd_, &originPoint)) {
				*x = (float)(originPoint.x - windowRect.left);
				*y = (float)(windowHeight - (originPoint.y - windowRect.top));
				*width = (float)(rect.right - rect.left);
				*height = (float)(rect.bottom - rect.top);

				return TRUE;
			}
		}
	}
	return FALSE;
}

/// <summary>
/// Register the callback fucnction called when window style changed
/// </summary>
/// <param name="callback"></param>
/// <returns></returns>
BOOL UNIWINC_API RegisterWindowStyleChangedCallback(WindowStyleChangedCallback callback) {
	if (callback == nullptr) return FALSE;

	hWindowStyleChangedHandler_= callback;
	return TRUE;
}

/// <summary>
/// Unregister the callback function
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API UnregisterWindowStyleChangedCallback() {
	hWindowStyleChangedHandler_ = nullptr;
	return TRUE;
}


#pragma endregion For window style


// ========================================================================
#pragma region For monitor Info.

/// <summary>
/// このウィンドウが現在表示されているモニタ番号を取得
/// </summary>
/// <returns></returns>
INT32 UNIWINC_API GetCurrentMonitor() {
	int primaryIndex = 0;

	//  ウィンドウ未取得ならプライマリモニタを探す
	if (hTargetWnd_ == NULL) {
		for (int i = 0; i < nMonitorCount_; i++) {
			RECT mr = pMonitorRect_[pMonitorIndices_[i]];

			// 原点にあるモニタはプライマリと判定
			if (mr.left == 0 && mr.top == 0) {
				primaryIndex = i;
				break;
			}
		}
		return primaryIndex;
	}

	// 現在のウィンドウの中心座標を取得
	RECT rect;
	GetWindowRect(hTargetWnd_, &rect);
	LONG cx = (rect.right - 1 + rect.left) / 2;
	LONG cy = (rect.bottom - 1 + rect.top) / 2;

	// ウィンドウの中央が含まれているモニタを検索
	for (int i = 0; i < nMonitorCount_; i++) {
		RECT mr = pMonitorRect_[pMonitorIndices_[i]];

		// ウィンドウ中心が入っていればその画面番号を返して終了
		if (mr.left <= cx && cx < mr.right && mr.top <= cy && cy < mr.bottom) {
			return i;
		}

		// 原点にあるモニタはプライマリと判定
		if (mr.left == 0 && mr.top == 0) {
			primaryIndex = i;
		}
	}

	// 判定できなければプライマリモニタの画面番号を返す
	return primaryIndex;
}


/// <summary>
/// 接続されているモニタ数を取得
/// </summary>
/// <returns>モニタ数</returns>
INT32  UNIWINC_API GetMonitorCount() {
	//// SM_CMONITORS では表示されているモニタのみ対象となる（EnumDisplayとは異なる）
	//return GetSystemMetrics(SM_CMONITORS);
	return nMonitorCount_;
}

/// <summary>
/// モニタの位置、サイズを取得
/// </summary>
/// <param name="width">幅 [px]</param>
/// <param name="height">高さ [px]</param>
/// <returns>成功すれば true</returns>
BOOL  UNIWINC_API GetMonitorRectangle(const INT32 monitorIndex, float* x, float* y, float* width, float* height) {
	*x = 0;
	*y = 0;
	*width = 0;
	*height = 0;

	if (monitorIndex < 0 || monitorIndex >= nMonitorCount_) {
		return FALSE;
	}

	RECT rect = pMonitorRect_[pMonitorIndices_[monitorIndex]];
	*x = (float)(rect.left);
	*y = (float)(nPrimaryMonitorHeight_ - rect.bottom);		// 左下基準とする
	*width = (float)(rect.right - rect.left);
	*height = (float)(rect.bottom - rect.top);
	return TRUE;
}

/// <summary>
/// Register the callback fucnction called when updated monitor information
/// </summary>
/// <param name="callback"></param>
/// <returns></returns>
BOOL UNIWINC_API RegisterMonitorChangedCallback(MonitorChangedCallback callback) {
	if (callback == nullptr) return FALSE;

	hMonitorChangedHandler_ = callback;
	return TRUE;
}

/// <summary>
/// Unregister the callback function
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API UnregisterMonitorChangedCallback() {
	hMonitorChangedHandler_ = nullptr;
	return TRUE;
}

#pragma endregion For monitor Info.


// ========================================================================
#pragma region For mouse cursor

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
		*y = (float)(nPrimaryMonitorHeight_ - pos.y - 1);	// 左下基準とする
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
	pos.y = nPrimaryMonitorHeight_ - (int)y - 1;

	return SetCursorPos(pos.x, pos.y);
}

/// <summary>
/// マウスボタン押下状況を取得
/// </summary>
/// <returns>押下中なら1となるビットフラグ（1:Left, 2:Right, 4:Middle）</returns>
INT32 UNIWINC_API GetMouseButtons() {
	INT32 state = 0;
	if (GetAsyncKeyState(VK_LBUTTON) & 0x8000) {
		state |= 1;
	}
	if (GetAsyncKeyState(VK_RBUTTON) & 0x8000) {
		state |= 2;
	}
	if (GetAsyncKeyState(VK_MBUTTON) & 0x8000) {
		state |= 4;
	}
	return state;
}

/// <summary>
/// 修飾キーの押下状況を取得
/// </summary>
/// <returns>押下中なら1となるビットフラグ（1:ALt, 2:Ctrl, 4:Shift, 8:Win）</returns>
INT32 UNIWINC_API GetModifierKeys() {
	INT32 state = 0;
	if (GetAsyncKeyState(VK_MENU) & 0x8000) {
		state |= 1;
	}
	if (GetAsyncKeyState(VK_CONTROL) & 0x8000) {
		state |= 2;
	}
	if (GetAsyncKeyState(VK_SHIFT) & 0x8000) {
		state |= 4;
	}
	if (GetAsyncKeyState(VK_LWIN) & 0x8000 || GetAsyncKeyState(VK_RWIN) & 0x8000) {
		state |= 8;
	}
	return state;
}

#pragma endregion For mouse cursor


// ========================================================================
#pragma region For file dropping and window procedure

/// <summary>
/// Process drop files
/// </summary>
/// <param name="hDrop"></param>
/// <returns></returns>
BOOL receiveDropFiles(HDROP hDrop) {
	// TODO: Windowsでは特殊文字がファイル名に入る例はまず無さそうだが、macOSと同様にダブルクォーテーション囲みにした方がよい
	//		CSVと同様にダブルクォーテーションが文字としてあれば二重にする
	UINT num = DragQueryFile(hDrop, 0xFFFFFFFF, NULL, 0);

	if (num > 0) {
		// Retrieve total buffer size
		UINT bufferSize = 0;
		for (UINT i = 0; i < num; i++) {
			UINT size = DragQueryFile(hDrop, i, NULL, 0);
			bufferSize += size + sizeof(L'\n');		// Add a delimiter size
		}
		bufferSize++;

		// Allocate buffer
		LPWSTR buffer;
		buffer = new (std::nothrow)WCHAR[bufferSize];

		if (buffer != NULL) {
			// Retrieve file paths
			UINT bufferIndex = 0;
			for (UINT i = 0; i < num; i++) {
				UINT cch = bufferSize - 1 - bufferIndex;
				UINT size = DragQueryFile(hDrop, i, buffer + bufferIndex, cch);
				bufferIndex += size;
				buffer[bufferIndex] = L'\n';	// Delimiter of each path
				bufferIndex++;
			}
			buffer[bufferIndex] = NULL;

			// Do callback function
			if (hDropFilesHandler_ != nullptr) {
				hDropFilesHandler_((WCHAR*)buffer);	// Charset of this project must be set U
			}

			delete[] buffer;
		}
	}

	return (num > 0);
}

/// <summary>
/// Custom window proceture to accept dropped files and display-changed event
/// </summary>
/// <param name="hWnd"></param>
/// <param name="uMsg"></param>
/// <param name="wParam"></param>
/// <param name="lParam"></param>
/// <returns></returns>
LRESULT CALLBACK customWindowProcedure(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	HDROP hDrop;
	INT32 count;

	switch (uMsg)
	{
	case WM_DROPFILES:
		hDrop = (HDROP)wParam;
		receiveDropFiles(hDrop);
		DragFinish(hDrop);
		break;

	case WM_DISPLAYCHANGE:
		updateScreenSize();

		// Run callback
		if (hMonitorChangedHandler_ != nullptr) {
			count = GetMonitorCount();
			hMonitorChangedHandler_(count);
		}
		break;

	case WM_WINDOWPOSCHANGING:
		// 常に最背面
		if (bIsBottommost_) {
			((WINDOWPOS*)lParam)->hwndInsertAfter = HWND_BOTTOM;
		}
		break;

	case WM_STYLECHANGED:	// スタイルの変化を検出
		// Run callback
		if (hWindowStyleChangedHandler_ != nullptr) {
			hWindowStyleChangedHandler_((INT32)WindowStateEventType::StyleChanged);
		}
		break;

	case WM_SIZE:		// 最大化、最小化による変化を検出
		switch (wParam)
		{
		case SIZE_RESTORED:		// 最小化でも最大化でもない通常のリサイズ
		case SIZE_MAXIMIZED:
		case SIZE_MINIMIZED:
			// Run callback
			if (hWindowStyleChangedHandler_ != nullptr) {
				hWindowStyleChangedHandler_((INT32)WindowStateEventType::Resized);
			}
			break;
		}
		break;
		
	default:
		break;
	}

	if (lpOriginalWndProc_ != NULL) {
		return CallWindowProc(lpOriginalWndProc_, hWnd, uMsg, wParam, lParam);
	}
	else {
		return DefWindowProc(hWnd, uMsg, wParam, lParam);
	}
}

/// <summary>
/// Wrapper of SetWindowLongPtr to set window procedure
/// </summary>
/// <param name="wndProc"></param>
/// <returns></returns>
WNDPROC setWindowProcedure(WNDPROC wndProc) {
	//return (WNDPROC)SetWindowLongPtr(hTargetWnd_, GWLP_WNDPROC, (LONG_PTR)wndProc);

#ifdef _WIN64
	// 64bit
	return (WNDPROC)SetWindowLongPtr(hTargetWnd_, GWLP_WNDPROC, (LONG_PTR)wndProc);
#else
	return (WNDPROC)SetWindowLong(hTargetWnd_, GWLP_WNDPROC, (LONG)wndProc);
#endif
}

/// <summary>
/// Remove the custom window procedure
/// </summary>
void destroyCustomWindowProcedure() {
	if (lpMyWndProc_ == NULL) return;

	if (lpOriginalWndProc_ != NULL) {
		if (hTargetWnd_ != NULL && IsWindow(hTargetWnd_)) {
			setWindowProcedure(lpOriginalWndProc_);
		}
		lpOriginalWndProc_ = NULL;
	}
	lpMyWndProc_ = NULL;
}

/// <summary>
/// Create and attach the custom window procedure
/// </summary>
void createCustomWindowProcedure() {
	if (lpMyWndProc_ != NULL) {
		destroyCustomWindowProcedure();
	}

	if (hTargetWnd_ != NULL) {
		lpMyWndProc_ = customWindowProcedure;
		lpOriginalWndProc_ = setWindowProcedure(lpMyWndProc_);
	}
}


// ↓ウィンドウプロシージャではなくメッセージをフックする場合はこちらを使う
//    解像度変更を検出するためにウィンドウプロシージャを使うものとした

///// <summary>
///// Callback when received WM_DROPFILE message
///// </summary>
///// <param name="nCode"></param>
///// <param name="wParam"></param>
///// <param name="lParam"></param>
///// <returns></returns>
//LRESULT CALLBACK messageHookCallback(int nCode, WPARAM wParam, LPARAM lParam) {
//	if (nCode < 0) {
//		return CallNextHookEx(NULL, nCode, wParam, lParam);
//	}
//
//	// lParam is a pointer to an MSG structure for WH_GETMESSAGE
//	LPMSG msg = (LPMSG)lParam;
//
//	switch (msg->message) {
//	case WM_DROPFILES:
//		if (hTargetWnd_ != NULL && msg->hwnd == hTargetWnd_) {
//			HDROP hDrop = (HDROP)msg->wParam;
//			ReceiveDropFiles(hDrop);
//			DragFinish(hDrop);
//		}
//		return TRUE;
//		break;
//
//	case WM_DISPLAYCHANGE:
//		updateScreenSize();
//		break;
//
//	case WM_STYLECHANGED:
//		break;
//	}
//
//	return CallNextHookEx(NULL, nCode, wParam, lParam);
//}
//
///// <summary>
///// Set the hook
///// </summary>
//void beginHook() {
//	if (hTargetWnd_ == NULL) return;
//
//	// Return if the hook is already set
//	if (hHook_ != NULL) return;
//
//	//HMODULE hMod = GetModuleHandle(NULL);
//	DWORD dwThreadId = GetCurrentThreadId();
//
//	hHook_ = SetWindowsHookEx(WH_GETMESSAGE, messageHookCallback, NULL, dwThreadId);
//}
//
///// <summary>
///// Unset the hook
///// </summary>
//void endHook() {
//	if (hTargetWnd_ == NULL) return;
//
//	// Return if the hook is not set
//	if (hHook_ == NULL) return;
//
//	UnhookWindowsHookEx(hHook_);
//	hHook_ = NULL;
//}


/// <summary>
/// Enable or disable file dropping
/// </summary>
/// <returns>Previous window procedure</returns>
BOOL UNIWINC_API SetAllowDrop(const BOOL bEnabled)
{
	if (hTargetWnd_ == NULL) return FALSE;

	bAllowDropFile_ = bEnabled;
	DragAcceptFiles(hTargetWnd_, bAllowDropFile_);

	//if (bEnabled && hHook == NULL) {
	//	beginHook();
	//}
	////else if (!bEnabled && hHook != NULL) {
	////	endHook();
	////}

	return TRUE;
}

/// <summary>
/// Register the callback fucnction for dropping files
/// </summary>
/// <param name="callback"></param>
/// <returns></returns>
BOOL UNIWINC_API RegisterDropFilesCallback(FilesCallback callback) {
	if (callback == nullptr) return FALSE;

	hDropFilesHandler_ = callback;
	return TRUE;
}

/// <summary>
/// Unregister the callback function
/// </summary>
/// <returns></returns>
BOOL UNIWINC_API UnregisterDropFilesCallback() {
	hDropFilesHandler_ = nullptr;
	return TRUE;
}

#pragma endregion For file dropping and window procedure


// ========================================================================
#pragma region File dialogs

/// <summary>
/// Convert multi-selection files string to new-line separated string
/// </summary>
/// <param name="lpBuffer"></param>
/// <param name="nBufferSize"></param>
BOOL parsePaths(LPWSTR lpBuffer, const UINT32 nBufferLength) {
	// 複製を保存するのに必要な長さを調べる
	int bufferLength = nBufferLength;
	int length = bufferLength;	// OPENFILENAME中で実際に利用した長さ
	int pathCount = 0;			// NULL区切りでみた行数。複数選択時は1より大きくなる
	int firstLineLength = bufferLength;	// 先頭要素の長さ。複数選択時にはフォルダ名が入る部分

	// 要素の数、全体の文字数を数える
	for (int i = 0; i < bufferLength; i++) {
		if (lpBuffer[i] == L'\0') {
			if (firstLineLength == bufferLength) firstLineLength = i;
			pathCount++;
			length = i;		// とりあえずここまでの文字数は利用している

			if ((i < (bufferLength - 1)) && (lpBuffer[i + 1] == L'\0')) {
				// NULLが連続していれば終端とみなす（連続する後ろがもう無い場合も終端）
				break;
			}
			else
			{
				// 次の文字がNULLでなければスキップできる
				i++;
			}
		}
	}

	// NULLが最後に来なかった場合は行数追加できていないので、1増やしておく
	if (length == bufferLength) pathCount++;

	// 複数選択でない場合は改行区切りやフォルダ名追加の必要はなく、そのままの値で終了
	if (pathCount <= 1) {
		return TRUE;
	}


	// パスのリストが返却バッファに入りきらない場合は失敗として空で返す
	if (((firstLineLength + 2) * pathCount + length - firstLineLength) > bufferLength) {
		// 結果返却バッファをクリア
		ZeroMemory(lpBuffer, bufferLength * sizeof(WCHAR));
		return FALSE;
	}

	// 完全なパスを改行区切り文字列で生成

	LPWSTR buffer = new (std::nothrow)WCHAR[length];
	if (buffer == NULL) {
		ZeroMemory(lpBuffer, bufferLength * sizeof(WCHAR));
		return FALSE;
	}
	ZeroMemory(buffer, length * sizeof(WCHAR));

	// 一時バッファに内容を退避
	memcpy(buffer, lpBuffer, length * sizeof(WCHAR));

	// 結果返却バッファをクリア
	ZeroMemory(lpBuffer, bufferLength * sizeof(WCHAR));


	int offset = 0;
	int index = firstLineLength;
	for (int i = firstLineLength; i < length; i++) {
		if (buffer[i] == NULL) {
			// 改行で区切り
			if (offset > 0) {
				lpBuffer[offset] = L'\n';
				offset++;
			}

			//  フォルダ名部分を複製
			memcpy(lpBuffer + offset, buffer, firstLineLength * sizeof(WCHAR));
			offset += firstLineLength;
			lpBuffer[offset] = L'\\';	// パスの区切り文字も追加
			offset++;
		}
		else
		{
			// NULL文字でなければファイル名の一部として追加
			lpBuffer[offset] = buffer[i];
			offset++;
		}
	}

	//// デバッグ用
	//swprintf_s(lpBuffer, bufferSize, L"Files: bufferSize %d, length %d, pathCount %d, firstLineLength %d, Dir %s", bufferSize, length, pathCount, firstLineLength, buffer);

	delete[] buffer;
	return TRUE;
}

/// <summary>
/// Create a null ended extension string from the first extension in the input string
/// e.g. "TitleA\ttExtA1\tExtA2\t...ExtAn\nTitleB\tExtB1\tExtB2...ExtBn\n"
/// </summary>
/// <param name="lpsFormTypeFilterText"></param>
/// <returns>Null ended string</returns>
LPWSTR createDefaultExtString(const LPWSTR lpsFormTypeFilterText) {
	const int MAX_EXT_LENGTH = 32;

	int resultIndex = 0;
	LPWSTR lpsResult = nullptr;
	if (lpsFormTypeFilterText != nullptr) {
		lpsResult = new (std::nothrow)WCHAR[MAX_EXT_LENGTH];

		if (lpsResult != nullptr) {
			ZeroMemory(lpsResult, MAX_EXT_LENGTH * sizeof(WCHAR));

			int inputIndex = 0;
			int section = 0;	// 0:title, 1:extensions
			bool includesWildCard = false;

			while (resultIndex < (MAX_EXT_LENGTH - 1)) {
				WCHAR c = lpsFormTypeFilterText[inputIndex++];

				// End of the input string
				if (c == L'\0') {
					break;
				}
				// Tab separated
				else if (c == L'\t') {
					// The first element is title
					if (section <= 0) {
						section = 1;
					}
					else {
						break;
					}
				}
				// New line
				else if (c == L'\n') {
					break;
				}
				// Other character
				else {
					if (section == 1) {
						// Wild card
						if (c == L'*') {
							includesWildCard = true;
							break;
						}

						// A character of extension
						lpsResult[resultIndex++] = c;
					}
				}
			}

			lpsResult[resultIndex++] = L'\0';

			if (includesWildCard) {
				delete[] lpsResult;
				lpsResult = nullptr;
			}
		}
	}
	return lpsResult;
}

/// <summary>
/// Create a null separated filter text from tab separated text
/// e.g. "TitleA\ttExtA1\tExtA2\t...ExtAn\nTitleB\tExtB1\tExtB2...ExtBn\n"
/// </summary>
/// <param name="lpsFormTypeFilterText"></param>
/// <returns>e.g. "TitleA\0*.tExtA1;*.ExtA2;...;*.ExtAn\0TitleB\0*.ExtB1;*.ExtB2;...;*.ExtBn\0\0"</returns>
LPWSTR createFilterString(const LPWSTR lpsFormTypeFilterText) {
	const int MAX_FILTER_LENGTH = 1024;

	int resultIndex = 0;
	LPWSTR lpsResult = nullptr;
	if (lpsFormTypeFilterText != nullptr) {
		lpsResult = new (std::nothrow)WCHAR[MAX_FILTER_LENGTH];

		if (lpsResult != nullptr) {
			//ZeroMemory(lpsFilter, MAX_FILTER_LENGTH * sizeof(WCHAR));

			int firstExtStartIndex = 0;
			int firstExtEndIndex = 0;

			int inputIndex = 0;
			int section = 0;	// 0:title, 1:extensions
			bool isFirstChar = true;

			while (resultIndex < (MAX_FILTER_LENGTH - 2)) {
				WCHAR c = lpsFormTypeFilterText[inputIndex++];

				// End of the input string
				if (c == L'\0') {
					break;
				}
				// Tab separated
				else if (c == L'\t') {
					// The first element is title
					if (section <= 0) {
						lpsResult[resultIndex++] = L'\0';
						section = 1;
					}
					else {
						// In extensions' section
						lpsResult[resultIndex++] = ';';
					}
					isFirstChar = true;
				}
				// New line
				else if (c == L'\n') {
					// If there is no extension, add separator to skip to next pair
					if (section <= 0) {
						lpsResult[resultIndex++] = L'\0';
					}
					// The end of title and extensions pair
					lpsResult[resultIndex++] = L'\0';
					section = 0;
					isFirstChar = true;
				}
				// Other character
				else {
					// Add "*." before each extension.
					if (isFirstChar && section == 1) {
						if (resultIndex >= (MAX_FILTER_LENGTH - 4)) break;
						lpsResult[resultIndex++] = '*';
						lpsResult[resultIndex++] = '.';
					}
					lpsResult[resultIndex++] = c;
					isFirstChar = false;
				}
			}

			// Terminated by two NULL characters 
			lpsResult[resultIndex++] = L'\0';
			lpsResult[resultIndex++] = L'\0';
		}
	}
	return lpsResult;
}

DWORD GetPanelFlags(const INT32 flags) {
	DWORD result = OFN_EXPLORER | OFN_NOCHANGEDIR;	// Default

	if ((flags & (INT32)PanelFlag::AllowMultiSelect) > 0) result |= OFN_ALLOWMULTISELECT;
	if ((flags & (INT32)PanelFlag::FileMustExist) > 0) result |= OFN_FILEMUSTEXIST;
	if ((flags & (INT32)PanelFlag::FolderMustExist) > 0) result |= OFN_PATHMUSTEXIST;
	if ((flags & (INT32)PanelFlag::ShowHidden) > 0) result |= OFN_FORCESHOWHIDDEN;
	if ((flags & (INT32)PanelFlag::OverwritePrompt) > 0) result |= OFN_OVERWRITEPROMPT;
	if ((flags & (INT32)PanelFlag::CreatePrompt) > 0) result |= OFN_CREATEPROMPT;

	return result;
}

BOOL UNIWINC_API OpenFilePanel(const PPANELSETTINGS pSettings, LPWSTR pResultBuffer, const UINT32 nBufferSize) {
	// モーダルにするため、ウィンドウハンドル未取得なら探して設定
	HWND hwnd = hTargetWnd_;
	if (hwnd == NULL) {
		hwnd = hPanelOwnerWnd_;
		if (hwnd == NULL) {
			hwnd = hPanelOwnerWnd_ = FindOwnerWindowHandle();
		}
	}

	LPWSTR lpszDefaultExt = createDefaultExtString(pSettings->lpszFilter);
	LPWSTR lpsFilter = createFilterString(pSettings->lpszFilter);

	OPENFILENAMEW ofn;
	ZeroMemory(&ofn, sizeof(ofn));
	ofn.lStructSize = sizeof(ofn);
	ofn.hwndOwner = hwnd;
	ofn.lpstrTitle = pSettings->lpszTitle;
	ofn.lpstrFilter = lpsFilter;
	ofn.lpstrInitialDir = pSettings->lpszInitialDir;
	//ofn.lpstrDefExt = pSettings->lpszDefaultExt;		// Not implemented
	ofn.lpstrDefExt = lpszDefaultExt;
	ofn.Flags = GetPanelFlags(pSettings->nFlags);

	BOOL result = FALSE;

	if ((pResultBuffer != nullptr) && (nBufferSize > 0)) {
		ZeroMemory(pResultBuffer, nBufferSize * sizeof(WCHAR));
		ofn.lpstrFile = pResultBuffer;
		ofn.nMaxFile = nBufferSize;

		// Default path
		if (pSettings->lpszInitialFile != nullptr) {
			wcscpy_s(ofn.lpstrFile, ofn.nMaxFile, pSettings->lpszInitialFile);
		}

		result = GetOpenFileNameW(&ofn);
	}

	if (lpsFilter != nullptr) delete[] lpsFilter;
	if (lpszDefaultExt!= nullptr) delete[] lpszDefaultExt;

	if (result) {
		return parsePaths(pResultBuffer, nBufferSize);
	}
	return FALSE;
}

BOOL UNIWINC_API OpenSavePanel(const PPANELSETTINGS pSettings, LPWSTR pResultBuffer, const UINT32 nBufferSize) {
	// モーダルにするため、ウィンドウハンドル未取得なら探して設定
	HWND hwnd = hTargetWnd_;
	if (hwnd == NULL) {
		hwnd = hPanelOwnerWnd_;
		if (hwnd == NULL) {
			hwnd = hPanelOwnerWnd_ = FindOwnerWindowHandle();
		}
	}

	LPWSTR lpszDefaultExt = createDefaultExtString(pSettings->lpszFilter);
	LPWSTR lpsFilter = createFilterString(pSettings->lpszFilter);

	OPENFILENAMEW ofn;
	ZeroMemory(&ofn, sizeof(ofn));
	ofn.lStructSize = sizeof(ofn);
	ofn.hwndOwner = hwnd;
	ofn.lpstrTitle = pSettings->lpszTitle;
	ofn.lpstrFilter = lpsFilter;
	ofn.lpstrInitialDir = pSettings->lpszInitialDir;
	//ofn.lpstrDefExt = pSettings->lpszDefaultExt;		// Not implemented
	ofn.lpstrDefExt = lpszDefaultExt;
	ofn.Flags = GetPanelFlags(pSettings->nFlags);

	BOOL result = FALSE;

	if ((pResultBuffer != nullptr) && (nBufferSize > 0)) {
		ZeroMemory(pResultBuffer, nBufferSize * sizeof(WCHAR));
		ofn.lpstrFile = pResultBuffer;
		ofn.nMaxFile = nBufferSize;

		// Default path
		if (pSettings->lpszInitialFile != nullptr) {
			wcscpy_s(ofn.lpstrFile, ofn.nMaxFile, pSettings->lpszInitialFile);
		}

		result = GetSaveFileNameW(&ofn);
	}

	if (lpsFilter != nullptr) delete[] lpsFilter;
	if (lpszDefaultExt != nullptr) delete[] lpszDefaultExt;

	if (result) {
		return parsePaths(pResultBuffer, nBufferSize);
	}
	return FALSE;
}

#pragma endregion File dialogs


/// <summary>
/// デバッグ時に情報を渡すための関数
/// </summary>
/// <returns></returns>
INT32 UNIWINC_API GetDebugInfo() {
	LONG style = GetWindowLong(hTargetWnd_, GWL_STYLE);
	return style;
}

// ========================================================================
#pragma region Windows only public functions

/// <summary>
/// 現在選択されているウィンドウハンドルを取得
/// </summary>
/// <returns></returns>
HWND UNIWINC_API GetWindowHandle() {
	return hTargetWnd_;
}

/// <summary>
/// 壁紙化の親となるウィンドウハンドルを取得
/// </summary>
/// <returns></returns>
HWND UNIWINC_API GetDesktopWindowHandle() {
	return hDesktopWnd_;
}

/// <summary>
/// 自分のプロセスIDを取得
/// </summary>
/// <returns></returns>
DWORD UNIWINC_API GetMyProcessId() {
	return GetCurrentProcessId();
}

#pragma endregion Windows-only public functions
