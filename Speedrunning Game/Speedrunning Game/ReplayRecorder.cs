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

		public Dictionary<Keys, bool> keystates;

		public bool playing, start;
		public int frameCount;

		public ReplayRecorder()
		{
			frameCount = 0;
			playing = false;
			start = false;

			events = new Dictionary<int, List<string>>();

			keystates = new Dictionary<Keys, bool>();
			keystates.Add(Settings.controls["MoveLeft"], false);
			keystates.Add(Settings.controls["MoveRight"], false);
			keystates.Add(Settings.controls["Jump"], false);
			keystates.Add(Settings.controls["Slide"], false);
			keystates.Add(Settings.controls["Box"], false);
		}

		public ReplayRecorder(string filename) : base()
		{
			playing = true;
		}

		public void RecordFrame()
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
					evt.Add((keystates[element.Key] ? "press " : "release ") + element.Key.ToString());
				}
			}
			if (newEvent)
				events.Add(frameCount, evt);

			frameCount++;
		}

		public void Save()
		{

		}

		public void PlayFrame()
		{
			if (start)
			{
				keystates[Settings.controls["MoveLeft"]] = false;
				keystates[Settings.controls["MoveRight"]] = false;
				keystates[Settings.controls["Jump"]] = false;
				keystates[Settings.controls["Slide"]] = false;
				keystates[Settings.controls["Box"]] = false;

				frameCount = 0;
				start = false;
				enumerator = events.GetEnumerator();
				enumerator.MoveNext();
			}

			if (enumerator.Current.Key == frameCount)
			{
				foreach (string s in enumerator.Current.Value)
				{
					string[] split = s.Split(' ');
					Keys key = (Keys)Enum.Parse(typeof(Keys), split[1]);
					keystates[key] = split[0] == "press";
				}

				enumerator.MoveNext();
			}

			frameCount++;
		}
	}
}
