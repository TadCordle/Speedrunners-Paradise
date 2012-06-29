using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Game_Maker_Library;

namespace Speedrunning_Game
{
	class Mirror : Wall
	{
		public Mirror(Rectangle r) 
			: base (r)
		{
		}

		public Mirror(int x, int y, int w, int h)
			: base(x, y, w, h)
		{
		}
	}
}
