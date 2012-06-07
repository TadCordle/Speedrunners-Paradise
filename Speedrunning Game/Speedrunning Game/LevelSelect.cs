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
		string[] choices;

		public LevelSelect()
		{
			selected = 0;
			scope = 0;
			maxSelected = Directory.GetFiles("Content\\rooms").Length - 1;
			choices = Directory.GetFiles("Content\\rooms");
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
				Game1.currentRoom = new Room(choices[selected]);
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(264, 10), Color.White);
			sb.DrawString(Game1.mnufont, "Level Select", new Vector2(265, 11), Color.Black);
			for (int i = 0; i < choices.Length; i++)
			{
				sb.DrawString(Game1.mnufont, choices[i].Split('\\')[choices[i].Split('\\').Length - 1].Replace(".srl", "").Replace("_", " "), new Vector2(50, (1 + i + i / 7) * 60 + 10 - scope * 480), i == selected ? Color.Yellow : Color.White);
			}
		}
	}
}
