using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Speedrunning_Game_Forms;

namespace Speedrunning_Game
{
	class LevelSelect : Room
	{
		private int selected;
		private bool pressDown, pressUp, pressPageDown, pressPageUp, pressLeft, pressRight, pressEnter, pressS, pressDel;
		private int maxSelected;
		private int scope;
		private int tab;
		private List<Tuple<string, int, int, bool>> levels; // Name, record, goal, custom
		private List<Tuple<string, int, int, bool>> custompage;
		private List<Tuple<string, string, string, double>> dlpage; // level name, level id, creator name, rating
		private Texture2D background;
		private bool lastpage;
		private bool showingBox;
		private string criteria;

		public LevelSelect(int tab)
		{
			pressDown = false;
			pressUp = false;
			pressEnter = false;
			pressLeft = false;
			pressRight = false;
			lastpage = false;
			pressS = false;
			showingBox = false;
			pressDel = false;

			background = Game1.backgrounds[0];
			roomHeight = 720;
			roomWidth = 960;

			levels = new List<Tuple<string, int, int, bool>>();
			selected = 0;
			scope = 0;
			this.tab = tab;

			dlpage = new List<Tuple<string, string, string, double>>();
			criteria = "";

			// Add main levels
			SimpleAES decryptor = new SimpleAES();
			if (!File.Exists("Content\\records.txt"))
			{
				StreamWriter newRecords = new StreamWriter("Content\\records.txt");
				newRecords.WriteLine(decryptor.EncryptToString("fullgame 0 -1"));
				newRecords.Flush();
				newRecords.Close();
				newRecords.Dispose();
			}
			StreamReader findMainLevels = new StreamReader("Content\\records.txt");
			while (!findMainLevels.EndOfStream)
			{
				string[] level = decryptor.DecryptString(findMainLevels.ReadLine()).Split(' ');
				if (level[1] == "0" && level[0] != "fullgame")
					levels.Add(new Tuple<string, int, int, bool>(level[0], -1, -1, false));
			}
			findMainLevels.Close();
			findMainLevels.Dispose();

			// Add custom levels
			if (!Directory.Exists("Content\\rooms"))
				Directory.CreateDirectory("Content\\rooms");
			string[] choices = Directory.GetFiles("Content\\rooms");
			foreach (string s in choices)
				levels.Add(new Tuple<string, int, int, bool>(s.Split('\\')[s.Split('\\').Length - 1].Replace(".srl", ""), -1, -1, true));

			// Find record/medal achieved in each level
			for (int i = 0; i < levels.Count; i++)
			{
				// Find record
				string name = levels[i].Item1;
				StreamReader readRecords = new StreamReader("Content\\records.txt");
				if (readRecords.EndOfStream)
				{
					readRecords.Close();
					readRecords.Dispose();
					break;
				}
				string line = decryptor.DecryptString(readRecords.ReadLine());
				while (line.Split(' ')[0] != name && !readRecords.EndOfStream)
					line = decryptor.DecryptString(readRecords.ReadLine());
				bool iscustom = true;
				if (line.Split(' ')[0] == name)
				{
					levels[i] = new Tuple<string, int, int, bool>(levels[i].Item1, int.Parse(line.Split(' ')[2]), -1, levels[i].Item4);
					iscustom = line.Split(' ')[1] == "1";
				}
				readRecords.Close();
				readRecords.Dispose();

				// Find goal times
				if (!iscustom)
				{
					if (name != "")
					{
						string findIndex = name.Split('_')[1];
						int index = int.Parse(findIndex) - 1;
						string[] goals = decryptor.DecryptString(Levels.levels[index][5]).Split(' ');
						for (int j = 2; j >= 0; j--)
						{
							if (levels[i].Item2 != -1 && levels[i].Item2 <= int.Parse(goals[j]))
								levels[i] = new Tuple<string, int, int, bool>(levels[i].Item1, levels[i].Item2, j, levels[i].Item4);
						}
						if (levels[i].Item3 == 0)
						{
							if (levels[i].Item2 != -1 && levels[i].Item2 <= int.Parse(goals[3]))
								levels[i] = new Tuple<string, int, int, bool>(levels[i].Item1, levels[i].Item2, 3, levels[i].Item4);
						}
					}
				}
				else
				{
					StreamReader findGoals = new StreamReader("Content\\rooms\\" + name + ".srl");
					for (int skip = 1; skip <= 4; skip++)
						findGoals.ReadLine();
					string[] goals = decryptor.DecryptString(findGoals.ReadLine()).Split(' ');
					for (int j = 2; j >= 0; j--)
					{
						if (levels[i].Item2 != -1 && levels[i].Item2 <= int.Parse(goals[j]))
							levels[i] = new Tuple<string, int, int, bool>(levels[i].Item1, levels[i].Item2, j, levels[i].Item4);
					}
					findGoals.Close();
					findGoals.Dispose();
				}
			}

			custompage = new List<Tuple<string, int, int, bool>>();
			var it = from Tuple<string, int, int, bool> t in levels
					 where t.Item4 == (tab == 1)
					 select t;
			foreach (Tuple<string, int, int, bool> t in it)
				custompage.Add(t);
			maxSelected = custompage.Count - 1;

			if (!Game1.playingGrass)
				MediaPlayer.Play(Game1.grassMusic);
			Game1.ResetMusic();
			Game1.playingGrass = true;
		}

