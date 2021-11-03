using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using FDK;
using System.Drawing;
using System.Linq;
using TJAPlayer3.Common;

namespace TJAPlayer3
{
	// グローバル定数

	public enum Eシステムサウンド
	{
		BGMオプション画面 = 0,
		BGMコンフィグ画面,
		BGM起動画面,
		BGM選曲画面,
        BGM結果画面,
		SOUNDステージ失敗音,
		SOUNDカーソル移動音,
		SOUNDゲーム開始音,
		SOUNDゲーム終了音,
		SOUNDステージクリア音,
		SOUNDタイトル音,
		SOUNDフルコンボ音,
		SOUND歓声音,
		SOUND曲読込開始音,
		SOUND決定音,
		SOUND取消音,
		SOUND変更音,
        //SOUND赤,
        //SOUND青,
        SOUND風船,
        SOUND曲決定音,
        SOUND成績発表,
		Count				// システムサウンド総数の計算用
    }

    internal class CSkin : IDisposable
    {
        // クラス

        public class Cシステムサウンド : IDisposable
        {
            // static フィールド

            public static CSkin.Cシステムサウンド r最後に再生した排他システムサウンド;

            private readonly ESoundGroup _soundGroup;

            // フィールド、プロパティ

            public bool bループ;
            public bool b読み込み未試行;
            public bool b読み込み成功;
            public bool b排他;
            public string strファイル名 = "";
            public bool b再生中
            {
                get
                {
                    if (this.rSound[1 - this.n次に鳴るサウンド番号] == null)
                        return false;

                    return this.rSound[1 - this.n次に鳴るサウンド番号].b再生中;
                }
            }
            public int n位置_現在のサウンド
            {
                get
                {
                    CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
                    if (sound == null)
                        return 0;

                    return sound.n位置;
                }
                set
                {
                    CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
                    if (sound != null)
                        sound.n位置 = value;
                }
            }
            public int n位置_次に鳴るサウンド
            {
                get
                {
                    CSound sound = this.rSound[this.n次に鳴るサウンド番号];
                    if (sound == null)
                        return 0;

                    return sound.n位置;
                }
                set
                {
                    CSound sound = this.rSound[this.n次に鳴るサウンド番号];
                    if (sound != null)
                        sound.n位置 = value;
                }
            }
            public int nAutomationLevel_現在のサウンド
            {
                get
                {
                    CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
                    if (sound == null)
                        return 0;

                    return sound.AutomationLevel;
                }
                set
                {
                    CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
                    if (sound != null)
                    {
                        sound.AutomationLevel = value;
                    }
                }
            }
            public int n長さ_現在のサウンド
            {
                get
                {
                    CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
                    if (sound == null)
                    {
                        return 0;
                    }
                    return sound.n総演奏時間ms;
                }
            }
            public int n長さ_次に鳴るサウンド
            {
                get
                {
                    CSound sound = this.rSound[this.n次に鳴るサウンド番号];
                    if (sound == null)
                    {
                        return 0;
                    }
                    return sound.n総演奏時間ms;
                }
            }


            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="strファイル名"></param>
            /// <param name="bループ"></param>
            /// <param name="b排他"></param>
            public Cシステムサウンド(string strファイル名, bool bループ, bool b排他, ESoundGroup soundGroup)
            {
                this.strファイル名 = strファイル名;
                this.bループ = bループ;
                this.b排他 = b排他;
                _soundGroup = soundGroup;
                this.b読み込み未試行 = true;
            }


            // メソッド

