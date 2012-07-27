using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class Flamethrower
	{
		float angle;
		int intervalCount, interval;
		bool on;
		Texture2D texture;
		Vector2 position;
		public List<Flame> flames;
		public Rectangle drawBox;
		Random r;

		public class Flame
		{
			Texture2D texture;
			public Rectangle drawBox;
			public Vector2 position, velocity;
			const int LIFETIME = 22;
			int currentLife;

			public Flame(int x, int y, float angle)
			{
				texture = Game1.explosionTex;
				position = new Vector2(x + 8, y + 8);
				velocity = new Vector2(8 * (float)Math.Cos(angle), 8 * (float)Math.Sin(angle));
				drawBox = new Rectangle((int)position.X, (int)position.Y, 16, 16);
				currentLife = 0;
			}

			public bool Update()
			{
				position += velocity;
				UpdateDrawBox();
				currentLife++;
				return currentLife >= 22;
			}
			private void UpdateDrawBox()
			{
				drawBox.X = (int)position.X;
				drawBox.Y = (int)position.Y;
			}

			public void Draw(SpriteBatch sb)
			{
				Color hue = new Color(255, (22 - currentLife) * 11, 0);
				sb.Draw(texture, drawBox, hue);
			}
		}

		public Flamethrower(int x, int y, float angle, int interval)
		{
			flames = new List<Flame>();
			on = false;
			texture = Game1.flamethrowerTex;
			position = new Vector2(x, y);
			drawBox = new Rectangle((int)position.X, (int)position.Y, 32, 32);
			this.angle = angle;
			this.interval = interval;
			intervalCount = 0;
			r = new Random();
		}

		public void Update()
		{
			intervalCount++;
			if (intervalCount >= interval)
			{
				intervalCount = 0;
				on = !on;
			}

			if (on)
			{
				for (int i = 0; i < 3; i++)
					flames.Add(new Flame((int)position.X, (int)position.Y, angle + (float)(r.NextDouble() * 0.7 - 0.35)));
			}

			Flame[] flamesCopy = new Flame[flames.Count];
			flames.CopyTo(flamesCopy);
			foreach (Flame f in flamesCopy)
			{
				if (f.Update())
					flames.Remove(f);
			}
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.DrawString(Game1.mnufont, on.ToString(), new Vector2(position.X, position.Y - 32), Color.Red);
			sb.Draw(texture, new Vector2(position.X + 16 - Game1.currentRoom.ViewBox.X, position.Y + 16 - Game1.currentRoom.ViewBox.Y), null, c, angle + MathHelper.PiOver2, new Vector2(16, 16), Vector2.One, SpriteEffects.None, 0);
			foreach (Flame f in flames)
				f.Draw(sb);
		}
	}
}
