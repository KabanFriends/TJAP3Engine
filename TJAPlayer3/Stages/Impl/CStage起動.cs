using System.Collections.Generic;
using System.Diagnostics;
using FDK;


namespace TJAPlayer3
{
	internal class CStage起動 : CStage
	{
		// コンストラクタ

		public CStage起動()
		{
			base.eステージID = CStage.Eステージ.起動;
			base.b活性化してない = true;
		}

		public List<string> list進行文字列;

		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation( "起動ステージを活性化します。" );
			Trace.Indent();
			try
			{
				this.list進行文字列 = new List<string>();
				base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
				base.On活性化();
				Trace.TraceInformation( "起動ステージの活性化を完了しました。" );
			}
			finally
			{
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			Trace.TraceInformation( "起動ステージを非活性化します。" );
			Trace.Indent();
			try
			{
				this.list進行文字列 = null;
				base.On非活性化();
				Trace.TraceInformation( "起動ステージの非活性化を完了しました。" );
			}
			finally
			{
				Trace.Unindent();
			}
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				if( base.b初めての進行描画 )
				{
					this.list進行文字列.Add(TJAPlayer3.AppDisplayNameWithInformationalVersion);
                    this.list進行文字列.Add("");
                    this.list進行文字列.Add($"{TJAPlayer3.AppDisplayName} is open source software under the MIT license.");
                    this.list進行文字列.Add("See README for acknowledgments.");
                    this.list進行文字列.Add("");

					base.b初めての進行描画 = false;

					#region [ 0) システムサウンドの構築  ]
					//-----------------------------
					TJAPlayer3.stage起動.eフェーズID = CStage.Eフェーズ.起動0_システムサウンドを構築;

					Trace.TraceInformation("0) システムサウンドを構築します。");
					Trace.Indent();

					try
					{
						TJAPlayer3.Skin.bgm起動画面.t再生する();
						TJAPlayer3.Skin.ReloadSkin();

						lock (TJAPlayer3.stage起動.list進行文字列)
						{
							TJAPlayer3.stage起動.list進行文字列.Add("SYSTEM SOUND...");
						}
					}
					finally
					{
						Trace.Unindent();

						TJAPlayer3.stage起動.eフェーズID = CStage.Eフェーズ.起動1_完了;
					}
					//-----------------------------
					#endregion

					return 0;
				}

				// CSongs管理 s管理 = CDTXMania.Songs管理;

				//if( this.tx背景 != null )
				//	this.tx背景.t2D描画( CDTXMania.app.Device, 0, 0 );

				#region [ this.str現在進行中 の決定 ]
				//-----------------
				switch( base.eフェーズID )
				{
					case CStage.Eフェーズ.起動0_システムサウンドを構築:
						this.str現在進行中 = "SYSTEM SOUND...OK";
						break;
					case CStage.Eフェーズ.起動1_完了:
                        this.list進行文字列.Add("LOADING TEXTURES...");
                        TJAPlayer3.Tx.Load();
                        this.list進行文字列.Add("LOADING TEXTURES...OK");
                        this.str現在進行中 = "Setup done.";
						return 1;
				}
				//-----------------
				#endregion
				#region [ this.list進行文字列＋this.現在進行中 の表示 ]
				//-----------------
				lock( this.list進行文字列 )
				{
					int x = 320;
					int y = 20;
					foreach( string str in this.list進行文字列 )
					{
						TJAPlayer3.act文字コンソール.tPrint( x, y, C文字コンソール.Eフォント種別.白, str );
						y += 24;
					}
					TJAPlayer3.act文字コンソール.tPrint( x, y, C文字コンソール.Eフォント種別.白, this.str現在進行中 );
				}
				//-----------------
				#endregion
			}
			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		private string str現在進行中 = "";
		#endregion
	}
}
