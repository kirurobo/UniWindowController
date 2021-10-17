# UniWindowController
Unified window controller for macOS and Windows  
略称：UniWinC（ユニウィンク）

[![license](https://img.shields.io/badge/license-MIT-green.svg?style=flat)](https://github.com/kirurobo/UniWindowController/blob/master/LICENSE)


## 概要
Unity の Windows / macOS 向けビルドで、自ウィンドウの位置、サイズ、透過をコントロールします。  
また、ファイル、フォルダのドロップも受け付け可能にできます。

Windows、macOS それぞれではコード例やアセットが見つかりましたが、統一的に扱えるものが無さそうだったため作成しました。

![uniwinc](https://user-images.githubusercontent.com/1019117/96070514-5284e580-0edb-11eb-8a4d-d990a0a028a8.gif)  
https://twitter.com/i/status/1314440790945361920


## ダウンロード
https://github.com/kirurobo/UniWindowController/releases に .unitypackage ファイルがあります。
ビルドしたサンプルも置いています。


## 基本の利用法
1. Unitypackage から自分の Unity プロジェクトにインポート
2. Runtime/Prefabs にある UniWindowController プレハブをシーンに追加
3. そこで配置された UniWindowController をインスペクターで確認
    1. ProjectSettings を適切に直す（緑のボタンでまとめて設定が変更されます）
    2. `IsTransparent` 等、設定をお好みに合わせる
4. 左ドラッグでウィンドウ自体を動かしたい場合、 DragMoveCanvas プレハブも追加
5. PC / Mac スタンドアローンでビルドする
6. ビルドしたものを起動


## 制限事項
- Unityエディタ上では透過はできません。ビルドをしてお試しください。
  - 常に最前面やウィンドウ移動等は動作しますが、実行中にゲームビューを閉じたりドッキングの配置を変えることはお勧めしません。一応、ゲームビューにフォーカスを移すとウィンドウを再取得はします。
- マウスでは良いのですが、タッチ操作には適切な対応がまだ定まっていません。
  - Windows の場合、`TransparentType` を Alpha から ColorKey にすると、半透明の表現が失われる代わりにタッチ操作は自然になります。
- あまり動作検証をできている訳でもなく、安定して動くとは限りません。

既知の問題については [Issues](https://github.com/kirurobo/UniWindowController/issues) もご覧ください。


## 動作環境
- Unity 2018 以降
  - 開発は Unity 2018.4.30f1 で行っています
- Windows 10 or macOS
  - macOS の開発は 11.6 で行っています


## ヒットテストについて
正常にウィンドウを透過できると、あたかも長方形ではないウィンドウのように見えます。  
ですがそれは見た目だけで、実は長方形のウィンドウとして存在しています。  
そこで、マウスカーソルの直下を見て、透明ならばマウス操作を下のウィンドウに受け流す（クリックスルー）状態とし、不透明なら通常に戻す、という"ヒットテスト"を常に行なうことで、
あたかも見える部分しか存在しないように見せています。

このヒットテストについては2種類用意してあります。（自動ヒットテストを無効にして、自分で制御する、またはしないという選択も可能です。）

| Name | Description | Note |
|:-----|:-----|:------------|
|Opacity|透明度を見る|見た目と一致して自然だが、処理が重い|
|Raycast|Coliderを見る|より動作が軽いが、Coliderの用意が必要|

Raycastの方法の方がパフォーマンス的に推奨ですが、Coliderを忘れると触れなくなるため、デフォルトでは Opacity としています。

また注意として、タッチ操作だとあらかじめ指の下の色を確認できないため、操作に違和感を感じると思います。  
どのように対応すべきかベストの解決策が見つかっていないため、タッチ対応に関してはすみませんが後回しとなっています。


## 透過方法について（Windowsのみ選択可）
タッチ操作に対応する一つの方法として、layered window の単色透過を選択できるようにしてあります。  
これを選ぶと半透明が表現できず、パフォーマンスも落ちますが、ヒットテストをWindowsに任せるためタッチ操作に対しては感覚に一致するはずです。  

| Name | Description | Note |
|:-----|:-----|:------------|
|Alpha|レンダリング結果の透明度を反映|こちらが標準|
|ColorKey|RGBが一致する一色のみ透過|パフォーマンス悪いが、タッチは自然|


## C# スクリプト
Unityで他のスクリプトから操作できるものです。  
仕様は固まってはいないため、変更される場合があります。

### UniWindowController.cs
本体です。
他のスクリプトから操作できるプロパティとして下記があります。（他にも追加されたりします。）
| Name | Type | Description |
|:-----|:-----|:------------|
|isTransparent|bool| 透過（非矩形）ウィンドウに設定／解除します|
|isTopmost    |bool| 常に最前面に設定／解除します|
|isZoomed     |bool| 最大化／解除をします。また現在の状態を取得します |
|isHitTestEnabled|bool| 自動ヒットテストを有効／無効にします。有効だとマウスカーソル位置により isClickThrough が自動で変化します。 |
|isClickThrough|bool| クリックスルー状態に設定／解除します|
|windowPosition|Vector2| ウインドウ位置を取得／設定できます。※メインモニタ左下が原点で上向き正の座標系で、ウィンドウ左下座標です |
|windowSize|Vector2| ウインドウサイズを取得／設定できます |

### UniWindowMoveHandler.cs
このスクリプトをUI要素（Raycast Targetとなるもの）にアタッチしておくと、そのUI要素のドラッグでウィンドウを移動できるようになります。
例えば「ここを掴んで移動できます」というハンドルの画像にアタッチする想定です。

DragMoveCanvas というプレハブ内では、透明な全画面を覆うPanelを使っています。 
このとき Layer を「Ignore Raycast」にすることで、自動ヒットテストが Raycast の場合でも対象外となります。  
これにより画面のどこでもドラッグできるようになります。  
ただし他のUI上の操作はドラッグでの移動より優先されます。（DragMoveCanvas で Sort Order を小さくしているため。）

### LowLevel/FilePanel.cs
ファイル選択ダイアログを開く static メソッドがあります。  
UniWindowController のインスタンスがなくても使えますが、その場合は呼んだ時点のウィンドウを親として開きます。
- FilePanel.OpenFilePanel()
  - ファイルを開く場合の選択ダイアログ。複数選択も可能。
- FilePanel.SaveFilePanel()
  - ファイルを保存する際の選択ダイアログ。


## ソースのフォルダ構成
利用するだけならば、Release にある unitypackage をダウンロードしていただけばよく、このリポジトリをクローンする必要はありません。  
ソースを見たい／ビルドしたい方は、このようになっています。

- UniWinC
  - Unity のプロジェクトです。
  - ビルド済みの DLL、bundle も既に含みます。
  - ここの内容が unitypackage としてリリースにあります。
- VisualStudio
  - Windows版 x86, x64 の LibUniWinC.dll を生成するソリューションがあります。
  - Release でビルドすると Unity フォルダ下の DLL が上書きされます。
  - テスト用Windowsフォームアプリのプロジェクトも含まれます。
- Xcode
  - macOS版 LibUniWinC.bundle を生成するプロジェクトがあります。
  - ビルドすると Unity フォルダ下の .bundle が上書きされます。


## 謝辞
macOS側のコードは かりばぁ さんの[Unity + Mac + Swift で透過最前面ウィンドウを作る](https://qiita.com/KRiver1/items/9ecf65759cf1349f56af)をベースにさせていただきました。  
この場を借りて感謝を申し上げます。




