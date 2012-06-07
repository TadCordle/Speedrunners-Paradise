using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game_Maker_Library
{
	public class AnimatedTexture
	{
		Texture2D[] images;
		bool playOnce;
		public bool isPlaying;
		int timePerFrame, currentTime;
		public int frame;

		public AnimatedTexture(Texture2D[] images, int timePerFrame, bool isPlaying, bool playOnce)
		{
			this.isPlaying = isPlaying;
			this.images = images;
			this.timePerFrame = timePerFrame;
			this.playOnce = playOnce;
			currentTime = 0;
			frame = 0;
		}

		public AnimatedTexture(Texture2D texture)
		{
			this.images = new Texture2D[1];
			this.images[0] = texture;
			this.playOnce = true;
			this.isPlaying = false;
			this.timePerFrame = 1;
			currentTime = 0;
			frame = 0;
		}

		// Returns whether or not to call animation end event
		public bool Update()
		{
			if (isPlaying)
			{
				currentTime++;
				if (currentTime > timePerFrame)
				{
					currentTime = 0;
					frame++;
					if (frame >= images.Length)
					{
						if (playOnce)
						{
							isPlaying = false;
							return true;
						}
						frame = 0;
					}
				}
			}

			return false;
		}

		public Texture2D GetCurrentImage()
		{
			return images[currentTime];
		}

		public void Draw(SpriteBatch sb, Vector2 position, Color c)
		{
			sb.Draw(images[frame], position, c);
		}

		public void Draw(SpriteBatch sb, Vector2 position, Color c, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
			sb.Draw(images[frame], position, null, c, rotation, origin, scale, effects, layerDepth);
		}
	}
}
