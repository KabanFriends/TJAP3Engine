using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using FDK;
using FDK.ExtensionMethods;
using TJAPlayer3.Common;

namespace TJAPlayer3
{
	internal class CConfigIni : INotifyPropertyChanged
	{
	    private const int MinimumKeyboardSoundLevelIncrement = 1;
	    private const int MaximumKeyboardSoundLevelIncrement = 20;
	    private const int DefaultKeyboardSoundLevelIncrement = 5;

		//
		public enum ESoundDeviceTypeForConfig
		{
			ACM = 0,
			// DirectSound,
			ASIO,
			WASAPI,
			Unknown=99
		}
		// プロパティ

#if false		// #23625 2011.1.11 Config.iniからダメージ/回復値の定数変更を行う場合はここを有効にする 087リリースに合わせ機能無効化
		//----------------------------------------
		public float[,] fGaugeFactor = new float[5,2];
		public float[] fDamageLevelFactor = new float[3];
		//----------------------------------------
#endif
		public int nBGAlpha;
		public bool bAVI有効;
		public bool bBGA有効;
		public bool bBGM音を発声する;
		public bool bLogDTX詳細ログ出力;
		public bool bLog曲検索ログ出力;
		public bool bLog作成解放ログ出力;
		//public STDGBVALUE<E判定表示優先度> e判定表示優先度;
		public bool bScoreIniを出力する;
		public bool bSTAGEFAILED有効;
		public bool bTight;
		public bool bWave再生位置自動調整機能有効;
		public bool bストイックモード;
		public bool bランダムセレクトで子BOXを検索対象とする;
		public bool bログ出力;
		public bool b演奏情報を表示する;
		public bool b垂直帰線待ちを行う;
		public bool b全画面モード;
		public int n初期ウィンドウ開始位置X; // #30675 2013.02.04 ikanick add
		public int n初期ウィンドウ開始位置Y;  
		public int nウインドウwidth;				// #23510 2010.10.31 yyagi add
		public int nウインドウheight;				// #23510 2010.10.31 yyagi add
		public Dictionary<int, string> dicJoystick;
		public int n非フォーカス時スリープms;       // #23568 2010.11.04 ikanick add
		public int nフレーム毎スリープms;			// #xxxxx 2011.11.27 yyagi add
		public int n演奏速度;
		public int n曲が選択されてからプレビュー音が鳴るまでのウェイトms;
		public int n曲が選択されてからプレビュー画像が表示開始されるまでのウェイトms;

	    private bool _applyLoudnessMetadata;

	    public bool ApplyLoudnessMetadata
	    {
	        get => _applyLoudnessMetadata;
	        set => SetProperty(ref _applyLoudnessMetadata, value, nameof(ApplyLoudnessMetadata));
	    }

	    private double _targetLoudness;

	    public double TargetLoudness
	    {
	        get => _targetLoudness;
	        set => SetProperty(ref _targetLoudness, value, nameof(TargetLoudness));
	    }

	    private bool _applySongVol;

	    public bool ApplySongVol
	    {
	        get => _applySongVol;
	        set => SetProperty(ref _applySongVol, value, nameof(ApplySongVol));
	    }

	    private int _soundEffectLevel;

	    public int SoundEffectLevel
	    {
	        get => _soundEffectLevel;
	        set => SetProperty(ref _soundEffectLevel, value, nameof(SoundEffectLevel));
	    }

	    private int _voiceLevel;

	    public int VoiceLevel
	    {
	        get => _voiceLevel;
	        set => SetProperty(ref _voiceLevel, value, nameof(VoiceLevel));
	    }

	    private int _songPreviewLevel;

	    public int SongPreviewLevel
	    {
	        get => _songPreviewLevel;
	        set => SetProperty(ref _songPreviewLevel, value, nameof(SongPreviewLevel));
	    }

	    private int _songPlaybackLevel;

	    public int SongPlaybackLevel
	    {
	        get => _songPlaybackLevel;
	        set => SetProperty(ref _songPlaybackLevel, value, nameof(SongPlaybackLevel));
	    }

	    private int _keyboardSoundLevelIncrement;

	    public int KeyboardSoundLevelIncrement
	    {
	        get => _keyboardSoundLevelIncrement;
	        set => SetProperty(
	            ref _keyboardSoundLevelIncrement,
	            value.Clamp(MinimumKeyboardSoundLevelIncrement, MaximumKeyboardSoundLevelIncrement),
	            nameof(KeyboardSoundLevelIncrement));
	    }

		public string strDTXManiaのバージョン;
		public string str曲データ検索パス;
        public string FontName;
        public bool bBranchGuide;
        public int nScoreMode;
        public int nDefaultCourse; //2017.01.30 DD デフォルトでカーソルをあわせる難易度


        public int nPlayerCount; //2017.08.18 kairera0467 マルチプレイ対応
        public bool b太鼓パートAutoPlay;
        public bool b太鼓パートAutoPlay2P; //2017.08.16 kairera0467 マルチプレイ対応
        public bool bAuto先生の連打;
        public bool b大音符判定;
        public int n両手判定の待ち時間;
        public int nBranchAnime;

        public bool bJudgeCountDisplay;

        // 各画像の表示・非表示設定
        public bool ShowChara;
        public bool ShowDancer;
        public bool ShowRunner;
        public bool ShowFooter;
        public bool ShowMob;
        public bool ShowPuchiChara; // リザーブ
        //
        public bool bスクロールモードを上書き = false;

        public bool bゲージモードを上書き = false;

        public bool bHispeedRandom;
        public bool bNoInfo;

        public int nDefaultSongSort;

        public bool bSuperHard = false;
        public bool bJust;

