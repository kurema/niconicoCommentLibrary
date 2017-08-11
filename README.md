# niconicoCommentLibrary
ニコニコ動画の「コメントの表示」をするためのライブラリです。今の所。

まぁまぁいい感じに表示しますが完全ではないでしょう。  
完璧な表示は目指していません。  
表示の正確性以外にこっちの方が幾分賑やかになります。楽しい。

interfaceに沿って実装すれば描画先はなんだろうが動作するはずです。  
やろうと思えばコンソールでだって動くはず(座標を画面横幅との相対値で扱っているので非推奨)。

## ToDo
* ニコニコ動画のAPIに対応。外部ライブラリを使う方が安全確実。
* UWPに加えXamarin,Wpf,DirectX等対応。
  * .NetのWeb Assembly版も進んでいるようですね。JSより速くなる？
* UWP版の改良
  * 重いのでDirectX / ビットマップ描画に切り替えなど？
  * GetElementVisualとかを使ってコメントにドロップシャドーとか？[参考](https://stackoverflow.com/questions/41303196/how-to-create-a-drop-shadow-effect-for-the-button-in-uwp)
* さっさとアプリにする。
* Javaに移植。Android用。
* ストリーミング時に飛ばしてシークしても受信分は破棄しない。Streamを工夫。
  * ChromeやEdgeのHtml5 Video/Audioはそうなってる。使っているAndroidアプリはそうなっていないっぽい。

アプリを作った場合はオープンソースで公開するとは思いますが、多分ストア版には広告入れます。
