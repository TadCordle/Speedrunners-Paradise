using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Speedrunning_Game
{
	public partial class LoginForm : Form
	{
		public LoginForm()
		{
			System.Net.ServicePointManager.Expect100Continue = false;

			InitializeComponent();
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			string name = FindName(txtUsername.Text);
			if (name != "$ $ $")
			{
				if (txtPassword.Text == name.Split(' ')[1])
				{
					Game1.userName = txtUsername.Text;
					this.DialogResult = DialogResult.OK;
				}
				else
				{
					MessageBox.Show("Your password was invalid!");
				}
			}
			else
			{
				MessageBox.Show("That username doesn't exist. Click the \"New User?\" button to create an account, or \"Play Offline\" to skip logging in.");
			}
		}

		private void btnNew_Click(object sender, EventArgs e)
		{
			NewUserForm create = new NewUserForm();
			create.ShowDialog();
		}

		private void btnOffline_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Ignore;
		}

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

		private static string HashString(string _value)
		{
			System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] data = System.Text.Encoding.ASCII.GetBytes(_value);
			data = x.ComputeHash(data);
			string ret = "";
			for (int i = 0; i < data.Length; i++) ret += data[i].ToString("x2").ToLower();
			return ret;
		}

		public static string CreateName(string name, string password, string email)
		{
			string infoString = name + password + email + "test";
			string postString = "Name=" + name + "&Password=" + password + "&Email=" + email + "&Hash=" + HashString(infoString);
			string response = WebPost("http://srpgame.x10.mx/adduser.php", postString);
			return response.Trim();
		}
	}
}
