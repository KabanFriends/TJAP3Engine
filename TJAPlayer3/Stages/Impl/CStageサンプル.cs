using FDK;
using System.Diagnostics;

namespace TJAPlayer3
{
    class CStageサンプル : CStage
    {
        public CStageサンプル()
        {
            base.eステージID = CStage.Eステージ.サンプル;
            base.b活性化してない = true;
        }

		public override void On活性化()
		{
			Trace.TraceInformation("サンプルステージを活性化します。");
			Trace.Indent();
			try
			{
				hits = 0;
				cornerHits = 0;
				this.logo = new CDVDLogo();
				this.stepCounter = new CCounter(0, 100, 15, TJAPlayer3.Timer);

				base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
				base.On活性化();
				Trace.TraceInformation("サンプルステージの活性化を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
		}

		public override void On非活性化()
		{
			Trace.TraceInformation("サンプルステージを非活性化します。");
			Trace.Indent();
			try
			{
				base.On非活性化();
				Trace.TraceInformation("サンプルステージの非活性化を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
		}

		public override void OnManagedリソースの作成()
		{
			if (!base.b活性化してない)
			{
				base.OnManagedリソースの作成();
			}

		}
		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				base.OnManagedリソースの解放();
			}
		}

		public override int On進行描画()
        {
			if (!base.b活性化してない)
            {
				if (base.b初めての進行描画)
                {
					base.b初めての進行描画 = true;
				}
			}

			stepCounter.t進行Loop(t常時実行);

			TJAPlayer3.act文字コンソール.tPrint(10, 10, C文字コンソール.Eフォント種別.白, "TJAP3Engine DVD Screensaver Demo");
			TJAPlayer3.act文字コンソール.tPrint(10, 30, C文字コンソール.Eフォント種別.白, "Hits: " + hits);
			TJAPlayer3.act文字コンソール.tPrint(10, 50, C文字コンソール.Eフォント種別.白, "Corner Hits: " + cornerHits);

			return 0;
        }

		private void t常時実行()
        {
			logo.tStep();
		}

		private CDVDLogo logo;
		public static int hits;
		public static int cornerHits;

		private CCounter stepCounter;
	}
}
