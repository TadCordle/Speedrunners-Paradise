using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Speedrunning_Game
{
	public class RocketLauncher
	{
		public Rectangle hitBox;
		public bool isDeadly;
		float angle;
		public Rocket rocket;
		const int pauseTime = 50;
		public int pause;
		Texture2D texture;
		public Explosion explosion;

		public RocketLauncher(Vector2 position)
		{
			isDeadly = true;
			this.hitBox = new Rectangle((int)position.X, (int)position.Y, 32, 32);
			angle = 0;
			rocket = new Rocket(new Vector2(-1000, -1000), 0);
			this.texture = Game1.launcherTex;
			pause = 0;
		}

		public bool Update()
		{
			if (this.hitBox.Intersects(Game1.currentRoom.ViewBox))
			{
				Vector2 direction = new Vector2(Game1.currentRoom.Runner.hitBox.X - hitBox.X, Game1.currentRoom.Runner.hitBox.Y - hitBox.Y);
				angle = (float)Math.Acos(direction.X / direction.Length());
				if (direction.Y < 0)
					angle *= -1;

				if (pause != pauseTime)
					pause++;
				else
				{
					if (Game1.currentRoom.Runner.position.Y <= Game1.currentRoom.roomHeight)
					{
						Game1.rocketLaunch.Play(0.5f, 0f, 0f);
						rocket.position.X = hitBox.X + 8;
						rocket.position.Y = hitBox.Y + 16;
						rocket.angle = angle;
						pause++;
					}
				}
			}
			return rocket.Update();
		}

		public void Draw(SpriteBatch sb, Color drawHue)
		{
			sb.Draw(texture, new Rectangle(hitBox.X + 16 - Game1.currentRoom.ViewBox.X, hitBox.Y + 16 - Game1.currentRoom.ViewBox.Y, hitBox.Width, hitBox.Height), null, drawHue, angle, new Vector2(16, 16), SpriteEffects.None, 1);
			rocket.Draw(sb, drawHue);
		}
	}
}
