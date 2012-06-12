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
		public bool IsPlaying { get; set; }
		public int Frame { get; set; }
		public int Length
		{
			get { return images.Length; }
		}

		private Texture2D[] images;
		private bool playOnce;
		private int timePerFrame, currentTime;

		public AnimatedTexture(Texture2D[] images, int timePerFrame, bool isPlaying, bool playOnce)
		{
			this.IsPlaying = isPlaying;
			this.images = images;
			this.timePerFrame = timePerFrame;
			this.playOnce = playOnce;
			currentTime = 0;
			Frame = 0;
		}

		public AnimatedTexture(Texture2D texture)
		{
			this.images = new Texture2D[1];
			this.images[0] = texture;
			this.playOnce = true;
			this.IsPlaying = false;
			this.timePerFrame = 1;
			currentTime = 0;
			Frame = 0;
		}

		// Returns whether or not to call animation end event
		public bool Update()
		{
			if (IsPlaying)
			{
				currentTime++;
				if (currentTime > timePerFrame)
				{
					currentTime = 0;
					Frame++;
					if (Frame >= images.Length)
					{
						if (playOnce)
						{
							IsPlaying = false;
							return true;
						}
						Frame = 0;
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
			sb.Draw(images[Frame], position, c);
		}

		public void Draw(SpriteBatch sb, Vector2 position, Color c, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
			sb.Draw(images[Frame], position, null, c, rotation, origin, scale, effects, layerDepth);
		}
	}
}
