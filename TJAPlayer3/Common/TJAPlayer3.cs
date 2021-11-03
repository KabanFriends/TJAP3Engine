using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SlimDX;
using SlimDX.Direct3D9;
using FDK;
using SampleFramework;
using System.Reflection;

namespace TJAPlayer3
{
	internal class TJAPlayer3 : Game
	{
        public const string SLIMDXDLL = "c_net20x86_Jun2010";
        public const string D3DXDLL = "d3dx9_43.dll";   // June 2010
        //public const string D3DXDLL = "d3dx9_42.dll"; // February 2010
        //public const string D3DXDLL = "d3dx9_41.dll"; // March 2009

        // プロパティ
        #region [ properties ]

        public static readonly string AppDisplayName = Assembly.GetExecutingAssembly().GetName().Name;

        public static readonly string AppDisplayThreePartVersion = GetAppDisplayThreePartVersion();
        public static readonly string AppNumericThreePartVersion = GetAppNumericThreePartVersion();

        private static string GetAppDisplayThreePartVersion()
        {
            return $"v{GetAppNumericThreePartVersion()}";
        }

        private static string GetAppNumericThreePartVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        public static readonly string AppInformationalVersion =
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .Cast<AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()
                ?.InformationalVersion
            ?? $"{GetAppDisplayThreePartVersion()} (unknown informational version)";

        public static readonly string AppDisplayNameWithThreePartVersion = $"{AppDisplayName} {AppDisplayThreePartVersion}";
        public static readonly string AppDisplayNameWithInformationalVersion = $"{AppDisplayName} {AppInformationalVersion}";

        public static TJAPlayer3 app
		{
			get;
			private set;
		}
		public static C文字コンソール act文字コンソール
		{ 
			get;
			private set;
		}
		public static CConfigIni ConfigIni
		{
			get; 
			private set;
		}

	    public static bool IsPerformingCalibration;

		public static CFPS FPS
		{ 
			get; 
			private set;
		}
		public static CInput管理 Input管理 
		{
			get;
			private set;
		}
		public static Random Random
		{
			get;
			private set;
		}
		public static CSkin Skin
		{
			get; 
			private set;
		}

		public static CActFlushGPU actFlushGPU
		{
			get;
			private set;
		}

		public static CSound管理 Sound管理
		{
			get;
			private set;
		}

	    public static SongGainController SongGainController
	    {
	        get;
	        private set;
	    }

	    public static SoundGroupLevelController SoundGroupLevelController
	    {
	        get;
	        private set;
	    }

		public static CStage起動 stage起動 
		{
			get; 
			private set;
		}

		public static CStageサンプル stageサンプル
        {
			get;
			private set;
        }

		public static CStage r現在のステージ = null;
		public static CStage r直前のステージ = null;
		public static string strEXEのあるフォルダ 
		{
			get;
			private set;
		}
		public static CTimer Timer
		{
			get;
			private set;
		}
		public static Format TextureFormat = Format.A8R8G8B8;
		public bool bApplicationActive
		{
			get; 
			private set;
		}
		public bool b次のタイミングで垂直帰線同期切り替えを行う
		{
			get; 
			set;
		}
		public bool b次のタイミングで全画面_ウィンドウ切り替えを行う
		{
			get;
			set;
		}

		public DeviceCache Device => GraphicsDeviceManager.Device;

        private static Size currentClientSize		// #23510 2010.10.27 add yyagi to keep current window size
		{
			get;
			set;
		}
		//		public static CTimer ct;
		public IntPtr WindowHandle					// 2012.10.24 yyagi; to add ASIO support
		{
			get { return base.Window.Handle; }
		}

        #endregion

        // コンストラクタ

        public TJAPlayer3()
		{
			TJAPlayer3.app = this;
			this.t起動処理();
		}


		// メソッド

		public void t全画面_ウィンドウモード切り替え()
		{
#if WindowedFullscreen
			if ( ConfigIni != null )
#else
			DeviceSettings settings = base.GraphicsDeviceManager.CurrentSettings.Clone();
			if ( ( ConfigIni != null ) && ( ConfigIni.bウィンドウモード != settings.Windowed ) )
#endif
			{
#if !WindowedFullscreen
				settings.Windowed = ConfigIni.bウィンドウモード;
#endif
				if ( ConfigIni.bウィンドウモード == false )	// #23510 2010.10.27 yyagi: backup current window size before going fullscreen mode
				{
					currentClientSize = this.Window.ClientSize;
					ConfigIni.nウインドウwidth = this.Window.ClientSize.Width;
					ConfigIni.nウインドウheight = this.Window.ClientSize.Height;
//					FDK.CTaskBar.ShowTaskBar( false );
				}
#if !WindowedFullscreen
				base.GraphicsDeviceManager.ChangeDevice( settings );
#endif
				if ( ConfigIni.bウィンドウモード == true )	// #23510 2010.10.27 yyagi: to resume window size from backuped value
				{
#if WindowedFullscreen
															// #30666 2013.2.2 yyagi Don't use Fullscreen mode becasue NVIDIA GeForce is
															// tend to delay drawing on Fullscreen mode. So DTXMania uses Maximized window
															// in spite of using fullscreen mode.
					app.Window.WindowState = FormWindowState.Normal;
					app.Window.FormBorderStyle = FormBorderStyle.Sizable;
					app.Window.WindowState = FormWindowState.Normal;
#endif
					base.Window.ClientSize =
						new Size( currentClientSize.Width, currentClientSize.Height );
                    base.Window.Icon = Properties.Resources.tjap3;
//					FDK.CTaskBar.ShowTaskBar( true );
				}
#if WindowedFullscreen
				else 
				{
					app.Window.WindowState = FormWindowState.Normal;
					app.Window.FormBorderStyle = FormBorderStyle.None;
					app.Window.WindowState = FormWindowState.Maximized;
				}
				if ( ConfigIni.bウィンドウモード )
				{
					if ( !this.bマウスカーソル表示中 )
					{
						Cursor.Show();
						this.bマウスカーソル表示中 = true;
					}
				}
				else if ( this.bマウスカーソル表示中 )
				{
					Cursor.Hide();
					this.bマウスカーソル表示中 = false;
				}
#endif
			}
		}

