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
		public Vector2 pos1, pos2, acceleration;
		Rectangle pole1, pole2;
		float dY, dX, slope, angle;
		Color lineColor;

		Texture2D poleTex, lineTex;

		public ZipLine(Vector2 pos1, Vector2 pos2, Room.Theme theme)
		{
			this.pos1 = pos1;
			this.pos2 = pos2;

			slope = (dY = pos2.Y - pos1.Y) / (dX = pos2.X - pos1.X);
			angle = (float)Math.Atan2(dY, dX);
			while (angle < 0) angle += MathHelper.TwoPi;

			poleTex = Game1.poleTex;
			lineTex = Game1.lineTex;

			if (theme == Room.Theme.Grass || theme == Room.Theme.Factory)
				lineColor = Color.Black;
			else
				lineColor = Color.White;

			acceleration = new Vector2(0.4f * (float)(Math.Sin(angle) * Math.Cos(angle)), 0.4f * (float)(Math.Sin(angle) * Math.Sin(angle)));
		}

		public void SetPoles(Room r)
		{
			pole1 = FindPole(r, pos1);
			pole2 = FindPole(r, pos2);
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
				foreach (Wall w in r.walls)
				{
					if (w.bounds.Contains((int)pos.X + 1, above))
					{
						final = new Rectangle((int)pos.X, above + 16, 16, (int)pos.Y - above + 16);
						hit = true;
					}
					else if (w.bounds.Contains((int)pos.X + 1, below))
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
			float newM = velocity.Length() * (float)Math.Cos(newAngle);
			return new Vector2(newM * (float)Math.Cos(angle), newM * (float)Math.Sin(angle));
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(poleTex, new Rectangle(pole1.X - Game1.currentRoom.viewBox.X, pole1.Y - Game1.currentRoom.viewBox.Y, pole1.Width, pole1.Height), c);
			sb.Draw(poleTex, new Rectangle(pole2.X - Game1.currentRoom.viewBox.X, pole2.Y - Game1.currentRoom.viewBox.Y, pole2.Width, pole2.Height), c);
			sb.Draw(lineTex, new Rectangle((int)pos1.X - Game1.currentRoom.viewBox.X + 8, (int)pos1.Y - Game1.currentRoom.viewBox.Y, (int)Math.Sqrt(dY * dY + dX * dX), 2), null, lineColor == Color.White ? c : lineColor, angle, Vector2.Zero, SpriteEffects.None, 0);
		}
	}
}