            public void t読み込み()
            {
                this.b読み込み未試行 = false;
                this.b読み込み成功 = false;
                if (string.IsNullOrEmpty(this.strファイル名))
                    throw new InvalidOperationException("ファイル名が無効です。");

				if( !File.Exists( CSkin.Path( this.strファイル名 ) ) )
				{
                    Trace.TraceWarning($"ファイルが存在しません。: {this.strファイル名}");
				    return;
				}
////				for( int i = 0; i < 2; i++ )		// #27790 2012.3.10 yyagi 2回読み出しを、1回読みだし＋1回メモリコピーに変更
////				{
//                    try
//                    {
//                        this.rSound[ 0 ] = CDTXMania.Sound管理.tサウンドを生成する( CSkin.Path( this.strファイル名 ) );
//                    }
//                    catch
//                    {
//                        this.rSound[ 0 ] = null;
//                        throw;
//                    }
//                    if ( this.rSound[ 0 ] == null )	// #28243 2012.5.3 yyagi "this.rSound[ 0 ].bストリーム再生する"時もCloneするようにし、rSound[1]がnullにならないよう修正→rSound[1]の再生正常化
//                    {
//                        this.rSound[ 1 ] = null;
//                    }
//                    else
//                    {
//                        this.rSound[ 1 ] = ( CSound ) this.rSound[ 0 ].Clone();	// #27790 2012.3.10 yyagi add: to accelerate loading chip sounds
//                        CDTXMania.Sound管理.tサウンドを登録する( this.rSound[ 1 ] );	// #28243 2012.5.3 yyagi add (登録漏れによりストリーム再生処理が発生していなかった)
//                    }

                ////				}

                for (int i = 0; i < 2; i++)     // 一旦Cloneを止めてASIO対応に専念
                {
                    try
                    {
                        this.rSound[i] = TJAPlayer3.Sound管理.tサウンドを生成する(CSkin.Path(this.strファイル名), _soundGroup);
                    }
                    catch
                    {
                        this.rSound[i] = null;
                        throw;
                    }
                }
                this.b読み込み成功 = true;
            }
            public void t再生する()
            {
                if (this.b読み込み未試行)
                {
                    try
                    {
                        t読み込み();
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                        Trace.TraceError("例外が発生しましたが処理を継続します。 (17668977-4686-4aa7-b3f0-e0b9a44975b8)");
                        this.b読み込み未試行 = false;
                    }
                }
                if (this.b排他)
                {
                    if (r最後に再生した排他システムサウンド != null)
                        r最後に再生した排他システムサウンド.t停止する();

                    r最後に再生した排他システムサウンド = this;
                }
                CSound sound = this.rSound[this.n次に鳴るサウンド番号];
                if (sound != null)
                    sound.t再生を開始する(this.bループ);

                this.n次に鳴るサウンド番号 = 1 - this.n次に鳴るサウンド番号;
            }
            public void t停止する()
            {
                if (this.rSound[0] != null)
                    this.rSound[0].t再生を停止する();

                if (this.rSound[1] != null)
                    this.rSound[1].t再生を停止する();

                if (r最後に再生した排他システムサウンド == this)
                    r最後に再生した排他システムサウンド = null;
            }

            public void tRemoveMixer()
            {
                if (CSound管理.GetCurrentSoundDeviceType() != "DirectShow")
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (this.rSound[i] != null)
                        {
                            this.rSound[i].RemoveMixer();
                        }
                    }
                }
            }

