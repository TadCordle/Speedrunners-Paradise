using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Speedrunning_Game
{
	class MainMenu : Room
	{
		private int selected;
		private bool pressDown;
		private bool pressUp;
		private bool pressEnter;

		const int MAX_SELECTED = 4;

		public MainMenu(bool pressEnter)
		{
			this.pressEnter = pressEnter;
			this.pressDown = true;
			this.pressUp = true;
			roomHeight = 720;
			roomWidth = 960;
			Theme = LevelTheme.Grass;
			selected = 0;
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

			// Cycle through choices
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

			// Select a choice
			if (Keyboard.GetState().IsKeyDown(Keys.Enter) && pressEnter)
			{
				if (selected == 0)
				{
					SimpleAES enc = new SimpleAES();
					if (!File.Exists("Content\\records.txt"))
					{
						StreamWriter w = new StreamWriter("Content\\records.txt");
						w.WriteLine(enc.EncryptToString("fullgame 0 -1"));
						w.Flush();
						w.Dispose();
					}
					else
					{
						StreamReader r = new StreamReader("Content\\records.txt");
						string s = enc.DecryptString(r.ReadLine());
						while (s.Split(' ')[1] != "0" && s.Split(' ')[0] != "fullgame" && !r.EndOfStream)
							s = enc.DecryptString(r.ReadLine());
						if (s.Split(' ')[1] == "0" && s.Split(' ')[0] == "fullgame")
							Game1.totalRecord = int.Parse(s.Split(' ')[2]);
						else
							Game1.totalRecord = -1;
						r.Close();
						r.Dispose();
					}
					Game1.finishedSS = false;
					Levels.Index = 0;
					Game1.startTotalTime = true;
					Game1.currentRoom = new Room(Levels.levels[0], true, new ReplayRecorder());
				}
				else if (selected == 1)
					Game1.currentRoom = new LevelSelect(0);
				else if (selected == 2)
					Game1.currentRoom = new Settings();
				else if (selected == 3)
					Game1.exit = true;
			}

			if (selected < 0)
				selected = 0;
			else if (selected > MAX_SELECTED)
				selected = MAX_SELECTED;
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.Draw(Game1.backgrounds[(int)Theme], new Rectangle(0, 0, roomWidth, roomHeight), Color.White);

			sb.DrawString(Game1.titlefont, "Speed Runner's\n      Paradise", new Vector2(182, 100), Color.White);
			sb.DrawString(Game1.titlefont, "Speed Runner's\n      Paradise", new Vector2(184, 102), Color.Black);

			DrawOutlineText(sb, Game1.mnufont, "New Game", new Vector2(420, 330), selected == 0 ? Color.Yellow : Color.White, Color.Black);
			DrawOutlineText(sb, Game1.mnufont, "Select level", new Vector2(410, 400), selected == 1 ? Color.Yellow : Color.White, Color.Black);
			DrawOutlineText(sb, Game1.mnufont, "Settings", new Vector2(435, 470), selected == 2 ? Color.Yellow : Color.White, Color.Black);
			DrawOutlineText(sb, Game1.mnufont, "Exit", new Vector2(460, 540), selected == 3 ? Color.Yellow : Color.White, Color.Black);
			return;
		}

		private string TimeToString(int time) // time = Time in milliseconds
		{
			TimeSpan t = TimeSpan.FromMilliseconds(time);
			return String.Format("{0:00}:{1:00}.{2:000}", (int)t.TotalMinutes, t.Seconds, t.Milliseconds % 1000);
		}
	}
}