		// Game 実装

		protected override void Initialize()
		{
//			new GCBeep();
			//sw.Start();
			//swlist1 = new List<int>( 8192 );
			//swlist2 = new List<int>( 8192 );
			//swlist3 = new List<int>( 8192 );
			//swlist4 = new List<int>( 8192 );
			//swlist5 = new List<int>( 8192 );
			if ( this.listトップレベルActivities != null )
			{
				foreach( CActivity activity in this.listトップレベルActivities )
					activity.OnManagedリソースの作成();
			}

#if GPUFlushAfterPresent
			FrameEnd += dtxmania_FrameEnd;
#endif
		}
#if GPUFlushAfterPresent
		void dtxmania_FrameEnd( object sender, EventArgs e )	// GraphicsDeviceManager.game_FrameEnd()後に実行される
		{														// → Present()直後にGPUをFlushする
																// → 画面のカクツキが頻発したため、ここでのFlushは行わない
			actFlushGPU.On進行描画();		// Flush GPU
		}
#endif
		protected override void LoadContent()
		{
			if ( ConfigIni.bウィンドウモード )
			{
				if( !this.bマウスカーソル表示中 )
				{
					Cursor.Show();
					this.bマウスカーソル表示中 = true;
				}
			}
			else if( this.bマウスカーソル表示中 )
			{
				Cursor.Hide();
				this.bマウスカーソル表示中 = false;
			}
			this.Device.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0f, 0f, (float)(-SampleFramework.GameWindowSize.Height / 2 * Math.Sqrt(3.0))), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f)));
			this.Device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH(C変換.DegreeToRadian((float)60f), ((float)this.Device.Viewport.Width) / ((float)this.Device.Viewport.Height), -100f, 100f));
			this.Device.SetRenderState(RenderState.Lighting, false);
			this.Device.SetRenderState( RenderState.ZEnable, false );
			this.Device.SetRenderState( RenderState.AntialiasedLineEnable, false );
			this.Device.SetRenderState( RenderState.AlphaTestEnable, true );
			this.Device.SetRenderState( RenderState.AlphaRef, 10 );

			this.Device.SetRenderState( RenderState.MultisampleAntialias, true );
			this.Device.SetSamplerState( 0, SamplerState.MinFilter, TextureFilter.Linear );
			this.Device.SetSamplerState( 0, SamplerState.MagFilter, TextureFilter.Linear );

			this.Device.SetRenderState<Compare>( RenderState.AlphaFunc, Compare.Greater );
			this.Device.SetRenderState( RenderState.AlphaBlendEnable, true );
			this.Device.SetRenderState<Blend>( RenderState.SourceBlend, Blend.SourceAlpha );
			this.Device.SetRenderState<Blend>( RenderState.DestinationBlend, Blend.InverseSourceAlpha );
			this.Device.SetTextureStageState( 0, TextureStage.AlphaOperation, TextureOperation.Modulate );
			this.Device.SetTextureStageState( 0, TextureStage.AlphaArg1, 2 );
			this.Device.SetTextureStageState( 0, TextureStage.AlphaArg2, 1 );

			if( this.listトップレベルActivities != null )
			{
				foreach( CActivity activity in this.listトップレベルActivities )
					activity.OnUnmanagedリソースの作成();
			}
		}
		protected override void UnloadContent()
		{
			if( this.listトップレベルActivities != null )
			{
				foreach( CActivity activity in this.listトップレベルActivities )
					activity.OnUnmanagedリソースの解放();
			}
		}
		protected override void OnExiting( EventArgs e )
		{
			CPowerManagement.tEnableMonitorSuspend();		// スリープ抑止状態を解除
			this.t終了処理();
			base.OnExiting( e );
		}
		protected override void Update( GameTime gameTime )
		{
		}
		protected override void Draw( GameTime gameTime )
		{
            Timer?.t更新();
            CSound管理.rc演奏用タイマ?.t更新();
            Input管理?.tポーリング( this.bApplicationActive, TJAPlayer3.ConfigIni.bバッファ入力を行う );
            FPS?.tカウンタ更新();

			if( this.Device == null )
				return;

			if ( this.bApplicationActive )	// DTXMania本体起動中の本体/モニタの省電力モード移行を抑止
				CPowerManagement.tDisableMonitorSuspend();

			// #xxxxx 2013.4.8 yyagi; sleepの挿入位置を、EndScnene～Present間から、BeginScene前に移動。描画遅延を小さくするため。
			#region [ スリープ ]
			if ( ConfigIni.nフレーム毎スリープms >= 0 )			// #xxxxx 2011.11.27 yyagi
			{
				Thread.Sleep( ConfigIni.nフレーム毎スリープms );
			}
			#endregion

			this.Device.BeginScene();
			this.Device.Clear( ClearFlags.ZBuffer | ClearFlags.Target, Color.Black, 1f, 0 );

			if( r現在のステージ != null )
			{
				this.n進行描画の戻り値 = ( r現在のステージ != null ) ? r現在のステージ.On進行描画() : 0;

				switch ( r現在のステージ.eステージID )
				{
					case CStage.Eステージ.何もしない:
						break;

					case CStage.Eステージ.起動:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							r現在のステージ.On非活性化();
							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ サンプル");
							stageサンプル.On活性化();
							r直前のステージ = r現在のステージ;
							r現在のステージ = stageサンプル;

							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;
				}

                // オーバレイを描画する(テクスチャの生成されていない起動ステージは例外
                if(r現在のステージ != null && r現在のステージ.eステージID != CStage.Eステージ.起動 && TJAPlayer3.Tx.Overlay != null)
                {
                    TJAPlayer3.Tx.Overlay.t2D描画(app.Device, 0, 0);
                }
			}
			this.Device.EndScene();			// Present()は game.csのOnFrameEnd()に登録された、GraphicsDeviceManager.game_FrameEnd() 内で実行されるので不要
											// (つまり、Present()は、Draw()完了後に実行される)
#if !GPUFlushAfterPresent
			actFlushGPU?.On進行描画();		// Flush GPU	// EndScene()～Present()間 (つまりVSync前) でFlush実行
#endif

			#region [ 全画面_ウインドウ切り替え ]
			if ( this.b次のタイミングで全画面_ウィンドウ切り替えを行う )
			{
				ConfigIni.b全画面モード = !ConfigIni.b全画面モード;
				app.t全画面_ウィンドウモード切り替え();
				this.b次のタイミングで全画面_ウィンドウ切り替えを行う = false;
			}
			#endregion
			#region [ 垂直基線同期切り替え ]
			if ( this.b次のタイミングで垂直帰線同期切り替えを行う )
			{
				bool bIsMaximized = this.Window.IsMaximized;											// #23510 2010.11.3 yyagi: to backup current window mode before changing VSyncWait
				currentClientSize = this.Window.ClientSize;												// #23510 2010.11.3 yyagi: to backup current window size before changing VSyncWait
				DeviceSettings currentSettings = app.GraphicsDeviceManager.CurrentSettings;
				currentSettings.EnableVSync = ConfigIni.b垂直帰線待ちを行う;
				app.GraphicsDeviceManager.ChangeDevice( currentSettings );
				this.b次のタイミングで垂直帰線同期切り替えを行う = false;
				base.Window.ClientSize = new Size(currentClientSize.Width, currentClientSize.Height);	// #23510 2010.11.3 yyagi: to resume window size after changing VSyncWait
				if (bIsMaximized)
				{
					this.Window.WindowState = FormWindowState.Maximized;								// #23510 2010.11.3 yyagi: to resume window mode after changing VSyncWait
				}
			}
			#endregion
		}

		// その他

		#region [ 汎用ヘルパー ]
		//-----------------
		public static CTexture tテクスチャの生成( string fileName )
		{
			return tテクスチャの生成( fileName, false );
		}
		public static CTexture tテクスチャの生成( string fileName, bool b黒を透過する )
		{
			if ( app == null )
			{
				return null;
			}
			try
			{
				return new CTexture( app.Device, fileName, TextureFormat, b黒を透過する );
			}
			catch ( CTextureCreateFailedException e )
			{
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "テクスチャの生成に失敗しました。({0})", fileName );
				return null;
			}
			catch ( FileNotFoundException )
			{
				Trace.TraceWarning( "テクスチャファイルが見つかりませんでした。({0})", fileName );
				return null;
			}
		}

		public static CTexture tテクスチャの生成( Bitmap bitmap )
		{
			return tテクスチャの生成( bitmap, false );
		}
		public static CTexture tテクスチャの生成( Bitmap bitmap, bool b黒を透過する )
		{
			if ( app == null )
			{
				return null;
			}
            if (bitmap == null)
            {
                Trace.TraceError("テクスチャの生成に失敗しました。(bitmap==null)");
                return null;
            }
            try
			{
				return new CTexture( app.Device, bitmap, TextureFormat, b黒を透過する );
			}
			catch ( CTextureCreateFailedException e )
			{
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "テクスチャの生成に失敗しました。(txData)" );
				return null;
			}
		}

        public static CTextureAf tテクスチャの生成Af( string fileName )
		{
			return tテクスチャの生成Af( fileName, false );
		}

        private static CTextureAf tテクスチャの生成Af( string fileName, bool b黒を透過する )
		{
			if ( app == null )
			{
				return null;
			}
			try
			{
				return new CTextureAf( app.Device, fileName, TextureFormat, b黒を透過する );
			}
			catch ( CTextureCreateFailedException e )
			{
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "テクスチャの生成に失敗しました。({0})", fileName );
				return null;
			}
			catch ( FileNotFoundException e )
			{
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "テクスチャファイルが見つかりませんでした。({0})", fileName );
				return null;
			}
		}

        /// <summary>プロパティ、インデクサには ref は使用できないので注意。</summary>
        public static void t安全にDisposeする<T>(ref T obj) where T : class, IDisposable
        {
            if (obj == null)
            {
                return;
            }

            obj.Dispose();
            obj = null;
        }

        public static void t安全にDisposeする<T>(T[] array) where T : class, IDisposable
        {
            if (array == null)
            {
                return;
            }

            for (var i = 0; i < array.Length; i++)
            {
                array[i]?.Dispose();
                array[i] = null;
            }
        }

        /// <summary>
        /// そのフォルダの連番画像の最大値を返す。
        /// </summary>
        public static int t連番画像の枚数を数える(string ディレクトリ名, string プレフィックス = "", string 拡張子 = ".png")
        {
            int num = 0;
            while(File.Exists(ディレクトリ名 + プレフィックス + num + 拡張子))
            {
                num++;
            }
            return num;
        }

        /// <summary>
        /// 曲名テクスチャの縮小倍率を返す。
        /// </summary>
        /// <param name="cTexture">曲名テクスチャ。</param>
        /// <param name="samePixel">等倍で表示するピクセル数の最大値(デフォルト値:645)</param>
        /// <returns>曲名テクスチャの縮小倍率。そのテクスチャがnullならば一倍(1f)を返す。</returns>
        public static float GetSongNameXScaling(ref CTexture cTexture, int samePixel = 660)
        {
            if (cTexture == null) return 1f;
            float scalingRate = (float)samePixel / (float)cTexture.szテクスチャサイズ.Width;
            if (cTexture.szテクスチャサイズ.Width <= samePixel)
                scalingRate = 1.0f;
            return scalingRate;
        }

        //-----------------
        #endregion

        #region [ private ]
        //-----------------
        private bool bマウスカーソル表示中 = true;
		private bool b終了処理完了済み;

        public static TextureLoader Tx = new TextureLoader();

		private List<CActivity> listトップレベルActivities;
		private int n進行描画の戻り値;
		private MouseButtons mb = System.Windows.Forms.MouseButtons.Left;
		private CSound previewSound;
        public static long StartupTime
        {
            get;
            private set;
        }

        private void t起動処理()
		{
			#region [ strEXEのあるフォルダを決定する ]
			//-----------------
// BEGIN #23629 2010.11.13 from: デバッグ時は Application.ExecutablePath が ($SolutionDir)/bin/x86/Debug/ などになり System/ の読み込みに失敗するので、カレントディレクトリを採用する。（プロジェクトのプロパティ→デバッグ→作業ディレクトリが有効になる）
#if DEBUG
			strEXEのあるフォルダ = Environment.CurrentDirectory + @"\";
#else
			strEXEのあるフォルダ = Path.GetDirectoryName( Application.ExecutablePath ) + @"\";	// #23629 2010.11.9 yyagi: set correct pathname where DTXManiaGR.exe is.
#endif
// END #23629 2010.11.13 from
			//-----------------
			#endregion

			#region [ Config.ini の読込み ]
			//---------------------
			ConfigIni = new CConfigIni();
			string path = strEXEのあるフォルダ + "Config.ini";
			if( File.Exists( path ) )
			{
				try
				{
					ConfigIni.tファイルから読み込み( path );
				}
				catch (Exception e)
				{
					//ConfigIni = new CConfigIni();	// 存在してなければ新規生成
					Trace.TraceError( e.ToString() );
					Trace.TraceError( "例外が発生しましたが処理を継続します。 (b8d93255-bbe4-4ca3-8264-7ee5175b19f3)" );
				}
			}
			this.Window.EnableSystemMenu = TJAPlayer3.ConfigIni.bIsEnabledSystemMenu;	// #28200 2011.5.1 yyagi
			// 2012.8.22 Config.iniが無いときに初期値が適用されるよう、この設定行をifブロック外に移動

			//---------------------
			#endregion
			#region [ ログ出力開始 ]
			//---------------------
			Trace.AutoFlush = true;
			if( ConfigIni.bログ出力 )
			{
				try
				{
					Trace.Listeners.Add( new CTraceLogListener( new StreamWriter( System.IO.Path.Combine( strEXEのあるフォルダ, "TJAPlayer3.log" ), false, Encoding.GetEncoding( "Shift_JIS" ) ) ) );
				}
				catch ( System.UnauthorizedAccessException )			// #24481 2011.2.20 yyagi
				{
					int c = (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja")? 0 : 1;
					string[] mes_writeErr = {
						"DTXManiaLog.txtへの書き込みができませんでした。書き込みできるようにしてから、再度起動してください。",
						"Failed to write DTXManiaLog.txt. Please set it writable and try again."
					};
					MessageBox.Show( mes_writeErr[c], "DTXMania boot error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					Environment.Exit(1);
				}
			}
			Trace.WriteLine("");
			Trace.WriteLine(AppDisplayNameWithInformationalVersion);
			Trace.WriteLine( "" );
			Trace.TraceInformation( "----------------------" );
			Trace.TraceInformation( "■ アプリケーションの初期化" );
			Trace.TraceInformation( "OS Version: " + Environment.OSVersion );
			Trace.TraceInformation( "ProcessorCount: " + Environment.ProcessorCount.ToString() );
			Trace.TraceInformation( "CLR Version: " + Environment.Version.ToString() );
			//---------------------
			#endregion

			#region [ ウィンドウ初期化 ]
			//---------------------
			base.Window.StartPosition = FormStartPosition.Manual;                                                       // #30675 2013.02.04 ikanick add
			base.Window.Location = new Point( ConfigIni.n初期ウィンドウ開始位置X, ConfigIni.n初期ウィンドウ開始位置Y );   // #30675 2013.02.04 ikanick add

            
            base.Window.Text = "";

			base.Window.StartPosition = FormStartPosition.Manual;                                                       // #30675 2013.02.04 ikanick add
            base.Window.Location = new Point(ConfigIni.n初期ウィンドウ開始位置X, ConfigIni.n初期ウィンドウ開始位置Y);   // #30675 2013.02.04 ikanick add

			base.Window.ClientSize = new Size(ConfigIni.nウインドウwidth, ConfigIni.nウインドウheight);	// #34510 yyagi 2010.10.31 to change window size got from Config.ini
#if !WindowedFullscreen
			if (!ConfigIni.bウィンドウモード)						// #23510 2010.11.02 yyagi: add; to recover window size in case bootup with fullscreen mode
			{														// #30666 2013.02.02 yyagi: currentClientSize should be always made
#endif
				currentClientSize = new Size( ConfigIni.nウインドウwidth, ConfigIni.nウインドウheight );
#if !WindowedFullscreen
			}
#endif
			base.Window.MaximizeBox = true;							// #23510 2010.11.04 yyagi: to support maximizing window
			base.Window.FormBorderStyle = FormBorderStyle.Sizable;	// #23510 2010.10.27 yyagi: changed from FixedDialog to Sizable, to support window resize
																		// #30666 2013.02.02 yyagi: moved the code to t全画面_ウインドウモード切り替え()
			base.Window.ShowIcon = true;
			base.Window.Icon = global::TJAPlayer3.Properties.Resources.tjap3;
			base.Window.KeyDown += new KeyEventHandler( this.Window_KeyDown );
			base.Window.MouseUp +=new MouseEventHandler( this.Window_MouseUp);
			base.Window.MouseDoubleClick += new MouseEventHandler(this.Window_MouseDoubleClick);	// #23510 2010.11.13 yyagi: to go fullscreen mode
			base.Window.ResizeEnd += new EventHandler(this.Window_ResizeEnd);						// #23510 2010.11.20 yyagi: to set resized window size in Config.ini
			base.Window.ApplicationActivated += new EventHandler(this.Window_ApplicationActivated);
			base.Window.ApplicationDeactivated += new EventHandler( this.Window_ApplicationDeactivated );
			//---------------------
			#endregion
			#region [ Direct3D9Exを使うかどうか判定 ]
			#endregion
			#region [ Direct3D9 デバイスの生成 ]
			//---------------------
			DeviceSettings settings = new DeviceSettings();
#if WindowedFullscreen
			settings.Windowed = true;								// #30666 2013.2.2 yyagi: Fullscreenmode is "Maximized window" mode
#else
			settings.Windowed = ConfigIni.bウィンドウモード;
#endif
			settings.BackBufferWidth = SampleFramework.GameWindowSize.Width;
			settings.BackBufferHeight = SampleFramework.GameWindowSize.Height;
//			settings.BackBufferCount = 3;
			settings.EnableVSync = ConfigIni.b垂直帰線待ちを行う;
//			settings.BackBufferFormat = Format.A8R8G8B8;
//			settings.MultisampleType = MultisampleType.FourSamples;
//			settings.MultisampleQuality = 4;
//			settings.MultisampleType = MultisampleType.None;
//			settings.MultisampleQuality = 0;
			
			try
			{
				base.GraphicsDeviceManager.ChangeDevice(settings);
			}
			catch (DeviceCreationException e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show(e.ToString(), "DTXMania failed to boot: DirectX9 Initialize Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(-1);
			}
			
			base.IsFixedTimeStep = false;
//			base.TargetElapsedTime = TimeSpan.FromTicks( 10000000 / 75 );
			base.Window.ClientSize = new Size(ConfigIni.nウインドウwidth, ConfigIni.nウインドウheight);	// #23510 2010.10.31 yyagi: to recover window size. width and height are able to get from Config.ini.
			base.InactiveSleepTime = TimeSpan.FromMilliseconds((float)(ConfigIni.n非フォーカス時スリープms));	// #23568 2010.11.3 yyagi: to support valiable sleep value when !IsActive
																												// #23568 2010.11.4 ikanick changed ( 1 -> ConfigIni )
#if WindowedFullscreen
			this.t全画面_ウィンドウモード切り替え();				// #30666 2013.2.2 yyagi: finalize settings for "Maximized window mode"
#endif
			actFlushGPU = new CActFlushGPU();
			//---------------------
			#endregion

			#region [ Skin の初期化 ]
			//---------------------
			Trace.TraceInformation( "スキンの初期化を行います。" );
			Trace.Indent();
			try
			{
				Skin = new CSkin( TJAPlayer3.ConfigIni.strSystemSkinSubfolderFullName, false);
				TJAPlayer3.ConfigIni.strSystemSkinSubfolderFullName = TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName( true );	// 旧指定のSkinフォルダが消滅していた場合に備える
				Trace.TraceInformation( "スキンの初期化を完了しました。" );
			}
			catch (Exception e)
			{
				Trace.TraceInformation( "スキンの初期化に失敗しました。" );
				throw;
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
			#endregion
			//-----------
			#region [ Timer の初期化 ]
			//---------------------
			Trace.TraceInformation( "タイマの初期化を行います。" );
			Trace.Indent();
			try
			{
				Timer = new CTimer( CTimer.E種別.MultiMedia );
				Trace.TraceInformation( "タイマの初期化を完了しました。" );
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
			#endregion
			//-----------

			#region [ FPS カウンタの初期化 ]
			//---------------------
			Trace.TraceInformation( "FPSカウンタの初期化を行います。" );
			Trace.Indent();
			try
			{
				FPS = new CFPS();
				Trace.TraceInformation( "FPSカウンタを生成しました。" );
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
			#endregion
			#region [ act文字コンソールの初期化 ]
			//---------------------
			Trace.TraceInformation( "文字コンソールの初期化を行います。" );
			Trace.Indent();
			try
			{
				act文字コンソール = new C文字コンソール();
				Trace.TraceInformation( "文字コンソールを生成しました。" );
				act文字コンソール.On活性化();
				Trace.TraceInformation( "文字コンソールを活性化しました。" );
				Trace.TraceInformation( "文字コンソールの初期化を完了しました。" );
			}
			catch( Exception exception )
			{
				Trace.TraceError( exception.ToString() );
				Trace.TraceError( "文字コンソールの初期化に失敗しました。" );
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
			#endregion
			#region [ Input管理 の初期化 ]
			//---------------------
			Trace.TraceInformation( "DirectInput, MIDI入力の初期化を行います。" );
			Trace.Indent();
			try
			{
				Input管理 = new CInput管理( base.Window.Handle );
				foreach( IInputDevice device in Input管理.list入力デバイス )
				{
					if( ( device.e入力デバイス種別 == E入力デバイス種別.Joystick ) && !ConfigIni.dicJoystick.ContainsValue( device.GUID ) )
					{
						int key = 0;
						while( ConfigIni.dicJoystick.ContainsKey( key ) )
						{
							key++;
						}
						ConfigIni.dicJoystick.Add( key, device.GUID );
					}
				}
				foreach( IInputDevice device2 in Input管理.list入力デバイス )
				{
					if( device2.e入力デバイス種別 == E入力デバイス種別.Joystick )
					{
						foreach( KeyValuePair<int, string> pair in ConfigIni.dicJoystick )
						{
							if( device2.GUID.Equals( pair.Value ) )
							{
								( (CInputJoystick) device2 ).SetID( pair.Key );
								break;
							}
						}
						continue;
					}
				}
				Trace.TraceInformation( "DirectInput の初期化を完了しました。" );
			}
			catch( Exception exception2 )
			{
				Trace.TraceError( "DirectInput, MIDI入力の初期化に失敗しました。" );
				throw;
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
			#endregion
			#region [ Sound管理 の初期化 ]
			//---------------------
			Trace.TraceInformation( "サウンドデバイスの初期化を行います。" );
			Trace.Indent();
			try
			{
				ESoundDeviceType soundDeviceType;
				switch ( TJAPlayer3.ConfigIni.nSoundDeviceType )
				{
					case 0:
						soundDeviceType = ESoundDeviceType.DirectSound;
						break;
					case 1:
						soundDeviceType = ESoundDeviceType.ASIO;
						break;
					case 2:
						soundDeviceType = ESoundDeviceType.ExclusiveWASAPI;
						break;
					default:
						soundDeviceType = ESoundDeviceType.Unknown;
						break;
				}
				Sound管理 = new CSound管理(base.Window.Handle,
											soundDeviceType,
											TJAPlayer3.ConfigIni.nWASAPIBufferSizeMs,
					// CDTXMania.ConfigIni.nASIOBufferSizeMs,
											0,
											TJAPlayer3.ConfigIni.nASIODevice,
											TJAPlayer3.ConfigIni.bUseOSTimer
				);
				//Sound管理 = FDK.CSound管理.Instance;
				//Sound管理.t初期化( soundDeviceType, 0, 0, CDTXMania.ConfigIni.nASIODevice, base.Window.Handle );

				ShowWindowTitleWithSoundType();
				FDK.CSound管理.bIsTimeStretch = TJAPlayer3.ConfigIni.bTimeStretch;
				Sound管理.nMasterVolume = TJAPlayer3.ConfigIni.nMasterVolume;
				//FDK.CSound管理.bIsMP3DecodeByWindowsCodec = CDTXMania.ConfigIni.bNoMP3Streaming;
				Trace.TraceInformation( "サウンドデバイスの初期化を完了しました。" );
			}
			catch (Exception e)
			{
                throw new Exception("サウンドデバイスがひとつも有効になっていないため、サウンドデバイスの初期化ができませんでした。", e);
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
			#endregion
			#region [ CAvi の初期化 ]
			//---------------------
			CAvi.t初期化();
			//---------------------
			#endregion
			#region [ Random の初期化 ]
			//---------------------
			Random = new Random( (int) Timer.nシステム時刻 );
			//---------------------
			#endregion
			#region [ ステージの初期化 ]
			//---------------------
			r現在のステージ = null;
			r直前のステージ = null;

			stage起動 = new CStage起動();
			stageサンプル = new CStageサンプル();

			this.listトップレベルActivities = new List<CActivity>();
			this.listトップレベルActivities.Add( act文字コンソール );

			this.listトップレベルActivities.Add( stage起動 );
			this.listトップレベルActivities.Add( stageサンプル );

			this.listトップレベルActivities.Add( actFlushGPU );
			//---------------------
			#endregion

            #region Discordの処理
            Discord.Initialize("428233983025741855");
            StartupTime = Discord.GetUnixTime();
            Discord.UpdatePresence("", Properties.Discord.Stage_StartUp, StartupTime);
            #endregion


            Trace.TraceInformation( "アプリケーションの初期化を完了しました。" );


            #region [ 最初のステージの起動 ]
            //---------------------
            Trace.TraceInformation( "----------------------" );
			Trace.TraceInformation( "■ 起動" );

			r現在のステージ = stage起動;
			r現在のステージ.On活性化();

			//---------------------
			#endregion
		}

		public void ShowWindowTitleWithSoundType()
		{
			string delay = "";
			if ( CSound管理.GetCurrentSoundDeviceType() != "DirectSound" )
			{
				delay = " (" + Sound管理.GetSoundDelay() + "ms)";
			}
            base.Window.Text = $"{AppDisplayNameWithInformationalVersion} ({CSound管理.GetCurrentSoundDeviceType()}{delay})";
		}

		private void t終了処理()
		{
			if( !this.b終了処理完了済み )
			{
				Trace.TraceInformation( "----------------------" );
				Trace.TraceInformation( "■ アプリケーションの終了" );

				#region [ 現在のステージの終了処理 ]
				//---------------------
				if( TJAPlayer3.r現在のステージ != null && TJAPlayer3.r現在のステージ.b活性化してる )		// #25398 2011.06.07 MODIFY FROM
				{
					Trace.TraceInformation( "現在のステージを終了します。" );
					Trace.Indent();
					try
					{
						r現在のステージ.On非活性化();
						Trace.TraceInformation( "現在のステージの終了処理を完了しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion

                #region Discordの処理
                Discord.Shutdown();
                #endregion
                #region TextureLoaderの処理
                Tx.Dispose();
                Tx = null;
                #endregion
                #region [ スキンの終了処理 ]
                //---------------------
                if (Skin != null)
				{
					Trace.TraceInformation( "スキンの終了処理を行います。" );
					Trace.Indent();
					try
					{
						Skin.Dispose();
						Skin = null;
						Trace.TraceInformation( "スキンの終了処理を完了しました。" );
					}
					catch( Exception exception2 )
					{
						Trace.TraceError( exception2.ToString() );
						Trace.TraceError( "スキンの終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ DirectSoundの終了処理 ]
				//---------------------
				if (Sound管理 != null)
				{
					Trace.TraceInformation( "DirectSound の終了処理を行います。" );
					Trace.Indent();
					try
					{
						Sound管理.Dispose();
						Sound管理 = null;
						Trace.TraceInformation( "DirectSound の終了処理を完了しました。" );
					}
					catch( Exception exception3 )
					{
						Trace.TraceError( exception3.ToString() );
						Trace.TraceError( "DirectSound の終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ DirectInput, MIDI入力の終了処理 ]
				//---------------------
				if (Input管理 != null)
				{
					Trace.TraceInformation( "DirectInput, MIDI入力の終了処理を行います。" );
					Trace.Indent();
					try
					{
						Input管理.Dispose();
						Input管理 = null;
						Trace.TraceInformation( "DirectInput, MIDI入力の終了処理を完了しました。" );
					}
					catch( Exception exception5 )
					{
						Trace.TraceError( exception5.ToString() );
						Trace.TraceError( "DirectInput, MIDI入力の終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ 文字コンソールの終了処理 ]
				//---------------------
				if (act文字コンソール != null)
				{
					Trace.TraceInformation( "文字コンソールの終了処理を行います。" );
					Trace.Indent();
					try
					{
						act文字コンソール.On非活性化();
						act文字コンソール = null;
						Trace.TraceInformation( "文字コンソールの終了処理を完了しました。" );
					}
					catch( Exception exception6 )
					{
						Trace.TraceError( exception6.ToString() );
						Trace.TraceError( "文字コンソールの終了処理に失敗しました。" );
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ FPSカウンタの終了処理 ]
				//---------------------
				Trace.TraceInformation("FPSカウンタの終了処理を行います。");
				Trace.Indent();
				try
				{
					if( FPS != null )
					{
						FPS = null;
					}
					Trace.TraceInformation( "FPSカウンタの終了処理を完了しました。" );
				}
				finally
				{
					Trace.Unindent();
				}
				//---------------------
				#endregion
				#region [ タイマの終了処理 ]
				//---------------------
				Trace.TraceInformation("タイマの終了処理を行います。");
				Trace.Indent();
				try
				{
					if( Timer != null )
					{
						Timer.Dispose();
						Timer = null;
						Trace.TraceInformation( "タイマの終了処理を完了しました。" );
					}
					else
					{
						Trace.TraceInformation( "タイマは使用されていません。" );
					}
				}
				finally
				{
					Trace.Unindent();
				}
				//---------------------
				#endregion
				#region [ Config.iniの出力 ]
				//---------------------
				Trace.TraceInformation("Config.ini を出力します。");
//				if ( ConfigIni.bIsSwappedGuitarBass )			// #24063 2011.1.16 yyagi ギターベースがスワップしているときは元に戻す
				string str = strEXEのあるフォルダ + "Config.ini";
				Trace.Indent();
				try
				{
					ConfigIni.t書き出し( str );
					Trace.TraceInformation( "保存しました。({0})", str );
				}
				catch( Exception e )
				{
					Trace.TraceError( e.ToString() );
					Trace.TraceError( "Config.ini の出力に失敗しました。({0})", str );
				}
				finally
				{
					Trace.Unindent();
				}

			    ConfigIni = null;

				//---------------------
				#endregion

                #region [ DirectXの終了処理 ]
                base.GraphicsDeviceManager.Dispose();
                #endregion
                Trace.TraceInformation( "アプリケーションの終了処理を完了しました。" );


				this.b終了処理完了済み = true;
			}
		}

		private void tガベージコレクションを実行する()
		{
			GC.Collect(GC.MaxGeneration);
			GC.WaitForPendingFinalizers();
			GC.Collect(GC.MaxGeneration);
		}

        public void RefleshSkin()
        {
            Trace.TraceInformation("スキン変更:" + TJAPlayer3.Skin.GetCurrentSkinSubfolderFullName(false));

            TJAPlayer3.act文字コンソール.On非活性化();

            TJAPlayer3.Skin.Dispose();
            TJAPlayer3.Skin = null;
            TJAPlayer3.Skin = new CSkin(TJAPlayer3.ConfigIni.strSystemSkinSubfolderFullName, false);


            TJAPlayer3.Tx.Dispose();
            TJAPlayer3.Tx = new TextureLoader();
            TJAPlayer3.Tx.Load();

            TJAPlayer3.act文字コンソール.On活性化();
        }
		#region [ Windowイベント処理 ]
		//-----------------
		private void Window_ApplicationActivated( object sender, EventArgs e )
		{
			this.bApplicationActive = true;
		}
		private void Window_ApplicationDeactivated( object sender, EventArgs e )
		{
			this.bApplicationActive = false;
		}
		private void Window_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.KeyCode == Keys.Menu )
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if ( ( e.KeyCode == Keys.Return ) && e.Alt )
			{
				if ( ConfigIni != null )
				{
					ConfigIni.bウィンドウモード = !ConfigIni.bウィンドウモード;
					this.t全画面_ウィンドウモード切り替え();
				}
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}
		private void Window_MouseUp( object sender, MouseEventArgs e )
		{
			mb = e.Button;
		}

		private void Window_MouseDoubleClick( object sender, MouseEventArgs e)	// #23510 2010.11.13 yyagi: to go full screen mode
		{
			if ( mb.Equals(MouseButtons.Left) && ConfigIni.bIsAllowedDoubleClickFullscreen )	// #26752 2011.11.27 yyagi
			{
				ConfigIni.bウィンドウモード = false;
				this.t全画面_ウィンドウモード切り替え();
			}
		}
		private void Window_ResizeEnd(object sender, EventArgs e)				// #23510 2010.11.20 yyagi: to get resized window size
		{
			if ( ConfigIni.bウィンドウモード )
			{
				ConfigIni.n初期ウィンドウ開始位置X = base.Window.Location.X;	// #30675 2013.02.04 ikanick add
				ConfigIni.n初期ウィンドウ開始位置Y = base.Window.Location.Y;	//
			}

			ConfigIni.nウインドウwidth = (ConfigIni.bウィンドウモード) ? base.Window.ClientSize.Width : currentClientSize.Width;	// #23510 2010.10.31 yyagi add
			ConfigIni.nウインドウheight = (ConfigIni.bウィンドウモード) ? base.Window.ClientSize.Height : currentClientSize.Height;
		}
		#endregion
		#endregion
	}
}