        public bool bEndingAnime = false;   // 2017.01.27 DD 「また遊んでね」画面の有効/無効オプション追加


//		public int nハイハット切り捨て下限Velocity;
//		public int n切り捨て下限Velocity;			// #23857 2010.12.12 yyagi VelocityMin
		public int nInputAdjustTimeMs;
		public bool bIsAutoResultCapture;			// #25399 2011.6.9 yyagi リザルト画像自動保存機能のON/OFF制御
		public int nPoliphonicSounds;				// #28228 2012.5.1 yyagi レーン毎の最大同時発音数
		public bool bバッファ入力を行う;
		public bool bIsEnabledSystemMenu;			// #28200 2012.5.1 yyagi System Menuの使用可否切替
		public string strSystemSkinSubfolderFullName;	// #28195 2012.5.2 yyagi Skin切替用 System/以下のサブフォルダ名
		public bool bConfigIniがないかDTXManiaのバージョンが異なる
		{
			get
			{
				return ( !this.bConfigIniが存在している || !TJAPlayer3.AppNumericThreePartVersion.Equals( this.strDTXManiaのバージョン ) );
			}
		}
		public bool bウィンドウモード
		{
			get
			{
				return !this.b全画面モード;
			}
			set
			{
				this.b全画面モード = !value;
			}
		}
		public bool b演奏情報を表示しない
		{
			get
			{
				return !this.b演奏情報を表示する;
			}
			set
			{
				this.b演奏情報を表示する = !value;
			}
		}
		public int n背景の透過度
		{
			get
			{
				return this.nBGAlpha;
			}
			set
			{
				if( value < 0 )
				{
					this.nBGAlpha = 0;
				}
				else if( value > 0xff )
				{
					this.nBGAlpha = 0xff;
				}
				else
				{
					this.nBGAlpha = value;
				}
			}
		}
		public int nRisky;						// #23559 2011.6.20 yyagi Riskyでの残ミス数。0で閉店
		public bool bIsAllowedDoubleClickFullscreen;	// #26752 2011.11.27 yyagi ダブルクリックしてもフルスクリーンに移行しない
		public int nSoundDeviceType;				// #24820 2012.12.23 yyagi 出力サウンドデバイス(0=ACM(にしたいが設計がきつそうならDirectShow), 1=ASIO, 2=WASAPI)
		public int nWASAPIBufferSizeMs;				// #24820 2013.1.15 yyagi WASAPIのバッファサイズ
//		public int nASIOBufferSizeMs;				// #24820 2012.12.28 yyagi ASIOのバッファサイズ
		public int nASIODevice;						// #24820 2013.1.17 yyagi ASIOデバイス
		public bool bUseOSTimer;					// #33689 2014.6.6 yyagi 演奏タイマーの種類
		public bool bDynamicBassMixerManagement;	// #24820
		public bool bTimeStretch;					// #23664 2013.2.24 yyagi ピッチ変更無しで再生速度を変更するかどうか
		public int nDisplayTimesMs, nFadeoutTimeMs;

		//public bool bNoMP3Streaming;				// 2014.4.14 yyagi; mp3のシーク位置がおかしくなる場合は、これをtrueにすることで、wavにデコードしてからオンメモリ再生する
		public int nMasterVolume;
        public bool ShinuchiMode; // 真打モード
        public bool FastRender; // 事前画像描画モード
        public int MusicPreTimeMs; // 音源再生前の待機時間ms
        /// <summary>
        /// DiscordのRitch Presenceに再生中の.tjaファイルの情報を送信するかどうか。
        /// </summary>
        public bool SendDiscordPlayingInformation;
#if false
		[StructLayout( LayoutKind.Sequential )]
		public struct STAUTOPLAY								// C定数のEレーンとindexを一致させること
		{
			public bool LC;			// 0
			public bool HH;			// 1
			public bool SD;			// 2
			public bool BD;			// 3
			public bool HT;			// 4
			public bool LT;			// 5
			public bool FT;			// 6
			public bool CY;			// 7
			public bool RD;			// 8
			public bool Guitar;		// 9	(not used)
			public bool Bass;		// 10	(not used)
			public bool GtR;		// 11
			public bool GtG;		// 12
			public bool GtB;		// 13
			public bool GtPick;		// 14
			public bool GtW;		// 15
			public bool BsR;		// 16
			public bool BsG;		// 17
			public bool BsB;		// 18
			public bool BsPick;		// 19
			public bool BsW;		// 20
			public bool this[ int index ]
			{
				get
				{
					switch ( index )
					{
						case (int) Eレーン.LC:
							return this.LC;
						case (int) Eレーン.HH:
							return this.HH;
						case (int) Eレーン.SD:
							return this.SD;
						case (int) Eレーン.BD:
							return this.BD;
						case (int) Eレーン.HT:
							return this.HT;
						case (int) Eレーン.LT:
							return this.LT;
						case (int) Eレーン.FT:
							return this.FT;
						case (int) Eレーン.CY:
							return this.CY;
						case (int) Eレーン.RD:
							return this.RD;
						case (int) Eレーン.Guitar:
							return this.Guitar;
						case (int) Eレーン.Bass:
							return this.Bass;
						case (int) Eレーン.GtR:
							return this.GtR;
						case (int) Eレーン.GtG:
							return this.GtG;
						case (int) Eレーン.GtB:
							return this.GtB;
						case (int) Eレーン.GtPick:
							return this.GtPick;
						case (int) Eレーン.GtW:
							return this.GtW;
						case (int) Eレーン.BsR:
							return this.BsR;
						case (int) Eレーン.BsG:
							return this.BsG;
						case (int) Eレーン.BsB:
							return this.BsB;
						case (int) Eレーン.BsPick:
							return this.BsPick;
						case (int) Eレーン.BsW:
							return this.BsW;
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					switch ( index )
					{
						case (int) Eレーン.LC:
							this.LC = value;
							return;
						case (int) Eレーン.HH:
							this.HH = value;
							return;
						case (int) Eレーン.SD:
							this.SD = value;
							return;
						case (int) Eレーン.BD:
							this.BD = value;
							return;
						case (int) Eレーン.HT:
							this.HT = value;
							return;
						case (int) Eレーン.LT:
							this.LT = value;
							return;
						case (int) Eレーン.FT:
							this.FT = value;
							return;
						case (int) Eレーン.CY:
							this.CY = value;
							return;
						case (int) Eレーン.RD:
							this.RD = value;
							return;
						case (int) Eレーン.Guitar:
							this.Guitar = value;
							return;
						case (int) Eレーン.Bass:
							this.Bass = value;
							return;
						case (int) Eレーン.GtR:
							this.GtR = value;
							return;
						case (int) Eレーン.GtG:
							this.GtG = value;
							return;
						case (int) Eレーン.GtB:
							this.GtB = value;
							return;
						case (int) Eレーン.GtPick:
							this.GtPick = value;
							return;
						case (int) Eレーン.GtW:
							this.GtW = value;
							return;
						case (int) Eレーン.BsR:
							this.BsR = value;
							return;
						case (int) Eレーン.BsG:
							this.BsG = value;
							return;
						case (int) Eレーン.BsB:
							this.BsB = value;
							return;
						case (int) Eレーン.BsPick:
							this.BsPick = value;
							return;
						case (int) Eレーン.BsW:
							this.BsW = value;
							return;
					}
					throw new IndexOutOfRangeException();
				}
			}
		}
#endif
		
