using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Game_Maker_Library;

namespace Speedrunning_Game
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		public static string userName;

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		public static Game game;
		public static AnimatedTexture guyNormal, guyRunning, guyMidair, guySliding, guyZiplining, guyDeadGround, guyDeadMidair;
		public static Texture2D[] skinPreviews;
		public static string selectedSkin;
		public static Texture2D prevLocked;
		public static Room currentRoom;
		public static Texture2D wallTex;
		public static Texture2D finishTex;
		public static Texture2D poleTex, lineTex;
		public static Texture2D medalTex;
		public static Texture2D platformTex;
		public static Texture2D messageTex;
		public static Texture2D boxTex;
		public static AnimatedTexture boosterTex;
		public static Texture2D explosionTex, rocketTex, launcherTex;
		public static Texture2D flamethrowerTex;
		public static SpriteFont titlefont, mnufont, msgfont;
		public static Texture2D[] tileSet, deathWallSet;
		public static Texture2D mirrorTex;
		public static Texture2D[] backgrounds;
		public static SoundEffect rocketLaunch, explosion;
		public static SoundEffect boost, collide, jump, damage, finish;
		public static SoundEffectInstance run, slide;
		public static Song grassMusic, lavaMusic, nightMusic, caveMusic, factoryMusic;
		public static bool playingGrass, playingLava, playingNight, playingCave, playingFactory;
		public static bool exit = false;
		public static bool online;
		public static int totalTime;
		public static int totalRecord;
		public static bool startTotalTime;
		public static bool finishedGame;

		static ContentManager skinManager;

		private bool pressEscape = false;

		public Game1(bool on)
		{
			online = on;
			game = this;
			totalTime = 0;
			startTotalTime = false;
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferHeight = 720;
			graphics.PreferredBackBufferWidth = 960;
			Content.RootDirectory = "Content";
			skinManager = new ContentManager(this.Services, "Content");
			finishedGame = false;
			totalRecord = -1;
			SimpleAES enc = new SimpleAES();
			if (!File.Exists("Content\\records.txt"))
			{
				StreamWriter w = new StreamWriter("Content\\records.txt");
				w.WriteLine(enc.EncryptToString("fullgame 0 -1"));
				w.Flush();
				w.Dispose();
			}
			else
			{
				StreamReader r = new StreamReader("Content\\records.txt");
				string s = enc.DecryptString(r.ReadLine());
				while (s.Split(' ')[1] != "0" && s.Split(' ')[0] != "fullgame" && !r.EndOfStream)
					s = enc.DecryptString(r.ReadLine());
				if (s.Split(' ')[1] == "0" && s.Split(' ')[0] == "fullgame")
					totalRecord = int.Parse(s.Split(' ')[2]);
				else
					totalRecord = -1;
				r.Close();
				r.Dispose();
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

			finishTex = Content.Load<Texture2D>("finish");
			poleTex = Content.Load<Texture2D>("pole");
			lineTex = Content.Load<Texture2D>("pixel");
			Texture2D[] images = new Texture2D[3];
			for (int i = 1; i <= images.Length; i++)
				images[i - 1] = Content.Load<Texture2D>("booster000" + i.ToString());
			boosterTex = new AnimatedTexture(images, 600, true, false);
			platformTex = Content.Load<Texture2D>("floating platform");
			messageTex = Content.Load<Texture2D>("message");
			boxTex = Content.Load<Texture2D>("box");
			explosionTex = Content.Load<Texture2D>("explosion particle");
			rocketTex = Content.Load<Texture2D>("rocket");
			launcherTex = Content.Load<Texture2D>("launcher");
			flamethrowerTex = Content.Load<Texture2D>("flamethrower");

			tileSet = new Texture2D[5];
			deathWallSet = new Texture2D[5];
			backgrounds = new Texture2D[5];
			tileSet[0] = Content.Load<Texture2D>("tiles/tilegrass");
			tileSet[1] = Content.Load<Texture2D>("tiles/tilelava");
			tileSet[2] = Content.Load<Texture2D>("tiles/tilenight");
			tileSet[3] = Content.Load<Texture2D>("tiles/tilecave");
			tileSet[4] = Content.Load<Texture2D>("tiles/tilefactory");
			deathWallSet[0] = Content.Load<Texture2D>("tiles/deathgrass");
			deathWallSet[1] = Content.Load<Texture2D>("tiles/deathlava");
			deathWallSet[2] = Content.Load<Texture2D>("tiles/deathnight");
			deathWallSet[3] = Content.Load<Texture2D>("tiles/deathcave");
			deathWallSet[4] = Content.Load<Texture2D>("tiles/deathfactory");
			mirrorTex = Content.Load<Texture2D>("tiles/mirror");
			backgrounds[0] = Content.Load<Texture2D>("backgrounds/bggrass");
			backgrounds[1] = Content.Load<Texture2D>("backgrounds/bglava");
			backgrounds[2] = Content.Load<Texture2D>("backgrounds/bgnight");
			backgrounds[3] = Content.Load<Texture2D>("backgrounds/bgcave");
			backgrounds[4] = Content.Load<Texture2D>("backgrounds/bgfactory");

			medalTex = Content.Load<Texture2D>("medal");
			wallTex = Content.Load<Texture2D>("pixel");

			skinPreviews = new Texture2D[6];
			skinPreviews[0] = Content.Load<Texture2D>("skins/speed runner/speed runner normal0001");
//			skinPreviews[1] = Content.Load<Texture2D>("skins/squirrel/squirrel normal0001");
			skinPreviews[2] = Content.Load<Texture2D>("skins/stick figure/stick figure normal0001");
			skinPreviews[3] = Content.Load<Texture2D>("skins/mr guy/mr guy normal0001");
			skinPreviews[4] = Content.Load<Texture2D>("skins/mario/mario normal");
			skinPreviews[5] = Content.Load<Texture2D>("skins/ninja/ninja normal0001");
			prevLocked = Content.Load<Texture2D>("skins/locked");

			run = Content.Load<SoundEffect>("sounds/run").CreateInstance();
			slide = Content.Load<SoundEffect>("sounds/slide").CreateInstance();
			jump = Content.Load<SoundEffect>("sounds/jump");
			boost = Content.Load<SoundEffect>("sounds/boost");
			collide = Content.Load<SoundEffect>("sounds/collision");
			finish = Content.Load<SoundEffect>("sounds/finish");
			damage = Content.Load<SoundEffect>("sounds/damage");
			rocketLaunch = Content.Load<SoundEffect>("sounds/rocket launch");
			explosion = Content.Load<SoundEffect>("sounds/explosion");

			playingGrass = true;
			playingLava = false;
			playingNight = false;
			playingCave = false;
			playingFactory = false;
			grassMusic  = Content.Load<Song>("music/grass");
			lavaMusic = Content.Load<Song>("music/lava");
			nightMusic = Content.Load<Song>("music/night");
			caveMusic = Content.Load<Song>("music/cave");
			factoryMusic = Content.Load<Song>("music/factory");

			titlefont = Content.Load<SpriteFont>("titlefont");
			msgfont = Content.Load<SpriteFont>("msgfont");
			mnufont = Content.Load<SpriteFont>("mnufont");
			currentRoom = new MainMenu(false);

			Settings.GetSettings();
			LoadNewSkin(this, Settings.skin);

			MediaPlayer.IsRepeating = true;
			MediaPlayer.Play(grassMusic);
		}
		public static void ResetMusic()
		{
			playingGrass = false;
			playingLava = false;
			playingNight = false;
			playingCave = false;
			playingFactory = false;
		}
		public static void LoadNewSkin(Game game, string skinName)
		{
			selectedSkin = skinName;
			Settings.skin = skinName;

			skinManager.Dispose();
			skinManager = new ContentManager(game.Services, "Content/skins");

			Texture2D[] images;
			if (skinName == "mario")
				guyNormal = new AnimatedTexture(skinManager.Load<Texture2D>("mario/mario normal"));
			else
			{
				images = new Texture2D[19];
				for (int i = 1; i <= images.Length; i++)
					images[i - 1] = skinManager.Load<Texture2D>(skinName + "/" + skinName + " normal00" + (i > 9 ? "" : "0") + i.ToString());
				guyNormal = new AnimatedTexture(images, 3, true, false);
			}

			if (skinName == "mario")
			{
				images = new Texture2D[3];
				for (int i = 1; i <= images.Length; i++)
					images[i - 1] = skinManager.Load<Texture2D>(skinName + "/" + skinName + " running000" + i.ToString());
				guyRunning = new AnimatedTexture(images, 6, true, false);
			}
			else
			{
				images = new Texture2D[19];
				for (int i = 1; i <= images.Length; i++)
					images[i - 1] = skinManager.Load<Texture2D>(skinName + "/" + skinName + " running00" + (i > 9 ? "" : "0") + i.ToString());
				guyRunning = new AnimatedTexture(images, 1, true, false);
			}

			images = new Texture2D[9];
			for (int i = 1; i <= images.Length; i++)
				images[i - 1] = skinManager.Load<Texture2D>(skinName + "/" + skinName + " midair000" + i.ToString());
			guyMidair = new AnimatedTexture(images, 3, false, true);

			guyZiplining = new AnimatedTexture(skinManager.Load<Texture2D>(skinName + "/" + skinName + " ziplining"));
			guySliding = new AnimatedTexture(skinManager.Load<Texture2D>(skinName + "/" + skinName + " sliding"));
			guyDeadGround = new AnimatedTexture(skinManager.Load<Texture2D>(skinName + "/" + skinName + " dead"));

			if (skinName == "mario")
				guyDeadMidair = new AnimatedTexture(skinManager.Load<Texture2D>("mario/mario dead midair"));
			else
			{
				images = new Texture2D[25];
				for (int i = 1; i <= images.Length; i++)
					images[i - 1] = skinManager.Load<Texture2D>(skinName + "/" + skinName + " dead midair00" + (i > 9 ? "" : "0") + i.ToString());
				guyDeadMidair = new AnimatedTexture(images, 1, true, false);
			}
		}

		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		protected override void Update(GameTime gameTime)
		{
//			if (Keyboard.GetState().IsKeyDown(Keys.Z))
//				this.TargetElapsedTime = TimeSpan.FromSeconds(0.5f);
//			else
//				this.TargetElapsedTime = TimeSpan.FromSeconds(0.016f);

			if (!Keyboard.GetState().IsKeyDown(Keys.Escape))
				pressEscape = true;
			if (Keyboard.GetState().IsKeyDown(Keys.Escape) && pressEscape || exit)
			{
				if (Game1.finishedGame)
				{
					Game1.finishedGame = false;
					SimpleAES enc = new SimpleAES();
					if (!File.Exists("Content\\records.txt"))
					{
						StreamWriter w = new StreamWriter("Content\\records.txt");
						w.WriteLine(enc.EncryptToString("fullgame 0 -1"));
						w.Flush();
						w.Dispose();
					}
					else
					{
						StreamReader r = new StreamReader("Content\\records.txt");
						string s = enc.DecryptString(r.ReadLine());
						while (s.Split(' ')[1] != "0" && s.Split(' ')[0] != "fullgame" && !r.EndOfStream)
							s = enc.DecryptString(r.ReadLine());
						if (s.Split(' ')[1] == "0" && s.Split(' ')[0] == "fullgame")
							totalRecord = int.Parse(s.Split(' ')[2]);
						else
							totalRecord = -1;
						r.Close();
						r.Dispose();
					}
				}
				totalTime = 0;
				startTotalTime = false;
				currentRoom.viewingLeaderboards = false;
				pressEscape = false;
				if (currentRoom is MainMenu)
					this.Exit();
				else if (currentRoom is SkinSelection)
					currentRoom = new Settings();
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
