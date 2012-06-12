using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class Finish
	{
		private Rectangle hitBox;
		public Rectangle HitBox { get { return hitBox; } }
		
		private Texture2D finishTex;

		public Finish(Vector2 position)
		{
			hitBox = new Rectangle((int)position.X, (int)position.Y + 2, 64, 32);
			finishTex = Game1.finishTex;
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(finishTex, new Rectangle(HitBox.X - Game1.currentRoom.ViewBox.X, HitBox.Y - Game1.currentRoom.ViewBox.Y, HitBox.Width, HitBox.Height), c);
		}
	}
}
