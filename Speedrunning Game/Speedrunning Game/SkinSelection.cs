using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Speedrunning_Game
{
	class SkinSelection : Room
	{
		int currentSelection;
		bool upcheck, downcheck, entercheck;
		string[] choices;

		public SkinSelection()
		{
			currentSelection = 0;
			choices = new string[10];
			choices[0] = "speed runner";
			choices[1] = "squirrel";
			choices[2] = "stick figure";
			choices[3] = "adrien brody";
			choices[4] = "gabe newell";
			choices[5] = "ninja";
			upcheck = downcheck = true;
			entercheck = false;
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			if (!Keyboard.GetState().IsKeyDown(Keys.Up))
				upcheck = false;
			if (!Keyboard.GetState().IsKeyDown(Keys.Down))
				downcheck = false;
			if (!Keyboard.GetState().IsKeyDown(Keys.Enter))
				entercheck = false;

			if (Keyboard.GetState().IsKeyDown(Keys.Up) && upcheck)
				currentSelection = Math.Max(0, currentSelection - 1);
			if (Keyboard.GetState().IsKeyDown(Keys.Down) && downcheck)
				currentSelection = Math.Min(currentSelection + 1, choices.Length);

			if (Keyboard.GetState().IsKeyDown(Keys.Enter))
				Game1.LoadNewSkin(Game1.game, choices[currentSelection]);
		}

		public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
		{
			sb.Draw(Game1.backgrounds[0], new Rectangle(0, 0, 960, 720), Color.White);

			sb.DrawString(Game1.mnufont, "Skin Select", new Vector2(384, 10), Color.White);
			sb.DrawString(Game1.mnufont, "Skin Select", new Vector2(385, 11), Color.Black);

			// Draw selection stuff
		}
	}
}
