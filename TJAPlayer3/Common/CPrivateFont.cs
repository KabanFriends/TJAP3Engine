using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using FDK.ExtensionMethods;
using TJAPlayer3.Common;

namespace TJAPlayer3
{
    public class CPrivateFont : IDisposable
	{
		#region [ コンストラクタ ]
		public CPrivateFont( FontFamily fontfamily, int pt )
		{
			Initialize( null, fontfamily, pt, FontStyle.Regular );
		}

		protected CPrivateFont()
		{
			//throw new ArgumentException("CPrivateFont: 引数があるコンストラクタを使用してください。");
		}
		#endregion

		protected void Initialize( string fontpath, FontFamily fontfamily, int pt, FontStyle style )
		{
			this._pfc = null;
			this._fontfamily = null;
			this._font = null;
			this._pt = pt;
			this._rectStrings = new Rectangle(0, 0, 0, 0);
			this._ptOrigin = new Point(0, 0);
			this.bDispose完了済み = false;

			if (fontfamily != null)
			{
				this._fontfamily = fontfamily;
			}
			else
			{
				try
				{
					this._pfc = new System.Drawing.Text.PrivateFontCollection();	//PrivateFontCollectionオブジェクトを作成する
					this._pfc.AddFontFile(fontpath);								//PrivateFontCollectionにフォントを追加する
					_fontfamily = _pfc.Families[0];
				}
				catch
				{
					Trace.TraceWarning($"プライベートフォントの追加に失敗しました({fontpath})。代わりに{FontUtilities.FallbackFontName}の使用を試みます。");
					_fontfamily = null;
				}
			}

			// 指定されたフォントスタイルが適用できない場合は、フォント内で定義されているスタイルから候補を選んで使用する
			// 何もスタイルが使えないようなフォントなら、例外を出す。
			if (_fontfamily != null)
			{
				if (!_fontfamily.IsStyleAvailable(style))
				{
					FontStyle[] FS = { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Underline, FontStyle.Strikeout };
					style = FontStyle.Regular | FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout;	// null非許容型なので、代わりに全盛をNGワードに設定
					foreach (FontStyle ff in FS)
					{
						if (this._fontfamily.IsStyleAvailable(ff))
						{
							style = ff;
							Trace.TraceWarning("フォント{0}へのスタイル指定を、{1}に変更しました。", Path.GetFileName(fontpath), style.ToString());
							break;
						}
					}
					if (style == (FontStyle.Regular | FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout))
					{
						Trace.TraceWarning("フォント{0}は適切なスタイル{1}を選択できませんでした。", Path.GetFileName(fontpath), style.ToString());
					}
				}
				//this._font = new Font(this._fontfamily, pt, style);			//PrivateFontCollectionの先頭のフォントのFontオブジェクトを作成する
				float emSize = pt * 96.0f / 72.0f;
				this._font = new Font(this._fontfamily, emSize, style, GraphicsUnit.Pixel);	//PrivateFontCollectionの先頭のフォントのFontオブジェクトを作成する
				//HighDPI対応のため、pxサイズで指定
			}
			else
            {
                try
                {
                    _fontfamily = new FontFamily(FontUtilities.FallbackFontName);
                    float emSize = pt * 96.0f / 72.0f;
                    _font = new Font(_fontfamily, emSize, style, GraphicsUnit.Pixel);
                    Trace.TraceInformation($"{FontUtilities.FallbackFontName}を代わりに指定しました。");
                }
                catch (Exception e)
                {
                    throw new FileNotFoundException($"プライベートフォントの追加に失敗し、{FontUtilities.FallbackFontName}での代替処理にも失敗しました。({Path.GetFileName(fontpath)})", e);
                }
            }
		}

		[Flags]
		public enum DrawMode
		{
			Normal,
			Edge,
			Gradation,
            Vertical
		}

		#region [ DrawPrivateFontのオーバーロード群 ]

		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="edgeColor">縁取色</param>
		/// <returns>描画済テクスチャ</returns>
		public Bitmap DrawPrivateFont( string drawstr, Color fontColor, Color edgeColor )
		{
			return DrawPrivateFont( drawstr, DrawMode.Edge, fontColor, edgeColor, Color.White, Color.White );
		}

		#endregion


