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

		public static AnimatedTexture guyNormal, guyRunning, guyMidair, guySliding, guyZiplining;
		public static Room currentRoom;
		public static Texture2D wallTex;
		public static Texture2D finishTex;
		public static Texture2D poleTex, lineTex;
		public static Texture2D medalTex;
		public static SpriteFont titlefont, mnufont;
		public static Texture2D tileSet;
		public static bool exit = false;

		bool pressEscape = false;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferHeight = 480;
			graphics.PreferredBackBufferWidth = 640;
			Content.RootDirectory = "Content";
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

			poleTex = Content.Load<Texture2D>("pole");
			lineTex = Content.Load<Texture2D>("pixel");

			tileSet = Content.Load<Texture2D>("tilestest");

			medalTex = Content.Load<Texture2D>("medal");

			wallTex = Content.Load<Texture2D>("pixel");
			finishTex = Content.Load<Texture2D>("finish");
			titlefont = Content.Load<SpriteFont>("titlefont");
			mnufont = Content.Load<SpriteFont>("mnufont");
			currentRoom = new MainMenu();
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
					currentRoom = new MainMenu();
			}

			currentRoom.Update(gameTime);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();
			currentRoom.Draw(spriteBatch);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
