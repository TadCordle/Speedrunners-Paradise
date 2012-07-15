using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class Rocket
	{
		public Rectangle hitBox;
		public bool isDeadly;
		public Vector2 position, velocity;
		public float angle;
		Texture2D texture;
		Texture2D trailTexture;
		public List<TrailPiece> trail;
		public class TrailPiece
		{
			int aliveTime;
			Vector2 position;
			Texture2D texture;

			public TrailPiece(int aliveTime, Vector2 position)
			{
				this.position = position;
				this.aliveTime = aliveTime;
				this.texture = Game1.explosionTex;
			}

			public bool Update()
			{
				aliveTime--;
				if (aliveTime <= 0)
					return true;
				return false;
			}

			public void Draw(SpriteBatch sb, Color drawHue)
			{
				sb.Draw(texture, new Vector2(position.X - Game1.currentRoom.ViewBox.X + 4, position.Y - Game1.currentRoom.ViewBox.Y + 4), drawHue == Color.White ? Color.Orange : Color.DarkOrange);
			}
		}

		public Rocket(Vector2 position, float angle)
		{
			isDeadly = true;
			this.hitBox = new Rectangle((int)position.X + 8, (int)position.Y, 16, 16);
			this.trailTexture = Game1.explosionTex;
			this.position = position;
			this.texture = Game1.rocketTex;
			this.angle = angle;
			velocity = new Vector2((float)Math.Sin(angle) * 8, (float)Math.Cos(angle) * 8);
			trail = new List<TrailPiece>();
			for (int i = 0; i < 10; i++)
				trail.Add(new TrailPiece(10 - i, this.position));
		}

		public bool Update()
		{
			if (hitBox.Intersects(Game1.currentRoom.Runner.hitBox))
			{
				Game1.explosion.Play(0.5f * Settings.soundVol, 0f, 0f);
				return true;
			}

			foreach (Wall w in Game1.currentRoom.Walls)
				if (hitBox.Intersects(w.Bounds))
				{
					Game1.explosion.Play(0.5f * Settings.soundVol, 0f, 0f);
					return true;
				}

			Vector2 direction = new Vector2(Game1.currentRoom.Runner.hitBox.X - position.X, Game1.currentRoom.Runner.hitBox.Y - position.Y);
			float angleTo = (float)Math.Acos(direction.X / direction.Length());
			if (direction.Y < 0)
				angleTo *= -1;

			if (angleTo < 0)
				angleTo += MathHelper.TwoPi;
			else if (angleTo >= MathHelper.TwoPi)
				angleTo -= MathHelper.TwoPi;
			if (angle < 0)
				angle += MathHelper.TwoPi;
			else if (angle >= MathHelper.TwoPi)
				angle -= MathHelper.TwoPi;

			if (angleTo > angle)
				if (angleTo - angle < MathHelper.Pi)
					angle += 0.032f;
				else
					angle -= 0.032f;
			else
				if (angle - angleTo > MathHelper.Pi)
					angle += 0.032f;
				else
					angle -= 0.032f;

			if (position.X > -900 && position.Y > -900)
			{
				velocity = new Vector2((float)Math.Cos(angle) * 7.5f, (float)Math.Sin(angle) * 7.5f);
				position += velocity;
				hitBox.X = (int)position.X + 8;
				hitBox.Y = (int)position.Y;
			}

			for (int i = 0; i < trail.Count; i++)
				if (trail[i].Update())
					trail.RemoveAt(i);
			trail.Insert(0, new TrailPiece(10, position));

			return false;
		}

		public void Draw(SpriteBatch sb, Color drawHue)
		{
			foreach (TrailPiece t in trail)
				t.Draw(sb, drawHue);
			sb.Draw(texture, new Rectangle(hitBox.X - Game1.currentRoom.ViewBox.X, hitBox.Y + 8 - Game1.currentRoom.ViewBox.Y, texture.Width, texture.Height), null, drawHue, angle, new Vector2(8, 8), SpriteEffects.None, 1);
		}
	}
}
