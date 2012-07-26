using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Speedrunning_Game
{
	class LevelSelect : Room
	{
		private int selected;
		private bool pressDown;
		private bool pressUp;
		private bool pressEnter;
		private int maxSelected;
		private int scope;
		private List<Tuple<string, int, int, bool>> levels; // Name, record, goal, custom
		private Texture2D background;

		public LevelSelect()
		{
			pressDown = true;
			pressUp = true;
			pressEnter = false;

			background = Game1.backgrounds[0];
			roomHeight = 720;
			roomWidth = 960;

			levels = new List<Tuple<string, int, int, bool>>();
			selected = 0;
			scope = 0;

			// Add main levels
			if (!File.Exists("Content\\records.txt"))
				File.Create("Content\\records.txt");
			StreamReader findMainLevels = new StreamReader("Content\\records.txt");
			SimpleAES decryptor = new SimpleAES();
			while (!findMainLevels.EndOfStream)
			{
				string[] level = decryptor.DecryptString(findMainLevels.ReadLine()).Split(' ');
				if (level[1] == "0")
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
			maxSelected = levels.Count - 1;

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
					string findIndex = name.Split('_')[1];
					int index = int.Parse(findIndex) - 1;
					string[] goals = decryptor.DecryptString(Levels.levels[index][5]).Split(' ');
					for (int j = 2; j >= 0; j--)
					{
						if (levels[i].Item2 != -1 && levels[i].Item2 <= int.Parse(goals[j]))
							levels[i] = new Tuple<string, int, int, bool>(levels[i].Item1, levels[i].Item2, j, levels[i].Item4);
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
				}
			}

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

			// Cycle through choices when keys are pressed
			if (Keyboard.GetState().IsKeyDown(Keys.Down) && pressDown)
			{
				pressDown = false;
				selected++;
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Up) && pressUp)
			{
				pressUp = false;
				selected--;
			}

			// Bound selections
			if (selected < 0)
				selected = 0;
			else if (selected > maxSelected)
				selected = maxSelected;

			// Set page and change background according to page
			scope = selected / 11;
			background = Game1.backgrounds[scope % 5];

			// Reset enter key
			if (!Keyboard.GetState().IsKeyDown(Keys.Enter))
				pressEnter = true;

			// Load selected level
			if (Keyboard.GetState().IsKeyDown(Keys.Enter) && pressEnter)
			{
				if (!levels[selected].Item4)
				{
					string[] name = levels[selected].Item1.Split('_');
					int index = int.Parse(name[1]);
					Levels.Index = index - 1;
					Game1.currentRoom = new Room(Levels.levels[Levels.Index], true);
				}
				else
					Game1.currentRoom = new Room("Content\\rooms\\" + levels[selected].Item1 + ".srl", true);
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			// Draw background
			sb.Draw(background, new Rectangle(0, 0, roomWidth, roomHeight), Color.White);

			// Draw scroll arrows
			if (scope > 0)
				sb.DrawString(Game1.mnufont, "^", new Vector2(12, 70), Color.Lime);
			if (scope < maxSelected / 11 && maxSelected > 10)
				sb.DrawString(Game1.mnufont, "v", new Vector2(12, 668), Color.Lime);

			// Draw text
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(264, 10), Color.White);
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(265, 11), Color.Black);
			for (int i = 0; i < levels.Count; i++)
			{
				sb.DrawString(Game1.mnufont, levels[i].Item1.Split('\\')[levels[i].Item1.Split('\\').Length - 1].Replace(".srl", "").Replace("_", " "), new Vector2(50, (1 + i + i / 11) * 60 + 10 - scope * 720), i == selected ? Color.Yellow : Color.White);
				sb.DrawString(Game1.mnufont, levels[i].Item2 != -1 ? TimeToString(levels[i].Item2) : "-- : -- . ---", new Vector2(550, (1 + i + i / 11) * 60 + 10 - scope * 720), i == selected ? Color.Yellow : Color.White);
				if (levels[i].Item3 != -1)
					sb.Draw(Game1.medalTex, new Vector2(670, (1 + i + i / 11) * 60 + 10 - scope * 720), levels[i].Item3 == 0 ? Color.Gold : (levels[i].Item3 == 1 ? Color.Silver : Color.Brown));
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