		public STRANGE nヒット範囲ms;
		[StructLayout( LayoutKind.Sequential )]
		public struct STRANGE
		{
			public int Perfect;
			public int Great;
			public int Good;
			public int Poor;
			public int this[ int index ]
			{
				get
				{
					switch( index )
					{
						case 0:
							return this.Perfect;

						case 1:
							return this.Great;

						case 2:
							return this.Good;

						case 3:
							return this.Poor;
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					switch( index )
					{
						case 0:
							this.Perfect = value;
							return;

						case 1:
							this.Great = value;
							return;

						case 2:
							this.Good = value;
							return;

						case 3:
							this.Poor = value;
							return;
					}
					throw new IndexOutOfRangeException();
				}
			}
		}
		
		
		public STLANEVALUE nVelocityMin;
		[StructLayout( LayoutKind.Sequential )]
		public struct STLANEVALUE
		{
			public int LC;
			public int HH;
			public int SD;
			public int BD;
			public int HT;
			public int LT;
			public int FT;
			public int CY;
			public int RD;
            public int LP;
            public int LBD;
			public int Guitar;
			public int Bass;
			public int this[ int index ]
			{
				get
				{
					switch( index )
					{
						case 0:
							return this.LC;

						case 1:
							return this.HH;

						case 2:
							return this.SD;

						case 3:
							return this.BD;

						case 4:
							return this.HT;

						case 5:
							return this.LT;

						case 6:
							return this.FT;

						case 7:
							return this.CY;

						case 8:
							return this.RD;

						case 9:
							return this.LP;

						case 10:
							return this.LBD;

						case 11:
							return this.Guitar;

						case 12:
							return this.Bass;
					}
					throw new IndexOutOfRangeException();
				}
				set
				{
					switch( index )
					{
						case 0:
							this.LC = value;
							return;

						case 1:
							this.HH = value;
							return;

						case 2:
							this.SD = value;
							return;

						case 3:
							this.BD = value;
							return;

						case 4:
							this.HT = value;
							return;

						case 5:
							this.LT = value;
							return;

						case 6:
							this.FT = value;
							return;

						case 7:
							this.CY = value;
							return;

						case 8:
							this.RD = value;
							return;

						case 9:
							this.LP = value;
							return;

						case 10:
							this.LBD = value;
							return;

						case 11:
							this.Guitar = value;
							return;

						case 12:
							this.Bass = value;
							return;
					}
					throw new IndexOutOfRangeException();
				}
			}
		}
		


        
        //--------------------------
        //ゲーム内のオプションに加えて、
        //システム周りのオプションもこのブロックで記述している。
        
        //--------------------------
        
        
        public bool bDirectShowMode;
        

        //--------------------------
        


        // コンストラクタ

