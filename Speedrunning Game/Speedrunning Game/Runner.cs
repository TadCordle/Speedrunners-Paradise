using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class Runner
	{
		AnimatedTexture normal, running, midair, sliding, ziplining;
		AnimatedTexture current;
		public Vector2 position, velocity, acceleration;
		Vector2 groundFriction;
		Rectangle hitBox, groundHitBox, leftWallBox, rightWallBox, ziplineBox;
		float imageAngle;
		bool isTouchingGround, movedLeft, isSliding, staySliding, canWallToRight, canWallToLeft, isZipping;
		public bool controllable;
		bool jumppresscheck = false;
		bool wallpresscheck = false;
		ZipLine zippingLine;

		public Runner(Vector2 position)
		{
			zippingLine = null;

			// Set up animations
			normal = Game1.guyNormal;
			running = Game1.guyRunning;
			midair = Game1.guyMidair;
			sliding = Game1.guySliding;
			ziplining = Game1.guyZiplining;
			movedLeft = true;
			imageAngle = 0.0f;
			current = normal;

			// Set up kinematics
			this.position = position;
			velocity = new Vector2();

			// Set up hit boxes and things that have to do with interaction with other objects
			this.hitBox = new Rectangle((int)position.X + 16, (int)position.Y, 32, 64);
			this.groundHitBox = new Rectangle((int)position.X + 18, (int)position.Y + 64, 28, 2);
			this.leftWallBox = new Rectangle((int)position.X + 14, (int)position.Y + 2, 2, 60);
			this.rightWallBox = new Rectangle((int)position.X + 48, (int)position.Y + 2, 2, 60);
			this.ziplineBox = new Rectangle((int)position.X + 24, (int)position.Y - 16, 8, 32);
			isTouchingGround = false;
			isSliding = false;
			staySliding = false;

			controllable = true; // CHANGE THIS WHEN COUNT DOWN IS IMPLEMENTED
		}

		public void Update()
		{
			// Update animation
			current.Update();

			if (Math.Sign(velocity.X + acceleration.X) != Math.Sign(velocity.X) && velocity.X != 0) // This was to fix some weird moving back and forth bug
			{
				acceleration.X = 0;
				velocity.X = 0;
			}
			else
			{
				// Cap speed
				if (Math.Abs(velocity.X + acceleration.X) < 8.0f || velocity.X * acceleration.X < 0)
					velocity.X += acceleration.X;
			}

			// Apply accel to velocity and velocity to position
			velocity.Y += acceleration.Y;
			position += velocity;
			UpdateHitBox();

			isTouchingGround = false;
			bool isTouchingWall = false;
			foreach (Wall w in Game1.currentRoom.walls)
			{
				// If you're standing on it, apply ground friction and say that you're standing
				if (isSliding)
				{
					if (w.bounds.Intersects(groundHitBox))
					{
						isZipping = false;
						isTouchingGround = true;
					}
					else if (w.bounds.Intersects(leftWallBox))
					{
						canWallToRight = true;
						isTouchingWall = true;
					}
					else if (w.bounds.Intersects(rightWallBox))
					{
						canWallToLeft = true;
						isTouchingWall = true;
					}
				}

				// Apply other wall collisions
				if (this.hitBox.Intersects(w.bounds))
				{
					//--------------------------
					// REPLACE THIS WITH SAT
					//--------------------------
					Vector2 unit = velocity;
					unit.Normalize();
					while (this.hitBox.Intersects(w.bounds))
					{
						this.position -= unit;
						UpdateHitBox();
					}
					//--------------------------
					//--------------------------
					//--------------------------

					// Check for position of wall relative to character and collide accordingly
					if (this.hitBox.Top < w.bounds.Bottom && this.hitBox.Bottom > w.bounds.Top)
					{
						if (this.hitBox.Right <= w.bounds.Left)
						{
							this.position.X = w.bounds.Left - 49;
							velocity.X = 0;
						}
						else
						{
							this.position.X = w.bounds.Right - 15;
							velocity.X = 0;
						}
					}

					if (this.hitBox.Left < w.bounds.Right && this.hitBox.Right > w.bounds.Left)
					{
						if (this.hitBox.Top >= w.bounds.Bottom)
						{
							this.position.Y = w.bounds.Bottom;
							velocity.Y = 0;
						}
						else
						{
							this.position.Y = w.bounds.Top - 64;
							velocity.Y = 0;
						}
					}
					UpdateHitBox();
				}

				// If you're standing on it, apply ground friction and say that you're standing
				if (!isSliding)
				{
					if (w.bounds.Intersects(groundHitBox))
					{
						isZipping = false;
						isTouchingGround = true;
						groundFriction.X = Math.Sign(velocity.X) * -1 * 0.5f;
					}
					else if (w.bounds.Intersects(leftWallBox))
					{
						canWallToRight = true;
						isTouchingWall = true;
					}
					else if (w.bounds.Intersects(rightWallBox))
					{
						canWallToLeft = true;
						isTouchingWall = true;
					}
				}
			}

			foreach (Booster b in Game1.currentRoom.boosters)
				if (b.hitBox.Intersects(this.hitBox))
					velocity += b.acceleration;

			if (Game1.currentRoom.finish != null)
				if (this.hitBox.Intersects(Game1.currentRoom.finish.hitBox))
					Game1.currentRoom.finished = true;
	
			// Remove ground friction if midair
			if (!isTouchingGround)
			{
				current = midair;
				groundFriction = Vector2.Zero;
			}
			else
			{	// Change to normal animation if standing still
				if (velocity.X == 0)
					current = normal;
			}

			// Reset space key
			if (!Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				if (isTouchingGround)
					jumppresscheck = false;
				else if (canWallToLeft || canWallToRight)
					wallpresscheck = false;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				// Jumping
				if (isTouchingGround && !staySliding && !jumppresscheck)
				{
					velocity.Y = -10.0f;
					current = midair;
					isTouchingGround = false;
					jumppresscheck = true;
					wallpresscheck = true;
				}
				// Wall jumping
				else if (!wallpresscheck && !isTouchingGround && isTouchingWall)
				{
					velocity.X = canWallToRight ? 8 : -8;
					movedLeft = !movedLeft;
					if (velocity.Y > -7.5f)
						velocity.Y = -7.5f;
					wallpresscheck = true;
					canWallToRight = false;
					canWallToLeft = false;
				}
			}

			if (zippingLine != null)
			{
				current = ziplining;
				position.Y = zippingLine.GetY(ziplineBox.Center.X);
				acceleration = zippingLine.acceleration * 3;
				velocity = zippingLine.GetNewVelocity(velocity);
				movedLeft = velocity.X < 0;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl))
			{
				bool zipping = false;
				foreach (ZipLine z in Game1.currentRoom.ziplines)
				{
					if ((z.pos1.X < z.pos2.X ? (ziplineBox.Left >= z.pos1.X && ziplineBox.X <= z.pos2.X) : (ziplineBox.Left <= z.pos1.X && ziplineBox.X >= z.pos2.X))
							&& ziplineBox.Contains(ziplineBox.Center.X, (int)z.GetY(ziplineBox.Center.X)))
					{
						zipping = true;
						isZipping = true;
						if (zippingLine == null)
						{
							zippingLine = z;
							current = ziplining;
							position.Y = z.GetY(ziplineBox.Center.X);
							acceleration = z.acceleration * 3;
							velocity = z.GetNewVelocity(velocity);
							movedLeft = velocity.X < 0;
						}
						break;

					}
				}

				if (!zipping)
				{
					zippingLine = null;
					isZipping = false;
					acceleration.Y = 0.4f;
				}
			}
			else
			{
				zippingLine = null;
				isZipping = false;
				acceleration.Y = 0.4f;
			}

			// Slide when holding control
			isSliding = (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl) || staySliding) && isTouchingGround;

			// Don't get up if sliding under low ceiling
			bool flag = true;
			if (isSliding || staySliding)
			{
				foreach (Wall w in Game1.currentRoom.walls)
				{
					if (w.bounds.Left < hitBox.Right && w.bounds.Right > hitBox.Left && w.bounds.Bottom + 32 > hitBox.Top && w.bounds.Top < hitBox.Top)
					{
						staySliding = true;
						flag = false;
						break;
					}
				}
			}
			if (flag)
				staySliding = false;

			// Move right
			if (Keyboard.GetState().IsKeyDown(Keys.Right) && !isSliding && !isZipping)
			{
				canWallToRight = false;
				acceleration.X = isTouchingGround ? 1.0f : 0.5f;
				movedLeft = false;
				if (isTouchingGround)
					current = running;
				if (Keyboard.GetState().IsKeyDown(Keys.Left))
				{
					acceleration.X = 0.0f;
					movedLeft = true;
					current = normal;
				}
			}
			// Move left
			else if (Keyboard.GetState().IsKeyDown(Keys.Left) && !isSliding && !isZipping)
			{
				canWallToLeft = false;
				acceleration.X = isTouchingGround ? -1.0f : -0.5f;
				movedLeft = true;
				if (isTouchingGround)
					current = running;
			}
			// Slow down if no keys are being held
			else
			{
				acceleration.X = 0.0f;
				if (acceleration.X < 0 && velocity.X < 0 && !isSliding && isTouchingGround)
					velocity.X++;
				else if (acceleration.X > 0 && velocity.X > 0 && !isSliding && isTouchingGround)
					velocity.X--;
			}

			// Set sliding animation if sliding
			if (isSliding && isTouchingGround)
			{
				canWallToLeft = false;
				canWallToRight = false;
				acceleration.X = 0.0f;
				groundFriction.X = 0.0f;
				current = sliding;
			}

			// Apply ground friction to horizontal acceleration
			acceleration.X += groundFriction.X;

			// Update midair animation based on vertical speed
			int midairFrame = (int)velocity.Y * 2 + 3;
			if (midairFrame < 0) midairFrame = 0;
			if (midairFrame > 8) midairFrame = 8;
			midair.frame = midairFrame;
		}
		private void UpdateHitBox()
		{
			if (isSliding && isTouchingGround)
			{
				hitBox.Height = 32;
				leftWallBox.Height = 28;
				rightWallBox.Height = 28;
			}
			else
			{
				hitBox.Height = 64;
				leftWallBox.Height = 60;
				rightWallBox.Height = 60;
			}
			ziplineBox.X = (int)position.X + 24;
			ziplineBox.Y = (int)position.Y - 8;
			hitBox.X = (int)position.X + 16;
			hitBox.Y = (int)position.Y;
			groundHitBox.X = (int)position.X + 18;
			groundHitBox.Y = (int)position.Y + 64;
			leftWallBox.X = (int)position.X + 14;
			leftWallBox.Y = (int)position.Y + 2;
			rightWallBox.X = (int)position.X + 48;
			rightWallBox.Y = (int)position.Y + 2;
			if (isSliding && isTouchingGround)
			{
				hitBox.Y += 32;
				ziplineBox.Y += 32;
				leftWallBox.Y += 32;
				rightWallBox.Y += 32;
			}
		}

		public void Draw(SpriteBatch sb)
		{
//			sb.Draw(Game1.wallTex, hitBox, Color.Black);
//			sb.Draw(Game1.wallTex, groundHitBox, Color.White);
//			sb.Draw(Game1.wallTex, leftWallBox, Color.Blue);
//			sb.Draw(Game1.wallTex, rightWallBox, Color.Blue);
//			sb.Draw(Game1.wallTex, ziplineBox, Color.Lime);
			current.Draw(sb, new Vector2(position.X - Game1.currentRoom.viewBox.X, position.Y - Game1.currentRoom.viewBox.Y), Color.White, imageAngle, Vector2.Zero, Vector2.One, (!movedLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 0);
		}
	}
}
