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
using Speedrunning_Game_Forms;

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

		private List<Flamethrower> flamethrowers;
		public List<Flamethrower> Flamethrowers { get { return flamethrowers; } }

		public Runner Runner { get; set; }
		public Finish Finish { get; set; }
		public LevelTheme Theme { get; set; }
		public bool Finished { get; set; }
		public bool Paused { get; set; }

		private List<int>[,] tiles;
		private int[] goals; // 0 = gold, 1 = silver, 2 = bronze
		private Tileset wallSet;
		private string levelName;
		private string levelID;
		public int time;
		private int record, goalBeaten;
		private bool custom, write, writeNext, upload, pcheck, rcheck, fcheck, scheck, ocheck, freeroaming, lcheck, upcheck, downcheck;
		public bool viewingLeaderboards, canViewLeaderboards;
		private string[][] leaderboardData;
		private int leaderboardPage;
		private bool canScrollDown;
		public ReplayRecorder recorder;
		private bool recorderSaved;

		public bool Freeroam { get { return freeroaming; } }

		public Room()
		{
			upload = true;
			canScrollDown = false;
			goalBeaten = 0;
			pcheck = true;
			rcheck = false;
			fcheck = false;
			lcheck = true;
			scheck = false;
			ocheck = false;
			write = true;
			writeNext = true;
			time = 0;
			Finished = false;
			goals = new int[3];
			walls = new List<Wall>();
			ziplines = new List<ZipLine>();
			boosters = new List<Booster>();
			platforms = new List<FloatingPlatform>();
			messages = new List<Message>();
			boxes = new List<Box>();
			launchers = new List<RocketLauncher>();
			flamethrowers = new List<Flamethrower>();
			ViewBox = new Rectangle(0, 0, VIEWSIZE_X, VIEWSIZE_Y);
			viewingLeaderboards = false;
			leaderboardPage = 0;
			recorderSaved = false;
		}

		public Room(string file, bool freeroam, ReplayRecorder recorder)
			: this()
		{
			freeroaming = freeroam;
			this.recorder = recorder;

			// Get level name
			levelName = file.Split('\\')[file.Split('\\').Length - 1].Replace(".srl", "");
			custom = true;

			SimpleAES decryptor = new SimpleAES();
			StreamReader levelReader = new StreamReader(file);
			string[] line;

			// Get level id
			levelID = levelReader.ReadLine();

			// Check if level will have a leaderboard
			canViewLeaderboards = decryptor.DecryptString(levelReader.ReadLine()) == "1";

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

		public Room(string[] lines, bool freeroam, ReplayRecorder recorder) 
			: this()
		{
			freeroaming = freeroam;
			this.recorder = recorder;

			SimpleAES decryptor = new SimpleAES();
			string[] line;

			// Get leve name
			levelName = lines[0];
			custom = false;

			// Get level id
			levelID = lines[1];

			// Check if level will have a leaderboard
			canViewLeaderboards = decryptor.DecryptString(lines[2]) == "1";

			// Get level theme
			line = decryptor.DecryptString(lines[3]).Split(' ');
			Theme = FindTheme(line[0]);
			wallSet = new Tileset(Game1.tileSet[(int)Theme], 32, 32, 3, 3);

			// Get room dimensions
			line = decryptor.DecryptString(lines[4]).Split(' ');
			roomWidth = int.Parse(line[0]);
			roomHeight = int.Parse(line[1]);

			// Get goal times
			line = decryptor.DecryptString(lines[5]).Split(' ');
			for (int i = 0; i < 3; i++)
				goals[i] = int.Parse(line[i]);

			// Get objects and tiles
			int index = 6;
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
					record = int.Parse(s.Split(' ')[2]);
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
				writer.WriteLine(decryptor.EncryptToString(levelName + " 0 -1"));
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
			else if (line[0] == "flamethrower")
				flamethrowers.Add(new Flamethrower(int.Parse(line[1]), int.Parse(line[2]), float.Parse(line[3]), int.Parse(line[4]), int.Parse(line[5])));
		}

		private void BuildTiles()
		{
			tiles = new List<int>[roomHeight / 32 + 1, roomWidth / 32 + 1];
			for (int i = 0; i < tiles.GetLength(0); i++)
				for (int j = 0; j < tiles.GetLength(1); j++)
				{
					tiles[i, j] = new List<int>();
					tiles[i, j].Add(-1);
				}

			// Fill each wall object with middle tiles
			var realWalls = from Wall w in walls
							where !(w is PlatformWall) && !(w is FloatingPlatform) && !(w is DeathWall) && !(w is Mirror)
							select w;
			foreach (Wall w in realWalls)
			{
				for (int x = w.Bounds.X; x < w.Bounds.Right; x += 32)
					for (int y = w.Bounds.Y; y < w.Bounds.Bottom; y += 32)
						if (x >= 0 && x <= roomWidth && y >= 0 && y <= roomHeight)
							tiles[y / 32, x / 32].Insert(0, 4);
			}
			
			// Fill each death wall with death tile
			var deathWalls = from Wall w in walls
							 where w is DeathWall
							 select w;
			foreach (Wall w in deathWalls)
			{
				for (int x = w.Bounds.X; x < w.Bounds.Right; x += 32)
					for (int y = w.Bounds.Y; y < w.Bounds.Bottom; y += 32)
						if (x >= 0 && x <= roomWidth && y >= 0 && y <= roomHeight)
							tiles[y / 32, x / 32].Insert(0, 9);
			}

			// Fill each mirror with mirror tile
			var mirrors = from Wall w in walls
							 where w is Mirror
							 select w;
			foreach (Wall w in mirrors)
			{
				for (int x = w.Bounds.X; x < w.Bounds.Right; x += 32)
					for (int y = w.Bounds.Y; y < w.Bounds.Bottom; y += 32)
						if (x >= 0 && x <= roomWidth && y >= 0 && y <= roomHeight)
							tiles[y / 32, x / 32].Insert(0, 10);
			}

			int newHeight = roomHeight / 32;
			int newWidth = roomWidth / 32;

			// Find all corners and sides and attach corresponding tile
			for (int x = 0; x <= newWidth; x++)
				for (int y = 0; y <= newHeight; y++)
					if (tiles[y, x].Count > 0 && tiles[y, x][0] == 4)
					{
						// Corners
						if (x - 1 >= 0 && x - 1 <= newWidth && y - 1 >= 0 && y - 1 <= newHeight && tiles[y, x - 1][0] != 4 && tiles[y - 1, x - 1][0] != 4 && tiles[y - 1, x][0] != 4)
							tiles[y - 1, x - 1].Add(0);
						if (x - 1 >= 0 && x - 1 <= newWidth && y + 1 >= 0 && y + 1 <= newHeight && tiles[y, x - 1][0] != 4 && tiles[y + 1, x - 1][0] != 4 && tiles[y + 1, x][0] != 4)
							tiles[y + 1, x - 1].Add(2);
						if (x + 1 >= 0 && x + 1 <= newWidth && y - 1 >= 0 && y - 1 <= newHeight && tiles[y, x + 1][0] != 4 && tiles[y - 1, x + 1][0] != 4 && tiles[y - 1, x][0] != 4)
							tiles[y - 1, x + 1].Add(6);
						if (x + 1 >= 0 && x + 1 <= newWidth && y + 1 >= 0 && y + 1 <= newHeight && tiles[y, x + 1][0] != 4 && tiles[y + 1, x + 1][0] != 4 && tiles[y + 1, x][0] != 4)
							tiles[y + 1, x + 1].Add(8);

						// Sides
						if (x - 1 >= 0 && x - 1 <= newWidth && tiles[y, x - 1][0] != 4)
							tiles[y, x - 1].Add(1);
						if (y - 1 >= 0 && y - 1 <= newHeight && tiles[y - 1, x][0] != 4)
							tiles[y - 1, x].Add(3);
						if (y + 1 >= 0 && y + 1 <= newHeight && tiles[y + 1, x][0] != 4)
							tiles[y + 1, x].Add(5);
						if (x + 1 >= 0 && x + 1 <= newWidth && tiles[y, x + 1][0] != 4)
							tiles[y, x + 1].Add(7);
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
					record = int.Parse(recordSearch.Split(' ')[2]);
			}
			else
				record = -1;
			levelReader.Dispose();
		}

		public virtual void Update(GameTime gameTime)
		{
			// Reset keys
			if (!Keyboard.GetState().IsKeyDown(Settings.controls["Restart"]))
				rcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Settings.controls["Pause"]))
				pcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Settings.controls["Freeroam"]))
				fcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.L))
				lcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Up))
				upcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Down))
				downcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.S))
				scheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.O))
				ocheck = true;

			// Restart when R is pressed
			if (Keyboard.GetState().IsKeyDown(Settings.controls["Restart"]) && rcheck)
			{
				if (custom)
					Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", false, new ReplayRecorder());
				else
					Game1.currentRoom = new Room(Levels.levels[Levels.Index], false, new ReplayRecorder());
			}
			else if (Keyboard.GetState().IsKeyDown(Settings.controls["Freeroam"]) && fcheck)
			{
				if (custom)
					Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", true, new ReplayRecorder());
				else
					Game1.currentRoom = new Room(Levels.levels[Levels.Index], true, new ReplayRecorder());
			}

			// Pause the game when P is pressed
			if (Keyboard.GetState().IsKeyDown(Settings.controls["Pause"]) && pcheck && !viewingLeaderboards)
			{
				pcheck = false;
				Paused = !Paused;
			}

			// Show leaderboards when L is pressed
			if (Keyboard.GetState().IsKeyDown(Keys.L) && lcheck && (Paused || Finished) && Game1.online && canViewLeaderboards)
			{
				lcheck = false;
				if (!viewingLeaderboards)
				{
					viewingLeaderboards = true;
					leaderboardData = WebStuff.GetScores(levelID, Game1.userName, leaderboardPage * 10);
					canScrollDown = leaderboardData.Length == 11;
				}
				else
					viewingLeaderboards = false;
			}

			if (viewingLeaderboards)
			{
				if (Keyboard.GetState().IsKeyDown(Keys.Up) && upcheck && leaderboardPage > 0)
				{
					leaderboardPage--;
					leaderboardData = WebStuff.GetScores(levelID, Game1.userName, leaderboardPage * 10);
					canScrollDown = true;
				}
				else if (Keyboard.GetState().IsKeyDown(Keys.Down) && downcheck && canScrollDown)
				{
					leaderboardPage++;
					leaderboardData = WebStuff.GetScores(levelID, Game1.userName, leaderboardPage * 10);
					canScrollDown = leaderboardData.Length == 11;
				}
			}

			// Update freeroam cam
			if (freeroaming && !Paused)
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
					if (recorder.playing)
						recorder.PlayFrame();
					else
						recorder.RecordFrame();

					// Update booster animations
					foreach (Booster b in boosters)
						b.Update();

					// Find platforms
					var plats = from Wall f in walls
								where f is FloatingPlatform
								select f as FloatingPlatform;
					foreach (FloatingPlatform f in plats)
						f.Update();

					// Update boxes
					foreach (Box b in boxes)
						b.Update();

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

					// Update flamethrowers
					foreach (Flamethrower f in flamethrowers)
						f.Update();

					// Move viewbox to keep up with character
					UpdateViewBox(freeroaming);

					// If the runner can be moved, increment the timer
					if (Runner.controllable)
						time += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					Game1.run.Stop();
					Game1.slide.Stop();

					if (Keyboard.GetState().IsKeyDown(Keys.O) && ocheck)
					{
						ocheck = false;
						System.Windows.Forms.OpenFileDialog openFD = new System.Windows.Forms.OpenFileDialog();
						if (!Directory.Exists("Content\\replays"))
							Directory.CreateDirectory("Content\\replays");
						openFD.InitialDirectory = "Content\\replays";
						openFD.Filter = "Replay Files (*.rpl)|*.rpl";
						if (openFD.ShowDialog() == System.Windows.Forms.DialogResult.OK && File.Exists(openFD.FileName))
						{
							ReplayRecorder rec = new ReplayRecorder(openFD.FileName);
							if (custom)
								Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", false, rec);
							else
								Game1.currentRoom = new Room(Levels.levels[Levels.Index], false, rec);
						}
					}
				}
			}
			else
			{
				Game1.run.Stop();
				Game1.slide.Stop();

				// Fix glitch where you can restart at the same time you hit the finish and achieve a time of 0 seconds
				if (time == 0)
				{
					if (custom)
						Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", false, new ReplayRecorder());
					else
						Game1.currentRoom = new Room(Levels.levels[Levels.Index], false, new ReplayRecorder());
					return;
				}

				if (write && !recorder.playing)
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
								writer.WriteLine(encryptor.EncryptToString(levelName + " " + (custom ? "1" : "0") + " " + time.ToString()));
							}
							else
								writer.WriteLine(encryptor.EncryptToString(line));
						}
						if (!found)
							writer.WriteLine(encryptor.EncryptToString(levelName + " " + (custom ? "1" : "0") + " " + time.ToString()));
						reader.Dispose();
						writer.Flush();
						writer.Dispose();
						File.Delete("Content\\records.txt");
						File.Move("Content\\recordstemp.txt", "Content\\records.txt");
					}
				}

				if (upload && canViewLeaderboards && !recorder.playing)
				{
					// Upload score to leaderboard
					upload = false;
					if (Game1.online)
						WebStuff.WriteScore(time, Game1.userName, levelID);
				}

				if (writeNext && !custom && Levels.Index < Levels.levels.Count() - 1 && !recorder.playing)
				{
					// Add next level to level select if not already unlocked
					writeNext = false;
					bool recordFound = false;
					StreamReader reader = new StreamReader("Content\\records.txt");
					SimpleAES encryptor = new SimpleAES();
					string name = Levels.levels[Levels.Index + 1][0];
					while (!reader.EndOfStream)
					{
						string s = encryptor.DecryptString(reader.ReadLine());
						if (s.Split(' ')[0] == name)
						{
							recordFound = true;
							break;
						}
					}
					reader.Close();
					reader.Dispose();
					if (!recordFound)
					{
						StreamWriter writer = new StreamWriter("Content\\records.txt", true);
						writer.WriteLine(encryptor.EncryptToString(name + " 0 -1"));
						writer.Flush();
						writer.Dispose();
					}
				}

				// Check if they want to see replay
				if (Keyboard.GetState().IsKeyDown(Keys.W))
				{
					recorder.playing = true;
					recorder.start = true;
					if (custom)
						Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl", false, recorder);
					else
						Game1.currentRoom = new Room(Levels.levels[Levels.Index], false, recorder);
				}

				// Check if they want to save replay
				if (Keyboard.GetState().IsKeyDown(Keys.S) && scheck)
				{
					scheck = false;
					recorder.Save(levelName);
					recorderSaved = true;
				}

				// Move to next level when enter is pressed, or back to menu if custom level
				if (Keyboard.GetState().IsKeyDown(Keys.Enter))
				{
					if (!custom)
					{
						if (record > -1 || !recorder.playing)
						{
							Levels.Index++;
							if (Levels.Index == Levels.levels.Count())
								Game1.currentRoom = new MainMenu(false);
							else
							{
								while (Levels.Index < Levels.levels.Length && Levels.levels[Levels.Index][0] == "")
									Levels.Index++;
								Game1.currentRoom = new Room(Levels.levels[Levels.Index], true, new ReplayRecorder());
							}
						}
					}
					else
						Game1.currentRoom = new LevelSelect(1);
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
			int startx = viewBox.X / 32, starty = Math.Max(0, viewBox.Y / 32);
			int maxX = Math.Min(viewBox.Right / 32, roomWidth / 32), maxY = viewBox.Bottom / 32;
			for (int x = startx; x <= maxX; x++)
				for (int y = starty; y <= maxY; y++)
					for (int i = 0; i < tiles[y, x].Count; i++)
					{
						if (tiles[y, x][i] <= 8 && tiles[y, x][i] != -1)
							sb.Draw(Game1.tileSet[(int)Theme], new Rectangle(x * 32 - viewBox.X, y * 32 - viewBox.Y, 32, 32), wallSet.Tiles[tiles[y, x][i]], drawHue);
						else if (tiles[y, x][i] == 9)
							sb.Draw(Game1.deathWallSet[(int)Theme], new Rectangle(x * 32 - viewBox.X, y * 32 - viewBox.Y, 32, 32), drawHue);
						else if (tiles[y, x][i] == 10)
							sb.Draw(Game1.mirrorTex, new Rectangle(x * 32 - viewBox.X, y * 32 - viewBox.Y, 32, 32), drawHue);
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

			// Draw flamethrowers
			foreach (Flamethrower f in flamethrowers)
			{
				if (!f.drawBox.Intersects(viewBox))
				{
					bool intersects = false;
					foreach (Flamethrower.Flame fl in f.flames)
					{
						if (fl.drawBox.Intersects(viewBox))
						{
							intersects = true;
							break;
						}
					}
					if (intersects)
						f.Draw(sb, drawHue);
				}
				else
				{
					f.Draw(sb, drawHue);
				}
			}

			// Draw launchers
			foreach (RocketLauncher r in launchers)
			{
				if (r.explosion != null)
					r.explosion.Draw(sb);
				if (r.hitBox.Intersects(viewBox) || r.rocket.hitBox.Intersects(viewBox))
					r.Draw(sb, drawHue);
			}

			// Freeroam instructions
			if (freeroaming && !Paused)
			{
				sb.DrawString(Game1.mnufont, "Freeroam cam", new Vector2(0, 30), Color.White);
				sb.DrawString(Game1.mnufont, "Freeroam cam", new Vector2(1, 31), Color.Black);
				sb.DrawString(Game1.mnufont, "Use the arrow keys to check out the level before you play", new Vector2(0, 60), Color.White);
				sb.DrawString(Game1.mnufont, "Use the arrow keys to check out the level before you play", new Vector2(1, 61), Color.Black);
			}

			if ((Paused || Finished || Runner.health <= 0 || freeroaming) && !viewingLeaderboards)
			{
				sb.DrawString(Game1.mnufont, "Press R to " + (freeroaming ? "play" : "restart"), new Vector2(freeroaming ? 782 : 750, 690), Color.White);
				if (freeroaming && !Paused || Runner.health <= 0 && !Paused)
					sb.DrawString(Game1.mnufont, "Press R to " + (freeroaming ? "play" : "restart"), new Vector2(freeroaming? 783 : 751, 691), Color.Black);
			}

			if (viewingLeaderboards)
			{
				// Draw level's leaderboard

				sb.DrawString(Game1.mnufont, "Leaderboards", new Vector2(340, 10), Color.White);
				sb.DrawString(Game1.mnufont, "Leaderboards", new Vector2(341, 11), drawHue);

				if (leaderboardPage > 0)
					sb.DrawString(Game1.mnufont, "^", new Vector2(12, 90), Color.Lime);
				if (canScrollDown)
					sb.DrawString(Game1.mnufont, "v", new Vector2(12, 540), Color.Lime);

				sb.DrawString(Game1.mnufont, "Worldwide Records", new Vector2(40, 55), Color.Lime);
				if (leaderboardData[0][0] == "")
					sb.DrawString(Game1.mnufont, "There's nothing here... yet.", new Vector2(40, 100), Color.White);
				else
				{
					for (int i = 0; i < leaderboardData.Length - 1; i++)
					{
						sb.DrawString(Game1.mnufont, leaderboardData[i][0], new Vector2(40, i * 50 + 100), Color.White);
						sb.DrawString(Game1.mnufont, leaderboardData[i][1], new Vector2(200, i * 50 + 100), Color.White);
						sb.DrawString(Game1.mnufont, TimeToString(int.Parse(leaderboardData[i][2])), new Vector2(500, i * 50 + 100), Color.White);
					}
				}
				sb.DrawString(Game1.mnufont, "Your Rank", new Vector2(40, 620), Color.Yellow);
				sb.DrawString(Game1.mnufont, leaderboardData[leaderboardData.Length - 1][0] == "-1" ? "--" : leaderboardData[leaderboardData.Length - 1][0], new Vector2(40, 665), Color.Lime);
				sb.DrawString(Game1.mnufont, Game1.userName, new Vector2(200, 665), Color.Lime);
				sb.DrawString(Game1.mnufont, leaderboardData[leaderboardData.Length - 1][1] == "-1" ? "-- : -- . ---" : TimeToString(int.Parse(leaderboardData[leaderboardData.Length - 1][1])), new Vector2(500, 665), Color.Lime);
			}
			else if (Finished)
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

				if (Game1.online && canViewLeaderboards)
					sb.DrawString(Game1.mnufont, "Press L to view leaderboards", new Vector2(620, 540), Color.White);
				sb.DrawString(Game1.mnufont, (record > -1 || !recorder.playing ? "Press Enter" : "You must beat the level") + " to continue", new Vector2((record > -1 || !recorder.playing ? 670 : 516), 570), Color.White);
				sb.DrawString(Game1.mnufont, "Press W to watch replay", new Vector2(670, 600), Color.White);
				sb.DrawString(Game1.mnufont, "Press S to save replay", new Vector2(710, 630), Color.White);
				if (recorderSaved)
					sb.DrawString(Game1.mnufont, "Saved!", new Vector2(600, 630), Color.Cyan);
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

				if (Game1.online && canViewLeaderboards)
					sb.DrawString(Game1.mnufont, "Press L to view leaderboards", new Vector2(620, 570), Color.White);
				sb.DrawString(Game1.mnufont, "Press O to open a replay", new Vector2(670, 600), Color.White);
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

				// Show that playing replay
				if (recorder.playing)
					sb.DrawString(Game1.mnufont, "Replay", new Vector2(0, 30), Color.Lime);
			}
		}
		
		// Returns millisecond count in "mm:ss.sss" format
		private string TimeToString(int time) // time = Time in milliseconds
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