		public CConfigIni()
		{
#if false		// #23625 2011.1.11 Config.iniからダメージ/回復値の定数変更を行う場合はここを有効にする 087リリースに合わせ機能無効化
			//----------------------------------------
			this.fGaugeFactor[0,0] =  0.004f;
			this.fGaugeFactor[0,1] =  0.006f;
			this.fGaugeFactor[1,0] =  0.002f;
			this.fGaugeFactor[1,1] =  0.003f;
			this.fGaugeFactor[2,0] =  0.000f;
			this.fGaugeFactor[2,1] =  0.000f;
			this.fGaugeFactor[3,0] = -0.020f;
			this.fGaugeFactor[3,1] = -0.030f;
			this.fGaugeFactor[4,0] = -0.050f;
			this.fGaugeFactor[4,1] = -0.050f;

			this.fDamageLevelFactor[0] = 0.5f;
			this.fDamageLevelFactor[1] = 1.0f;
			this.fDamageLevelFactor[2] = 1.5f;
			//----------------------------------------
#endif
			this.strDTXManiaのバージョン = "Unknown";
			this.str曲データ検索パス = @".\";
			this.b全画面モード = false;
			this.b垂直帰線待ちを行う = true;
			this.n初期ウィンドウ開始位置X = 0; // #30675 2013.02.04 ikanick add
			this.n初期ウィンドウ開始位置Y = 0;  
			this.nウインドウwidth = SampleFramework.GameWindowSize.Width;			// #23510 2010.10.31 yyagi add
			this.nウインドウheight = SampleFramework.GameWindowSize.Height;			// 
			this.nフレーム毎スリープms = -1;			// #xxxxx 2011.11.27 yyagi add
			this.n非フォーカス時スリープms = 1;			// #23568 2010.11.04 ikanick add
			this._bGuitar有効 = true;
			this._bDrums有効 = true;
			this.nBGAlpha = 100;
			this.bSTAGEFAILED有効 = true;
			this.bAVI有効 = false;
			this.bBGA有効 = true;
			this.n曲が選択されてからプレビュー音が鳴るまでのウェイトms = 1000;
			this.n曲が選択されてからプレビュー画像が表示開始されるまでのウェイトms = 100;
			//this.bWave再生位置自動調整機能有効 = true;
			this.bWave再生位置自動調整機能有効 = false;
			this.bBGM音を発声する = true;
			this.bScoreIniを出力する = true;
			this.bランダムセレクトで子BOXを検索対象とする = true;
            this.FontName = FontUtilities.FallbackFontName;
		    this.ApplyLoudnessMetadata = true;

		    // 2018-08-28 twopointzero:
            // There exists a particular large, well-known, well-curated, and
            // regularly-updated collection of content for use with Taiko no
            // Tatsujin simulators. A statistical analysis was performed on the
            // the integrated loudness and true peak loudness of the thousands
            // of songs within this collection as of late August 2018.
            //
            // The analysis allows us to select a target loudness which
            // results in the smallest total amount of loudness adjustment
            // applied to the songs of that collection. The selected target
            // loudness should result in the least-noticeable average
            // adjustment for the most users, assuming their collection is
            // similar to the exemplar.
            //
            // The target loudness which achieves this is -7.4 LUFS.
		    this.TargetLoudness = -7.4;

		    this.ApplySongVol = false;
		    this.SoundEffectLevel = CSound.DefaultSoundEffectLevel;
		    this.VoiceLevel = CSound.DefaultVoiceLevel;
		    this.SongPreviewLevel = CSound.DefaultSongPreviewLevel;
		    this.SongPlaybackLevel = CSound.DefaultSongPlaybackLevel;
		    this.KeyboardSoundLevelIncrement = DefaultKeyboardSoundLevelIncrement;
			this.bログ出力 = true;
			this.nInputAdjustTimeMs = 0;
			for ( int i = 0; i < 3; i++ )
			{
				//this.e判定表示優先度[ i ] = E判定表示優先度.Chipより下;
			}
			this.n演奏速度 = 20;

            this.b太鼓パートAutoPlay = true;
            this.b太鼓パートAutoPlay2P = true;
            this.bAuto先生の連打 = true;
			
			this.nヒット範囲ms = new STRANGE();
			this.nヒット範囲ms.Perfect = 30;
			this.nヒット範囲ms.Great = -1; //使用しません。
			this.nヒット範囲ms.Good = 100;
			this.nヒット範囲ms.Poor = 130;
			this.ConfigIniファイル名 = "";
			this.dicJoystick = new Dictionary<int, string>( 10 );
			
			this.nVelocityMin.LC = 0;					// #23857 2011.1.31 yyagi VelocityMin
			this.nVelocityMin.HH = 20;
			this.nVelocityMin.SD = 0;
			this.nVelocityMin.BD = 0;
			this.nVelocityMin.HT = 0;
			this.nVelocityMin.LT = 0;
			this.nVelocityMin.FT = 0;
			this.nVelocityMin.CY = 0;
			this.nVelocityMin.RD = 0;
            this.nVelocityMin.LP = 0;
            this.nVelocityMin.LBD = 0;
			
			this.nRisky = 0;							// #23539 2011.7.26 yyagi RISKYモード
			this.bIsAutoResultCapture = false;			// #25399 2011.6.9 yyagi リザルト画像自動保存機能ON/OFF

			this.bバッファ入力を行う = true;
			this.bIsAllowedDoubleClickFullscreen = true;	// #26752 2011.11.26 ダブルクリックでのフルスクリーンモード移行を許可
			this.nPoliphonicSounds = 4;					// #28228 2012.5.1 yyagi レーン毎の最大同時発音数
														// #24820 2013.1.15 yyagi 初期値を4から2に変更。BASS.net使用時の負荷軽減のため。
														// #24820 2013.1.17 yyagi 初期値を4に戻した。動的なミキサー制御がうまく動作しているため。
			this.bIsEnabledSystemMenu = true;			// #28200 2012.5.1 yyagi System Menuの利用可否切替(使用可)
			this.strSystemSkinSubfolderFullName = "";	// #28195 2012.5.2 yyagi 使用中のSkinサブフォルダ名
			this.bTight = false;                        // #29500 2012.9.11 kairera0467 TIGHTモード
			
			this.nSoundDeviceType = FDK.COS.bIsVistaOrLater ?
				(int) ESoundDeviceTypeForConfig.WASAPI : (int) ESoundDeviceTypeForConfig.ACM;	// #24820 2012.12.23 yyagi 初期値はACM | #31927 2013.8.25 yyagi OSにより初期値変更
			this.nWASAPIBufferSizeMs = 50;				// #24820 2013.1.15 yyagi 初期値は50(0で自動設定)
			this.nASIODevice = 0;						// #24820 2013.1.17 yyagi
//			this.nASIOBufferSizeMs = 0;					// #24820 2012.12.25 yyagi 初期値は0(自動設定)
			
			this.bUseOSTimer = false;;					// #33689 2014.6.6 yyagi 初期値はfalse (FDKのタイマー。ＦＲＯＭ氏考案の独自タイマー)
			this.bDynamicBassMixerManagement = true;	//
			this.bTimeStretch = false;					// #23664 2013.2.24 yyagi 初期値はfalse (再生速度変更を、ピッチ変更にて行う)
			this.nDisplayTimesMs = 3000;				// #32072 2013.10.24 yyagi Semi-Invisibleでの、チップ再表示期間
			this.nFadeoutTimeMs = 2000;					// #32072 2013.10.24 yyagi Semi-Invisibleでの、チップフェードアウト時間

            this.bBranchGuide = false;
            this.nScoreMode = 2;
            this.nDefaultCourse = 3;
            this.nBranchAnime = 1;

            this.b大音符判定 = false;
            this.n両手判定の待ち時間 = 50;

            this.bJudgeCountDisplay = false;

            ShowChara = true;
            ShowDancer = true;
            ShowRunner = true;
            ShowFooter = true;
            ShowMob = true;
            ShowPuchiChara = true;

            this.bNoInfo = false;
            
            //this.bNoMP3Streaming = false;
			this.nMasterVolume = 100;					// #33700 2014.4.26 yyagi マスターボリュームの設定(WASAPI/ASIO用)

            this.bHispeedRandom = false;
            this.nDefaultSongSort = 2;
            this.bEndingAnime = false;
            this.nPlayerCount = 1; //2017.08.18 kairera0467 マルチプレイ対応
            ShinuchiMode = false;
            FastRender = true;
            MusicPreTimeMs = 1000; // 一秒
            SendDiscordPlayingInformation = true;
            
            this.bDirectShowMode = false;
            
        }

		// メソッド

