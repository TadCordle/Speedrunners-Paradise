using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Speedrunning_Game
{
	class WebStuff
	{
		public static string WebPost(string _URI, string _postString)
		{
			Stream dataStream = null;
			StreamReader reader = null;
			WebResponse response = null;
			string responseString = null;

			// Create a request using a URL that can receive a post.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_URI);
			request.KeepAlive = false;
			request.Timeout = -1;
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			// Create POST data and convert it to a byte array.
			string postData = _postString;
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);

			// Set the ContentLength property of the WebRequest.
			request.ContentLength = byteArray.Length;

			// Get the request stream.
			dataStream = request.GetRequestStream();

			// Write the data to the request stream.
			dataStream.Write(byteArray, 0, byteArray.Length);

			// Close the Stream object.
			dataStream.Flush();
			dataStream.Close();
			dataStream.Dispose();

			// Get the response.
			response = request.GetResponse();

			// Get the stream containing content returned by the server.
			dataStream = response.GetResponseStream();

			// Open the stream using a StreamReader for easy access.
			reader = new StreamReader(dataStream);

			// Read the content.
			responseString = reader.ReadToEnd();

			// Clean up the streams.
			if (reader != null)
			{
				reader.Close();
				reader.Dispose();
			}
			if (dataStream != null)
			{
				dataStream.Close();
				dataStream.Dispose();
			}
			if (response != null)
			{
				response.Close();
			}
			return responseString;
		}

		// user pass email
		public static string FindName(string name)
		{
			string postString = "Name=" + name;
			return WebPost("http://srpgame.x10.mx/checkforuser.php", postString);
		}

		public static string CreateName(string name, string password, string email)
		{
			string infoString = name + password + email + "test";
			string postString = "Name=" + name + "&Password=" + password + "&Email=" + email + "&Hash=" + HashString(infoString);
			string response = WebPost("http://srpgame.x10.mx/adduser.php", postString);
			return response.Trim();
		}
		private static string HashString(string _value)
		{
			System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] data = System.Text.Encoding.ASCII.GetBytes(_value);
			data = x.ComputeHash(data);
			string ret = "";
			for (int i = 0; i < data.Length; i++) ret += data[i].ToString("x2").ToLower();
			return ret;
		}

		public static string WriteScore(int time, string username, string levelID)
		{
			string infostring = levelID.Substring(0, 64) + username + time + "test";
			string postString = "Code=" + levelID.Substring(0, 64) + "&Username=" + username + "&Time=" + time + "&Hash=" + HashString(infostring);
			string response = WebPost("http://srpgame.x10.mx/uploadscore.php", postString);
			return response;
		}

		public static string[][] GetScores(string levelID, string username, int start)
		{
			string postString = "Code=" + levelID.Substring(0, 64) + "&Username=" + username + "&Start=" + start;
			string response = WebPost("http://srpgame.x10.mx/getscores.php", postString);
			string[] separate = response.Split('*');
			string[] rows = separate[0].Split('@');
			string[][] table = new string[rows.Length + 1][];
			for (int i = 0; i < rows.Length; i++)
				table[i] = rows[i].Split('$');
			table[rows.Length] = separate[1].Split('$');
			return table;
		}
	}
}
