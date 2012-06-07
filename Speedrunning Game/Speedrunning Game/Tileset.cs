using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class Tileset
	{
		static Texture2D tilesetTexture;
		List<Rectangle> tiles = new List<Rectangle>();
		static int tileWidth;
		static int tileHeight;
		int tilesWide;
		int tilesHigh;
		public Tileset(Texture2D tilesetTexture, int tileWidth, int tileHeight,
		int tilesWide, int tilesHigh)
		{
			Tileset.tilesetTexture = tilesetTexture;
			Tileset.tileWidth = tileWidth;
			Tileset.tileHeight = tileHeight; this.tilesWide = tilesWide;
			this.tilesHigh = tilesHigh;
			CreateRectangles(tilesWide, tilesHigh);
		}
		public List<Rectangle> Tiles
		{
			get { return tiles; }
		}
		public static Texture2D TilesetTexture
		{
			get { return tilesetTexture; }
		}
		public static int TileWidth
		{
			get { return tileWidth; }
		}
		public static int TileHeight
		{
			get { return tileHeight; }
		}

		// 0 3 6
		// 1 4 7
		// 2 5 8
		public void CreateRectangles(int tilesWide, int tilesHigh)
		{
			Rectangle rectangle = new Rectangle();
			rectangle.Width = Tileset.tileWidth;
			rectangle.Height = Tileset.tileHeight;
			tiles.Clear();
			for (int i = 0; i < tilesWide; i++)
			{
				for (int j = 0; j < tilesHigh; j++)
				{
					rectangle.X = i * Tileset.tileWidth;
					rectangle.Y = j * Tileset.tileHeight;
					tiles.Add(rectangle);
				}
			}
		}
	}
}