		public void t書き出し( string iniファイル名 )
		{
			StreamWriter sw = new StreamWriter( iniファイル名, false, Encoding.GetEncoding( "Shift_JIS" ) );
			sw.WriteLine( ";-------------------" );
			
			
			sw.WriteLine( "[System]" );
			sw.WriteLine();

#if false		// #23625 2011.1.11 Config.iniからダメージ/回復値の定数変更を行う場合はここを有効にする 087リリースに合わせ機能無効化
	//------------------------------
			sw.WriteLine("; ライフゲージのパラメータ調整(調整完了後削除予定)");
			sw.WriteLine("; GaugeFactorD: ドラムのPerfect, Great,... の回復量(ライフMAXを1.0としたときの値を指定)");
			sw.WriteLine("; GaugeFactorG:  Gt/BsのPerfect, Great,... の回復量(ライフMAXを1.0としたときの値を指定)");
			sw.WriteLine("; DamageFactorD: DamageLevelがSmall, Normal, Largeの時に対するダメージ係数");
			sw.WriteLine("GaugeFactorD={0}, {1}, {2}, {3}, {4}", this.fGaugeFactor[0, 0], this.fGaugeFactor[1, 0], this.fGaugeFactor[2, 0], this.fGaugeFactor[3, 0], this.fGaugeFactor[4, 0]);
			sw.WriteLine("GaugeFactorG={0}, {1}, {2}, {3}, {4}", this.fGaugeFactor[0, 1], this.fGaugeFactor[1, 1], this.fGaugeFactor[2, 1], this.fGaugeFactor[3, 1], this.fGaugeFactor[4, 1]);
			sw.WriteLine("DamageFactor={0}, {1}, {2}", this.fDamageLevelFactor[0], this.fDamageLevelFactor[1], fDamageLevelFactor[2]);
			sw.WriteLine();
	//------------------------------
#endif
			
			sw.WriteLine( "; リリースバージョン" );
			sw.WriteLine( "; Release Version." );
			sw.WriteLine( "Version={0}", TJAPlayer3.AppNumericThreePartVersion );
			sw.WriteLine();
			
			
			
			Uri uriRoot = new Uri( System.IO.Path.Combine( TJAPlayer3.strEXEのあるフォルダ, "System" + System.IO.Path.DirectorySeparatorChar ) );
			if ( strSystemSkinSubfolderFullName != null && strSystemSkinSubfolderFullName.Length == 0 )
			{
				strSystemSkinSubfolderFullName = System.IO.Path.Combine( TJAPlayer3.strEXEのあるフォルダ, "System" + System.IO.Path.DirectorySeparatorChar + "Default" + System.IO.Path.DirectorySeparatorChar );
			}
			Uri uriPath = new Uri( System.IO.Path.Combine( this.strSystemSkinSubfolderFullName, "." + System.IO.Path.DirectorySeparatorChar ) );
			string relPath = uriRoot.MakeRelativeUri( uriPath ).ToString();				// 相対パスを取得
			relPath = System.Web.HttpUtility.UrlDecode( relPath );						// デコードする
			relPath = relPath.Replace( '/', System.IO.Path.DirectorySeparatorChar );	// 区切り文字が\ではなく/なので置換する
			
			sw.WriteLine( "; 使用するSkinのフォルダ名。" );
			sw.WriteLine( "; 例えば System\\Default\\Graphics\\... などの場合は、SkinPath=.\\Default\\ を指定します。" );
			sw.WriteLine( "; Skin folder path." );
			sw.WriteLine( "; e.g. System\\Default\\Graphics\\... -> Set SkinPath=.\\Default\\" );
			sw.WriteLine( "SkinPath={0}", relPath );
			sw.WriteLine();
            sw.WriteLine("; 事前画像描画機能を使うかどうか。(0: OFF, 1: ON)");
            sw.WriteLine("; Use pre-textures render.");
            sw.WriteLine("{0}={1}", nameof(FastRender), FastRender ? 1 : 0);
            sw.WriteLine();
            
            
            sw.WriteLine( "; 画面モード(0:ウィンドウ, 1:全画面)" );
			sw.WriteLine( "; Screen mode. (0:Window, 1:Fullscreen)" );
			sw.WriteLine( "FullScreen={0}", this.b全画面モード ? 1 : 0 );
            sw.WriteLine();
			sw.WriteLine("; ウインドウモード時の画面幅");				// #23510 2010.10.31 yyagi add
			sw.WriteLine("; A width size in the window mode.");			//
			sw.WriteLine("WindowWidth={0}", this.nウインドウwidth);		//
			sw.WriteLine();												//
			sw.WriteLine("; ウインドウモード時の画面高さ");				//
			sw.WriteLine("; A height size in the window mode.");		//
			sw.WriteLine("WindowHeight={0}", this.nウインドウheight);	//
			sw.WriteLine();												//
			sw.WriteLine( "; ウィンドウモード時の位置X" );				            // #30675 2013.02.04 ikanick add
			sw.WriteLine( "; X position in the window mode." );			            //
			sw.WriteLine( "WindowX={0}", this.n初期ウィンドウ開始位置X );			//
			sw.WriteLine();											            	//
			sw.WriteLine( "; ウィンドウモード時の位置Y" );			            	//
			sw.WriteLine( "; Y position in the window mode." );	            	    //
			sw.WriteLine( "WindowY={0}", this.n初期ウィンドウ開始位置Y );   		//
			sw.WriteLine();												            //

			sw.WriteLine( "; ウインドウをダブルクリックした時にフルスクリーンに移行するか(0:移行しない,1:移行する)" );	// #26752 2011.11.27 yyagi
			sw.WriteLine( "; Whether double click to go full screen mode or not.(0:No, 1:Yes)" );		//
			sw.WriteLine( "DoubleClickFullScreen={0}", this.bIsAllowedDoubleClickFullscreen? 1 : 0);	//
			sw.WriteLine();																				//
			sw.WriteLine( "; ALT+SPACEのメニュー表示を抑制するかどうか(0:抑制する 1:抑制しない)" );		// #28200 2012.5.1 yyagi
			sw.WriteLine( "; Whether ALT+SPACE menu would be masked or not.(0=masked 1=not masked)" );	//
			sw.WriteLine( "EnableSystemMenu={0}", this.bIsEnabledSystemMenu? 1 : 0 );					//
			sw.WriteLine();																				//
			sw.WriteLine( "; 非フォーカス時のsleep値[ms]" );	    			    // #23568 2011.11.04 ikanick add
			sw.WriteLine( "; A sleep time[ms] while the window is inactive." );	//
			sw.WriteLine( "BackSleep={0}", this.n非フォーカス時スリープms );		// そのまま引用（苦笑）
			sw.WriteLine();                                                             //
            
            
            sw.WriteLine("; フォントレンダリングに使用するフォント名");
            sw.WriteLine("; Font name used for font rendering.");
            sw.WriteLine("FontName={0}", this.FontName);
            sw.WriteLine();
            
            
            sw.WriteLine("; 垂直帰線同期(0:OFF,1:ON)");
			sw.WriteLine( "VSyncWait={0}", this.b垂直帰線待ちを行う ? 1 : 0 );
            sw.WriteLine();
			sw.WriteLine( "; フレーム毎のsleep値[ms] (-1でスリープ無し, 0以上で毎フレームスリープ。動画キャプチャ等で活用下さい)" );	// #xxxxx 2011.11.27 yyagi add
			sw.WriteLine( "; A sleep time[ms] per frame." );							//
			sw.WriteLine( "SleepTimePerFrame={0}", this.nフレーム毎スリープms );		//
			sw.WriteLine();                                                             //
            
            
            
            sw.WriteLine( "; サウンド出力方式(0=ACM(って今はまだDirectSoundですが), 1=ASIO, 2=WASAPI)" );
			sw.WriteLine( "; WASAPIはVista以降のOSで使用可能。推奨方式はWASAPI。" );
			sw.WriteLine( "; なお、WASAPIが使用不可ならASIOを、ASIOが使用不可ならACMを使用します。" );
			sw.WriteLine( "; Sound device type(0=ACM, 1=ASIO, 2=WASAPI)" );
			sw.WriteLine( "; WASAPI can use on Vista or later OSs." );
			sw.WriteLine( "; If WASAPI is not available, DTXMania try to use ASIO. If ASIO can't be used, ACM is used." );
			sw.WriteLine( "SoundDeviceType={0}", (int) this.nSoundDeviceType );
			sw.WriteLine();

			sw.WriteLine( "; WASAPI使用時のサウンドバッファサイズ" );
			sw.WriteLine( "; (0=デバイスに設定されている値を使用, 1～9999=バッファサイズ(単位:ms)の手動指定" );
			sw.WriteLine( "; WASAPI Sound Buffer Size." );
			sw.WriteLine( "; (0=Use system default buffer size, 1-9999=specify the buffer size(ms) by yourself)" );
			sw.WriteLine( "WASAPIBufferSizeMs={0}", (int) this.nWASAPIBufferSizeMs );
			sw.WriteLine();

			sw.WriteLine( "; ASIO使用時のサウンドデバイス" );
			sw.WriteLine( "; 存在しないデバイスを指定すると、DTXManiaが起動しないことがあります。" );
			sw.WriteLine( "; Sound device used by ASIO." );
			sw.WriteLine( "; Don't specify unconnected device, as the DTXMania may not bootup." );
			string[] asiodev = CEnumerateAllAsioDevices.GetAllASIODevices();
			for ( int i = 0; i < asiodev.Length; i++ )
			{
				sw.WriteLine( "; {0}: {1}", i, asiodev[ i ] );
			}
			sw.WriteLine( "ASIODevice={0}", (int) this.nASIODevice );
			sw.WriteLine();

			//sw.WriteLine( "; ASIO使用時のサウンドバッファサイズ" );
			//sw.WriteLine( "; (0=デバイスに設定されている値を使用, 1～9999=バッファサイズ(単位:ms)の手動指定" );
			//sw.WriteLine( "; ASIO Sound Buffer Size." );
			//sw.WriteLine( "; (0=Use the value specified to the device, 1-9999=specify the buffer size(ms) by yourself)" );
			//sw.WriteLine( "ASIOBufferSizeMs={0}", (int) this.nASIOBufferSizeMs );
			//sw.WriteLine();

			//sw.WriteLine( "; Bass.Mixの制御を動的に行うか否か。" );
			//sw.WriteLine( "; ONにすると、ギター曲などチップ音の多い曲も再生できますが、画面が少しがたつきます。" );
			//sw.WriteLine( "; (0=行わない, 1=行う)" );
			//sw.WriteLine( "DynamicBassMixerManagement={0}", this.bDynamicBassMixerManagement ? 1 : 0 );
			//sw.WriteLine();

			sw.WriteLine( "; WASAPI/ASIO時に使用する演奏タイマーの種類" );
			sw.WriteLine( "; Playback timer used for WASAPI/ASIO" );
			sw.WriteLine( "; (0=FDK Timer, 1=System Timer)" );
			sw.WriteLine( "SoundTimerType={0}", this.bUseOSTimer ? 1 : 0 );
			sw.WriteLine();

			//sw.WriteLine( "; 全体ボリュームの設定" );
			//sw.WriteLine( "; (0=無音 ～ 100=最大。WASAPI/ASIO時のみ有効)" );
			//sw.WriteLine( "; Master volume settings" );
			//sw.WriteLine( "; (0=Silent - 100=Max)" );
			//sw.WriteLine( "MasterVolume={0}", this.nMasterVolume );
			//sw.WriteLine();

			

			//sw.WriteLine( "; Waveの再生位置自動補正(0:OFF, 1:ON)" );
			//sw.WriteLine( "AdjustWaves={0}", this.bWave再生位置自動調整機能有効 ? 1 : 0 );
			sw.WriteLine( "; バッファ入力モード(0:OFF, 1:ON)" );
			sw.WriteLine( "; Using Buffered input (0:OFF, 1:ON)" );
			sw.WriteLine( "BufferedInput={0}", this.bバッファ入力を行う ? 1 : 0 );
			sw.WriteLine();
			//sw.WriteLine( "; WASAPI/ASIO使用時に、MP3をストリーム再生するかどうか(0:ストリーム再生する, 1:しない)" );			//
			//sw.WriteLine( "; (mp3のシークがおかしくなる場合は、これを1にしてください) " );	//
			//sw.WriteLine( "; Set \"0\" if you'd like to use mp3 streaming playback on WASAPI/ASIO." );		//
			//sw.WriteLine( "; Set \"1\" not to use streaming playback for mp3." );			//
			//sw.WriteLine( "; (If you feel illegal seek with mp3, please set it to 1.)" );	//
			//sw.WriteLine( "NoMP3Streaming={0}", this.bNoMP3Streaming ? 1 : 0 );				//
			//sw.WriteLine();
            sw.WriteLine( "; 動画再生にDirectShowを使用する(0:OFF, 1:ON)" );
			sw.WriteLine( "; 動画再生にDirectShowを使うことによって、再生時の負担を軽減できます。");
			sw.WriteLine( "; ただし使用時にはセットアップが必要になるのでご注意ください。");
			sw.WriteLine( "DirectShowMode={0}", this.bDirectShowMode ? 1 : 0 );
		    sw.WriteLine();

            
            sw.WriteLine( "[Log]" );
			sw.WriteLine();
			sw.WriteLine( "; Log出力(0:OFF, 1:ON)" );
			sw.WriteLine( "OutputLog={0}", this.bログ出力 ? 1 : 0 );
			sw.WriteLine();
			sw.WriteLine( ";-------------------" );
			

			
			sw.WriteLine( "[GUID]" );
			sw.WriteLine();
			foreach( KeyValuePair<int, string> pair in this.dicJoystick )
			{
				sw.WriteLine( "JoystickID={0},{1}", pair.Key, pair.Value );
			}

			sw.Close();
		}
		public void tファイルから読み込み( string iniファイル名 )
		{
			this.ConfigIniファイル名 = iniファイル名;
			this.bConfigIniが存在している = File.Exists( this.ConfigIniファイル名 );
			if( this.bConfigIniが存在している )
			{
				string str;

				using ( StreamReader reader = new StreamReader( this.ConfigIniファイル名, Encoding.GetEncoding( "Shift_JIS" ) ) )
				{
					str = reader.ReadToEnd();
				}
				t文字列から読み込み( str );

				//if( version.n整数部 <= 69 )
				//{
				//	this.tデフォルトのキーアサインに設定する();
				//}
			}
		}

