using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
		private List<string> levels;
		private Texture2D background;

		public LevelSelect()
		{
			pressDown = true;
			pressUp = true;
			pressEnter = false;

			background = Game1.backgrounds[0];
			roomHeight = 720;
			roomWidth = 960;

			levels = new List<string>();
			selected = 0;
			scope = 0;
			if (!Directory.Exists("Content\\rooms"))
				Directory.CreateDirectory("Content\\rooms");
			string[] choices = Directory.GetFiles("Content\\rooms");

			if (!File.Exists("Content\\records.txt"))
				File.Create("Content\\records.txt");
			StreamReader findMainLevels = new StreamReader("Content\\records.txt");
			SimpleAES decryptor = new SimpleAES();
			while (!findMainLevels.EndOfStream)
			{
				string name = decryptor.DecryptString(findMainLevels.ReadLine()).Split(' ')[0];
				if (name.Length > 6 && name.Substring(0, 6) == ".MAIN.")
					levels.Add(name);
			}
			findMainLevels.Close();
			findMainLevels.Dispose();

			levels.AddRange(choices);
			maxSelected = levels.Count - 1;
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
				if (levels[selected].Substring(0, 6) == ".MAIN.")
				{
					int ret = 0;
					int newRet = 0;
					int index = levels[selected].Length - 1;
					while (int.TryParse(levels[selected].Substring(index), out ret))
					{
						newRet = ret;
						index--;
					}
					Levels.Index = newRet - 1;
					Game1.currentRoom = new Room(Levels.levels[Levels.Index]);
				}
				else
					Game1.currentRoom = new Room(levels[selected]);
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			// Draw background
			sb.Draw(background, new Rectangle(0, 0, roomWidth, roomHeight), Color.White);

			// Draw scroll arrows
			if (scope > 0)
				sb.DrawString(Game1.mnufont, "^", new Vector2(12, 70), Color.Lime);
			if (scope < maxSelected / 11 && maxSelected > 11)
				sb.DrawString(Game1.mnufont, "v", new Vector2(12, 668), Color.Lime);

			// Draw text
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(264, 10), Color.White);
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(265, 11), Color.Black);
			for (int i = 0; i < levels.Count; i++)
				sb.DrawString(Game1.mnufont, levels[i].Split('\\')[levels[i].Split('\\').Length - 1].Replace(".srl", "").Replace("_", " "), new Vector2(50, (1 + i + i / 11) * 60 + 10 - scope * 720), i == selected ? Color.Yellow : Color.White);
		}
	}
}
