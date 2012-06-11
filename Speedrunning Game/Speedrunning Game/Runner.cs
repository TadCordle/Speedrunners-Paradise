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
		AnimatedTexture normal, running, midair, sliding, ziplining; // Animations
		AnimatedTexture current; // Current animation to be drawn
		public Vector2 position, velocity, acceleration; // Kinematics
		Vector2 groundFriction;
		Rectangle hitBox, groundHitBox, leftWallBox, rightWallBox, ziplineBox; // Hit boxes
		float imageAngle; // The angle at which to draw the image. Might need this later
		bool isTouchingGround, movedLeft, isSliding, staySliding, canWallToRight, canWallToLeft, isZipping, 
			jumppresscheck, wallpresscheck; // A bunch of flags
		public bool controllable; // Only reads key feedback when true
		ZipLine zippingLine; // Current zipline being used
		FloatingPlatform platform; // Current platform standing on
		int health, healthTracker; // Health variable and timer used for health regeneration
		const int HEALTHINTERVAL = 2000; // Healt regeneration timer limit

		public Runner(Vector2 position)
		{
			// Set up animations
			normal = Game1.guyNormal;
			running = Game1.guyRunning;
			midair = Game1.guyMidair;
			sliding = Game1.guySliding;
			ziplining = Game1.guyZiplining;
			movedLeft = true;
			imageAngle = 0.0f;
			current = normal;

			// Set up character physics
			this.position = position;
			velocity = new Vector2();
			isTouchingGround = false;
			isSliding = false;
			isZipping = false;
			staySliding = false;
			controllable = true; // CHANGE THIS WHEN COUNT DOWN IS IMPLEMENTED
			jumppresscheck = false;
			wallpresscheck = false;

			// Set up hit boxes
			this.hitBox = new Rectangle((int)position.X + 16, (int)position.Y, 32, 64);
			this.groundHitBox = new Rectangle((int)position.X + 18, (int)position.Y + 64, 28, 2);
			this.leftWallBox = new Rectangle((int)position.X + 14, (int)position.Y + 2, 2, 60);
			this.rightWallBox = new Rectangle((int)position.X + 48, (int)position.Y + 2, 2, 60);
			this.ziplineBox = new Rectangle((int)position.X + 24, (int)position.Y - 16, 8, 32);

			// Set up other variables
			health = 10;
			healthTracker = 0;
		}

		public void Update()
		{
			// Update animation
			current.Update();

			// Update health regen
			if (health < 10)
			{
				healthTracker++;
				if (healthTracker >= HEALTHINTERVAL)
				{
					healthTracker = 0;
					health++;
				}
			}

			// If something is slowing you down, stop it from speeding you up in the opposite direction
			if (Math.Sign(velocity.X + acceleration.X) != Math.Sign(velocity.X) && velocity.X != 0)
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
			if (platform != null)
				position += platform.velocity;
			UpdateHitBox();

			// Stay inside screen
			if (hitBox.Left < 0)
			{
				position.X = -16;
				UpdateHitBox();
			}
			else if (hitBox.Right > Game1.currentRoom.roomWidth)
			{
				position.X = Game1.currentRoom.roomWidth - 48;
				UpdateHitBox();
			}

			// Die if character falls below room bounds
			if (hitBox.Top > Game1.currentRoom.roomHeight)
				health = 0;

			// Reset flags
			isTouchingGround = false;
			bool isTouchingWall = false;
			bool isOnPlatform = false;

			// Iterate through walls
			foreach (Wall w in Game1.currentRoom.walls)
			{
				// Make sure wall effects player
				if (w is PlatformWall || !w.bounds.Intersects(Game1.currentRoom.viewBox))
					continue;

				// If you're standing on it, apply ground friction and say that you're standing
				if (isSliding)
				{
					if (w.bounds.Intersects(groundHitBox))
					{
						isZipping = false;
						isTouchingGround = true;
						if (w is FloatingPlatform )
						{
							if (platform == null)
								platform = (FloatingPlatform)w;
							isOnPlatform = true;
						}
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
				if (this.hitBox.Intersects(w.bounds) && !(w is PlatformWall))
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
						if (w is FloatingPlatform)
						{
							if (platform == null)
								platform = (FloatingPlatform)w;
							isOnPlatform = true;
						}
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

			// Apply platform velocity when leaving platform
			if (!isOnPlatform && platform != null)
			{
				velocity += platform.velocity;
				platform = null;
			}

			// Apply booster acceleration
			foreach (Booster b in Game1.currentRoom.boosters)
				if (b.hitBox.Intersects(this.hitBox))
					velocity += b.acceleration;

			// Check if level finish reached
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

			// If ziplining, update according to zipline's values
			if (zippingLine != null)
			{
				current = ziplining;
				position.Y = zippingLine.GetY(ziplineBox.Center.X);
				acceleration = zippingLine.acceleration * 3;
				velocity = zippingLine.GetNewVelocity(velocity);
				movedLeft = velocity.X < 0;
			}

			// Check for whether or not ziplining if control is being held
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

				// If not hitting a zipline, turn off zipling variables
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
			// Move each hitbox to its corresponding spot relative to the position vector
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

		public void Draw(SpriteBatch sb, Color c)
		{
			// Draw hit boxes (for debugging)
//			sb.Draw(Game1.wallTex, hitBox, Color.Black);
//			sb.Draw(Game1.wallTex, groundHitBox, Color.White);
//			sb.Draw(Game1.wallTex, leftWallBox, Color.Blue);
//			sb.Draw(Game1.wallTex, rightWallBox, Color.Blue);
//			sb.Draw(Game1.wallTex, ziplineBox, Color.Lime);

			// Draw character
			current.Draw(sb, new Vector2(position.X - Game1.currentRoom.viewBox.X, position.Y - Game1.currentRoom.viewBox.Y), c, imageAngle, Vector2.Zero, Vector2.One, (!movedLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 0);
		}
	}
}
