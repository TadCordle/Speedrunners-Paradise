using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class ZipLine
	{
		public Vector2 pos1, pos2;
		private Vector2 acceleration;
		public Vector2 Acceleration { get { return acceleration; } }
		public Rectangle DrawBox { get; set; }
		public float Slope { get { return slope; } }

		private Rectangle pole1, pole2;
		private float dY, dX, slope, angle;
		private Color lineColor;

		Texture2D poleTex, lineTex;

		public ZipLine(Vector2 pos1, Vector2 pos2, Room.LevelTheme theme)
		{
			this.pos1 = pos1;
			this.pos2 = pos2;

			slope = (dY = pos2.Y - pos1.Y) / (dX = pos2.X - pos1.X);
			angle = (float)Math.Atan2(dY, dX);
			while (angle < 0) angle += MathHelper.TwoPi;

			poleTex = Game1.poleTex;
			lineTex = Game1.lineTex;

			if (theme == Room.LevelTheme.Grass || theme == Room.LevelTheme.Factory)
				lineColor = Color.Black;
			else
				lineColor = Color.White;

			acceleration = new Vector2(0.4f * (float)(Math.Sin(angle) * Math.Cos(angle)), 0.4f * (float)(Math.Sin(angle) * Math.Sin(angle)));
		}

		public void SetPoles(Room r)
		{
			pole1 = FindPole(r, pos1);
			pole2 = FindPole(r, pos2);

			if (pole1.X < pole2.X)
				DrawBox = new Rectangle(pole1.X, Math.Min(pole1.Y, pole2.Y), pole2.X + 16 - pole1.X, Math.Max(pole1.Bottom, pole2.Bottom) - Math.Min(pole1.Top, pole2.Top));
			else
				DrawBox = new Rectangle(pole2.X, Math.Min(pole1.Y, pole2.Y), pole1.X + 16 - pole2.X, Math.Max(pole1.Height, pole2.Height));
		}

		private Rectangle FindPole(Room r, Vector2 pos)
		{
			int above, below;
			above = below = (int)pos.Y;

			above -= 16;
			below += 48;

			Rectangle final = new Rectangle();
			bool hit = false;
			while (!hit && (above > 0 || below < r.roomHeight))
			{
				foreach (Wall w in r.Walls)
				{
					if (w is PlatformWall || w is FloatingPlatform)
						continue;

					if (w.Bounds.Contains((int)pos.X + 1, above))
					{
						final = new Rectangle((int)pos.X, above + 16, 16, (int)pos.Y - above + 16);
						hit = true;
					}
					else if (w.Bounds.Contains((int)pos.X + 1, below))
					{
						final = new Rectangle((int)pos.X, (int)pos.Y, 16, below - (int)pos.Y - 16);
						hit = true;
					}
				}
				above -= 32;
				below += 32;
			}

			if (!hit)
				final = new Rectangle((int)pos.X, (int)pos.Y, 16, below - (int)pos.Y);

			return final;
		}

		public float GetY(float x)
		{
			return pos1.Y + slope * (x - pos1.X);
		}

		public Vector2 GetNewVelocity(Vector2 velocity)
		{
			float newAngle = (float)Math.Atan2(velocity.Y, velocity.X) - angle;
			float newM = velocity.Length() *(float)Math.Cos(newAngle);
			return new Vector2(newM * (float)Math.Cos(angle), newM * (float)Math.Sin(angle));
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(poleTex, new Rectangle(pole1.X - Game1.currentRoom.ViewBox.X, pole1.Y - Game1.currentRoom.ViewBox.Y, pole1.Width, pole1.Height), c);
			sb.Draw(poleTex, new Rectangle(pole2.X - Game1.currentRoom.ViewBox.X, pole2.Y - Game1.currentRoom.ViewBox.Y, pole2.Width, pole2.Height), c);
			sb.Draw(lineTex, new Rectangle((int)pos1.X - Game1.currentRoom.ViewBox.X + 8, (int)pos1.Y - Game1.currentRoom.ViewBox.Y, (int)Math.Sqrt(dY * dY + dX * dX), 2), null, lineColor == Color.White ? c : lineColor, angle, Vector2.Zero, SpriteEffects.None, 0);
		}
	}
}
