using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class FloatingPlatform : Wall
	{
		public Vector2 velocity, position;
		Texture2D texture;

		public FloatingPlatform(Vector2 position, float angle, float speed) : base(new Rectangle((int)position.X, (int)position.Y, 96, 32))
		{
			this.position = position;
			this.velocity = new Vector2(speed * (float)Math.Cos(angle), speed * (float)Math.Sin(angle));
			texture = Game1.platformTex;
			UpdateHitBox();
		}
		private void UpdateHitBox()
		{
			bounds.X = (int)position.X;
			bounds.Y = (int)position.Y;
		}

		public void Update()
		{
			position += velocity;
			UpdateHitBox();
			foreach (Wall w in Game1.currentRoom.walls)
			{
				if (this == w)
					continue;
				if (w.bounds.Intersects(bounds))
				{
					velocity *= -1;
					position += velocity;
				}
			}
		}

		public override void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(texture, new Vector2(position.X - Game1.currentRoom.viewBox.X, position.Y - Game1.currentRoom.viewBox.Y), c);
		}
	}
}