		private void t文字列から読み込み( string strAllSettings )	// 2011.4.13 yyagi; refactored to make initial KeyConfig easier.
		{
			Eセクション種別 unknown = Eセクション種別.Unknown;
			string[] delimiter = { "\n" };
			string[] strSingleLine = strAllSettings.Split( delimiter, StringSplitOptions.RemoveEmptyEntries );
			foreach ( string s in strSingleLine )
			{
				string str = s.Replace( '\t', ' ' ).TrimStart( new char[] { '\t', ' ' } );
				if ( ( str.Length != 0 ) && ( str[ 0 ] != ';' ) )
				{
					try
					{
						string str3;
						string str4;
						if ( str[ 0 ] == '[' )
						{
							
							//-----------------------------
							StringBuilder builder = new StringBuilder( 0x20 );
							int num = 1;
							while ( ( num < str.Length ) && ( str[ num ] != ']' ) )
							{
								builder.Append( str[ num++ ] );
							}
							string str2 = builder.ToString();
							if ( str2.Equals( "System" ) )
							{
								unknown = Eセクション種別.System;
							}
                            else if ( str2.Equals( "Log" ) )
							{
								unknown = Eセクション種別.Log;
							}
							else if ( str2.Equals( "GUID" ) )
							{
								unknown = Eセクション種別.GUID;
							}
							else if ( str2.Equals( "SystemKeyAssign" ) )
							{
								unknown = Eセクション種別.SystemKeyAssign;
							}
							else if( str2.Equals( "Temp" ) )
							{
								unknown = Eセクション種別.Temp;
							}
							else
							{
								unknown = Eセクション種別.Unknown;
							}
							//-----------------------------
							
						}
						else
						{
							string[] strArray = str.Split( new char[] { '=' } );
							if( strArray.Length == 2 )
							{
								str3 = strArray[ 0 ].Trim();
								str4 = strArray[ 1 ].Trim();
								switch( unknown )
								{
									
									//-----------------------------
									case Eセクション種別.System:
										{
#if false		// #23625 2011.1.11 Config.iniからダメージ/回復値の定数変更を行う場合はここを有効にする 087リリースに合わせ機能無効化
										//----------------------------------------
												if (str3.Equals("GaugeFactorD"))
												{
													int p = 0;
													string[] splittedFactor = str4.Split(',');
													foreach (string s in splittedFactor) {
														this.fGaugeFactor[p++, 0] = Convert.ToSingle(s);
													}
												} else
												if (str3.Equals("GaugeFactorG"))
												{
													int p = 0;
													string[] splittedFactor = str4.Split(',');
													foreach (string s in splittedFactor)
													{
														this.fGaugeFactor[p++, 1] = Convert.ToSingle(s);
													}
												}
												else
												if (str3.Equals("DamageFactor"))
												{
													int p = 0;
													string[] splittedFactor = str4.Split(',');
													foreach (string s in splittedFactor)
													{
														this.fDamageLevelFactor[p++] = Convert.ToSingle(s);
													}
												}
												else
										//----------------------------------------
#endif
											
											if ( str3.Equals( "Version" ) )
											{
												this.strDTXManiaのバージョン = str4;
											}
											

											
											else if ( str3.Equals( "SkinPath" ) )
											{
												string absSkinPath = str4;
												if ( !System.IO.Path.IsPathRooted( str4 ) )
												{
													absSkinPath = System.IO.Path.Combine( TJAPlayer3.strEXEのあるフォルダ, "System" );
													absSkinPath = System.IO.Path.Combine( absSkinPath, str4 );
													Uri u = new Uri( absSkinPath );
													absSkinPath = u.AbsolutePath.ToString();	// str4内に相対パスがある場合に備える
													absSkinPath = System.Web.HttpUtility.UrlDecode( absSkinPath );						// デコードする
													absSkinPath = absSkinPath.Replace( '/', System.IO.Path.DirectorySeparatorChar );	// 区切り文字が\ではなく/なので置換する
												}
												if ( absSkinPath[ absSkinPath.Length - 1 ] != System.IO.Path.DirectorySeparatorChar )	// フォルダ名末尾に\を必ずつけて、CSkin側と表記を統一する
												{
													absSkinPath += System.IO.Path.DirectorySeparatorChar;
												}
												this.strSystemSkinSubfolderFullName = absSkinPath;
											}
                                            else if (str3.Equals(nameof(FastRender)))
                                            {
                                                FastRender = C変換.bONorOFF(str4[0]);
                                            }
                                            
                                            
                                            else if (str3.Equals("FullScreen"))
                                            {
                                                this.b全画面モード = C変換.bONorOFF(str4[0]);
                                            }
                                            else if ( str3.Equals( "WindowX" ) )		// #30675 2013.02.04 ikanick add
											{
												this.n初期ウィンドウ開始位置X = C変換.n値を文字列から取得して範囲内に丸めて返す(
                                                    str4, 0,  System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 1 , this.n初期ウィンドウ開始位置X );
											}
											else if ( str3.Equals( "WindowY" ) )		// #30675 2013.02.04 ikanick add
											{
												this.n初期ウィンドウ開始位置Y = C変換.n値を文字列から取得して範囲内に丸めて返す(
                                                    str4, 0,  System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 1 , this.n初期ウィンドウ開始位置Y );
											}
											else if ( str3.Equals( "WindowWidth" ) )		// #23510 2010.10.31 yyagi add
											{
												this.nウインドウwidth = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 1, 65535, this.nウインドウwidth );
												if( this.nウインドウwidth <= 0 )
												{
													this.nウインドウwidth = SampleFramework.GameWindowSize.Width;
												}
											}
											else if( str3.Equals( "WindowHeight" ) )		// #23510 2010.10.31 yyagi add
											{
												this.nウインドウheight = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 1, 65535, this.nウインドウheight );
												if( this.nウインドウheight <= 0 )
												{
													this.nウインドウheight = SampleFramework.GameWindowSize.Height;
												}
											}
											else if ( str3.Equals( "DoubleClickFullScreen" ) )	// #26752 2011.11.27 yyagi
											{
												this.bIsAllowedDoubleClickFullscreen = C変換.bONorOFF( str4[ 0 ] );
											}
											else if ( str3.Equals( "EnableSystemMenu" ) )		// #28200 2012.5.1 yyagi
											{
												this.bIsEnabledSystemMenu = C変換.bONorOFF( str4[ 0 ] );
											}
											else if ( str3.Equals( "BackSleep" ) )				// #23568 2010.11.04 ikanick add
											{
												this.n非フォーカス時スリープms = C変換.n値を文字列から取得して範囲内にちゃんと丸めて返す( str4, 0, 50, this.n非フォーカス時スリープms );
											}
                                            

                                            
                                            else if ( str3.Equals( "SoundDeviceType" ) )
											{
												this.nSoundDeviceType = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 2, this.nSoundDeviceType );
											}
											else if ( str3.Equals( "WASAPIBufferSizeMs" ) )
											{
											    this.nWASAPIBufferSizeMs = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 9999, this.nWASAPIBufferSizeMs );
											}
											else if ( str3.Equals( "ASIODevice" ) )
											{
												string[] asiodev = CEnumerateAllAsioDevices.GetAllASIODevices();
												this.nASIODevice = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, asiodev.Length - 1, this.nASIODevice );
											}
											//else if ( str3.Equals( "ASIOBufferSizeMs" ) )
											//{
											//    this.nASIOBufferSizeMs = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 9999, this.nASIOBufferSizeMs );
											//}
											//else if ( str3.Equals( "DynamicBassMixerManagement" ) )
											//{
											//    this.bDynamicBassMixerManagement = C変換.bONorOFF( str4[ 0 ] );
											//}
											else if ( str3.Equals( "SoundTimerType" ) )			// #33689 2014.6.6 yyagi
											{
												this.bUseOSTimer = C変換.bONorOFF( str4[ 0 ] );
											}
                                            //else if ( str3.Equals( "MasterVolume" ) )
                                            //{
                                            //    this.nMasterVolume = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 100, this.nMasterVolume );
                                            //}
                                            

                                            
                                            else if (str3.Equals("FontName"))
                                            {
                                                this.FontName = str4;
                                            }
                                            

                                            else if ( str3.Equals( "VSyncWait" ) )
											{
												this.b垂直帰線待ちを行う = C変換.bONorOFF( str4[ 0 ] );
											}
											else if( str3.Equals( "SleepTimePerFrame" ) )		// #23568 2011.11.27 yyagi
											{
												this.nフレーム毎スリープms = C変換.n値を文字列から取得して範囲内にちゃんと丸めて返す( str4, -1, 50, this.nフレーム毎スリープms );
											}
											else if( str3.Equals( "BufferedInput" ) )
											{
												this.bバッファ入力を行う = C変換.bONorOFF( str4[ 0 ] );
											}
											
											else if ( str3.Equals( "LCVelocityMin" ) )			// #23857 2010.12.12 yyagi
											{
												this.nVelocityMin.LC = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.LC );
											}
											else if( str3.Equals( "HHVelocityMin" ) )
											{
												this.nVelocityMin.HH = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.HH );
											}
											else if( str3.Equals( "SDVelocityMin" ) )			// #23857 2011.1.31 yyagi
											{
												this.nVelocityMin.SD = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.SD );
											}
											else if( str3.Equals( "BDVelocityMin" ) )			// #23857 2011.1.31 yyagi
											{
												this.nVelocityMin.BD = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.BD );
											}
											else if( str3.Equals( "HTVelocityMin" ) )			// #23857 2011.1.31 yyagi
											{
												this.nVelocityMin.HT = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.HT );
											}
											else if( str3.Equals( "LTVelocityMin" ) )			// #23857 2011.1.31 yyagi
											{
												this.nVelocityMin.LT = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.LT );
											}
											else if( str3.Equals( "FTVelocityMin" ) )			// #23857 2011.1.31 yyagi
											{
												this.nVelocityMin.FT = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.FT );
											}
											else if( str3.Equals( "CYVelocityMin" ) )			// #23857 2011.1.31 yyagi
											{
												this.nVelocityMin.CY = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.CY );
											}
											else if( str3.Equals( "RDVelocityMin" ) )			// #23857 2011.1.31 yyagi
											{
												this.nVelocityMin.RD = C変換.n値を文字列から取得して範囲内に丸めて返す( str4, 0, 127, this.nVelocityMin.RD );
											}
											
                                            
											else if ( str3.Equals( "DirectShowMode" ) )		// #28228 2012.5.1 yyagi
											{
                                                this.bDirectShowMode = C変換.bONorOFF( str4[ 0 ] ); ;
											}
                                            

                                            continue;
										}
                                    //-----------------------------
                                    

                                    
                                    //-----------------------------
                                    case Eセクション種別.Log:
										{
											if( str3.Equals( "OutputLog" ) )
											{
												this.bログ出力 = C変換.bONorOFF( str4[ 0 ] );
											}
											else if( str3.Equals( "TraceCreatedDisposed" ) )
											{
												this.bLog作成解放ログ出力 = C変換.bONorOFF( str4[ 0 ] );
											}
											continue;
										}
									//-----------------------------
									

									
									//-----------------------------
									case Eセクション種別.GUID:
										if( str3.Equals( "JoystickID" ) )
										{
											this.tJoystickIDの取得( str4 );
										}
										continue;
									//-----------------------------
									
								}
							}
						}
						continue;
					}
					catch ( Exception exception )
					{
						Trace.TraceError( exception.ToString() );
						Trace.TraceError( "例外が発生しましたが処理を継続します。 (93c4c5cd-4996-4e8c-a82f-a179ff590b44)" );
						continue;
					}
				}
			}
		}

		/// <summary>
		/// ギターとベースのキーアサイン入れ替え
		/// </summary>
		//public void SwapGuitarBassKeyAssign()		// #24063 2011.1.16 yyagi
		//{
		//    for ( int j = 0; j <= (int)EKeyConfigPad.Capture; j++ )
		//    {
		//        CKeyAssign.STKEYASSIGN t; //= new CConfigIni.CKeyAssign.STKEYASSIGN();
		//        for ( int k = 0; k < 16; k++ )
		//        {
		//            t = this.KeyAssign[ (int)EKeyConfigPart.GUITAR ][ j ][ k ];
		//            this.KeyAssign[ (int)EKeyConfigPart.GUITAR ][ j ][ k ] = this.KeyAssign[ (int)EKeyConfigPart.BASS ][ j ][ k ];
		//            this.KeyAssign[ (int)EKeyConfigPart.BASS ][ j ][ k ] = t;
		//        }
		//    }
		//    this.bIsSwappedGuitarBass = !bIsSwappedGuitarBass;
		//}


		// その他

		
		//-----------------
		private enum Eセクション種別
		{
			Unknown,
			System,
			Log,
			GUID,
			SystemKeyAssign,
			Temp,
		}

		private bool _bDrums有効;
		private bool _bGuitar有効;
		private bool bConfigIniが存在している;
		private string ConfigIniファイル名;

		private void tJoystickIDの取得( string strキー記述 )
		{
			string[] strArray = strキー記述.Split( new char[] { ',' } );
			if( strArray.Length >= 2 )
			{
				int result = 0;
				if( ( int.TryParse( strArray[ 0 ], out result ) && ( result >= 0 ) ) && ( result <= 9 ) )
				{
					if( this.dicJoystick.ContainsKey( result ) )
					{
						this.dicJoystick.Remove( result );
					}
					this.dicJoystick.Add( result, strArray[ 1 ] );
				}
			}
		}
		


		public event PropertyChangedEventHandler PropertyChanged;

	    private bool SetProperty<T>(ref T storage, T value, string propertyName = null)
	    {
	        if (Equals(storage, value))
	        {
	            return false;
	        }

	        storage = value;
	        OnPropertyChanged(propertyName);
	        return true;
	    }

	    private void OnPropertyChanged(string propertyName)
	    {
	        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	    }
	}
}
