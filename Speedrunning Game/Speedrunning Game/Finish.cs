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
		Texture2D finishTex;
		public Rectangle hitBox;

		public Finish(Vector2 position)
		{
			hitBox = new Rectangle((int)position.X, (int)position.Y + 2, 64, 32);
			finishTex = Game1.finishTex;
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(finishTex, new Rectangle(hitBox.X - Game1.currentRoom.viewBox.X, hitBox.Y - Game1.currentRoom.viewBox.Y, hitBox.Width, hitBox.Height), c);
		}
	}
}
