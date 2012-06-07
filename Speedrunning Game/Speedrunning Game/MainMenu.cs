using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Speedrunning_Game
{
	class MainMenu : Room
	{
		int selected;
		bool pressDown = true;
		bool pressUp = true;
		public bool pressEnter = true;
		int maxSelected;

		public MainMenu()
		{
			selected = 0;
			maxSelected = 2;
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
					Game1.currentRoom = new Room(Levels.levels[0]);
				else if (selected == 1)
					Game1.currentRoom = new LevelSelect();
				else if (selected == 2)
					Game1.exit = true;
			}

			if (selected < 0)
				selected = 0;
			else if (selected > maxSelected)
				selected = 2;
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.DrawString(Game1.titlefont, "Speed Runner's\n      Paradise", new Vector2(32, 0), Color.White);
			sb.DrawString(Game1.titlefont, "Speed Runner's\n      Paradise", new Vector2(34, 2), Color.Black);

			sb.DrawString(Game1.mnufont, "New game", new Vector2(270, 200), selected == 0 ? Color.Yellow : Color.White);
			sb.DrawString(Game1.mnufont, "Select level", new Vector2(260, 270), selected == 1 ? Color.Yellow : Color.White);
			sb.DrawString(Game1.mnufont, "Exit", new Vector2(310, 340), selected == 2 ? Color.Yellow : Color.White);
			return;
		}
	}
}
