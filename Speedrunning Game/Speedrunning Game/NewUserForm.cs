using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Speedrunning_Game
{
	public partial class NewUserForm : Form
	{
		public NewUserForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (txtUsername.Text.Length < 4)
			{
				MessageBox.Show("You must enter a username of 4 or more characters.");
				return;
			}
			if (txtPassword.Text == "")
			{
				MessageBox.Show("You must choose a password.");
				return;
			}
			else if (txtPassword.Text.Length < 4)
			{
				MessageBox.Show("You must choose a password with 4 or more characters.");
				return;
			}
			else if (txtConfirm.Text != txtPassword.Text)
			{
				MessageBox.Show("Your confirmed password does not match your password.");
				return;
			}
			else if (LoginForm.FindName(txtUsername.Text) != "$ $ $")
			{
				MessageBox.Show("That username already exists! Pick another.");
				return;
			}
			LoginForm.CreateName(txtUsername.Text, txtPassword.Text, txtEmail.Text);
			DialogResult = DialogResult.OK;
		}
	}
}
