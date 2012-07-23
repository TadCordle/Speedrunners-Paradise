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
			string name = WebStuff.FindName(txtUsername.Text);
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

	}
}
