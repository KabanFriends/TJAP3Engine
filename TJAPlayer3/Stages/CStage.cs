using FDK;

namespace TJAPlayer3
{
	public class CStage : CActivity
	{
		// プロパティ

		internal Eステージ eステージID;
		public enum Eステージ
		{
			何もしない,
			起動,
			サンプル
		}
		
		internal Eフェーズ eフェーズID;
		public enum Eフェーズ
		{
			共通_通常状態,
			共通_終了状態,
			起動0_システムサウンドを構築,
			起動1_完了
		}
	}
}
