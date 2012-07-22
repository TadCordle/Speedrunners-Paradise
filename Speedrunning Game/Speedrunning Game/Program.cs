using System;
using System.Windows.Forms;

namespace Speedrunning_Game
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
			LoginForm login = new LoginForm();
			DialogResult result = login.ShowDialog();
			if (result == DialogResult.OK || result == DialogResult.Ignore)
			{
				using (Game1 game = new Game1(result == DialogResult.OK))
				{
					game.Run();
				}
			}
        }
    }
}

