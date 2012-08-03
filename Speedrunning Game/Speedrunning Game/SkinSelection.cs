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
			choices = new string[6];
			choices[0] = "speed runner";
			choices[1] = "squirrel";
			choices[2] = "stick figure";
			choices[3] = "mr guy";
			choices[4] = "mario";
			choices[5] = "ninja";
			upcheck = downcheck = true;
			entercheck = false;
		}

		public override void Update(GameTime gameTime)
		{
			if (!Keyboard.GetState().IsKeyDown(Keys.Up))
				upcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Down))
				downcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Enter))
				entercheck = true;

			if (Keyboard.GetState().IsKeyDown(Keys.Up) && upcheck)
			{
				currentSelection = Math.Max(0, currentSelection - 1);
				upcheck = false;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Down) && downcheck)
			{
				currentSelection = Math.Min(currentSelection + 1, choices.Length - 1);
				downcheck = false;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Enter) && entercheck)
			{
				if (Game1.skinPreviews[currentSelection] != null)
				{
					Game1.LoadNewSkin(Game1.game, choices[currentSelection]);
					Settings.SaveSettings();
					entercheck = false;
				}
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.Draw(Game1.backgrounds[0], new Rectangle(0, 0, 960, 720), Color.White);

			sb.DrawString(Game1.mnufont, "Skin Select", new Vector2(384, 10), Color.White);
			sb.DrawString(Game1.mnufont, "Skin Select", new Vector2(385, 11), Color.Black);

			for (int i = 0; i < 6; i++)
			{
				sb.Draw(Game1.skinPreviews[i] != null ? Game1.skinPreviews[i] : Game1.prevLocked, new Vector2(50, i * 80 + 100), currentSelection == i ? Color.White : Color.DarkGray);
				sb.DrawString(Game1.mnufont, choices[i], new Vector2(130, i * 80 + 120), currentSelection == i ? Color.Yellow : (Game1.selectedSkin == choices[i] ? Color.Lime : Color.White));
			}
		}
	}
}
