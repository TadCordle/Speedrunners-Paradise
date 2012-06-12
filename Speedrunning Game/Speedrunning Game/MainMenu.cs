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
		private int selected;
		private bool pressDown;
		private bool pressUp;
		private bool pressEnter;

		const int MAX_SELECTED = 3;

		public MainMenu(bool pressEnter)
		{
			this.pressEnter = pressEnter;
			this.pressDown = true;
			this.pressUp = true;
			roomHeight = 720;
			roomWidth = 960;
			Theme = LevelTheme.Grass;
			selected = 0;
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
//				else if (selected == 2)
//					Game1.currentRoom = new HelpRoom();
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

			sb.DrawString(Game1.mnufont, "New game", new Vector2(420, 330), selected == 0 ? Color.Yellow : Color.White);
			sb.DrawString(Game1.mnufont, "Select level", new Vector2(410, 400), selected == 1 ? Color.Yellow : Color.White);
			sb.DrawString(Game1.mnufont, "Help", new Vector2(456, 470), selected == 2 ? Color.Yellow : Color.White);
			sb.DrawString(Game1.mnufont, "Exit", new Vector2(460, 540), selected == 3 ? Color.Yellow : Color.White);
			return;
		}
	}
}
