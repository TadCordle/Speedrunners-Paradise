using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class Room
	{
		public const int VIEWSIZE_X = 960;
		public const int VIEWSIZE_Y = 720;

		private Rectangle viewBox;
		public Rectangle ViewBox 
		{ 
			get { return viewBox; } 
			set { viewBox = value; } 
		}

		public int roomWidth { get; set; }
		public int roomHeight { get; set; }

		private List<Wall> walls;
		public List<Wall> Walls { get { return walls; } }

		private List<ZipLine> ziplines;
		public List<ZipLine> ZipLines { get { return ziplines; } }
		
		private List<Booster> boosters;
		public List<Booster> Boosters { get { return boosters; } }

		private List<FloatingPlatform> platforms;
		public List<FloatingPlatform> Platforms { get { return platforms; } }

		private List<Message> messages;
		public List<Message> Messages { get { return messages; } }

		private List<Box> boxes;
		public List<Box> Boxes { get { return boxes; } }

		private List<RocketLauncher> launchers;
		public List<RocketLauncher> Launchers { get { return launchers; } }

		public Runner Runner { get; set; }
		public Finish Finish { get; set; }
		public LevelTheme Theme { get; set; }
		public bool Finished { get; set; }
		public bool Paused { get; set; }

		private HashSet<Vector3> tiles; // x, y, z = index
		private int[] goals; // 0 = gold, 1 = silver, 2 = bronze
		private Tileset wallSet;
		private string levelName;
		private int time, record, goalBeaten;
		private bool custom, write, pcheck, rcheck, fcheck, freeroaming;

		public Room()
		{
			goalBeaten = 0;
			pcheck = true;
			rcheck = false;
			write = true;
			time = 0;
			tiles = new HashSet<Vector3>();
			Finished = false;
			goals = new int[3];
			walls = new List<Wall>();
			ziplines = new List<ZipLine>();
			boosters = new List<Booster>();
			platforms = new List<FloatingPlatform>();
			messages = new List<Message>();
			boxes = new List<Box>();
			launchers = new List<RocketLauncher>();
			ViewBox = new Rectangle(0, 0, VIEWSIZE_X, VIEWSIZE_Y);
		}

		public Room(string file, bool freeroam)
			: this()
		{
			freeroaming = freeroam;
			
			// Get level name
			levelName = file.Split('\\')[file.Split('\\').Length - 1].Replace(".srl", "");
			custom = true;

			SimpleAES decryptor = new SimpleAES();
			StreamReader levelReader = new StreamReader(file);
			string[] line;

			// Get level theme
			line = decryptor.DecryptString(levelReader.ReadLine()).Split(' ');
			Theme = FindTheme(line[0]);
			wallSet = new Tileset(Game1.tileSet[(int)Theme], 32, 32, 3, 3);
			
			// Get room dimensions
			line = decryptor.DecryptString(levelReader.ReadLine()).Split(' ');
			roomWidth = int.Parse(line[0]);
			roomHeight = int.Parse(line[1]);
			
			// Get goal times
			line = decryptor.DecryptString(levelReader.ReadLine()).Split(' ');
			for (int i = 0; i < 3; i++)
				goals[i] = int.Parse(line[i]);

			// Get objects and tiles
			while (!levelReader.EndOfStream)
			{
				line = decryptor.DecryptString(levelReader.ReadLine()).Split(' ');
				ParseObjectOrTile(line, freeroam);
			}
			levelReader.Dispose();

			BuildTiles();

			// Generate zipline poles
			foreach (ZipLine z in ziplines)
				z.SetPoles(this);

			// Get current record for this level
			FindRecord(decryptor);

			UpdateViewBox(false);
		}

		public Room(string[] lines, bool freeroam) 
			: this()
		{
			freeroaming = freeroam;
			
			// Get level name
			levelName = ".MAIN.Level" + (Levels.Index + 1).ToString();
			custom = false;

			SimpleAES decryptor = new SimpleAES();
			string[] line;

			// Get level theme
			line = decryptor.DecryptString(lines[0]).Split(' ');
			Theme = FindTheme(line[0]);
			wallSet = new Tileset(Game1.tileSet[(int)Theme], 32, 32, 3, 3);

			// Get room dimensions
			line = decryptor.DecryptString(lines[1]).Split(' ');
			roomWidth = int.Parse(line[0]);
			roomHeight = int.Parse(line[1]);

			// Get goal times
			line = decryptor.DecryptString(lines[2]).Split(' ');
			for (int i = 0; i < 3; i++)
				goals[i] = int.Parse(line[i]);

			// Get objects and tiles
			int index = 3;
			while (index < lines.Length)
			{
				line = decryptor.DecryptString(lines[index]).Split(' ');
				ParseObjectOrTile(line, freeroam);
				index++;
			}
			BuildTiles();

			// Generate zipline poles
			foreach (ZipLine z in ziplines)
				z.SetPoles(this);

			// Check to see if level is already in records file. If not, add it.
			// This unlocks the level in level select, since main levels (levels hard coded into the game)
			// are added from the records file.
			bool recordFound = false;
			StreamReader reader = new StreamReader("Content\\records.txt");
			while (!reader.EndOfStream)
			{
				string s = decryptor.DecryptString(reader.ReadLine());
				if (s.Split(' ')[0] == levelName)
				{
					record = int.Parse(s.Split(' ')[1]);
					recordFound = true;
					break;
				}
			}
			reader.Close();
			reader.Dispose();
			if (!recordFound)
			{
				record = -1;
				StreamWriter writer = new StreamWriter("Content\\records.txt", true);
				writer.WriteLine(decryptor.EncryptToString(levelName + " -1"));
				writer.Flush();
				writer.Dispose();
			}

			UpdateViewBox(false);
		}

		// Parses a theme from a line of text
		private LevelTheme FindTheme(string t)
		{
			switch (t)
			{
				case "Grass":
					if (!Game1.playingGrass)
						MediaPlayer.Play(Game1.grassMusic);
					Game1.ResetMusic();
					Game1.playingGrass = true;
					return LevelTheme.Grass;
				case "Lava":
					if (!Game1.playingLava)
						MediaPlayer.Play(Game1.lavaMusic);
					Game1.ResetMusic();
					Game1.playingLava = true;
					return LevelTheme.Lava;
				case "Night":
					if (!Game1.playingNight)
						MediaPlayer.Play(Game1.nightMusic);
					Game1.ResetMusic();
					Game1.playingNight = true;
					return LevelTheme.Night;
				case "Cave":
					if (!Game1.playingCave)
						MediaPlayer.Play(Game1.caveMusic);
					Game1.ResetMusic();
					Game1.playingCave = true;
					return LevelTheme.Cave;
				case "Factory":
					if (!Game1.playingFactory)
						MediaPlayer.Play(Game1.factoryMusic);
					Game1.ResetMusic();
					Game1.playingFactory = true;
					return LevelTheme.Factory;
				default:
					if (!Game1.playingGrass)
						MediaPlayer.Play(Game1.grassMusic);
					Game1.ResetMusic();
					Game1.playingGrass = true;
					return LevelTheme.Grass;
			}
		}

		// Instantiates an object from a line of text and adds it to the appropriate list
		private void ParseObjectOrTile(string[] line, bool freeroam)
		{
			if (line[0] == "runner")
			{
				Runner = new Runner(new Vector2(int.Parse(line[1]), int.Parse(line[2])), freeroam);
				viewBox.X = (int)Runner.position.X - VIEWSIZE_X / 2 - 32;
				viewBox.Y = (int)Runner.position.Y - VIEWSIZE_Y / 2 - 32;
				if (ViewBox.X < 0) viewBox.X = 0;
				if (ViewBox.Y < 0) viewBox.Y = 0;
			}
			else if (line[0] == "wall")
				walls.Add(new Wall(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
			else if (line[0] == "finish")
				Finish = new Finish(new Vector2(int.Parse(line[1]), int.Parse(line[2])));
			else if (line[0] == "zipline")
				ziplines.Add(new ZipLine(new Vector2(int.Parse(line[1]), int.Parse(line[2])), new Vector2(int.Parse(line[3]), int.Parse(line[4])), Theme));
			else if (line[0] == "booster")
				boosters.Add(new Booster(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
			else if (line[0] == "floatingplatform")
				walls.Add(new FloatingPlatform(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
			else if (line[0] == "platformwall")
				walls.Add(new PlatformWall(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
			else if (line[0] == "message")
				messages.Add(new Message(new Vector2(int.Parse(line[1]), int.Parse(line[2])), line[3].Replace('_', ' ').Replace("\\n", "\n")));
			else if (line[0] == "deathwall")
				walls.Add(new DeathWall(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
			else if (line[0] == "box")
				boxes.Add(new Box(int.Parse(line[1]), int.Parse(line[2])));
			else if (line[0] == "mirror")
				walls.Add(new Mirror(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
			else if (line[0] == "launcher")
				launchers.Add(new RocketLauncher(new Vector2(int.Parse(line[1]), int.Parse(line[2]))));
		}

		private void BuildTiles()
		{
			tiles.Clear();

			// Fill each wall object with middle tiles
			var realWalls = from Wall w in walls
							where !(w is PlatformWall) && !(w is FloatingPlatform) && !(w is DeathWall) && !(w is Mirror)
							select w;
			foreach (Wall w in realWalls)
			{
				for (int x = w.Bounds.X; x < w.Bounds.Right; x += 32)
					for (int y = w.Bounds.Y; y < w.Bounds.Bottom; y += 32)
						tiles.Add(new Vector3(x, y, 4));
			}
			
			// Fill each death wall with death tile
			var deathWalls = from Wall w in walls
							 where w is DeathWall
							 select w;
			foreach (Wall w in deathWalls)
			{
				for (int x = w.Bounds.X; x < w.Bounds.Right; x += 32)
					for (int y = w.Bounds.Y; y < w.Bounds.Bottom; y += 32)
						tiles.Add(new Vector3(x, y, 9));
			}

			// Fill each mirror with mirror tile
			var mirrors = from Wall w in walls
							 where w is Mirror
							 select w;
			foreach (Wall w in mirrors)
			{
				for (int x = w.Bounds.X; x < w.Bounds.Right; x += 32)
					for (int y = w.Bounds.Y; y < w.Bounds.Bottom; y += 32)
						tiles.Add(new Vector3(x, y, 10));
			}

			// Find all corners and sides and attach corresponding tile
			for (int x = 0; x < roomWidth; x += 32)
				for (int y = 0; y < roomHeight; y++)
					if (tiles.Contains(new Vector3(x, y, 4)))
					{
						// Corners
						if (!tiles.Contains(new Vector3(x - 32, y, 4)) && !tiles.Contains(new Vector3(x - 32, y - 32, 4)) && !tiles.Contains(new Vector3(x, y - 32, 4)))
							tiles.Add(new Vector3(x - 32, y - 32, 0));
						if (!tiles.Contains(new Vector3(x - 32, y, 4)) && !tiles.Contains(new Vector3(x - 32, y + 32, 4)) && !tiles.Contains(new Vector3(x, y + 32, 4)))
							tiles.Add(new Vector3(x - 32, y + 32, 2));
						if (!tiles.Contains(new Vector3(x + 32, y, 4)) && !tiles.Contains(new Vector3(x + 32, y - 32, 4)) && !tiles.Contains(new Vector3(x, y - 32, 4)))
							tiles.Add(new Vector3(x + 32, y - 32, 6));
						if (!tiles.Contains(new Vector3(x + 32, y, 4)) && !tiles.Contains(new Vector3(x + 32, y + 32, 4)) && !tiles.Contains(new Vector3(x, y + 32, 4)))
							tiles.Add(new Vector3(x + 32, y + 32, 8));

						// Sides
						if (!tiles.Contains(new Vector3(x - 32, y, 4)))
							tiles.Add(new Vector3(x - 32, y, 1));
						if (!tiles.Contains(new Vector3(x, y - 32, 4)))
							tiles.Add(new Vector3(x, y - 32, 3));
						if (!tiles.Contains(new Vector3(x, y + 32, 4)))
							tiles.Add(new Vector3(x, y + 32, 5));
						if (!tiles.Contains(new Vector3(x + 32, y, 4)))
							tiles.Add(new Vector3(x + 32, y, 7));
					}
		}

		// Searches through records file and finds this level's current record, sets it to -1 if not found
		private void FindRecord(SimpleAES decryptor)
		{
			StreamReader levelReader = new StreamReader("Content\\records.txt");
			string nullCheck = levelReader.ReadLine();
			if (nullCheck != null)
			{
				string recordSearch = decryptor.DecryptString(nullCheck);
				while (recordSearch.Split(' ')[0] != levelName && !levelReader.EndOfStream)
					recordSearch = decryptor.DecryptString(levelReader.ReadLine());

				if (recordSearch.Split(' ')[0] != levelName)
					record = -1;
				else
					record = int.Parse(recordSearch.Split(' ')[1]);
			}
			else
				record = -1;
			levelReader.Dispose();
		}

		public virtual void Update(GameTime gameTime)
		{
			// Reset keys
			if (!Keyboard.GetState().IsKeyDown(Keys.R))
				rcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.P))
				pcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.F))
				fcheck = true;

			// Restart the current level when R is pressed
			if (Keyboard.GetState().IsKeyDown(Keys.R) && rcheck)
			{
				// Play damaged sound
				if (custom)
					Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", false);
				else
					Game1.currentRoom = new Room(Levels.levels[Levels.Index], false);
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.F) && fcheck)
			{
				// Play damaged sound
				if (custom)
					Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", true);
				else
					Game1.currentRoom = new Room(Levels.levels[Levels.Index], true);
			}

			// Pause the game when P is pressed
			if (Keyboard.GetState().IsKeyDown(Keys.P) && pcheck)
			{
				pcheck = false;
				Paused = !Paused;
			}

			// Update freeroam cam
			if (freeroaming)
			{
				if (Keyboard.GetState().IsKeyDown(Keys.Left))
					viewBox.X -= 8;
				if (Keyboard.GetState().IsKeyDown(Keys.Right))
					viewBox.X += 8;
				if (Keyboard.GetState().IsKeyDown(Keys.Up))
					viewBox.Y -= 8;
				if (Keyboard.GetState().IsKeyDown(Keys.Down))
					viewBox.Y += 8;
			}

			if (!Finished)
			{
				if (!Paused)
				{
					// Update booster animations
					foreach (Booster b in boosters)
						b.Update();

					// Update boxes
					foreach (Box b in boxes)
						b.Update();

					// Update floating platforms
					var plats = from Wall f in walls
								where f is FloatingPlatform
								select f as FloatingPlatform;
					foreach (FloatingPlatform f in plats)
						f.Update();

					// Update character
					Runner.Update();

					// Update rocket launchers
					foreach (RocketLauncher r in launchers)
					{
						if (r.explosion != null)
							r.explosion.Update();
						if (r.Update())
						{
							r.explosion = new Explosion(new Vector2(r.rocket.hitBox.X, r.rocket.hitBox.Y), Color.OrangeRed);
							r.pause = 0;
							r.rocket.position.X = -10000;
							r.rocket.position.Y = -10000;
							r.rocket.hitBox.X = -10000;
							r.rocket.hitBox.Y = -10000;
						}
					}

					// Move viewbox to keep up with character
					UpdateViewBox(freeroaming);

					// If the runner can be moved, increment the timer
					if (Runner.controllable)
						time += gameTime.ElapsedGameTime.Milliseconds;
				}
			}
			else
			{
				// Fix glitch where you can restart at the same time you hit the finish and achieve a time of 0 seconds
				if (time == 0)
				{
					if (custom)
						Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", false);
					else
						Game1.currentRoom = new Room(Levels.levels[Levels.Index], false);
					return;
				}

				if (write)
				{
					write = false;
					
					// Get goal beaten, if any
					if (time <= goals[0])
						goalBeaten = 1;
					else if (time <= goals[1])
						goalBeaten = 2;
					else if (time <= goals[2])
						goalBeaten = 3;

					StreamWriter writer;
					SimpleAES encryptor = new SimpleAES();

					// If record is beaten, save new record
					if (time < record || record == -1)
					{
						StreamReader reader = new StreamReader("Content\\records.txt");
						writer = new StreamWriter("Content\\recordstemp.txt", false);
						bool found = false;

						// Rewrite records file, but only change current level's time
						while (!reader.EndOfStream)
						{
							string line = encryptor.DecryptString(reader.ReadLine());
							if (line.Split(' ')[0] == levelName)
							{
								found = true;
								writer.WriteLine(encryptor.EncryptToString(levelName + " " + time.ToString()));
							}
							else
								writer.WriteLine(encryptor.EncryptToString(line));
						}
						if (!found)
							writer.WriteLine(encryptor.EncryptToString(levelName + " " + time.ToString()));
						reader.Dispose();
						writer.Flush();
						writer.Dispose();
						File.Delete("Content\\records.txt");
						File.Move("Content\\recordstemp.txt", "Content\\records.txt");
					}
				}

				// Move to next level when enter is pressed, or back to menu if custom level
				if (Keyboard.GetState().IsKeyDown(Keys.Enter))
				{
					if (!custom)
					{
						Levels.Index++;
						Game1.currentRoom = new Room(Levels.levels[Levels.Index], true);
					}
					else
					{
						MainMenu back = new MainMenu(false);
						Game1.currentRoom = back;
					}
				}
			}
		}

		private void UpdateViewBox(bool freeroam)
		{
			if (!freeroam)
			{
				if (Runner.position.X + 32 > ViewBox.X + (VIEWSIZE_X / 2 + 10))
					viewBox.X = (int)Runner.position.X + 32 - (VIEWSIZE_X / 2 + 10);
				else if (Runner.position.X + 32 < ViewBox.X + (VIEWSIZE_X / 2 - 10))
					viewBox.X = (int)Runner.position.X + 32 - (VIEWSIZE_X / 2 - 10);
			}

			if (ViewBox.X < 0 || VIEWSIZE_X > roomWidth)
				viewBox.X = 0;
			else if (ViewBox.Right > roomWidth)
				viewBox.X = roomWidth - ViewBox.Width;

			if (!freeroam)
			{
				if (Runner.position.Y + 32 > ViewBox.Y + (VIEWSIZE_Y / 2 + 10))
					viewBox.Y = (int)Runner.position.Y + 32 - (VIEWSIZE_Y / 2 + 10);
				else if (Runner.position.Y + 32 < ViewBox.Y + (VIEWSIZE_Y / 2 - 10))
					viewBox.Y = (int)Runner.position.Y + 32 - (VIEWSIZE_Y / 2 - 10);
			}

			if (ViewBox.Bottom > roomHeight || VIEWSIZE_Y > roomHeight)
				viewBox.Y = roomHeight - ViewBox.Height;
			else if (ViewBox.Y < 0)
				viewBox.Y = 0;
		}

		public virtual void Draw(SpriteBatch sb)
		{
			// Used to darken drawings when game is paused or level is finished
			Color drawHue = (Paused || Finished ? new Color(100, 100, 100) : Color.White);

			// Draw background
			sb.Draw(Game1.backgrounds[(int)Theme], new Rectangle(0, 0, viewBox.Width, viewBox.Height), drawHue);

			// Draw tiles
			var tilesInView = from Vector3 v in tiles
							  where v.X >= viewBox.Left - 32 && v.X <= viewBox.Right && v.Y >= viewBox.Top - 32 && v.Y <= viewBox.Bottom
							  select v;
			foreach (Vector3 v in tilesInView)
			{
				if (v.Z <= 8)
					sb.Draw(Game1.tileSet[(int)Theme], new Rectangle((int)v.X - viewBox.X, (int)v.Y - viewBox.Y, 32, 32), wallSet.Tiles[(int)v.Z], drawHue);
				else if (v.Z == 9)
					sb.Draw(Game1.deathWallSet[(int)Theme], new Rectangle((int)v.X - viewBox.X, (int)v.Y - viewBox.Y, 32, 32), drawHue);
				else if (v.Z == 10)
					sb.Draw(Game1.mirrorTex, new Rectangle((int)v.X - viewBox.X, (int)v.Y - viewBox.Y, 32, 32), drawHue);
			}

			// Draw messages
			var msgInView = from Message m in messages
							where m.HitBox.Intersects(viewBox)
							select m;
			foreach (Message m in msgInView)
				m.Draw(sb, drawHue);

			// Draw boosters
			var boostersInView = from Booster b in boosters
								 where b.HitBox.Intersects(viewBox)
								 select b;
			foreach (Booster b in boostersInView)
				b.Draw(sb, drawHue);

			// Draw boxes
			var boxesInView = from Box b in boxes
							  where b.hitBox.Intersects(viewBox)
							  select b;
			foreach (Box b in boxesInView)
				b.Draw(sb, drawHue);

			// Draw moving platforms
			var platsInView = from Wall f in walls
							  where f is FloatingPlatform && f.Bounds.Intersects(viewBox)
							  select f as FloatingPlatform;
			foreach (FloatingPlatform f in platsInView)
				f.Draw(sb, drawHue);

			// Draw runner
			if (Runner != null) Runner.Draw(sb, drawHue);

			// Draw finish platform
			if (Finish != null) Finish.Draw(sb, drawHue);

			// Draw ziplines
			var zipsInView = from ZipLine z in ziplines
							 where z.DrawBox.Intersects(viewBox)
							 select z;
			foreach (ZipLine z in zipsInView)
				z.Draw(sb, drawHue);

			// Draw launchers
			foreach (RocketLauncher r in launchers)
			{
				if (r.explosion != null)
					r.explosion.Draw(sb);
				if (r.hitBox.Intersects(viewBox) || r.rocket.hitBox.Intersects(viewBox))
					r.Draw(sb, drawHue);
			}

			// Freeroam instructions
			if (freeroaming)
			{
				sb.DrawString(Game1.mnufont, "Freeroam cam", new Vector2(0, 30), Color.White);
				sb.DrawString(Game1.mnufont, "Freeroam cam", new Vector2(1, 31), Color.Black);
				sb.DrawString(Game1.mnufont, "Use the arrow keys to check out the level before you play", new Vector2(0, 60), Color.White);
				sb.DrawString(Game1.mnufont, "Use the arrow keys to check out the level before you play", new Vector2(1, 61), Color.Black);
			}

			if (Paused || Finished || Runner.health <= 0 || freeroaming)
			{
				sb.DrawString(Game1.mnufont, "Press R to " + (freeroaming ? "play" : "restart"), new Vector2(freeroaming ? 782 : 750, 690), Color.White);
				if (freeroaming && !Paused || Runner.health <= 0 && !Paused)
					sb.DrawString(Game1.mnufont, "Press R to " + (freeroaming ? "play" : "restart"), new Vector2(freeroaming? 783 : 751, 691), Color.Black);
			}

			if (Finished)
			{
				// Draw finished level screen

				sb.DrawString(Game1.titlefont, "Level Complete!", new Vector2(180, 182), Color.White);
				sb.DrawString(Game1.titlefont, "Level Complete!", new Vector2(182, 184), drawHue);

				sb.DrawString(Game1.mnufont, "Previous Record: " + (record == -1 ? "--" : TimeToString(record)), new Vector2(176, 280), Color.White);
				if (record != -1)
				{
					if (record <= goals[0])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Gold);
					else if (record <= goals[1])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Silver);
					else if (record <= goals[2])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Brown);
				}

				sb.DrawString(Game1.mnufont, "Time: " + TimeToString(time), new Vector2(315, 310), time < record || record == -1 ? Color.Lime : Color.Red);
				if (time < record || record == -1)
					sb.DrawString(Game1.mnufont, "New Personal Record!", new Vector2(208, 340), Color.Yellow);

				sb.Draw(Game1.medalTex, new Vector2(612, 280), Color.Gold);
				sb.Draw(Game1.medalTex, new Vector2(612, 310), Color.White);
				sb.Draw(Game1.medalTex, new Vector2(612, 340), Color.Brown);
				sb.DrawString(Game1.mnufont, TimeToString(goals[0]), new Vector2(644, 280), goalBeaten == 1 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[1]), new Vector2(644, 310), goalBeaten == 2 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[2]), new Vector2(644, 340), goalBeaten == 3 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, goalBeaten != 0 ? ("You ran a " + (goalBeaten == 1 ? "gold" : (goalBeaten == 2 ? "silver" : "bronze")) + " time!") : "Do better next time!", new Vector2(382, 400), Color.Yellow);

				sb.DrawString(Game1.mnufont, "Press Enter to continue", new Vector2(670, 630), Color.White);
				sb.DrawString(Game1.mnufont, "Press F to freeroam", new Vector2(725, 660), Color.White);
			}
			else if (Paused)
			{
				// Draw pause menu

				sb.DrawString(Game1.titlefont, "Paused", new Vector2(360, 182), Color.White);
				sb.DrawString(Game1.titlefont, "Paused", new Vector2(362, 184), drawHue);

				sb.DrawString(Game1.mnufont, "Current Record: " + (record == -1 ? "--" : TimeToString(record)), new Vector2(176, 280), Color.White);
				if (record != -1)
				{
					if (record <= goals[0])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Gold);
					else if (record <= goals[1])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Silver);
					else if (record <= goals[2])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Brown);
				}

				sb.DrawString(Game1.mnufont, "Time: " + TimeToString(time), new Vector2(315, 310), time < record || record == -1 ? Color.Lime : Color.Red);

				sb.Draw(Game1.medalTex, new Vector2(612, 280), Color.Gold);
				sb.Draw(Game1.medalTex, new Vector2(612, 310), Color.White);
				sb.Draw(Game1.medalTex, new Vector2(612, 340), Color.Brown);
				sb.DrawString(Game1.mnufont, TimeToString(goals[0]), new Vector2(644, 280), goalBeaten == 1 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[1]), new Vector2(644, 310), goalBeaten == 2 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[2]), new Vector2(644, 340), goalBeaten == 3 ? Color.Lime : Color.White);

				sb.DrawString(Game1.mnufont, "Press P to unpause", new Vector2(736, 630), Color.White);
				sb.DrawString(Game1.mnufont, "Press F to restart/freeroam", new Vector2(630, 660), Color.White);
			}
			else if (Runner.health <= 0)
			{
				sb.DrawString(Game1.titlefont, "You died!", new Vector2(360, 182), Color.White);
				sb.DrawString(Game1.titlefont, "You died!", new Vector2(362, 184), Color.Black);

				sb.DrawString(Game1.mnufont, "Current Record: " + (record == -1 ? "--" : TimeToString(record)), new Vector2(176, 280), Color.White);
				if (record != -1)
				{
					if (record <= goals[0])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Gold);
					else if (record <= goals[1])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Silver);
					else if (record <= goals[2])
						sb.Draw(Game1.medalTex, new Vector2(495, 280), Color.Brown);
				}

				sb.DrawString(Game1.mnufont, "Time: " + TimeToString(time), new Vector2(315, 310), Color.Red);

				sb.Draw(Game1.medalTex, new Vector2(612, 280), Color.Gold);
				sb.Draw(Game1.medalTex, new Vector2(612, 310), Color.White);
				sb.Draw(Game1.medalTex, new Vector2(612, 340), Color.Brown);
				sb.DrawString(Game1.mnufont, TimeToString(goals[0]), new Vector2(644, 280), goalBeaten == 1 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[1]), new Vector2(644, 310), goalBeaten == 2 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[2]), new Vector2(644, 340), goalBeaten == 3 ? Color.Lime : Color.White);

				sb.DrawString(Game1.mnufont, "Press F to freeroam", new Vector2(725, 660), Color.White);
				sb.DrawString(Game1.mnufont, "Press F to freeroam", new Vector2(726, 661), Color.Black);
			}
			else
			{
				// Draw timer
				sb.DrawString(Game1.mnufont, TimeToString(time), Vector2.Zero, Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(time), Vector2.One, new Color(100, 100, 100));
			}
		}
		
		// Returns millisecond count in "mm:ss.sss" format
		private string TimeToString(int time)
		{
			TimeSpan t = TimeSpan.FromMilliseconds(time);
			return String.Format("{0:00}:{1:00}.{2:000}", (int)t.TotalMinutes, t.Seconds, t.Milliseconds % 1000);
		}

		public enum LevelTheme
		{
			Grass = 0,
			Lava = 1,
			Night = 2,
			Cave = 3,
			Factory = 4
		}
	}
}