            #region [ IDisposable 実装 ]
            //-----------------
            public void Dispose()
            {
                if (!this.bDisposed済み)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (this.rSound[i] != null)
                        {
                            TJAPlayer3.Sound管理.tサウンドを破棄する(this.rSound[i]);
                            this.rSound[i] = null;
                        }
                    }
                    this.b読み込み成功 = false;
                    this.bDisposed済み = true;
                }
            }
            //-----------------
            #endregion

            #region [ private ]
            //-----------------
            private bool bDisposed済み;
            private int n次に鳴るサウンド番号;
            private CSound[] rSound = new CSound[2];
            //-----------------
            #endregion
        }


        // プロパティ

        public Cシステムサウンド bgmオプション画面 = null;
        public Cシステムサウンド bgmコンフィグ画面 = null;
        public Cシステムサウンド bgm起動画面 = null;
        public Cシステムサウンド bgm選曲画面 = null;
        public Cシステムサウンド bgm結果画面 = null;
        public Cシステムサウンド soundSTAGEFAILED音 = null;
        public Cシステムサウンド soundカーソル移動音 = null;
        public Cシステムサウンド soundゲーム開始音 = null;
        public Cシステムサウンド soundゲーム終了音 = null;
        public Cシステムサウンド soundステージクリア音 = null;
        public Cシステムサウンド soundタイトル音 = null;
        public Cシステムサウンド soundフルコンボ音 = null;
        public Cシステムサウンド sound歓声音 = null;
        public Cシステムサウンド sound曲読込開始音 = null;
        public Cシステムサウンド sound決定音 = null;
        public Cシステムサウンド sound取消音 = null;
        public Cシステムサウンド sound変更音 = null;
        //add
        public Cシステムサウンド bgmリザルト = null;
        public Cシステムサウンド bgmリザルトループ = null;
        public Cシステムサウンド sound曲決定音 = null;
        public Cシステムサウンド sound成績発表 = null;

        //public Cシステムサウンド soundRed = null;
        //public Cシステムサウンド soundBlue = null;
        public Cシステムサウンド soundBalloon = null;


        private readonly int nシステムサウンド数 = (int)Eシステムサウンド.Count;

        public Cシステムサウンド this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.soundカーソル移動音;

                    case 1:
                        return this.sound決定音;

                    case 2:
                        return this.sound変更音;

                    case 3:
                        return this.sound取消音;

                    case 4:
                        return this.sound歓声音;

                    case 5:
                        return this.soundSTAGEFAILED音;

                    case 6:
                        return this.soundゲーム開始音;

                    case 7:
                        return this.soundゲーム終了音;

                    case 8:
                        return this.soundステージクリア音;

                    case 9:
                        return this.soundフルコンボ音;

                    case 10:
                        return this.sound曲読込開始音;

                    case 11:
                        return this.soundタイトル音;

                    case 12:
                        return this.bgm起動画面;

                    case 13:
                        return this.bgmオプション画面;

                    case 14:
                        return this.bgmコンフィグ画面;

                    case 15:
                        return this.bgm選曲画面;

                    case 16:
                        return this.bgm結果画面;

                    //case 16:
                    //    return this.soundRed;

                    //case 17:
                    //    return this.soundBlue;

                    case 17:
                        return this.soundBalloon;

                    case 18:
                        return this.sound曲決定音;

                    case 19:
                        return this.sound成績発表;
                }
                throw new IndexOutOfRangeException();
            }
        }


        // スキンの切り替えについて___
        //
        // _スキンの種類は大きく分けて2種類。Systemスキンとboxdefスキン。
        // 　前者はSystem/フォルダにユーザーが自らインストールしておくスキン。
        // 　後者はbox.defで指定する、曲データ制作者が提示するスキン。
        //
        // _Config画面で、2種のスキンを区別無く常時使用するよう設定することができる。
        // _box.defの#SKINPATH 設定により、boxdefスキンを一時的に使用するよう設定する。
        // 　(box.defの効果の及ばない他のmuxic boxでは、当該boxdefスキンの有効性が無くなる)
        //
        // これを実現するために___
        // _Systemスキンの設定情報と、boxdefスキンの設定情報は、分離して持つ。
        // 　(strSystem～～ と、strBoxDef～～～)
        // _Config画面からは前者のみ書き換えできるようにし、
        // 　選曲画面からは後者のみ書き換えできるようにする。(SetCurrent...())
        // _読み出しは両者から行えるようにすると共に
        // 　選曲画面用に二種の情報を区別しない読み出し方法も提供する(GetCurrent...)

        private object lockBoxDefSkin;
        public static bool bUseBoxDefSkin = true;                       // box.defからのスキン変更を許容するか否か

        public string strSystemSkinRoot = null;
        public string[] strSystemSkinSubfolders = null;     // List<string>だとignoreCaseな検索が面倒なので、配列に逃げる :-)
        private string[] _strBoxDefSkinSubfolders = null;
        public string[] strBoxDefSkinSubfolders
        {
            get
            {
                lock (lockBoxDefSkin)
                {
                    return _strBoxDefSkinSubfolders;
                }
            }
            set
            {
                lock (lockBoxDefSkin)
                {
                    _strBoxDefSkinSubfolders = value;
                }
            }
        }           // 別スレッドからも書き込みアクセスされるため、スレッドセーフなアクセス法を提供

        private static string strSystemSkinSubfolderFullName;           // Config画面で設定されたスキン
        private static string strBoxDefSkinSubfolderFullName = "";      // box.defで指定されているスキン

        public static string GetCurrentBoxDefSkinName()
        {
            return GetSkinName(strBoxDefSkinSubfolderFullName, false);
        }

        public static string GetCurrentSystemSkinName()
        {
            return GetSkinName(strSystemSkinSubfolderFullName, false);
        }

        /// <summary>
        /// スキンパス名をフルパスで取得する
        /// </summary>
        /// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
        /// <returns></returns>
        public string GetCurrentSkinSubfolderFullName(bool bFromUserConfig)
        {
            if (!bUseBoxDefSkin || bFromUserConfig == true || strBoxDefSkinSubfolderFullName == "")
            {
                return strSystemSkinSubfolderFullName;
            }
            else
            {
                return strBoxDefSkinSubfolderFullName;
            }
        }
        /// <summary>
        /// スキンパス名をフルパスで設定する
        /// </summary>
        /// <param name="value">スキンパス名</param>
        /// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
        public void SetCurrentSkinSubfolderFullName(string value, bool bFromUserConfig)
        {
            if (bFromUserConfig)
            {
                strSystemSkinSubfolderFullName = value;
            }
            else
            {
                strBoxDefSkinSubfolderFullName = value;
            }
        }


        // コンストラクタ
        public CSkin(string _strSkinSubfolderFullName, bool _bUseBoxDefSkin)
        {
            lockBoxDefSkin = new object();
            strSystemSkinSubfolderFullName = _strSkinSubfolderFullName;
            bUseBoxDefSkin = _bUseBoxDefSkin;
            InitializeSkinPathRoot();
            ReloadSkinPaths();
            PrepareReloadSkin();
        }

        private string InitializeSkinPathRoot()
        {
            strSystemSkinRoot = System.IO.Path.Combine(TJAPlayer3.strEXEのあるフォルダ, "System" + System.IO.Path.DirectorySeparatorChar);
            return strSystemSkinRoot;
        }

        /// <summary>
        /// Skin(Sounds)を再読込する準備をする(再生停止,Dispose,ファイル名再設定)。
        /// あらかじめstrSkinSubfolderを適切に設定しておくこと。
        /// その後、ReloadSkinPaths()を実行し、strSkinSubfolderの正当性を確認した上で、本メソッドを呼び出すこと。
        /// 本メソッド呼び出し後に、ReloadSkin()を実行することで、システムサウンドを読み込み直す。
        /// ReloadSkin()の内容は本メソッド内に含めないこと。起動時はReloadSkin()相当の処理をCEnumSongsで行っているため。
        /// </summary>
        public void PrepareReloadSkin()
        {
            Trace.TraceInformation("SkinPath設定: {0}",
                (strBoxDefSkinSubfolderFullName == "") ?
                strSystemSkinSubfolderFullName :
                strBoxDefSkinSubfolderFullName
            );

            for (int i = 0; i < nシステムサウンド数; i++)
            {
                if (this[i] != null && this[i].b読み込み成功)
                {
                    this[i].t停止する();
                    this[i].Dispose();
                }
            }
            this.soundカーソル移動音 = new Cシステムサウンド(@"Sounds\Move.ogg", false, false, ESoundGroup.SoundEffect);
            this.sound決定音 = new Cシステムサウンド(@"Sounds\Decide.ogg", false, false, ESoundGroup.SoundEffect);
            this.sound変更音 = new Cシステムサウンド(@"Sounds\Change.ogg", false, false, ESoundGroup.SoundEffect);
            this.sound取消音 = new Cシステムサウンド(@"Sounds\Cancel.ogg", false, false, ESoundGroup.SoundEffect);
            this.sound歓声音 = new Cシステムサウンド(@"Sounds\Audience.ogg", false, false, ESoundGroup.SoundEffect);
            this.soundSTAGEFAILED音 = new Cシステムサウンド(@"Sounds\Stage failed.ogg", false, true, ESoundGroup.Voice);
            this.soundゲーム開始音 = new Cシステムサウンド(@"Sounds\Game start.ogg", false, false, ESoundGroup.Voice);
            this.soundゲーム終了音 = new Cシステムサウンド(@"Sounds\Game end.ogg", false, true, ESoundGroup.Voice);
            this.soundステージクリア音 = new Cシステムサウンド(@"Sounds\Stage clear.ogg", false, true, ESoundGroup.Voice);
            this.soundフルコンボ音 = new Cシステムサウンド(@"Sounds\Full combo.ogg", false, false, ESoundGroup.Voice);
            this.sound曲読込開始音 = new Cシステムサウンド(@"Sounds\Now loading.ogg", false, true, ESoundGroup.Unknown);
            this.soundタイトル音 = new Cシステムサウンド(@"Sounds\Title.ogg", false, true, ESoundGroup.SongPlayback);
            this.bgm起動画面 = new Cシステムサウンド(@"Sounds\Setup BGM.ogg", true, true, ESoundGroup.SongPlayback);
            this.bgmオプション画面 = new Cシステムサウンド(@"Sounds\Option BGM.ogg", true, true, ESoundGroup.SongPlayback);
            this.bgmコンフィグ画面 = new Cシステムサウンド(@"Sounds\Config BGM.ogg", true, true,  ESoundGroup.SongPlayback);
            this.bgm選曲画面 = new Cシステムサウンド(@"Sounds\Select BGM.ogg", true, true, ESoundGroup.SongPreview);
            this.bgm結果画面 = new Cシステムサウンド(@"Sounds\Result BGM.ogg", true, true, ESoundGroup.SongPreview);

            //this.soundRed               = new Cシステムサウンド( @"Sounds\dong.ogg",            false, false, ESoundType.SoundEffect );
            //this.soundBlue              = new Cシステムサウンド( @"Sounds\ka.ogg",              false, false, ESoundType.SoundEffect );
            this.soundBalloon = new Cシステムサウンド(@"Sounds\balloon.ogg", false, false, ESoundGroup.SoundEffect);
            this.sound曲決定音 = new Cシステムサウンド(@"Sounds\SongDecide.ogg", false, false, ESoundGroup.Voice);
            this.sound成績発表 = new Cシステムサウンド(@"Sounds\ResultIn.ogg", false, false, ESoundGroup.Voice);
            ReloadSkin();
            tReadSkinConfig();
        }

        public void ReloadSkin()
        {
            for (int i = 0; i < nシステムサウンド数; i++)
            {
                var cシステムサウンド = this[i];

                if (cシステムサウンド.b排他)
                {
                    continue;
                }

                try
                {
                    cシステムサウンド.t読み込み();
                    Trace.TraceInformation("システムサウンドを読み込みました。({0})", cシステムサウンド.strファイル名);
                }
                catch (FileNotFoundException e)
                {
                    Trace.TraceWarning(e.ToString());
                    Trace.TraceWarning("システムサウンドが存在しません。({0})", cシステムサウンド.strファイル名);
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    Trace.TraceWarning("システムサウンドの読み込みに失敗しました。({0})", cシステムサウンド.strファイル名);
                }
            }
        }


        /// <summary>
        /// Skinの一覧を再取得する。
        /// System/*****/Graphics (やSounds/) というフォルダ構成を想定している。
        /// もし再取得の結果、現在使用中のSkinのパス(strSystemSkinSubfloderFullName)が消えていた場合は、
        /// 以下の優先順位で存在確認の上strSystemSkinSubfolderFullNameを再設定する。
        /// 1. System/Default/
        /// 2. System/*****/ で最初にenumerateされたもの
        /// 3. System/ (従来互換)
        /// </summary>
        public void ReloadSkinPaths()
        {
            #region [ まず System/*** をenumerateする ]
            string[] tempSkinSubfolders = System.IO.Directory.GetDirectories(strSystemSkinRoot, "*");
            strSystemSkinSubfolders = new string[tempSkinSubfolders.Length];
            int size = 0;
            for (int i = 0; i < tempSkinSubfolders.Length; i++)
            {
                #region [ 検出したフォルダがスキンフォルダかどうか確認する]
                if (!bIsValid(tempSkinSubfolders[i]))
                    continue;
                #endregion
                #region [ スキンフォルダと確認できたものを、strSkinSubfoldersに入れる ]
                // フォルダ名末尾に必ず\をつけておくこと。さもないとConfig読み出し側(必ず\をつける)とマッチできない
                if (tempSkinSubfolders[i][tempSkinSubfolders[i].Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    tempSkinSubfolders[i] += System.IO.Path.DirectorySeparatorChar;
                }
                strSystemSkinSubfolders[size] = tempSkinSubfolders[i];
                Trace.TraceInformation("SkinPath検出: {0}", strSystemSkinSubfolders[size]);
                size++;
                #endregion
            }
            Trace.TraceInformation("SkinPath入力: {0}", strSystemSkinSubfolderFullName);
            Array.Resize(ref strSystemSkinSubfolders, size);
            Array.Sort(strSystemSkinSubfolders);    // BinarySearch実行前にSortが必要
            #endregion

            #region [ 現在のSkinパスがbox.defスキンをCONFIG指定していた場合のために、最初にこれが有効かチェックする。有効ならこれを使う。 ]
            if (bIsValid(strSystemSkinSubfolderFullName) &&
                Array.BinarySearch(strSystemSkinSubfolders, strSystemSkinSubfolderFullName,
                StringComparer.InvariantCultureIgnoreCase) < 0)
            {
                strBoxDefSkinSubfolders = new string[1] { strSystemSkinSubfolderFullName };
                return;
            }
            #endregion

            #region [ 次に、現在のSkinパスが存在するか調べる。あれば終了。]
            if (Array.BinarySearch(strSystemSkinSubfolders, strSystemSkinSubfolderFullName,
                StringComparer.InvariantCultureIgnoreCase) >= 0)
                return;
            #endregion
            #region [ カレントのSkinパスが消滅しているので、以下で再設定する。]
            /// 以下の優先順位で現在使用中のSkinパスを再設定する。
            /// 1. System/Default/
            /// 2. System/*****/ で最初にenumerateされたもの
            /// 3. System/ (従来互換)
            #region [ System/Default/ があるなら、そこにカレントSkinパスを設定する]
            string tempSkinPath_default = System.IO.Path.Combine(strSystemSkinRoot, "Default" + System.IO.Path.DirectorySeparatorChar);
            if (Array.BinarySearch(strSystemSkinSubfolders, tempSkinPath_default,
                StringComparer.InvariantCultureIgnoreCase) >= 0)
            {
                strSystemSkinSubfolderFullName = tempSkinPath_default;
                return;
            }
            #endregion
            #region [ System/SkinFiles.*****/ で最初にenumerateされたものを、カレントSkinパスに再設定する ]
            if (strSystemSkinSubfolders.Length > 0)
            {
                strSystemSkinSubfolderFullName = strSystemSkinSubfolders[0];
                return;
            }
            #endregion
            #region [ System/ に、カレントSkinパスを再設定する。]
            strSystemSkinSubfolderFullName = strSystemSkinRoot;
            strSystemSkinSubfolders = new string[1] { strSystemSkinSubfolderFullName };
            #endregion
            #endregion
        }

        // メソッド

        public static string Path(string strファイルの相対パス)
        {
            if (strBoxDefSkinSubfolderFullName == "" || !bUseBoxDefSkin)
            {
                return System.IO.Path.Combine(strSystemSkinSubfolderFullName, strファイルの相対パス);
            }
            else
            {
                return System.IO.Path.Combine(strBoxDefSkinSubfolderFullName, strファイルの相対パス);
            }
        }

        /// <summary>
        /// フルパス名を与えると、スキン名として、ディレクトリ名末尾の要素を返す
        /// 例: C:\foo\bar\ なら、barを返す
        /// </summary>
        /// <param name="skinpath">スキンが格納されたパス名(フルパス)</param>
        /// <returns>スキン名</returns>
        public static string GetSkinName(string skinPathFullName, bool fallBackToSystemOnEmpty = true)
        {
            if (skinPathFullName != null)
            {
                if (skinPathFullName == "")     // 「box.defで未定義」用
                {
                    if (fallBackToSystemOnEmpty)
                    {
                        skinPathFullName = strSystemSkinSubfolderFullName;
                    }
                    else
                    {
                        return skinPathFullName;
                    }
                }
                string[] tmp = skinPathFullName.Split(System.IO.Path.DirectorySeparatorChar);
                return tmp[tmp.Length - 2];     // ディレクトリ名の最後から2番目の要素がスキン名(最後の要素はnull。元stringの末尾が\なので。)
            }
            return null;
        }
        public static string[] GetSkinName(string[] skinPathFullNames)
        {
            string[] ret = new string[skinPathFullNames.Length];
            for (int i = 0; i < skinPathFullNames.Length; i++)
            {
                ret[i] = GetSkinName(skinPathFullNames[i]);
            }
            return ret;
        }


        public string GetSkinSubfolderFullNameFromSkinName(string skinName)
        {
            foreach (string s in strSystemSkinSubfolders)
            {
                if (GetSkinName(s) == skinName)
                    return s;
            }
            foreach (string b in strBoxDefSkinSubfolders)
            {
                if (GetSkinName(b) == skinName)
                    return b;
            }
            return null;
        }

        /// <summary>
        /// スキンパス名が妥当かどうか
        /// (タイトル画像にアクセスできるかどうかで判定する)
        /// </summary>
        /// <param name="skinPathFullName">妥当性を確認するスキンパス(フルパス)</param>
        /// <returns>妥当ならtrue</returns>
        public bool bIsValid(string skinPathFullName)
        {
            string filePathTitle;
            filePathTitle = System.IO.Path.Combine(skinPathFullName, @"Graphics\1_Title\Background.png");
            return (File.Exists(filePathTitle));
        }


        public void tRemoveMixerAll()
        {
            for (int i = 0; i < nシステムサウンド数; i++)
            {
                if (this[i] != null && this[i].b読み込み成功)
                {
                    this[i].t停止する();
                    this[i].tRemoveMixer();
                }
            }

        }

        public void tReadSkinConfig()
        {
            var str = "";
            LoadSkinConfigFromFile(Path(@"SkinConfig.ini"), ref str);
            this.t文字列から読み込み(str);

            void LoadSkinConfigFromFile(string path, ref string work)
            {
                if (!File.Exists(Path(path))) return;
                using (var streamReader = new StreamReader(Path(path), Encoding.GetEncoding("Shift_JIS")))
                {
                    while (streamReader.Peek() > -1) // 一行ずつ読み込む。
                    {
                        var nowLine = streamReader.ReadLine();
                        if (nowLine.StartsWith("#include"))
                        {
                            // #include hogehoge.iniにぶち当たった
                            var includePath = nowLine.Substring("#include ".Length).Trim();
                            LoadSkinConfigFromFile(includePath, ref work); // 再帰的に読み込む
                        }
                        else
                        {
                            work += nowLine + Environment.NewLine;
                        }
                    }
                }
            }
        }

        private void t文字列から読み込み(string strAllSettings)	// 2011.4.13 yyagi; refactored to make initial KeyConfig easier.
        {
            string[] delimiter = { "\n" };
            string[] strSingleLine = strAllSettings.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in strSingleLine)
            {
                string str = s.Replace('\t', ' ').TrimStart(new char[] { '\t', ' ' });
                if ((str.Length != 0) && (str[0] != ';'))
                {
                    try
                    {
                        string strCommand;
                        string strParam;
                        string[] strArray = str.Split(new char[] { '=' });
                        if (strArray.Length == 2)
                        {
                            strCommand = strArray[0].Trim();
                            strParam = strArray[1].Trim();

                            #region スキン設定

                            void ParseInt32(Action<int> setValue)
                            {
                                if (int.TryParse(strParam, out var unparsedValue))
                                {
                                    setValue(unparsedValue);
                                }
                                else
                                {
                                    Trace.TraceWarning($"SkinConfigの値 {strCommand} は整数値である必要があります。現在の値: {strParam}");
                                }
                            }

                            if (strCommand == "Name")
                            {
                                this.Skin_Name = strParam;
                            }
                            else if (strCommand == "Version")
                            {
                                this.Skin_Version = strParam;
                            }
                            else if (strCommand == "Creator")
                            {
                                this.Skin_Creator = strParam;
                            }
                            #endregion
                            #region Font
                            else if (strCommand == nameof(Font_Edge_Ratio)) //Config画面や簡易メニューのフォントについて(rhimm)
                            {
                                if (int.Parse(strParam) > 0)
                                    Font_Edge_Ratio = int.Parse(strParam);
                            }
                            else if (strCommand == nameof(Font_Edge_Ratio_Vertical)) //TITLEやSUBTITLEなど、縦に書かれることのあるフォントについて(rhimm)
                            {
                                if (int.Parse(strParam) > 0)
                                    Font_Edge_Ratio_Vertical = int.Parse(strParam);
                            }
                            else if (strCommand == nameof(Text_Correction_XY))
                            {
                                Text_Correction_XY = strParam.Split(',').Select(int.Parse).ToArray();
                            }
                            #endregion
                        }
                        continue;
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceError(exception.ToString());
                        Trace.TraceError("例外が発生しましたが処理を継続します。 (6a32cc37-1527-412e-968a-512c1f0135cd)");
                        continue;
                    }
                }
            }
        }

        #region [ IDisposable 実装 ]
        //-----------------
        public void Dispose()
        {
            if (!this.bDisposed済み)
            {
                for (int i = 0; i < this.nシステムサウンド数; i++)
                    this[i].Dispose();

                this.bDisposed済み = true;
            }
        }
        //-----------------
        #endregion


        // その他

        #region [ private ]
        //-----------------
        private bool bDisposed済み;
        //-----------------
        #endregion

        #region 背景(スクロール)
        public int[] Background_Scroll_Y = new int[] { 0, 536 };
        #endregion


        #region[ 座標 ]
        //2017.08.11 kairera0467 DP実用化に向けてint配列に変更
        //2019.01.05 rhimm 自由化のため、レーン座標・判定枠関連を 新・SkinConfig>Lane 下に移動

        //SEnotes
        //音符座標に加算
        public int[] nSENotesY = new int[] { 131, 131 };

        //光る太鼓部分
        public int nMtaikoBackgroundX = 0;
        public int nMtaikoBackgroundY = 184;
        public int nMtaikoFieldX = 0;
        public int nMtaikoFieldY = 184;
        public int nMtaikoMainX = 0;
        public int nMtaikoMainY = 0;

        //コンボ
        public int[] nComboNumberX = new int[] { 0, 0, 0, 0 };
        public int[] nComboNumberY = new int[] { 212, 388, 0, 0 };
        public int[] nComboNumberTextY = new int[] { 271, 447, 0, 0 };
        public int[] nComboNumberTextLargeY = new int[] { 270, 446, 0, 0 };
        public float fComboNumberSpacing = 0;
        public float fComboNumberSpacing_l = 0;

        public bool b現在のステージ数を表示しない;

        //リザルト画面
        //現在のデフォルト値はダミーです。
        public int nResultPanelP1X = 515;
        public int nResultPanelP1Y = 75;
        public int nResultPanelP2X = 515;
        public int nResultPanelP2Y = 75;
        public int nResultScoreP1X = 582;
        public int nResultScoreP1Y = 252;
        public int nResultJudge1_P1X = 815;
        public int nResultJudge1_P1Y = 182;
        public int nResultJudge2_P1X = 968;
        public int nResultJudge2_P1Y = 174;
        public int nResultGreatP1X = 875;
        public int nResultGreatP1Y = 188;
        public int nResultGreatP2X = 875;
        public int nResultGreatP2Y = 188;
        public int nResultGoodP1X = 875;
        public int nResultGoodP1Y = 226;
        public int nResultGoodP2X = 875;
        public int nResultGoodP2Y = 226;
        public int nResultBadP1X = 875;
        public int nResultBadP1Y = 266;
        public int nResultBadP2X = 875;
        public int nResultBadP2Y = 266;
        public int nResultComboP1X = 1144;
        public int nResultComboP1Y = 188;
        public int nResultComboP2X = 1144;
        public int nResultComboP2Y = 188;
        public int nResultRollP1X = 1144;
        public int nResultRollP1Y = 226;
        public int nResultRollP2X = 1144;
        public int nResultRollP2Y = 226;
        public int nResultGaugeBaseP1X = 555;
        public int nResultGaugeBaseP1Y = 122;
        public int nResultGaugeBaseP2X = 555;
        public int nResultGaugeBaseP2Y = 122;
        public int nResultGaugeBodyP1X = 559;
        public int nResultGaugeBodyP1Y = 125;
        #endregion

        public enum RollColorMode
        {
            None, // PS4, Switchなど
            All, // 旧筐体(旧作含む)
            WithoutStart // 新筐体
        }

        #region 新・SkinConfig
        #region General
        public string Skin_Name = "Unknown";
        public string Skin_Version = "Unknown";
        public string Skin_Creator = "Unknown";
        #endregion
        #region Font
        public int Font_Edge_Ratio = 30;
        public int Font_Edge_Ratio_Vertical = 30;
        public int[] Text_Correction_XY = new int[] { 0, 0 };
        #endregion
        #endregion
    }
}