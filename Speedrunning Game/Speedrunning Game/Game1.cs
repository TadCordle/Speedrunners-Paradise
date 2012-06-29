using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public static AnimatedTexture guyNormal, guyRunning, guyMidair, guySliding, guyZiplining, guyDeadGround, guyDeadMidair;
		public static Room currentRoom;
		public static Texture2D wallTex;
		public static Texture2D finishTex;
		public static Texture2D poleTex, lineTex;
		public static Texture2D medalTex;
		public static Texture2D platformTex;
		public static Texture2D messageTex;
		public static Texture2D boxTex;
		public static AnimatedTexture boosterTex;
		public static SpriteFont titlefont, mnufont, msgfont;
		public static Texture2D[] tileSet, deathWallSet;
		public static Texture2D mirrorTex;
		public static Texture2D[] backgrounds;
		public static bool exit = false;

		private bool pressEscape = false;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferHeight = 720;
			graphics.PreferredBackBufferWidth = 960;
			Content.RootDirectory = "Content";

//			this.TargetElapsedTime = TimeSpan.FromSeconds(0.5f);

			if (!File.Exists("Content\\records.txt"))
			{
				StreamWriter w = new StreamWriter("Content\\records.txt");
				w.Flush();
				w.Dispose();
			}
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			Texture2D[] images = new Texture2D[19];
			for (int i = 1; i <= images.Length; i++)
				images[i - 1] = Content.Load<Texture2D>("character/speed runner normal00" + (i > 9 ? "" : "0") + i.ToString());
			guyNormal = new AnimatedTexture(images, 3, true, false);

			images = new Texture2D[19];
			for (int i = 1; i <= images.Length; i++)
				images[i - 1] = Content.Load<Texture2D>("character/speed runner running00" + (i > 9 ? "" : "0") + i.ToString());
			guyRunning = new AnimatedTexture(images, 1, true, false);

			images = new Texture2D[9];
			for (int i = 1; i <= images.Length; i++)
				images[i - 1] = Content.Load<Texture2D>("character/speed runner midair000" + i.ToString());
			guyMidair = new AnimatedTexture(images, 3, false, true);

			guyZiplining = new AnimatedTexture(images[images.Length - 1]);
			guySliding = new AnimatedTexture(Content.Load<Texture2D>("character/speed runner sliding"));
			guyDeadGround = new AnimatedTexture(Content.Load<Texture2D>("character/speed runner dead"));
			images = new Texture2D[25];
			for (int i = 1; i <= images.Length; i++)
				images[i - 1] = Content.Load<Texture2D>("character/speed runner dead midair00" + (i > 9 ? "" : "0") + i.ToString());
			guyDeadMidair = new AnimatedTexture(images, 1, true, false);

			finishTex = Content.Load<Texture2D>("finish");
			poleTex = Content.Load<Texture2D>("pole");
			lineTex = Content.Load<Texture2D>("pixel");
			images = new Texture2D[3];
			for (int i = 1; i <= images.Length; i++)
				images[i - 1] = Content.Load<Texture2D>("booster000" + i.ToString());
			boosterTex = new AnimatedTexture(images, 100, true, false);
			platformTex = Content.Load<Texture2D>("floating platform");
			messageTex = Content.Load<Texture2D>("message");
			boxTex = Content.Load<Texture2D>("box");

			tileSet = new Texture2D[5];
			deathWallSet = new Texture2D[5];
			backgrounds = new Texture2D[5];
			tileSet[0] = Content.Load<Texture2D>("tiles/tilegrass");
			tileSet[1] = Content.Load<Texture2D>("tiles/tilelava");
			tileSet[2] = Content.Load<Texture2D>("tiles/tilenight");
			tileSet[3] = Content.Load<Texture2D>("tiles/tilecave");
//			tileSet[4] = Content.Load<Texture2D>("tiles/tilefactory");
			tileSet[4] = Content.Load<Texture2D>("tiles/tilestest");
			deathWallSet[0] = Content.Load<Texture2D>("tiles/deathgrass");
			deathWallSet[1] = Content.Load<Texture2D>("tiles/deathlava");
//			deathWallSet[2] = Content.Load<Texture2D>("tiles/deathnight");
//			deathWallSet[3] = Content.Load<Texture2D>("tiles/deathcave");
//			deathWallSet[4] = Content.Load<Texture2D>("tiles/deathfactory");
			mirrorTex = Content.Load<Texture2D>("tiles/mirror");
			backgrounds[0] = Content.Load<Texture2D>("backgrounds/bggrass");
			backgrounds[1] = Content.Load<Texture2D>("backgrounds/bglava");
			backgrounds[2] = Content.Load<Texture2D>("backgrounds/bgnight");
			backgrounds[3] = Content.Load<Texture2D>("backgrounds/bgcave");
			backgrounds[4] = Content.Load<Texture2D>("backgrounds/bgfactory");

			medalTex = Content.Load<Texture2D>("medal");
			wallTex = Content.Load<Texture2D>("pixel");

			titlefont = Content.Load<SpriteFont>("titlefont");
			msgfont = Content.Load<SpriteFont>("msgfont");
			mnufont = Content.Load<SpriteFont>("mnufont");
			currentRoom = new MainMenu(true);
		}

		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		protected override void Update(GameTime gameTime)
		{
			if (!Keyboard.GetState().IsKeyDown(Keys.Escape))
				pressEscape = true;
			if (Keyboard.GetState().IsKeyDown(Keys.Escape) && pressEscape || exit)
			{
				pressEscape = false;
				if (currentRoom is MainMenu)
					this.Exit();
				else
					currentRoom = new MainMenu(true);
			}

			currentRoom.Update(gameTime);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin();
			currentRoom.Draw(spriteBatch);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
