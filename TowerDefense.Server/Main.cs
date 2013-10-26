using System;
using CSharpHelper;

namespace TowerDefense.Server
{
	public class Cell:Cell<Cell>
	{
		public Cell (int x, int y, Grid<Cell> list):
			base(x, y, list, false)
		{
			
		}
	}

	public class Grid:Grid<Cell>
	{
		public Grid (int x, int y)
			: base(x, y, CreateCell)
		{
			
		}
			
		private new static Cell CreateCell(int x, int y, Grid<Cell> list)
		{
			return new Cell(x, y, list);
		}
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			//Grid grid=new Grid(5, 5);

			Console.WriteLine ("Hello World!");
			Console.Read ();
		}
	}
}
