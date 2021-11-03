using System;
using System.Collections.Generic;
using System.Linq;
using FDK;

namespace TJAPlayer3
{
    class TextureLoader
    {
        const string BASE = @"Graphics\";

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

        private T Track<T>(T texture) where T : CTexture
        {
            if (texture != null)
            {
                _trackedTextures.Add(texture);
            }

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
            Overlay = TxC("Overlay.png");
            #endregion

            #region サンプル
            DVD_Logo = TxC("Sample\\DVD_Logo.png");
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
            Overlay;
        #endregion

        #region サンプル
        public CTexture DVD_Logo;
        #endregion
    }
}
