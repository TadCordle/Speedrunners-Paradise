using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using Speedrunning_Game_Forms;

namespace Speedrunning_Game
{
	class FGLeaderboard : Room
	{
		private string[][] leaderboardData;
		private int leaderboardPage;
		private bool canScrollDown;
		private bool upcheck;
		private bool downcheck;

		public FGLeaderboard()
		{
			if (Game1.online)
			{
				leaderboardData = WebStuff.GetScores("fullgame", Game1.userName, leaderboardPage * 10);
				canScrollDown = leaderboardData.Length == 11;
			}
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			if (Game1.online)
			{
				if (!Keyboard.GetState().IsKeyDown(Keys.Up))
					upcheck = true;
				if (!Keyboard.GetState().IsKeyDown(Keys.Down))
					downcheck = true;

				if (Keyboard.GetState().IsKeyDown(Keys.Up) && upcheck && leaderboardPage > 0)
				{
					upcheck = false;
					leaderboardPage--;
					leaderboardData = WebStuff.GetScores("fullgame", Game1.userName, leaderboardPage * 10);
					canScrollDown = true;
				}
				else if (Keyboard.GetState().IsKeyDown(Keys.Down) && downcheck && canScrollDown)
				{
					downcheck = false;
					leaderboardPage++;
					leaderboardData = WebStuff.GetScores("fullgame", Game1.userName, leaderboardPage * 10);
					canScrollDown = leaderboardData.Length == 11;
				}
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.Draw(Game1.backgrounds[0], new Rectangle(0, 0, 960, 720), Color.White);

			DrawOutlineText(sb, Game1.mnufont, "Full Game Leaderboards", new Vector2(280, 10), Color.White, Color.Black);

			if (leaderboardPage > 0)
				sb.DrawString(Game1.mnufont, "^", new Vector2(12, 90), Color.Lime);
			if (canScrollDown)
				sb.DrawString(Game1.mnufont, "v", new Vector2(12, 540), Color.Lime);

			DrawOutlineText(sb, Game1.mnufont, "Worldwide Records", new Vector2(40, 55), Color.Lime, Color.Black);
			if (!Game1.online)
				DrawOutlineText(sb, Game1.mnufont, "You must log in to view leaderboards", new Vector2(40, 100), Color.White, Color.Black);
			else
			{
				if (leaderboardData[0][0] == "")
					DrawOutlineText(sb, Game1.mnufont, "There's nothing here... yet.", new Vector2(40, 100), Color.White, Color.Black);
				else
				{
					for (int i = 0; i < leaderboardData.Length - 1; i++)
					{
						DrawOutlineText(sb, Game1.mnufont, leaderboardData[i][0], new Vector2(40, i * 50 + 100), Color.White, Color.Black);
						DrawOutlineText(sb, Game1.mnufont, leaderboardData[i][1], new Vector2(200, i * 50 + 100), Color.White, Color.Black);
						DrawOutlineText(sb, Game1.mnufont, TimeToString(int.Parse(leaderboardData[i][2])), new Vector2(500, i * 50 + 100), Color.White, Color.Black);
					}
				}
				DrawOutlineText(sb, Game1.mnufont, "Your Rank", new Vector2(40, 620), Color.Yellow, Color.Black);
				DrawOutlineText(sb, Game1.mnufont, leaderboardData[leaderboardData.Length - 1][0] == "-1" ? "--" : leaderboardData[leaderboardData.Length - 1][0], new Vector2(40, 665), Color.Lime, Color.Black);
				DrawOutlineText(sb, Game1.mnufont, Game1.userName, new Vector2(200, 665), Color.Lime, Color.Black);
				DrawOutlineText(sb, Game1.mnufont, leaderboardData[leaderboardData.Length - 1][1] == "-1" ? "-- : -- . ---" : TimeToString(int.Parse(leaderboardData[leaderboardData.Length - 1][1])), new Vector2(500, 665), Color.Lime, Color.Black);
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
