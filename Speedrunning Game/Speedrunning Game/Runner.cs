using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class Runner
	{
		public Vector2 position, velocity, acceleration; // Kinematics
		public bool controllable; // Only reads key feedback when true
		public bool movedLeft; // To determine which way the character is facing
		public int health; // Tells you when to die

		private AnimatedTexture normal, running, midair, sliding, ziplining, deadGround, deadMidair; // Animations
		private AnimatedTexture current; // Current animation to be drawn
		private Vector2 groundFriction;
		public Rectangle hitBox; // Main hitbox
		private Rectangle groundHitBox, leftWallBox, rightWallBox, ziplineBox; // Other hit boxes
		private float imageAngle; // The angle at which to draw the image. Might need this later
		private bool isTouchingGround, isTouchingWall, isSliding, staySliding, canWallToRight, canWallToLeft, isZipping, 
			jumppresscheck, wallpresscheck; // A bunch of ballers
		private ZipLine zippingLine; // Current zipline being used
		private FloatingPlatform platform; // Current platform standing on
		private float prevPlatSpeed;
		private Box heldBox;
		private int healthTracker; // Health variable and timer used for health regeneration
		private int crushCount; // Used to fix crushing glitch
		
		const int HEALTHINTERVAL = 30; // Health regeneration timer limit

		public Runner(Vector2 position, bool freeroam)
		{
			// Set up animations, like a baller
			normal = Game1.guyNormal;
			running = Game1.guyRunning;
			midair = Game1.guyMidair;
			sliding = Game1.guySliding;
			ziplining = Game1.guyZiplining;
			deadGround = Game1.guyDeadGround;
			deadMidair = Game1.guyDeadMidair;
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
			controllable = !freeroam;
			jumppresscheck = false;
			wallpresscheck = false;
			prevPlatSpeed = 0;

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
			if (health < 10 && health > 0)
			{
				healthTracker++;
				if (healthTracker >= HEALTHINTERVAL)
				{
					healthTracker = 0;
					health++;
				}
			}
			else
				healthTracker = 0;

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
			{
				// Fix retarded platform switching direction and making you spazz out glitch
				if (Math.Sign(platform.velocity.Y) != Math.Sign(prevPlatSpeed) && prevPlatSpeed != 0)
					position.Y += 2;

				position += platform.velocity;
				prevPlatSpeed = platform.velocity.Y;
			}
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
			isTouchingWall = false;
			bool isOnPlatform = false;

			// Iterate through walls
			foreach (Wall w in Game1.currentRoom.Walls)
			{
				// Make sure wall effects player
				if (w is PlatformWall || !w.Bounds.Intersects(Game1.currentRoom.ViewBox))
					continue;

				// If you're standing on it, apply ground friction and say that you're standing
				if (isSliding && !(w is DeathWall))
				{
					if (w.Bounds.Intersects(groundHitBox))
					{
						isZipping = false;
						isTouchingGround = true;
						if (w is FloatingPlatform)
						{
							if (platform == null)
							{
								platform = (FloatingPlatform)w;
								if (!Keyboard.GetState().IsKeyDown(Settings.controls["MoveLeft"]) && !Keyboard.GetState().IsKeyDown(Settings.controls["MoveRight"]))
									velocity.X = 0;
							}
							isOnPlatform = true;
						}
					}
					else if (w.Bounds.Intersects(leftWallBox) && !(w is Mirror))
					{
						canWallToRight = true;
						isTouchingWall = true;
					}
					else if (w.Bounds.Intersects(rightWallBox) && !(w is Mirror))
					{
						canWallToLeft = true;
						isTouchingWall = true;
					}
				}

				// Apply other wall collisions
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
							if (w is DeathWall && health > 0)
							{
								Game1.damage.Play(0.5f * Settings.soundVol, 0f, 0f);
								this.velocity.X = Math.Sign(velocity.X) * -6;
								health -= 9;
							}
							else
								this.velocity.X = 0;
						}
					}
					else
					{
						if (resV.Y > 0 && velocity.Y < 0 || resV.Y < 0 && velocity.Y > 0)
						{
							if (w is DeathWall && health > 0)
							{
								Game1.damage.Play(0.5f * Settings.soundVol, 0f, 0f);
								this.velocity.Y = Math.Sign(velocity.Y) * -6;
								health -= 9;
							}
							else
							{
								if (Math.Abs(this.velocity.Y) >= 3)
									Game1.collide.Play(0.3f * Settings.soundVol, 0f, 0f);
								this.velocity.Y = 0;
							}
						}
					}
					this.position += resV;
					UpdateHitBox();
				}

				// If you're standing on it, apply ground friction and say that you're standing
				if (!isSliding)
				{
					if (w.Bounds.Intersects(groundHitBox))
					{
						isZipping = false;
						isTouchingGround = true;
						if (w is FloatingPlatform)
						{
							if (platform == null)
							{
								platform = (FloatingPlatform)w;
								if (!Keyboard.GetState().IsKeyDown(Settings.controls["MoveLeft"]) && !Keyboard.GetState().IsKeyDown(Settings.controls["MoveRight"]))
									velocity.X = 0;
							}
							isOnPlatform = true;
						}
						groundFriction.X = Math.Sign(velocity.X) * -1 * 0.5f;
					}
					else if (w.Bounds.Intersects(leftWallBox) && !(w is Mirror))
					{
						canWallToRight = true;
						isTouchingWall = true;
					}
					else if (w.Bounds.Intersects(rightWallBox) && !(w is Mirror))
					{
						canWallToLeft = true;
						isTouchingWall = true;
					}
				}
			}

			// Land on boxes
			if (velocity.Y > 0 && !isTouchingGround)
			{
				foreach (Box b in Game1.currentRoom.Boxes)
				{
					if (groundHitBox.Intersects(b.hitBox))
					{
						position.Y = b.position.Y - 64;
						velocity.Y = 0;
						if (!isSliding)
							groundFriction.X = Math.Sign(velocity.X) * -1 * 0.5f;
						else
							groundFriction.X = 0;
						isTouchingGround = true;
					}
				}
			}

			// Check for crushing
			bool iscrushed = false;
			foreach (Wall w in Game1.currentRoom.Walls)
			{
				if (!(w is PlatformWall) && this.hitBox.Intersects(w.Bounds))
				{
					if (!(isSliding || staySliding))
					{
						staySliding = true;
						break;
					}
					crushCount++;
					if (crushCount >= 3)
						health = 0;
					iscrushed = true;
					break;
				}
			}
			if (!iscrushed)
				crushCount = 0;

			if (health <= 0)
				controllable = false;

			// Apply platform velocity when leaving platform
			if (!isOnPlatform && platform != null)
			{
				velocity += platform.velocity;
				platform = null;
			}

			// Apply booster acceleration
			foreach (Booster b in Game1.currentRoom.Boosters)
				if (b.HitBox.Intersects(this.hitBox))
				{
					Game1.boost.Play(0.5f * Settings.soundVol, 0f, 0f);
					velocity += b.Acceleration;
				}

			// Check for rocket collision
			foreach (RocketLauncher r in Game1.currentRoom.Launchers)
				if (r.rocket.hitBox.Intersects(this.hitBox))
				{
					this.health -= 6;
					this.velocity += r.rocket.velocity * 1.5f;
				}

			// Check if level finish reached
			if (Game1.currentRoom.Finish != null && !Game1.currentRoom.Finished)
				if (this.hitBox.Intersects(Game1.currentRoom.Finish.HitBox))
				{
					Game1.finish.Play(Settings.soundVol, 0f, 0f);
					Game1.currentRoom.Finished = true;
				}

			// Remove ground friction if midair
			if (!isTouchingGround)
			{
				Game1.run.Stop();
				if (health <= 0)
					current = deadMidair;
				else
					current = midair;
				groundFriction = Vector2.Zero;
			}
			else
			{	// Change to normal animation if standing still
				if (health <= 0)
					current = deadGround;
				else if (velocity.X == 0)
					current = normal;
			}

			// Reset space key
			if (!Keyboard.GetState().IsKeyDown(Settings.controls["Jump"]))
			{
				if (heldBox != null)
					jumppresscheck = false;
				if (isTouchingGround)
					jumppresscheck = false;
				else if (canWallToLeft || canWallToRight)
					wallpresscheck = false;
			}
			if (Keyboard.GetState().IsKeyDown(Settings.controls["Jump"]) && controllable)
			{
				// Jumping
				if (isTouchingGround && !staySliding && !jumppresscheck)
				{
					Game1.jump.Play(Settings.soundVol, 0f, 0f);
					velocity.Y = -10.0f;
					current = midair;
					isTouchingGround = false;
					jumppresscheck = true;
					wallpresscheck = true;
				}
				// Wall jumping
				else if (!wallpresscheck && !isTouchingGround && isTouchingWall)
				{
					Game1.jump.Play(Settings.soundVol, 0f, 0f);
					velocity.X = canWallToRight ? 8 : -8;
					movedLeft = !movedLeft;
					if (velocity.Y > -7.5f)
						velocity.Y = -7.5f;
					wallpresscheck = true;
					jumppresscheck = true;
					canWallToRight = false;
					canWallToLeft = false;
				}
				// Box jumping
				else if (heldBox != null && !isTouchingGround && !jumppresscheck)
				{
					Game1.jump.Play(Settings.soundVol, 0f, 0f);
					velocity.Y = -10.0f;
					current = midair;
					jumppresscheck = true;
					wallpresscheck = true;
					heldBox.velocity.Y = 4;
					heldBox.velocity.X = this.velocity.X;
					heldBox.grabbed = false;
					heldBox = null;
				}
			}

			// If ziplining, update according to zipline's values
			if (zippingLine != null)
			{
				current = ziplining;
				position.Y = zippingLine.GetY(ziplineBox.Center.X);
				acceleration = zippingLine.Acceleration * (Math.Abs(zippingLine.Slope) > 1.5f ? 1 : 3);
				velocity = zippingLine.GetNewVelocity(velocity);
				movedLeft = velocity.X < 0;
			}

			// Check for whether or not ziplining if control is being held
			if ((Keyboard.GetState().IsKeyDown(Settings.controls["Slide"])) && controllable && !isTouchingGround)
			{
				bool zipping = false;
				foreach (ZipLine z in Game1.currentRoom.ZipLines)
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
							acceleration = z.Acceleration * 3;
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

			// Check for picking up boxes
			if (Keyboard.GetState().IsKeyDown(Settings.controls["Box"]))
			{
				if (isTouchingGround && heldBox == null)
					foreach (Box b in Game1.currentRoom.Boxes)
					{
						if (b.hitBox.Intersects(rightWallBox) && !movedLeft || b.hitBox.Intersects(leftWallBox) && movedLeft)
						{
							b.grabbed = true;
							heldBox = b;
							break;
						}
					}
			}
			else
			{
				if (heldBox != null)
				{
					heldBox.grabbed = false;
					heldBox.velocity = this.velocity;
					heldBox = null;
				}
			}

			// Slide when holding control
			isSliding = (Keyboard.GetState().IsKeyDown(Settings.controls["Slide"]) || staySliding) && controllable && isTouchingGround;

			// Don't get up if sliding under low ceiling
			bool flag = true;
			if (isSliding || staySliding)
			{
				foreach (Wall w in Game1.currentRoom.Walls)
				{
					if (w.Bounds.Left < hitBox.Right && w.Bounds.Right > hitBox.Left && w.Bounds.Bottom + 32 > hitBox.Top && w.Bounds.Top < hitBox.Top)
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
			if (Keyboard.GetState().IsKeyDown(Settings.controls["MoveRight"]) && !isSliding && !isZipping && controllable)
			{
				canWallToRight = false;
				acceleration.X = isTouchingGround ? 1.0f : 0.5f;
				movedLeft = false;
				if (isTouchingGround)
					current = running;
				if (Keyboard.GetState().IsKeyDown(Settings.controls["MoveLeft"]))
				{
					acceleration.X = 0.0f;
					movedLeft = true;
					current = normal;
				}
			}
			// Move left
			else if (Keyboard.GetState().IsKeyDown(Settings.controls["MoveLeft"]) && !isSliding && !isZipping && controllable)
			{
				canWallToLeft = false;
				acceleration.X = isTouchingGround ? -1.0f : -0.5f;
				movedLeft = true;
				if (isTouchingGround)
					current = running;
			}
			// Slow down if no keys are being held
			else
				acceleration.X = 0.0f;

			if (Math.Abs(velocity.X) > 8.0f && !isSliding && isTouchingGround)
			{
				if (acceleration.X < 0 && velocity.X < 0)
					velocity.X++;
				else if (acceleration.X > 0 && velocity.X > 0)
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
			midair.Frame = midairFrame;

			// Play sounds
			if (current == running)
			{
				Game1.slide.Stop();
				if (Game1.run.State != SoundState.Playing)
					Game1.run.Play();
			}
			else if (current == ziplining || current == sliding)
			{
				Game1.run.Stop();
				if (Game1.slide.State != SoundState.Playing && velocity.X != 0)
					Game1.slide.Play();
			}
			else
			{
				Game1.run.Stop();
				Game1.slide.Stop();
			}
		}
		private void UpdateHitBox()
		{
			// Move each hitbox to its corresponding spot relative to the position vector
			if (isSliding && isTouchingGround || health <= 0)
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
			ziplineBox.X = (int)position.X + 28;
			ziplineBox.Y = (int)position.Y - 16;
			hitBox.X = (int)position.X + 16;
			hitBox.Y = (int)position.Y;
			groundHitBox.X = (int)position.X + 18;
			groundHitBox.Y = (int)position.Y + 64;
			leftWallBox.X = (int)position.X + 14;
			leftWallBox.Y = (int)position.Y + 2;
			rightWallBox.X = (int)position.X + 48;
			rightWallBox.Y = (int)position.Y + 2;
			if (isSliding && isTouchingGround || health <= 0)
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
//			sb.Draw(Game1.wallTex, new Rectangle(hitBox.X - Game1.currentRoom.ViewBox.X, hitBox.Y - Game1.currentRoom.ViewBox.Y, hitBox.Width, hitBox.Height), Color.Black);
//			sb.Draw(Game1.wallTex, new Rectangle(groundHitBox.X - Game1.currentRoom.ViewBox.X, groundHitBox.Y - Game1.currentRoom.ViewBox.Y, groundHitBox.Width, groundHitBox.Height), Color.White);
//			sb.Draw(Game1.wallTex, new Rectangle(leftWallBox.X - Game1.currentRoom.ViewBox.X, leftWallBox.Y - Game1.currentRoom.ViewBox.Y, leftWallBox.Width, leftWallBox.Height), Color.Blue);
//			sb.Draw(Game1.wallTex, new Rectangle(rightWallBox.X - Game1.currentRoom.ViewBox.X, rightWallBox.Y - Game1.currentRoom.ViewBox.Y, rightWallBox.Width, rightWallBox.Height), Color.Blue);
//			sb.Draw(Game1.wallTex, new Rectangle(ziplineBox.X - Game1.currentRoom.ViewBox.X, ziplineBox.Y - Game1.currentRoom.ViewBox.Y, ziplineBox.Width, ziplineBox.Height), Color.Lime);
			
			// Draw health numbers (for debugging)
//			sb.DrawString(Game1.mnufont, health.ToString(), new Vector2(0, 32), Color.White);
//			sb.DrawString(Game1.mnufont, healthTracker.ToString(), new Vector2(0, 64), Color.Blue);

			// Draw other values
//			sb.DrawString(Game1.mnufont, isTouchingGround.ToString(), new Vector2(0, 32), Color.Lime);
//			sb.DrawString(Game1.mnufont, wallpresscheck.ToString(), new Vector2(0, 64), Color.Lime);
//			sb.DrawString(Game1.mnufont, isTouchingWall.ToString(), new Vector2(0, 96), Color.Lime);
//			sb.DrawString(Game1.mnufont, canWallToLeft.ToString(), new Vector2(0, 128), Color.Lime);
//			sb.DrawString(Game1.mnufont, canWallToRight.ToString(), new Vector2(0, 160), Color.Lime);

			// Adjust player color based on health
			if (health > 0)
			{
				c.B -= (byte)((10 - health) * 25.5);
				c.G -= (byte)((10 - health) * 25.5);
			}
			
			// Draw character
			current.Draw(sb, new Vector2(position.X - Game1.currentRoom.ViewBox.X, position.Y - Game1.currentRoom.ViewBox.Y), c, imageAngle, Vector2.Zero, Vector2.One, (!movedLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 0);
		}
	}
}
