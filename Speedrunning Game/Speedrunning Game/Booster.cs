using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class Booster
	{
		AnimatedTexture texture;
		public Rectangle hitBox;
		public Vector2 acceleration;
		float angle;

		public Booster(Vector2 position, float angle, float power)
		{
			texture = Game1.boosterTex;
			hitBox = new Rectangle((int)position.X, (int)position.Y, 32, 32);
			this.angle = angle;
			acceleration = new Vector2((float)(power * Math.Cos(angle)), (float)(power * Math.Sin(angle)));
		}

		public void Update()
		{
			texture.Update();
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			texture.Draw(sb, new Vector2(hitBox.X + 16 - Game1.currentRoom.viewBox.X, hitBox.Y + 16 - Game1.currentRoom.viewBox.Y), c, angle, new Vector2(16, 16), Vector2.One, SpriteEffects.None, 0);
		}
	}
}
