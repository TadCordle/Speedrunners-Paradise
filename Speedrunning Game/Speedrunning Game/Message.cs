using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Speedrunning_Game
{
	public class Message
	{
		string msg;
		Vector2 position;

		private Rectangle hitBox;
		public Rectangle HitBox { get { return hitBox; } }

		public Message(Vector2 position, string message)
		{
			this.position = position;
			this.msg = message.Replace("<Left>", Settings.controls["MoveLeft"].ToString())
							  .Replace("<Right>", Settings.controls["MoveRight"].ToString())
							  .Replace("<Jump>", Settings.controls["Jump"].ToString())
							  .Replace("<Slide>", Settings.controls["Slide"].ToString())
							  .Replace("<Box>", Settings.controls["Box"].ToString())
							  .Replace("<Pause>", Settings.controls["Pause"].ToString())
							  .Replace("<Restart>", Settings.controls["Restart"].ToString())
							  .Replace("<Freeroam>", Settings.controls["Freeroam"].ToString());
			hitBox = new Rectangle((int)position.X, (int)position.Y, 32, 32);
		}

		public void Draw(SpriteBatch sb, Color drawHue)
		{
			sb.Draw(Game1.messageTex, new Vector2(position.X - Game1.currentRoom.ViewBox.X, position.Y - Game1.currentRoom.ViewBox.Y), drawHue);

			if (Game1.currentRoom.Runner != null && hitBox.Intersects(Game1.currentRoom.Runner.hitBox))
			{
				Rectangle drawBox = new Rectangle((int)position.X + 16 - ((int)Game1.msgfont.MeasureString(msg).X + 2) / 2 - Game1.currentRoom.ViewBox.X, (int)position.Y - 40 - (int)Game1.msgfont.MeasureString(msg).Y + 2 - Game1.currentRoom.ViewBox.Y, (int)Game1.msgfont.MeasureString(msg).X + 2, (int)Game1.msgfont.MeasureString(msg).Y + 2);
				sb.Draw(Game1.wallTex, drawBox, drawHue == Color.White ? Color.DarkGray : Color.Gray);
				sb.DrawString(Game1.msgfont, msg, new Vector2(drawBox.X, drawBox.Y), Color.Black);
			}
		}
	}
}
