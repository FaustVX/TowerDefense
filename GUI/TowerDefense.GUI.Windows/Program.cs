#region Using Statements
using System;
using System.Windows.Forms;
using CSharpHelper;

#endregion

namespace TowerDefense.GUI.Windows
{
#if WINDOWS || LINUX
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		private static Random random;

		public static Random Random
		{
			get { return random; }
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			random = new Random();

			if (args.Length == 0)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new StartPage((nom, money, size, fullscreen, mode, fancy) =>
					{
						using (var game = new Game1(nom, money, size, fullscreen, fancy))
							game.Run();
					}));
			}
			else
				using (var game = new Game1("FaustVX", 100, 20, false, true))
					game.Run();
		}
	}
#endif
}
