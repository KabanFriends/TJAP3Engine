using System;
using System.Collections.Generic;
using System.Linq;
using FDK;

namespace TJAPlayer3
{
    class TextureLoader
    {
        const string BASE = @"Graphics\";

        // Stage
        const string TITLE = @"1_Title\";
        const string CONFIG = @"2_Config\";
        const string SONGSELECT = @"3_SongSelect\";
        const string SONGLOADING = @"4_SongLoading\";
        const string GAME = @"5_Game\";
        const string RESULT = @"6_Result\";
        const string EXIT = @"7_Exit\";

        // InGame
        const string CHARA = @"1_Chara\";
        const string DANCER = @"2_Dancer\";
        const string MOB = @"3_Mob\";
        const string COURSESYMBOL = @"4_CourseSymbol\";
        const string BACKGROUND = @"5_Background\";
        const string TAIKO = @"6_Taiko\";
        const string GAUGE = @"7_Gauge\";
        const string FOOTER = @"8_Footer\";
        const string END = @"9_End\";
        const string EFFECTS = @"10_Effects\";
        const string BALLOON = @"11_Balloon\";
        const string LANE = @"12_Lane\";
        const string GENRE = @"13_Genre\";
        const string GAMEMODE = @"14_GameMode\";
        const string FAILED = @"15_Failed\";
        const string RUNNER = @"16_Runner\";
        const string PUCHICHARA = @"18_PuchiChara\";
        const string DANC = @"17_DanC\";

        // InGame_Effects
        const string FIRE = @"Fire\";
        const string HIT = @"Hit\";
        const string ROLL = @"Roll\";
        const string SPLASH = @"Splash\";

        private readonly List<CTexture> _trackedTextures = new List<CTexture>();
        private readonly Dictionary<string, CTexture> _genreTexturesByFileNameWithoutExtension = new Dictionary<string, CTexture>();

        private (int skinGameCharaPtnNormal, CTexture[] charaNormal) TxCFolder(string folder)
        {
            var count = TJAPlayer3.t連番画像の枚数を数える(CSkin.Path(BASE + folder));
            var texture = count == 0 ? null : TxC(count, folder + "{0}.png");
            return (count, texture);
        }

        private CTexture[] TxC(int count, string format, int start = 0)
        {
            return TxC(format, Enumerable.Range(start, count).Select(o => o.ToString()).ToArray());
        }

        private CTexture[] TxC(string format, params string[] parts)
        {
            return parts.Select(o => TxC(string.Format(format, o))).ToArray();
        }

        private CTexture TxC(string path)
        {
            return Track(TxCUntracked(path));
        }

        private CTextureAf TxCAf(string path)
        {
            return Track(TxCAfUntracked(path));
        }

        private T Track<T>(T texture) where T : CTexture
        {
            if (texture != null)
            {
                _trackedTextures.Add(texture);
            }

            return texture;
        }

        internal CTexture TxCGenre(string fileNameWithoutExtension)
        {
            if (_genreTexturesByFileNameWithoutExtension.TryGetValue(fileNameWithoutExtension, out var texture))
            {
                return texture;
            }

            texture = TxC($"{GAME}{GENRE}{fileNameWithoutExtension}.png");

            _genreTexturesByFileNameWithoutExtension.Add(fileNameWithoutExtension, texture);

            return texture;
        }

        internal CTexture TxCUntracked(string path)
        {
            return TJAPlayer3.tテクスチャの生成(CSkin.Path(BASE + path));
        }

        private CTextureAf TxCAfUntracked(string path)
        {
            return TJAPlayer3.tテクスチャの生成Af(CSkin.Path(BASE + path));
        }

        public void Load()
        {
            #region 共通
            Tile_Black = TxC("Tile_Black.png");
            Tile_White = TxC("Tile_White.png");
            Menu_Title = TxC("Menu_Title.png");
            Menu_Highlight = TxC("Menu_Highlight.png");
            Enum_Song = TxC("Enum_Song.png");
            Scanning_Loudness = TxC("Scanning_Loudness.png");
            Overlay = TxC("Overlay.png");

            NamePlate = TxC(2, "{0}P_NamePlate.png", 1);
            #endregion
        }

        public void Dispose()
        {
            _genreTexturesByFileNameWithoutExtension.Clear();

            foreach (var texture in _trackedTextures)
            {
                texture.Dispose();
            }
            _trackedTextures.Clear();
        }

        #region 共通
        public CTexture Tile_Black,
            Tile_White,
            Menu_Title,
            Menu_Highlight,
            Enum_Song,
            Scanning_Loudness,
            Overlay;
        public CTexture[] NamePlate;
        #endregion
    }
}
