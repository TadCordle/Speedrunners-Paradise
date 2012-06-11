using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class Room
	{
		public const int VIEWSIZE_X = 960;
		public const int VIEWSIZE_Y = 720;

		public Rectangle viewBox;
		public int roomWidth, roomHeight;
		int[] goals; // 0 = gold, 1 = silver, 2 = bronze
		public List<Wall> walls;
		public List<ZipLine> ziplines;
		public List<Booster> boosters;
		public List<FloatingPlatform> platforms;
		public List<Vector3> tiles; // x, y, z = index
		public Runner runner;
		public Finish finish;
		public Theme theme;
		Tileset wallSet;
		string levelName;
		bool custom;
		public bool finished, paused;
		int time, record;
		bool write;
		bool pcheck, rcheck;
		int goalBeaten;

		public Room()
		{
			goalBeaten = 0;
			pcheck = true;
			rcheck = false;
			write = true;
			time = 0;
			tiles = new List<Vector3>();
			finished = false;
			goals = new int[3];
			walls = new List<Wall>();
			ziplines = new List<ZipLine>();
			boosters = new List<Booster>();
			platforms = new List<FloatingPlatform>();
			viewBox = new Rectangle(0, 0, VIEWSIZE_X, VIEWSIZE_Y);
		}

		public Room(string file)
			: this()
		{
			// Get level name
			levelName = file.Split('\\')[file.Split('\\').Length - 1].Replace(".srl", "");
			custom = true;

			SimpleAES decryptor = new SimpleAES();
			StreamReader levelReader = new StreamReader(file);
			string[] line;

			// Get level theme
			line = decryptor.DecryptString(levelReader.ReadLine()).Split(' ');
			theme = FindTheme(line[0]);
			wallSet = new Tileset(Game1.tileSet[(int)theme], 32, 32, 3, 3);
			
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
				ParseObjectOrTile(line);
			}
			levelReader.Dispose();

			// Generate zipline poles
			foreach (ZipLine z in ziplines)
				z.SetPoles(this);

			// Get current record for this level
			FindRecord(decryptor);
		}

		public Room(string[] lines) : this()
		{
			// Get level name
			levelName = ".MAIN.Level" + (Levels.index + 1).ToString();
			custom = false;

			SimpleAES decryptor = new SimpleAES();
			string[] line;

			// Get level theme
			line = decryptor.DecryptString(lines[0]).Split(' ');
			theme = FindTheme(line[0]);
			wallSet = new Tileset(Game1.tileSet[(int)theme], 32, 32, 3, 3);

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
				ParseObjectOrTile(line);
				index++;
			}

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
		}

		// Parses a theme from a line of text
		private Theme FindTheme(string t)
		{
			switch (t)
			{
				case "Grass":
					return Theme.Grass;
				case "Lava":
					return Theme.Lava;
				case "Night":
					return Theme.Night;
				case "Cave":
					return Theme.Cave;
				case "Factory":
					return Theme.Factory;
				default:
					return Theme.Grass;
			}
		}

		// Instantiates an object from a line of text and adds it to the appropriate list
		private void ParseObjectOrTile(string[] line)
		{
			if (line[0] == "runner")
			{
				runner = new Runner(new Vector2(int.Parse(line[1]), int.Parse(line[2])));
				viewBox.X = (int)runner.position.X - VIEWSIZE_X / 2 - 32;
				viewBox.Y = (int)runner.position.Y - VIEWSIZE_Y / 2 - 32;
				if (viewBox.X < 0) viewBox.X = 0;
				if (viewBox.Y < 0) viewBox.Y = 0;
			}
			else if (line[0] == "wall")
				walls.Add(new Wall(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
			else if (line[0] == "tile")
				tiles.Add(new Vector3(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3])));
			else if (line[0] == "finish")
				finish = new Finish(new Vector2(int.Parse(line[1]), int.Parse(line[2])));
			else if (line[0] == "zipline")
				ziplines.Add(new ZipLine(new Vector2(int.Parse(line[1]), int.Parse(line[2])), new Vector2(int.Parse(line[3]), int.Parse(line[4])), theme));
			else if (line[0] == "booster")
				boosters.Add(new Booster(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
			else if (line[0] == "floatingplatform")
				walls.Add(new FloatingPlatform(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
			else if (line[0] == "platformwall")
				walls.Add(new PlatformWall(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
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

			// Restart the current level when R is pressed
			if (Keyboard.GetState().IsKeyDown(Keys.R) && rcheck)
			{
				if (custom)
					Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl");
				else
					Game1.currentRoom = new Room(Levels.levels[Levels.index]);
			}

			// Pause the game when P is pressed
			if (Keyboard.GetState().IsKeyDown(Keys.P) && pcheck)
			{
				pcheck = false;
				paused = !paused;
			}

			if (!finished)
			{
				if (!paused)
				{
					// Update booster animations
					foreach (Booster b in boosters)
						b.Update();
					var plats = from Wall f in walls
								where f is FloatingPlatform
								select f as FloatingPlatform;

					// Update floating platforms
					foreach (FloatingPlatform f in plats)
						f.Update();

					// Update character
					runner.Update();

					// Move viewbox to keep up with character
					if (runner.position.X + 32 > viewBox.X + (VIEWSIZE_X / 2 + 10))
						viewBox.X = (int)runner.position.X + 32 - (VIEWSIZE_X / 2 + 10);
					else if (runner.position.X + 32 < viewBox.X + (VIEWSIZE_X / 2 - 10))
						viewBox.X = (int)runner.position.X + 32 - (VIEWSIZE_X / 2 - 10);

					if (viewBox.X < 0 || VIEWSIZE_X > roomWidth)
						viewBox.X = 0;
					else if (viewBox.Right > roomWidth)
						viewBox.X = roomWidth - viewBox.Width;

					if (runner.position.Y + 32 > viewBox.Y + (VIEWSIZE_Y / 2 + 10))
						viewBox.Y = (int)runner.position.Y + 32 - (VIEWSIZE_Y / 2 + 10);
					else if (runner.position.Y + 32 < viewBox.Y + (VIEWSIZE_Y / 2 - 10))
						viewBox.Y = (int)runner.position.Y + 32 - (VIEWSIZE_Y / 2 - 10);

					if (viewBox.Bottom > roomHeight || VIEWSIZE_Y > roomHeight)
						viewBox.Y = roomHeight - viewBox.Height;
					else if (viewBox.Y < 0)
						viewBox.Y = 0;

					// If the runner can be moved, increment the timer
					if (runner.controllable)
						time += gameTime.ElapsedGameTime.Milliseconds;
				}
			}
			else
			{
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
						Levels.index++;
						Game1.currentRoom = new Room(Levels.levels[Levels.index]);
					}
					else
					{
						MainMenu back = new MainMenu();
						back.pressEnter = false;
						Game1.currentRoom = back;
					}
				}
			}
		}

		public virtual void Draw(SpriteBatch sb)
		{
			// Used to darken drawings when game is paused or level is finished
			Color drawHue = (paused || finished ? new Color(100, 100, 100) : Color.White);

			// Draw background
			sb.Draw(Game1.backgrounds[(int)theme], new Rectangle(0, 0, viewBox.Width, viewBox.Height), drawHue);

			// Draw tiles
			var tilesInView = from Vector3 v in tiles
							  where v.X >= viewBox.Left - 32 && v.X <= viewBox.Right && v.Y >= viewBox.Top - 32 && v.Y <= viewBox.Bottom
							  select v;
			foreach (Vector3 v in tilesInView)
				sb.Draw(Game1.tileSet[(int)theme], new Rectangle((int)v.X - viewBox.X, (int)v.Y - viewBox.Y, 32, 32), wallSet.Tiles[(int)v.Z], drawHue);

			// Draw boosters
			var boostersInView = from Booster b in boosters
								 where b.hitBox.Intersects(viewBox)
								 select b;
			foreach (Booster b in boostersInView)
				b.Draw(sb, drawHue);

			// Draw moving platforms
			var platsInView = from Wall f in walls
							  where f is FloatingPlatform && f.bounds.Intersects(viewBox)
							  select f as FloatingPlatform;
			foreach (FloatingPlatform f in platsInView)
				f.Draw(sb, drawHue);

			// Draw runner
			if (runner != null) runner.Draw(sb, drawHue);
			
			// Draw finish platform
			if (finish != null) finish.Draw(sb, drawHue);

			// Draw ziplines
			var zipsInView = from ZipLine z in ziplines
							 where (z.pos1.X < z.pos2.X ?
										(z.pos1.Y > z.pos2.Y ?
											new Rectangle((int)z.pos1.X, (int)z.pos2.Y, (int)z.pos2.X, (int)z.pos1.Y).Intersects(viewBox) :
											new Rectangle((int)z.pos1.X, (int)z.pos1.Y, (int)z.pos2.X, (int)z.pos2.Y).Intersects(viewBox)) :
										(z.pos1.Y > z.pos2.Y ?
											new Rectangle((int)z.pos2.X, (int)z.pos2.Y, (int)z.pos1.X, (int)z.pos1.Y).Intersects(viewBox) :
											new Rectangle((int)z.pos2.X, (int)z.pos1.Y, (int)z.pos1.X, (int)z.pos2.Y).Intersects(viewBox))
									)
							 select z;
			foreach (ZipLine z in zipsInView)
				z.Draw(sb, Color.White);

			if (finished)
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

				sb.DrawString(Game1.mnufont, "Press Enter to continue", new Vector2(670, 660), Color.White);
				sb.DrawString(Game1.mnufont, "Press R to restart", new Vector2(750, 690), Color.White);
			}
			else if (paused)
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

				sb.DrawString(Game1.mnufont, "Press P to unpause", new Vector2(736, 660), Color.White);
				sb.DrawString(Game1.mnufont, "Press R to restart", new Vector2(750, 690), Color.White);
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
			return ((int)(time / 60000) >= 10 ? "" : "0") + ((int)(time / 60000)).ToString() + ":" + (((time - (int)(time / 60000)) % 60000 / 1000.0) >= 10 ? "" : "0") + (((time - (int)(time / 60000)) % 60000 / 1000.0f)).ToString();
		}

		public enum Theme
		{
			Grass = 0,
			Lava = 1,
			Night = 2,
			Cave = 3,
			Factory = 4
		}
	}
}
