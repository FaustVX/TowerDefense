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
			//args = new string[] {"741", "diagonal"};

			if (args.Length == 0)
			{

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new StartPage((nom, size, fullscreen, mode) =>
					{
						random = new Random();

						using (var game = new Game1(nom, size, fullscreen, mode))
							game.Run();
					}));
			}
			else
			{

				int seed = Environment.TickCount;
				string name = "";
				ArroundSelectMode mode = ArroundSelectMode.Round;
				int size = 20;
				bool fullscreen = true;

				if (args.Length >= 1)
				{
					if (!int.TryParse(args[0], out seed))
						seed = args[0].GetHashCode();
				}
				if (args.Length >= 2)
				{
					if (!int.TryParse(args[1], out size))
						seed = 20;
				}
				if (args.Length >= 3)
				{
					if (!Enum.TryParse(args[2], true, out mode))
						mode = ArroundSelectMode.Round;
				}
				if (args.Length >= 4)
				{
					if (bool.TryParse(args[3], out fullscreen))
						fullscreen = true;
				}
				if (args.Length >= 5)
				{
					name = args[4];
				}

				random = new Random(seed);

				using (var game = new Game1(name, size, fullscreen, mode))
					game.Run();
			}
		}
	}
#endif
}
