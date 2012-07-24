using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class Box
	{
		public bool grabbed, crushed;
		public Rectangle hitBox;
		private Rectangle groundBox;
		public Vector2 position, velocity, acceleration;
		public FloatingPlatform platform;

		private Vector2 startPosition;
		private Texture2D tex;
		private int updateCount = 0;

		public Box(int x, int y)
		{
			grabbed = false;
			crushed = false;
			startPosition = position = new Vector2(x, y);
			acceleration = new Vector2(0, 0.4f);

			velocity = Vector2.Zero;
			hitBox = new Rectangle(x, y, 32, 32);
			groundBox = new Rectangle(x, y + 32, 32, 3);
			tex = Game1.boxTex;
		}
		private void UpdateHitBox()
		{
			hitBox.X = (int)position.X;
			hitBox.Y = (int)position.Y;
			groundBox.X = (int)position.X;
			groundBox.Y = (int)position.Y + 32;
		}

		public void Update()
		{
			updateCount++;
			if (grabbed)
			{
				// Snap to player
				position.X = Game1.currentRoom.Runner.movedLeft ? Game1.currentRoom.Runner.hitBox.Left - 16 : Game1.currentRoom.Runner.hitBox.Right - 16;
				position.Y = (int)Game1.currentRoom.Runner.position.Y + 24;
				UpdateHitBox();
			}
			else
			{
				// Update position and velocity
				velocity += acceleration;
				position += velocity;
				if (platform != null)
					position += platform.velocity;
				UpdateHitBox();

				// Wall collisions
				platform = null;
				foreach (Wall w in Game1.currentRoom.Walls)
				{
					if (w is PlatformWall)
						continue;

					if (w is FloatingPlatform && this.groundBox.Intersects(w.Bounds))
					{
						if (groundBox.Intersects(w.Bounds))
							platform = (FloatingPlatform)w;
					}

					if (this.hitBox.Intersects(w.Bounds))
					{
						List<Vector2> resolutions = new List<Vector2>();

						// Resolve Y
						if (!(this.hitBox.Bottom <= w.Bounds.Top || this.hitBox.Top >= w.Bounds.Bottom))
						{
							if (this.hitBox.Top < w.Bounds.Bottom)
								resolutions.Add(new Vector2(0, w.Bounds.Bottom - this.hitBox.Top));
							if (this.hitBox.Bottom > w.Bounds.Top)
								resolutions.Add(new Vector2(0, w.Bounds.Top - this.hitBox.Bottom));
						}

						// Resolve X
						if (!(this.hitBox.Right <= w.Bounds.Left || this.hitBox.Left >= w.Bounds.Right))
						{
							if (this.hitBox.Right > w.Bounds.Left)
								resolutions.Add(new Vector2(w.Bounds.Left - this.hitBox.Right, 0));
							if (this.hitBox.Left < w.Bounds.Right)
								resolutions.Add(new Vector2(w.Bounds.Right - this.hitBox.Left, 0));
						}

						// Find smallest overlap
						while (resolutions.Count > 1)
						{
							if (resolutions[0].Length() > resolutions[1].Length())
								resolutions.RemoveAt(0);
							else
								resolutions.RemoveAt(1);
						}

						// Set new velocity and position
						Vector2 resV = resolutions[0];
						if (resV.X != 0)
						{
							if (resV.X > 0 && velocity.X < 0 || resV.X < 0 && velocity.X > 0)
							{
								if (w is DeathWall)
									position = startPosition;
								else
									this.velocity.X = 0;
							}
						}
						else
						{
							if (resV.Y > 0 && velocity.Y < 0 || resV.Y < 0 && velocity.Y > 0)
							{
								if (w is DeathWall)
									position = startPosition;
								else
								{
									this.velocity.Y = 0;
									float temp = this.velocity.X;
									this.velocity.X += Math.Sign(velocity.X) * -1 * 0.5f;
									if (Math.Sign(temp) != Math.Sign(velocity.X))
										velocity.X = 0;
								}
							}
						}
						this.position += resV;
						UpdateHitBox();
					}
				}

				// Box collisions
				foreach (Box b in Game1.currentRoom.Boxes)
				{
					if (this == b)
						continue;
					if (this.groundBox.Intersects(b.hitBox))
					{
						this.position.Y = b.position.Y - 32;
						this.position.X += b.velocity.X;
						this.velocity.Y = 0;
						float temp = this.velocity.X;
						this.velocity.X += Math.Sign(velocity.X) * -1 * 0.5f;
						if (Math.Sign(temp) != Math.Sign(velocity.X))
							velocity.X = 0;
						if (b.platform != null && this.platform == null)
							this.platform = b.platform;
						UpdateHitBox();
					}
				}

				// Used to block platforms
				crushed = false;
				foreach (Wall w in Game1.currentRoom.Walls)
				{
					if (!(w is PlatformWall) && this.hitBox.Intersects(w.Bounds))
					{
						crushed = true;
						break;
					}
				}
				foreach (Box b in Game1.currentRoom.Boxes)
				{
					if (this.groundBox.Intersects(b.hitBox))
					{
						crushed = true;
						break;
					}
				}

				// Send back to starting position if fallen off screen
				if (this.position.Y > Game1.currentRoom.roomHeight)
					position = startPosition;

				UpdateHitBox();
			}
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(tex, new Vector2(position.X - Game1.currentRoom.ViewBox.X, position.Y - Game1.currentRoom.ViewBox.Y), c);
		}
	}
}
