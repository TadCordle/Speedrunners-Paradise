using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Speedrunning_Game
{
	public class ReplayRecorder
	{
		Dictionary<int, List<string>> events;
		Dictionary<int, List<string>>.Enumerator enumerator;
		List<Tuple<Vector2, Vector2>> recallibrater;

		public Dictionary<Keys, bool> keystates;

		public bool playing, start;
		public int time;

		public ReplayRecorder()
		{
			time = 0;
			playing = false;
			start = false;

			events = new Dictionary<int, List<string>>();

			keystates = new Dictionary<Keys, bool>();
			keystates.Add(Settings.controls["MoveLeft"], false);
			keystates.Add(Settings.controls["MoveRight"], false);
			keystates.Add(Settings.controls["Jump"], false);
			keystates.Add(Settings.controls["Slide"], false);
			keystates.Add(Settings.controls["Box"], false);

			recallibrater = new List<Tuple<Vector2, Vector2>>();
		}

		public ReplayRecorder(string filename)
			: this()
		{
			playing = true;
			start = true;

			StreamReader reader = new StreamReader(filename);
			string[] line = reader.ReadLine().Replace("}", "").Split('{');
			while (line[0] != "")
			{
				int frame = int.Parse(line[0]);
				string[] evts = line[1].Split(',');
				events.Add(frame, evts.ToList());
				line = reader.ReadLine().Replace("}", "").Split('{');
			}
			while (!reader.EndOfStream)
			{
				line = reader.ReadLine().Split(' ');
				recallibrater.Add(new Tuple<Vector2, Vector2>(new Vector2(float.Parse(line[0]), float.Parse(line[1])), new Vector2(float.Parse(line[2]), float.Parse(line[3]))));
			}
		}

		public void RecordFrame(GameTime gameTime)
		{
			List<string> evt = new List<string>();
			KeyValuePair<Keys, bool>[] temp = keystates.ToArray();
			bool newEvent = false;

			foreach (KeyValuePair<Keys, bool> element in temp)
			{
				if (element.Value != Keyboard.GetState().IsKeyDown(element.Key))
				{
					newEvent = true;
					keystates[element.Key] = Keyboard.GetState().IsKeyDown(element.Key);
					string result = "";
					foreach (KeyValuePair<string, Keys> k in Settings.controls)
					{
						if (k.Value == element.Key)
							result = k.Key;
					}
					evt.Add((keystates[element.Key] ? "press " : "release ") + result.ToString());
				}
			}
			if (newEvent)
				events.Add(time, evt);

			if (time % 1000 < 10)
				recallibrater.Add(new Tuple<Vector2, Vector2>(Game1.currentRoom.Runner.position, Game1.currentRoom.Runner.velocity));

			time += gameTime.ElapsedGameTime.Milliseconds;
		}

		public void Save(string levelName)
		{
			int count = 0;
			if (!Directory.Exists("Content\\replays"))
				Directory.CreateDirectory("Content\\replays");
			while (File.Exists("Content\\replays\\" + levelName + "_" + count.ToString() + ".rpl"))
				count++;
			StreamWriter writer = new StreamWriter("Content\\replays\\" + levelName + "_" + count.ToString() + ".rpl");
			foreach (KeyValuePair<int, List<string>> element in events)
			{
				writer.Write(element.Key.ToString() + "{");
				for (int i = 0; i < element.Value.Count; i++)
					writer.Write(element.Value[i] + (i == element.Value.Count - 1 ? "" : ","));
				writer.WriteLine("}");
			}
			writer.WriteLine();
			for (int i = 0; i < recallibrater.Count; i++)
				writer.WriteLine(recallibrater[i].Item1.X + " " + recallibrater[i].Item1.Y + " " + recallibrater[i].Item2.X + " " + recallibrater[i].Item2.Y);

			writer.Flush();
			writer.Close();
			writer.Dispose();
		}

		public void PlayFrame(GameTime gameTime)
		{
			if (start)
			{
				keystates[Settings.controls["MoveLeft"]] = false;
				keystates[Settings.controls["MoveRight"]] = false;
				keystates[Settings.controls["Jump"]] = false;
				keystates[Settings.controls["Slide"]] = false;
				keystates[Settings.controls["Box"]] = false;

				time = 0;
				start = false;
				enumerator = events.GetEnumerator();
				enumerator.MoveNext();
			}

			if (enumerator.Current.Key == time)
			{
				foreach (string s in enumerator.Current.Value)
				{
					string[] split = s.Split(' ');
					Keys key = Settings.controls[split[1]];
					keystates[key] = split[0] == "press";
				}

				enumerator.MoveNext();
			}

			if (time % 1000 < 10)
			{
				Game1.currentRoom.Runner.position = recallibrater[time / 1000].Item1;
				Game1.currentRoom.Runner.velocity = recallibrater[time / 1000].Item2;
			}

			time += gameTime.ElapsedGameTime.Milliseconds;
		}
	}
}
