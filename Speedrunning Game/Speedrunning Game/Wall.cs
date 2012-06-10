using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class Wall
	{
		public Rectangle bounds;
		public Texture2D tex;

		public Wall(Rectangle bounds)
		{
			this.bounds = bounds;
			tex = Game1.wallTex;
		}
		public Wall(int x, int y, int width, int height)
		{
			this.bounds = new Rectangle(x, y, width, height);
			tex = Game1.wallTex;
		}

		public virtual void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(tex, new Rectangle(bounds.X - Game1.currentRoom.viewBox.X, bounds.Y - Game1.currentRoom.viewBox.Y, bounds.Width, bounds.Height), c);
		}
	}
}