		/// <summary>
		/// 文字列を描画したテクスチャを返す(メイン処理)
		/// </summary>
		/// <param name="rectDrawn">描画された領域</param>
		/// <param name="ptOrigin">描画文字列</param>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="drawmode">描画モード</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="edgeColor">縁取色</param>
		/// <param name="gradationTopColor">グラデーション 上側の色</param>
		/// <param name="gradationBottomColor">グラデーション 下側の色</param>
		/// <returns>描画済テクスチャ</returns>
		protected Bitmap DrawPrivateFont( string drawstr, DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor )
		{
			if ( this._fontfamily == null || _font == null || drawstr == null || drawstr == "" )
			{
				// nullを返すと、その後bmp→texture処理や、textureのサイズを見て__の処理で全部例外が発生することになる。
				// それは非常に面倒なので、最小限のbitmapを返してしまう。
				// まずはこの仕様で進めますが、問題有れば(上位側からエラー検出が必要であれば)例外を出したりエラー状態であるプロパティを定義するなり検討します。
				if ( drawstr != "" )
				{
					Trace.TraceWarning( "DrawPrivateFont()の入力不正。最小値のbitmapを返します。" );
				}
				_rectStrings = new Rectangle( 0, 0, 0, 0 );
				_ptOrigin = new Point( 0, 0 );
				return new Bitmap(1, 1);
			}
			bool bEdge =      ( ( drawmode & DrawMode.Edge      ) == DrawMode.Edge );
			bool bGradation = ( ( drawmode & DrawMode.Gradation ) == DrawMode.Gradation );

            // 縁取りの縁のサイズは、とりあえずフォントの大きさの1/4とする
            //int nEdgePt = (bEdge)? _pt / 4 : 0;
            //int nEdgePt = (bEdge) ? (_pt / 3) : 0; // 縁取りが少なすぎるという意見が多かったため変更。 (AioiLight)
            int nEdgePt = (bEdge) ? (10 * _pt / TJAPlayer3.Skin.Font_Edge_Ratio) : 0; //SkinConfigにて設定可能に(rhimm)

            // 描画サイズを測定する
            Size stringSize = System.Windows.Forms.TextRenderer.MeasureText( drawstr, this._font, new Size( int.MaxValue, int.MaxValue ),
				System.Windows.Forms.TextFormatFlags.NoPrefix |
				System.Windows.Forms.TextFormatFlags.NoPadding
			);
            stringSize.Width += 10; //2015.04.01 kairera0467 ROTTERDAM NATIONの描画サイズがうまくいかんので。

			//取得した描画サイズを基に、描画先のbitmapを作成する
			Bitmap bmp = new Bitmap( stringSize.Width + nEdgePt * 2, stringSize.Height + nEdgePt * 2 );
			bmp.MakeTransparent();
			Graphics g = Graphics.FromImage( bmp );
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			StringFormat sf = new StringFormat();
			sf.LineAlignment = StringAlignment.Far;	// 画面下部（垂直方向位置）
			sf.Alignment = StringAlignment.Center;	// 画面中央（水平方向位置）     
            sf.FormatFlags = StringFormatFlags.NoWrap; // どんなに長くて単語の区切りが良くても改行しない (AioiLight)
            sf.Trimming = StringTrimming.None; // どんなに長くてもトリミングしない (AioiLight)
			// レイアウト枠
			Rectangle r = new Rectangle( 0, 0, stringSize.Width + nEdgePt * 2 + (TJAPlayer3.Skin.Text_Correction_XY[0] * stringSize.Width / 100), stringSize.Height + nEdgePt * 2 + (TJAPlayer3.Skin.Text_Correction_XY[1] * stringSize.Height / 100));

			if ( bEdge )	// 縁取り有りの描画
			{
				// DrawPathで、ポイントサイズを使って描画するために、DPIを使って単位変換する
				// (これをしないと、単位が違うために、小さめに描画されてしまう)
				float sizeInPixels = _font.SizeInPoints * g.DpiY / 72;  // 1 inch = 72 points

				System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
				gp.AddString( drawstr, this._fontfamily, (int) this._font.Style, sizeInPixels, r, sf );

				// 縁取りを描画する
				Pen p = new Pen( edgeColor, nEdgePt );
				p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
				g.DrawPath( p, gp );

				// 塗りつぶす
				Brush br;
				if ( bGradation )
				{
					br = new LinearGradientBrush( r, gradationTopColor, gradationBottomColor, LinearGradientMode.Vertical );
				}
				else
				{
					br = new SolidBrush( fontColor );
				}
				g.FillPath( br, gp );

				if ( br != null ) br.Dispose(); br = null;
				if ( p != null ) p.Dispose(); p = null;
				if ( gp != null ) gp.Dispose(); gp = null;
			}
			else
			{
				// 縁取りなしの描画
				System.Windows.Forms.TextRenderer.DrawText( g, drawstr, _font, new Point( 0, 0 ), fontColor );
			}
#if debug表示
			g.DrawRectangle( new Pen( Color.White, 1 ), new Rectangle( 1, 1, stringSize.Width-1, stringSize.Height-1 ) );
			g.DrawRectangle( new Pen( Color.Green, 1 ), new Rectangle( 0, 0, bmp.Width - 1, bmp.Height - 1 ) );
#endif
			_rectStrings = new Rectangle( 0, 0, stringSize.Width, stringSize.Height );
			_ptOrigin = new Point( nEdgePt * 2, nEdgePt * 2 );
			

			#region [ リソースを解放する ]
			if ( sf != null )	sf.Dispose();	sf = null;
			if ( g != null )	g.Dispose();	g = null;
			#endregion

			return bmp;
		}

        //------------------------------------------------

		/// <summary>
		/// 最後にDrawPrivateFont()した文字列の描画領域を取得します。
		/// </summary>
		public Rectangle RectStrings
		{
			get
			{
				return _rectStrings;
			}
			protected set
			{
				_rectStrings = value;
			}
		}
		public Point PtOrigin
		{
			get
			{
				return _ptOrigin;
			}
			protected set
			{
				_ptOrigin = value;
			}
		}

        #region [ IDisposable 実装 ]
        //-----------------
        public void Dispose()
        {
            if (!this.bDispose完了済み)
            {
                if (this._font != null)
                {
                    this._font.Dispose();
                    this._font = null;
                }
                if (this._pfc != null)
                {
                    this._pfc.Dispose();
                    this._pfc = null;
                }

                this.bDispose完了済み = true;
            }
        }
        //-----------------
        #endregion
        #region [ private ]
        //-----------------
        protected bool bDispose完了済み;
		protected Font _font;

		private System.Drawing.Text.PrivateFontCollection _pfc;
		private FontFamily _fontfamily;
		private int _pt;
		private Rectangle _rectStrings;
		private Point _ptOrigin;
		//-----------------
		#endregion
	}
}
