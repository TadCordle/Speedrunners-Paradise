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
		
		private Rectangle drawBox;
		public Rectangle DrawBox { get { return drawBox; } }

		public Message(Vector2 position, string message)
		{
			this.position = position;
			this.msg = message;
			drawBox = new Rectangle((int)position.X, (int)position.Y, (int)Game1.msgfont.MeasureString(msg).X, (int)Game1.msgfont.MeasureString(msg).Y);
		}

		public void Draw(SpriteBatch sb, Color drawHue)
		{
			sb.Draw(Game1.messageTex, new Rectangle((int)position.X - 1 - Game1.currentRoom.ViewBox.X, (int)position.Y - 1 - Game1.currentRoom.ViewBox.Y, (int)Game1.msgfont.MeasureString(msg).X + 2, (int)Game1.msgfont.MeasureString(msg).Y + 2), drawHue);
			sb.DrawString(Game1.msgfont, msg, new Vector2(position.X - Game1.currentRoom.ViewBox.X, position.Y - Game1.currentRoom.ViewBox.Y), Color.Black);
		}
	}
}
