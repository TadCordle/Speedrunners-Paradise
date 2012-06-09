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
		Theme theme;
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
			wallSet = new Tileset(Game1.tileSet, 32, 32, 3, 3);
			tiles = new List<Vector3>();
			finished = false;
			goals = new int[3];
			walls = new List<Wall>();
			ziplines = new List<ZipLine>();
			boosters = new List<Booster>();
			platforms = new List<FloatingPlatform>();
			viewBox = new Rectangle(0, 0, 640, 480);
		}

		public Room(string file)
			: this()
		{
			levelName = file.Split('\\')[file.Split('\\').Length - 1].Replace(".srl", "");
			custom = true;
			// Theme
			// Room size
			// Gold, silver, bronze
			// runner [x, y]
			// wall [x, y, w, h]
			// finish [x, y]
			// zipline [x1, y1, x2, y2]
			// tile [x, y, index]
			SimpleAES decrypter = new SimpleAES();

			StreamReader levelReader = new StreamReader(file);
			string[] line;
			line = decrypter.DecryptString(levelReader.ReadLine()).Split(' ');
			switch (line[0])
			{
				case "Grass":
					theme = Theme.Grass;
					break;
				case "Lava":
					theme = Theme.Lava;
					break;
				case "Night":
					theme = Theme.Night;
					break;
				case "Cave":
					theme = Theme.Cave;
					break;
				case "Factory":
					theme = Theme.Factory;
					break;
			}
			line = decrypter.DecryptString(levelReader.ReadLine()).Split(' ');
			roomWidth = int.Parse(line[0]);
			roomHeight = int.Parse(line[1]);
			line = decrypter.DecryptString(levelReader.ReadLine()).Split(' ');
			for (int i = 0; i < 3; i++)
				goals[i] = int.Parse(line[i]);

			while (!levelReader.EndOfStream)
			{
				line = decrypter.DecryptString(levelReader.ReadLine()).Split(' ');
				if (line[0] == "runner")
				{
					runner = new Runner(new Vector2(int.Parse(line[1]), int.Parse(line[2])));
					viewBox.X = (int)runner.position.X - 288;
					viewBox.Y = (int)runner.position.Y - 208;
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
					ziplines.Add(new ZipLine(new Vector2(int.Parse(line[1]), int.Parse(line[2])), new Vector2(int.Parse(line[3]), int.Parse(line[4]))));
				else if (line[0] == "booster")
					boosters.Add(new Booster(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
				else if (line[0] == "floatingplatform")
					walls.Add(new FloatingPlatform(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
				else if (line[0] == "platformwall")
					walls.Add(new PlatformWall(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
			}
			levelReader.Dispose();

			foreach (ZipLine z in ziplines)
				z.SetPoles(this);

			levelReader = new StreamReader("Content\\records.txt");
			string nullCheck = levelReader.ReadLine();
			if (nullCheck != null)
			{
				string recordSearch = decrypter.DecryptString(nullCheck);
				if (!levelReader.EndOfStream)
				while (recordSearch.Split(' ')[0] != levelName && !levelReader.EndOfStream)
					recordSearch = decrypter.DecryptString(levelReader.ReadLine());

				if (recordSearch.Split(' ')[0] != levelName)
					record = -1;
				else
					record = int.Parse(recordSearch.Split(' ')[1]);
			}
			else
				record = -1;
			levelReader.Dispose();
		}

		public Room(string[] lines) : this()
		{
			custom = false;
			SimpleAES decryptor = new SimpleAES();
			string[] line;
			line = decryptor.DecryptString(lines[0]).Split(' ');
			switch (line[0])
			{
				case "Grass":
					theme = Theme.Grass;
					break;
				case "Lava":
					theme = Theme.Lava;
					break;
				case "Night":
					theme = Theme.Night;
					break;
				case "Cave":
					theme = Theme.Cave;
					break;
				case "Factory":
					theme = Theme.Factory;
					break;
			}
			levelName = ".MAIN.Level" + (Levels.index + 1).ToString();
			bool recordFound = false;
			StreamReader reader = new StreamReader("Content\\records.txt");
			while (!reader.EndOfStream)
			{
				string s = decryptor.DecryptString(reader.ReadLine());
				if (s.Split(' ')[0] == levelName)
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
				writer.WriteLine(decryptor.EncryptToString(levelName + " -1"));
				writer.Flush();
				writer.Dispose();
			}

			line = decryptor.DecryptString(lines[1]).Split(' ');
			roomWidth = int.Parse(line[0]);
			roomHeight = int.Parse(line[1]);
			line = decryptor.DecryptString(lines[2]).Split(' ');
			for (int i = 0; i < 3; i++)
				goals[i] = int.Parse(line[i]);

			int index = 3;
			while (index < lines.Length)
			{
				line = decryptor.DecryptString(lines[index]).Split(' ');
				if (line[0] == "runner")
				{
					runner = new Runner(new Vector2(int.Parse(line[1]), int.Parse(line[2])));
					viewBox.X = (int)runner.position.X - 288;
					viewBox.Y = (int)runner.position.Y - 208;
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
					ziplines.Add(new ZipLine(new Vector2(int.Parse(line[1]), int.Parse(line[2])), new Vector2(int.Parse(line[3]), int.Parse(line[4]))));
				else if (line[0] == "booster")
					boosters.Add(new Booster(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
				else if (line[0] == "floatingplatform")
					walls.Add(new FloatingPlatform(new Vector2(int.Parse(line[1]), int.Parse(line[2])), float.Parse(line[3]), float.Parse(line[4])));
				else if (line[0] == "platformwall")
					walls.Add(new PlatformWall(int.Parse(line[1]), int.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4])));
				index++;
			}

			foreach (ZipLine z in ziplines)
				z.SetPoles(this);

			StreamReader levelReader = new StreamReader("Content\\records.txt");
			string nullCheck = levelReader.ReadLine();
			if (nullCheck != null)
			{
				string recordSearch = decryptor.DecryptString(levelReader.ReadLine());
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
			if (!Keyboard.GetState().IsKeyDown(Keys.R))
				rcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.P))
				pcheck = true;

			if (Keyboard.GetState().IsKeyDown(Keys.R) && rcheck)
			{
				if (custom)
					Game1.currentRoom = new Room("Content\\rooms\\" + levelName + ".srl");
				else
					Game1.currentRoom = new Room(Levels.levels[Levels.index]);
			}

			if (Keyboard.GetState().IsKeyDown(Keys.P) && pcheck)
			{
				pcheck = false;
				paused = !paused;
			}

			if (!finished)
			{
				if (!paused)
				{
					foreach (Booster b in boosters)
						b.Update();
					var plats = from Wall f in walls
								where f is FloatingPlatform
								select f as FloatingPlatform;
					foreach (FloatingPlatform f in plats)
						f.Update();
					runner.Update();
					if (runner.position.X + 32 > viewBox.X + 330)
						viewBox.X = (int)runner.position.X + 32 - 330;
					else if (runner.position.X + 32 < viewBox.X + 310)
						viewBox.X = (int)runner.position.X + 32 - 310;

					if (viewBox.X < 0)
						viewBox.X = 0;
					else if (viewBox.Right > roomWidth)
						viewBox.X = roomWidth - viewBox.Width;

					if (runner.position.Y + 32 > viewBox.Y + 250)
						viewBox.Y = (int)runner.position.Y + 32 - 250;
					else if (runner.position.Y + 32 < viewBox.Y + 230)
						viewBox.Y = (int)runner.position.Y + 32 - 230;

					if (viewBox.Y < 0)
						viewBox.Y = 0;
					else if (viewBox.Bottom > roomHeight)
						viewBox.Y = roomHeight - viewBox.Height;

					if (runner.controllable)
						time += gameTime.ElapsedGameTime.Milliseconds;
				}
			}
			else
			{
				if (write)
				{
					if (time <= goals[0])
						goalBeaten = 1;
					else if (time <= goals[1])
						goalBeaten = 2;
					else if (time <= goals[2])
						goalBeaten = 3;

					write = false;
					StreamWriter writer;
					SimpleAES encryptor = new SimpleAES();

					if (time < record || record == -1)
					{
						StreamReader reader = new StreamReader("Content\\records.txt");
						writer = new StreamWriter("Content\\recordstemp.txt", false);
						bool found = false;
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
			var tilesInView = from Vector3 v in tiles
							  where v.X >= viewBox.Left - 32 && v.X <= viewBox.Right && v.Y >= viewBox.Top - 32 && v.Y <= viewBox.Bottom
							  select v;
			foreach (Vector3 v in tilesInView)
				sb.Draw(Game1.tileSet, new Rectangle((int)v.X - viewBox.X, (int)v.Y - viewBox.Y, 32, 32), wallSet.Tiles[(int)v.Z], Color.White);

			var boostersInView = from Booster b in boosters
								 where b.hitBox.Intersects(viewBox)
								 select b;
			foreach (Booster b in boostersInView)
				b.Draw(sb);

			var platsInView = from Wall f in walls
							  where f is FloatingPlatform && f.bounds.Intersects(viewBox)
							  select f as FloatingPlatform;
			foreach (FloatingPlatform f in platsInView)
				f.Draw(sb);

			if (runner != null) runner.Draw(sb);
			if (finish != null) finish.Draw(sb);

			var zipsInView = from ZipLine z in ziplines
							 where (z.pos1.X < z.pos2.X ?  
										(z.pos1.Y > z.pos2.Y ?
											new Rectangle((int)z.pos1.X, (int)z.pos2.Y, (int)z.pos2.X, (int)z.pos1.Y).Intersects(viewBox) : 
											new Rectangle((int)z.pos1.X, (int)z.pos1.Y, (int)z.pos2.X, (int)z.pos2.Y).Intersects(viewBox) ) :
										(z.pos1.Y > z.pos2.Y ?
											new Rectangle((int)z.pos2.X, (int)z.pos2.Y, (int)z.pos1.X, (int)z.pos1.Y).Intersects(viewBox) : 
											new Rectangle((int)z.pos2.X, (int)z.pos1.Y, (int)z.pos1.X, (int)z.pos2.Y).Intersects(viewBox) )
									)
							 select z;
			foreach (ZipLine z in zipsInView)
				z.Draw(sb);

			if (finished)
			{
				sb.DrawString(Game1.titlefont, "Level Complete!", new Vector2(16, 2), Color.White);
				sb.DrawString(Game1.titlefont, "Level Complete!", new Vector2(18, 4), Color.Black);

				sb.DrawString(Game1.mnufont, "Previous Record: " + (record == -1 ? "--" : TimeToString(record)), new Vector2(60, 100), Color.White);
				if (record != -1)
				{
					if (record <= goals[0])
						sb.Draw(Game1.medalTex, new Vector2(382, 100), Color.Gold);
					else if (record <= goals[1])
						sb.Draw(Game1.medalTex, new Vector2(382, 100), Color.Silver);
					else if (record <= goals[2])
						sb.Draw(Game1.medalTex, new Vector2(382, 100), Color.Brown);
				}
				sb.DrawString(Game1.mnufont, "Time: " + TimeToString(time), new Vector2(199, 130), time < record || record == -1 ? Color.Lime : Color.Red);
				if (time < record || record == -1)
					sb.DrawString(Game1.mnufont, "New Personal Record!", new Vector2(114, 160), Color.Yellow);

				sb.Draw(Game1.medalTex, new Vector2(432, 100), Color.Gold);
				sb.Draw(Game1.medalTex, new Vector2(432, 130), Color.White);
				sb.Draw(Game1.medalTex, new Vector2(432, 160), Color.Brown);
				sb.DrawString(Game1.mnufont, TimeToString(goals[0]), new Vector2(464, 100), goalBeaten == 1 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[1]), new Vector2(464, 130), goalBeaten == 2 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[2]), new Vector2(464, 160), goalBeaten == 3 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, goalBeaten != 0 ? ("You ran a " + (goalBeaten == 1 ? "gold" : (goalBeaten == 2 ? "silver" : "bronze")) + " time!") : "Do better next time!", new Vector2(202, 220), Color.Yellow);
				
				sb.DrawString(Game1.mnufont, "Press Enter to continue", new Vector2(350, 420), Color.White);
				sb.DrawString(Game1.mnufont, "Press R to restart", new Vector2(430, 450), Color.White);
			}
			else if (paused)
			{
				sb.DrawString(Game1.titlefont, "Paused ", new Vector2(40, 2), Color.White);
				sb.DrawString(Game1.titlefont, "Paused", new Vector2(42, 4), Color.Black);
				sb.DrawString(Game1.mnufont, "Current Record: " + (record == -1 ? "--" : TimeToString(record)), new Vector2(74, 100), Color.White);
				if (record != -1)
				{
					if (record <= goals[0])
						sb.Draw(Game1.medalTex, new Vector2(382, 100), Color.Gold);
					else if (record <= goals[1])
						sb.Draw(Game1.medalTex, new Vector2(382, 100), Color.Silver);
					else if (record <= goals[2])
						sb.Draw(Game1.medalTex, new Vector2(382, 100), Color.Brown);
				}
				sb.DrawString(Game1.mnufont, "Current Time: " + TimeToString(time), new Vector2(106, 130), time < record || record == -1 ? Color.Lime : Color.Red);
				sb.Draw(Game1.medalTex, new Vector2(432, 100), Color.Gold);
				sb.Draw(Game1.medalTex, new Vector2(432, 130), Color.White);
				sb.Draw(Game1.medalTex, new Vector2(432, 160), Color.Brown);
				sb.DrawString(Game1.mnufont, TimeToString(goals[0]), new Vector2(464, 100), goalBeaten == 1 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[1]), new Vector2(464, 130), goalBeaten == 2 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, TimeToString(goals[2]), new Vector2(464, 160), goalBeaten == 3 ? Color.Lime : Color.White);
				sb.DrawString(Game1.mnufont, "Press P to unpause", new Vector2(418, 420), Color.White);
				sb.DrawString(Game1.mnufont, "Press R to restart", new Vector2(430, 450), Color.White);
			}
			else
				sb.DrawString(Game1.mnufont, TimeToString(time), Vector2.Zero, Color.White);
		}
		private string TimeToString(int time)
		{
			return ((int)(time / 60000) >= 10 ? "" : "0") + ((int)(time / 60000)).ToString() + ":" + (((time - (int)(time / 60000)) % 60000 / 1000.0) >= 10 ? "" : "0") + (((time - (int)(time / 60000)) % 60000 / 1000.0f)).ToString();
		}

		public enum Theme
		{
			Grass,
			Lava,
			Night,
			Cave,
			Factory
		}
	}
}
