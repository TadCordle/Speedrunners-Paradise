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
		int selected;
		bool pressDown = true;
		bool pressUp = true;
		bool pressEnter = false;
		int maxSelected;
		int scope;
		List<string> levels;

		public LevelSelect()
		{
			levels = new List<string>();
			selected = 0;
			scope = 0;
			string[] choices = Directory.GetFiles("Content\\rooms");

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

			if (selected < 0)
				selected = 0;
			else if (selected > maxSelected)
				selected = maxSelected;

			scope = selected / 7;

			if (!Keyboard.GetState().IsKeyDown(Keys.Enter))
				pressEnter = true;

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
					Levels.index = newRet - 1;
					Game1.currentRoom = new Room(Levels.levels[Levels.index]);
				}
				else
					Game1.currentRoom = new Room(levels[selected]);
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(264, 10), Color.White);
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(265, 11), Color.Black);
			for (int i = 0; i < levels.Count; i++)
			{
				sb.DrawString(Game1.mnufont, levels[i].Split('\\')[levels[i].Split('\\').Length - 1].Replace(".srl", "").Replace("_", " "), new Vector2(50, (1 + i + i / 7) * 60 + 10 - scope * 480), i == selected ? Color.Yellow : Color.White);
			}
		}
	}
}