		public override void Update(GameTime gameTime)
		{
			// Reset keys
			if (!Keyboard.GetState().IsKeyDown(Keys.Down))
				pressDown = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Up))
				pressUp = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Enter))
				pressEnter = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.PageDown))
				pressPageDown = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.PageUp))
				pressPageUp = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Left))
				pressLeft = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Right))
				pressRight = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.S))
				pressS = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Delete))
				pressDel = true;

			// Enter search stuff
			if (Keyboard.GetState().IsKeyDown(Keys.S) && pressS && tab == 2 && !showingBox && Game1.online)
			{
				pressS = false;
				showingBox = true;
				criteria = Microsoft.VisualBasic.Interaction.InputBox("Enter keyword for search.", "Enter Search Criteria");
				pressEnter = false;
				showingBox = false;
				if (criteria != "")
				{
					scope = 0;
					selected = 0;
					dlpage = WebStuff.GetLevels(criteria, scope * 11);
					if (dlpage.Count == 0)
					{
						System.Windows.Forms.MessageBox.Show("The search didn't yield any results.", "Search Failed");
						criteria = "";
						dlpage = WebStuff.GetLevels(criteria, scope * 11);
					}
					maxSelected = dlpage.Count - 1;
				}
				else
				{
					scope = 0;
					selected = 0;
					dlpage = WebStuff.GetLevels(criteria, scope * 11);
					maxSelected = dlpage.Count - 1;
				}
			}

			// Delete selected custom level
			if (Keyboard.GetState().IsKeyDown(Keys.Delete) && pressDel && tab == 1 && !showingBox)
			{
				pressDel = false;
				Tuple<string, int, int, bool> lvl = custompage[selected];
				levels.Remove(lvl);
				custompage.Remove(lvl);
				maxSelected--;
				string filename = "Content\\rooms\\" + lvl.Item1.Replace(' ', '_') + ".srl";
				File.Delete(filename);
			}

			// Cycle through choices when keys are pressed
			if (Keyboard.GetState().IsKeyDown(Keys.Down) && pressDown && !showingBox)
			{
				pressDown = false;
				selected++;
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Up) && pressUp && !showingBox)
			{
				pressUp = false;
				selected--;
			}

			// Cycle through pages when keys are pressed
			if (Keyboard.GetState().IsKeyDown(Keys.PageDown) && pressPageDown && !showingBox)
			{
				pressPageDown = false;
				if (tab != 2)
				{
					selected += 11;
					selected -= selected % 11;
					if (selected > maxSelected)
						selected = maxSelected;
				}
				else if (Game1.online)
				{
					List<Tuple<string, string, string, double>> check = WebStuff.GetLevels(criteria, (scope + 1) * 11);
					if (check.Count > 0)
					{
						dlpage = check;
						maxSelected = dlpage.Count - 1;
						scope++;
						selected = 0;
					}
					else
						lastpage = true;
				}
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.PageUp) && pressPageUp && !showingBox)
			{
				pressPageUp = false;
				if (tab != 2)
				{
					selected -= selected % 11;
					selected -= 11;
					if (selected < 0)
						selected = 0;
				}
				else if (Game1.online)
				{
					if (scope > 0)
					{
						selected = 0;
						scope--;
						dlpage = WebStuff.GetLevels(criteria, scope * 11);
						maxSelected = dlpage.Count - 1;
					}
				}
			}

			// Change category when arrow keys are pressed
			if (Keyboard.GetState().IsKeyDown(Keys.Left) && pressLeft && tab > 0 && !showingBox)
			{
				pressLeft = false;
				tab--;
				lastpage = false;
				if (tab == 0)
				{
					custompage.Clear();
					var it = from Tuple<string, int, int, bool> t in levels
							 where !t.Item4
							 select t;
					foreach (Tuple<string, int, int, bool> t in it)
						custompage.Add(t);
					maxSelected = custompage.Count - 1;
					selected = 0;
				}
				else if (tab == 1)
				{
					custompage.Clear();
					var it = from Tuple<string, int, int, bool> t in levels
							 where t.Item4
							 select t;
					foreach (Tuple<string, int, int, bool> t in it)
						custompage.Add(t);
					maxSelected = custompage.Count - 1;
					selected = 0;
				}
				else if (tab == 2)
				{
					scope = 0;
					dlpage = WebStuff.GetLevels(criteria, 0);
					maxSelected = dlpage.Count - 1;
				}
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Right) && pressRight && tab < 2 && !showingBox)
			{
				pressRight = false;
				tab++;
				lastpage = false;
				if (tab == 0)
				{
					custompage.Clear();
					var it = from Tuple<string, int, int, bool> t in levels
							 where !t.Item4
							 select t;
					foreach (Tuple<string, int, int, bool> t in it)
						custompage.Add(t);
					maxSelected = custompage.Count - 1;
					selected = 0;
				}
				else if (tab == 1)
				{
					custompage.Clear();
					var it = from Tuple<string, int, int, bool> t in levels
							 where t.Item4
							 select t;
					foreach (Tuple<string, int, int, bool> t in it)
						custompage.Add(t);
					maxSelected = custompage.Count - 1;
					selected = 0;
				}
				else if (tab == 2)
				{
					scope = 0;
					dlpage = WebStuff.GetLevels(criteria, 0);
					maxSelected = dlpage.Count - 1;
				}
			}

			// Bound selections
			if (selected < 0)
			{
				if (tab != 2)
					selected = 0;
				else if (Game1.online)
				{
					if (scope > 0)
					{
						dlpage = WebStuff.GetLevels(criteria, (scope - 1) * 11);
						maxSelected = dlpage.Count - 1;
						scope--;
						selected = 10;
						lastpage = false;
					}
					else
						selected = 0;
				}
			}
			else if (selected > maxSelected)
			{
				if (tab != 2)
					selected = maxSelected;
				else if (Game1.online)
				{
					if (maxSelected == 10 && !lastpage)
					{
						List<Tuple<string, string, string, double>> check = WebStuff.GetLevels(criteria, (scope + 1) * 11);
						if (check.Count > 0)
						{
							dlpage = check;
							maxSelected = dlpage.Count - 1;
							scope++;
							selected = 0;
						}
						else
						{
							lastpage = true;
							selected = maxSelected;
						}
					}
					else
						selected = maxSelected;
				}
			}

			// Set page and change background according to page
			if (tab != 2)
				scope = selected / 11;
			background = Game1.backgrounds[scope % 5];

			// Reset enter key
			if (!Keyboard.GetState().IsKeyDown(Keys.Enter))
				pressEnter = true;

			// Load selected level
			if (Keyboard.GetState().IsKeyDown(Keys.Enter) && pressEnter && !showingBox)
			{
				pressEnter = false;
				if (tab == 0)
				{
					string[] name = custompage[selected].Item1.Split('_');
					int index = int.Parse(name[1]);
					Levels.Index = index - 1;
					Game1.currentRoom = new Room(Levels.levels[Levels.Index], true, new ReplayRecorder());
				}
				else if (tab == 1)
				{
					try
					{
						Game1.currentRoom = new Room("Content\\rooms\\" + custompage[selected].Item1 + ".srl", true, new ReplayRecorder());
					}
					catch (Exception)
					{
						System.Windows.Forms.MessageBox.Show("There was a problem loading the level; the level file may not be in the correct format", "Level Load Error");
					}
				}
				else if (Game1.online)
				{
					WebStuff.DownloadFile(dlpage[selected].Item2);
					System.Windows.Forms.MessageBox.Show("The level has been downloaded and can be selected in the custom levels tab.", "Level Downloaded");
					levels.Add(new Tuple<string, int, int, bool>(dlpage[selected].Item1, -1, -1, true));
				}
			}
		}
		public override void Draw(SpriteBatch sb)
		{
			// Draw background
			sb.Draw(background, new Rectangle(0, 0, roomWidth, roomHeight), Color.White);

			// Draw scroll arrows
			if (scope > 0)
				sb.DrawString(Game1.mnufont, "^", new Vector2(12, 70), Color.Lime);
			if (scope < maxSelected / 11 && maxSelected > 10 || tab == 2 && (!lastpage || maxSelected < 10))
				sb.DrawString(Game1.mnufont, "v", new Vector2(12, 668), Color.Lime);

			// Draw header
			DrawOutlineText(sb, Game1.mnufont, "Level Select", new Vector2(265, 11), Color.White, Color.Black);
			sb.DrawString(Game1.mnufont, "<-", new Vector2(450, 10), Color.Lime);
			sb.DrawString(Game1.mnufont, "->", new Vector2(822, 10), Color.Lime);
			DrawOutlineText(sb, Game1.mnufont, "Main", new Vector2(485, 10), tab == 0 ? Color.Yellow : Color.White, Color.Black);
			DrawOutlineText(sb, Game1.mnufont, "Custom", new Vector2(565, 10), tab == 1 ? Color.Yellow : Color.White, Color.Black);
			DrawOutlineText(sb, Game1.mnufont, "Download", new Vector2(675, 10), tab == 2 ? Color.Yellow : Color.White, Color.Black);

			if (tab == 2)
				DrawOutlineText(sb, Game1.msgfont, "Press S to enter a\nstring to search", Vector2.One * 5, Color.Cyan, Color.Black);

			// Draw levels list
			if (custompage.Count == 0)
				DrawOutlineText(sb, Game1.mnufont, "You don't have any " + (tab == 0 ? "levels unlocked!" : "custom levels!"), new Vector2(50, 70), Color.White, Color.Black);
			else
				if (tab != 2)
				{
					for (int i = 0; i < custompage.Count; i++)
					{
						DrawOutlineText(sb, Game1.mnufont, custompage[i].Item1.Split('\\')[custompage[i].Item1.Split('\\').Length - 1].Replace(".srl", "").Replace("_", " "), new Vector2(50, (1 + i + i / 11) * 60 + 10 - scope * 720), i == selected ? Color.Yellow : Color.White, Color.Black);
						DrawOutlineText(sb, Game1.mnufont, custompage[i].Item2 != -1 ? TimeToString(custompage[i].Item2) : "-- : -- . ---", new Vector2(550, (1 + i + i / 11) * 60 + 10 - scope * 720), i == selected ? Color.Yellow : Color.White, Color.Black);
						if (custompage[i].Item3 != -1)
							sb.Draw(Game1.medalTex, new Vector2(670, (1 + i + i / 11) * 60 + 10 - scope * 720), custompage[i].Item3 == 0 ? Color.Gold : (custompage[i].Item3 == 1 ? Color.Silver : (custompage[i].Item3 == 2 ? Color.Brown : Color.SteelBlue)));
					}
				}
				else
				{
					if (!Game1.online)
						DrawOutlineText(sb, Game1.mnufont, "You have to log in to download levels.", new Vector2(50, 70), Color.White, Color.Black);
					else
					{
						DrawOutlineText(sb, Game1.msgfont, "Level Name", new Vector2(40, 50), Color.Lime, Color.Black);
						DrawOutlineText(sb, Game1.msgfont, "Level Author", new Vector2(420, 50), Color.Lime, Color.Black);
						DrawOutlineText(sb, Game1.msgfont, "Rating", new Vector2(660, 50), Color.Lime, Color.Black);
						for (int i = 0; i < dlpage.Count; i++)
						{
							DrawOutlineText(sb, Game1.mnufont, dlpage[i].Item1, new Vector2(50, (1 + i + i / 11) * 60 + 10), i == selected ? Color.Yellow : Color.White, Color.Black);
							DrawOutlineText(sb, Game1.mnufont, dlpage[i].Item3, new Vector2(430, (1 + i + i / 11) * 60 + 10), i == selected ? Color.Yellow : Color.White, Color.Black);
							for (int j = 1; j <= 5; j++)
								DrawOutlineText(sb, Game1.mnufont, "*", new Vector2(660 + j * 10, (1 + i + i / 11) * 60 + 15), j <= dlpage[i].Item4 ? Color.Yellow : Color.DarkGray, Color.Black);
							DrawOutlineText(sb, Game1.mnufont, dlpage[i].Item4.ToString(), new Vector2(730, (1 + i + i / 11) * 60 + 10), i == selected ? Color.Yellow : Color.White, Color.Black);
						}
					}
				}
		}

		// Returns millisecond count in "mm:ss.sss" format
		private string TimeToString(int time) // time = Time in milliseconds
		{
			TimeSpan t = TimeSpan.FromMilliseconds(time);
			return String.Format("{0:00}:{1:00}.{2:000}", (int)t.TotalMinutes, t.Seconds, t.Milliseconds % 1000);
		}
	}
}
