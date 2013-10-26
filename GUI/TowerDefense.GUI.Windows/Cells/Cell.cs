using System;
using System.Diagnostics;
using CSharpHelper;

namespace TowerDefense.GUI.Windows.Cells
{
#if (DEBUG)
	public class CellProxy
	{
		private readonly Cell _cell;

		public CellProxy(Cell cell)
		{
			_cell = cell;
		}

		public string CellName
		{
			get { return _cell.InnerCell.Name; }
		}

		public bool CanWalk
		{
			get { return _cell.InnerCell.CanWalk; }
		}

		public string Position
		{
			get { return string.Format("X: {0}, Y: {1}", _cell.X, _cell.Y); }
		}
	}

	[DebuggerDisplay("Type: {InnerCell.Name}, Pos: {X}, {Y}")]
	[DebuggerTypeProxy(typeof (CellProxy))]
#endif
	public class Cell : Cell<Cell>, Grid<Cell>.IPathCell
	{
		private static readonly Random Rnd = new Random();
		private BoardCell _innerCell;
		//private static readonly double s, f;
		//private static readonly bool os, of;
		public static Cell Start, Goal;

		//static Cell()
		//{
		//	//s = Rnd.NextDouble();
		//	//f = Rnd.NextDouble();
		//	//os = Rnd.Next(20) < 10;
		//	//of = Rnd.Next(20) < 10;
		//}

		/// <summary>
		/// Constructeur de la  cellule
		/// </summary>
		/// <param name="x">position en X</param>
		/// <param name="y">position en Y</param>
		/// <param name="list"></param>
		public Cell(int x, int y, Grid<Cell> list)
			: base(x, y, list, false)
		{
			if (x == 0 && y == 0)
			{
				InnerCell = new StartCell();
				Start = this;
			}
			else if (x == list.Width - 1 && y == list.Height - 1)
			{
				InnerCell = new GoalCell();
				Goal = this;
			}
			else if (x <= 3 && y <= 3)
				InnerCell = new GroundCell();
			else if (x >= list.Width - 4 && y >= list.Height - 4)
				InnerCell = new GroundCell();
			else
			{
				//InnerCell = new GroundCell();
				switch (Rnd.Next(8))
				{
					case 1:
						InnerCell = new Tower1Cell();
						break;
					case 2:
						InnerCell = new Tower2Cell();
						break;
					case 3:
						InnerCell = new FreezeCell();
						break;
					default:
						InnerCell = new GroundCell();
						break;
				}
			}
		}

		public BoardCell InnerCell
		{
			get { return _innerCell; }
			set { _innerCell = value; }
		}

		public void SwitchCell()
		{
			if (InnerCell is GroundCell)
				switch (Rnd.Next(1, 4))
				{
					case 1:
						_innerCell = new Tower1Cell();
						break;
					case 2:
						_innerCell = new Tower2Cell();
						break;
					case 3:
						InnerCell = new FreezeCell();
						break;
				}
			else
				_innerCell = new GroundCell();
		}

		public int CellCost
		{
			get { return InnerCell.CellCost; }
		}

		public bool CanWalk
		{
			get { return InnerCell.CanWalk; }
		}
	}

	public class FreezeCell : BoardCell
	{
		public override bool CanWalk
		{
			get { return false; }
		}

		public override Texture Texture
		{
			get { return Texture.Freeze; }
		}

		public override string Name
		{
			get { return "Freezer"; }
		}

		public override int CellCost
		{
			get { return 10; }
		}
	}

	public abstract class BoardCell
	{
		public abstract bool CanWalk
		{
			get;
		}

		public abstract Texture Texture
		{
			get;
		}

		public abstract string Name
		{
			get;
		}

		public abstract int CellCost
		{
			get;
		}
	}

	class Tower1Cell : BoardCell
	{
		public override bool CanWalk
		{
			get { return false; }
		}

		public override Texture Texture
		{
			get { return Texture.Tower1; }
		}

		public override string Name
		{
			get { return "Tower 1"; }
		}

		public override int CellCost
		{
			get { return 0; }
		}
	}

	class Tower2Cell : BoardCell
	{
		public override bool CanWalk
		{
			get { return false; }
		}

		public override Texture Texture
		{
			get { return Texture.Tower2; }
		}

		public override string Name
		{
			get { return "Tower 2"; }
		}

		public override int CellCost
		{
			get { return 15; }
		}
	}

	class StartCell : BoardCell
	{
		public override bool CanWalk
		{
			get { return true; }
		}

		public override Texture Texture
		{
			get { return Texture.Start; }
		}

		public override string Name
		{
			get { return "Start"; }
		}

		public override int CellCost
		{
			get { return 0; }
		}
	}

	class GoalCell : BoardCell
	{
		public override bool CanWalk
		{
			get { return false; }
		}

		public override Texture Texture
		{
			get { return Texture.Goal; }
		}

		public override string Name
		{
			get { return "Goal"; }
		}

		public override int CellCost
		{
			get { return 0; }
		}
	}

	class GroundCell : BoardCell
	{
		public override bool CanWalk
		{
			get { return true; }
		}

		public override Texture Texture
		{
			get { return Texture.Ground; }
		}

		public override string Name
		{
			get { return "Ground"; }
		}

		public override int CellCost
		{
			get { return 0; }
		}
	}
}