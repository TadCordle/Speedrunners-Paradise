using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Speedrunning_Game
{
	class Settings : Room
	{
		public static float musicVol, soundVol;
		public static Dictionary<string, Keys> controls = new Dictionary<string,Keys>();

		private int selectedIndex; // 0-10
		private bool entercheck, upcheck, downcheck, selected;
		private string[] selectableControl;

		public Settings()
		{
			selectedIndex = 0;
			entercheck = false;
			upcheck = true;
			downcheck = true;
			selected = false;
			selectableControl = new string[11];
			selectableControl[0] = "";
			selectableControl[1] = "";
			selectableControl[2] = "MoveLeft";
			selectableControl[3] = "MoveRight";
			selectableControl[4] = "Jump";
			selectableControl[5] = "Slide";
			selectableControl[6] = "Box";
			selectableControl[7] = "Pause";
			selectableControl[8] = "Restart";
			selectableControl[9] = "Freeroam";
			selectableControl[10] = "";
		}

		public static void GetSettings()
		{
			controls.Add("MoveLeft", Keys.Left);
			controls.Add("MoveRight", Keys.Right);
			controls.Add("Jump", Keys.Space);
			controls.Add("Slide", Keys.LeftControl);
			controls.Add("Box", Keys.LeftShift);
			controls.Add("Pause", Keys.P);
			controls.Add("Restart", Keys.R);
			controls.Add("Freeroam", Keys.F);
			if (!File.Exists("settings.txt"))
				ResetToDefault();
			else
			{
				StreamReader reader = new StreamReader("settings.txt");
				try
				{
					musicVol = float.Parse(reader.ReadLine().Split(' ')[1]);
					soundVol = float.Parse(reader.ReadLine().Split(' ')[1]);
					MediaPlayer.Volume = 0.6f * musicVol;
					Game1.run.Volume = Settings.soundVol;
					Game1.slide.Volume = Settings.soundVol;
					controls["MoveLeft"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					controls["MoveRight"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					controls["Jump"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					controls["Slide"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					controls["Box"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					controls["Pause"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					controls["Restart"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					controls["Freeroam"] = (Keys)Enum.Parse(typeof(Keys), reader.ReadLine().Split(' ')[1]);
					reader.Close();
					reader.Dispose();
				}
				catch (Exception)
				{
					reader.Close();
					reader.Dispose();
					System.Windows.Forms.MessageBox.Show("Error reading settings, resetting to default values.");
					ResetToDefault();
				}
			}
		}

		private static void ResetToDefault()
		{
			musicVol = 1f;
			soundVol = 1f;
			controls["MoveLeft"] = Keys.Left;
			controls["MoveRight"] = Keys.Right;
			controls["Jump"] = Keys.Space;
			controls["Slide"] = Keys.LeftControl;
			controls["Box"] = Keys.LeftShift;
			controls["Pause"] = Keys.P;
			controls["Restart"] = Keys.R;
			controls["Freeroam"] = Keys.F;
			StreamWriter writer = new StreamWriter("settings.txt");
			writer.WriteLine("MusicVolume " + (1f).ToString());
			writer.WriteLine("SFXVolume " + (1f).ToString());
			writer.WriteLine("MoveLeft " + Keys.Left.ToString());
			writer.WriteLine("MoveRight " + Keys.Right.ToString());
			writer.WriteLine("Jump/WallJump " + Keys.Space.ToString());
			writer.WriteLine("Slide/Zipline " + Keys.LeftControl.ToString());
			writer.WriteLine("PickUpBox " + Keys.LeftShift.ToString());
			writer.WriteLine("Pause " + Keys.P.ToString());
			writer.WriteLine("Restart/Play " + Keys.R.ToString());
			writer.WriteLine("Restart/Freeroam " + Keys.F.ToString());
			writer.Flush();
			writer.Dispose();
		}

		private static void SaveSettings()
		{
			StreamWriter writer = new StreamWriter("settings.txt");
			writer.WriteLine("MusicVolume " + musicVol.ToString());
			writer.WriteLine("SFXVolume " + soundVol.ToString());
			writer.WriteLine("MoveLeft " + controls["MoveLeft"].ToString());
			writer.WriteLine("MoveRight " + controls["MoveRight"].ToString());
			writer.WriteLine("Jump/WallJump " + controls["Jump"].ToString());
			writer.WriteLine("Slide/Zipline " + controls["Slide"].ToString());
			writer.WriteLine("PickUpBox " + controls["Box"].ToString());
			writer.WriteLine("Pause " + controls["Pause"].ToString());
			writer.WriteLine("Restart/Play " + controls["Restart"].ToString());
			writer.WriteLine("Restart/Freeroam " + controls["Freeroam"].ToString());
			writer.Flush();
			writer.Dispose();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Keyboard.GetState().IsKeyDown(Keys.Enter))
				entercheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Up))
				upcheck = true;
			if (!Keyboard.GetState().IsKeyDown(Keys.Down))
				downcheck = true;

			if (Keyboard.GetState().IsKeyDown(Keys.Up) && upcheck)
			{
				upcheck = false;
				if (!selected)
					selectedIndex = Math.Max(0, selectedIndex - 1);
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Down) && downcheck)
			{
				downcheck = false;
				if (!selected)
					selectedIndex = Math.Min(selectedIndex + 1, 10);
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Left))
			{
				if (selectedIndex == 0)
				{
					musicVol = Math.Max(0f, musicVol - 0.01f);
					MediaPlayer.Volume = 0.6f * musicVol;
					SaveSettings();
				}
				else if (selectedIndex == 1)
				{
					soundVol = Math.Max(0f, soundVol - 0.01f);
					Game1.run.Volume = Settings.soundVol;
					Game1.slide.Volume = Settings.soundVol;
					SaveSettings();
				}
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Right))
			{
				if (selectedIndex == 0)
				{
					musicVol = Math.Min(musicVol + 0.01f, 1f);
					MediaPlayer.Volume = 0.6f * musicVol;
					SaveSettings();
				}
				else if (selectedIndex == 1)
				{
					soundVol = Math.Min(soundVol + 0.01f, 1f);
					Game1.run.Volume = Settings.soundVol;
					Game1.slide.Volume = Settings.soundVol;
					SaveSettings();
				}
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Enter) && entercheck && !selected)
			{
				entercheck = false;
				if (!selected)
					if (selectableControl[selectedIndex] != "")
						selected = true;
					else if (selectedIndex == 10)
					{
						ResetToDefault();
						SaveSettings();
					}
			}

			if (selected)
			{
				if (Keyboard.GetState().GetPressedKeys().Count() > 0)
				{
					if (!(Keyboard.GetState().IsKeyDown(Keys.Enter) && !entercheck))
					{
						if (Keyboard.GetState().IsKeyDown(Keys.Enter))
							entercheck = false;
						controls[selectableControl[selectedIndex]] = Keyboard.GetState().GetPressedKeys()[0];
						SaveSettings();
						selected = false;
					}
				}
			}
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.Draw(Game1.backgrounds[0], new Rectangle(0, 0, 960, 720), Color.White);

			sb.DrawString(Game1.mnufont, "Settings", new Vector2(384, 10), Color.White);
			sb.DrawString(Game1.mnufont, "Settings", new Vector2(385, 11), Color.Black);

			sb.DrawString(Game1.mnufont, "Sound", new Vector2(56, 60), Color.Lime);
			sb.DrawString(Game1.mnufont, "Music Volume: < " + ((int)(musicVol * 100)).ToString() + "% >", new Vector2(44, 90), selectedIndex == 0 ? Color.Yellow : Color.White);
			sb.DrawString(Game1.mnufont, "SFX Volume: < " + ((int)(soundVol * 100)).ToString() + "% >", new Vector2(44, 120), selectedIndex == 1 ? Color.Yellow : Color.White);

			sb.DrawString(Game1.mnufont, "Controls", new Vector2(56, 170), Color.Lime);
			sb.DrawString(Game1.mnufont, "Move Left: " + (selected && selectedIndex == 2 ? "-Press key-" : controls["MoveLeft"].ToString()), new Vector2(44, 200),
				selectedIndex == 2 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);
			sb.DrawString(Game1.mnufont, "Move Right: " + (selected && selectedIndex == 3 ? "-Press key-" : controls["MoveRight"].ToString()), new Vector2(44, 230),
				selectedIndex == 3 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);
			sb.DrawString(Game1.mnufont, "Jump/Wall Jump: " + (selected && selectedIndex == 4 ? "-Press key-" : controls["Jump"].ToString()), new Vector2(44, 260),
				selectedIndex == 4 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);
			sb.DrawString(Game1.mnufont, "Slide/Zipline: " + (selected && selectedIndex == 5 ? "-Press key-" : controls["Slide"].ToString()), new Vector2(44, 290),
				selectedIndex == 5 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);
			sb.DrawString(Game1.mnufont, "Pick Up Box: " + (selected && selectedIndex == 6 ? "-Press key-" : controls["Box"].ToString()), new Vector2(44, 320),
				selectedIndex == 6 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);
			sb.DrawString(Game1.mnufont, "Pause: " + (selected && selectedIndex == 7 ? "-Press key-" : controls["Pause"].ToString()), new Vector2(44, 350),
				selectedIndex == 7 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);
			sb.DrawString(Game1.mnufont, "Restart/Play: " + (selected && selectedIndex == 8 ? "-Press key-" : controls["Restart"].ToString()), new Vector2(44, 380),
				selectedIndex == 8 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);
			sb.DrawString(Game1.mnufont, "Restart/Freeroam: " + (selected && selectedIndex == 9 ? "-Press key-" : controls["Freeroam"].ToString()), new Vector2(44, 410),
				selectedIndex == 9 ? (selected ? Color.Cyan : Color.Yellow) : Color.White);

			sb.DrawString(Game1.mnufont, "Reset Settings to Default", new Vector2(44, 470), selectedIndex == 10 ? Color.Yellow : Color.White);
		}
	}
}
