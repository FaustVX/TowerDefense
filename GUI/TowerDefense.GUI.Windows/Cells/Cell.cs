using System;
using System.Collections.Generic;
using CSharpHelper;
using TowerDefense.GUI.Windows.Textures;

namespace TowerDefense.GUI.Windows.Cells
{
#if (DEBUG)
	using System.Diagnostics;
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
		private static readonly Random Rnd;
		private IBoardCell _innerCell;
		private static readonly double _s, _f;
		public static Cell Start, Goal;

		static Cell()
		{
			Rnd = Program.Random;

			_s = Rnd.NextDouble();
			_f = Rnd.NextDouble();
		}

		/// <summary>
		/// Constructeur de la  cellule
		/// </summary>
		/// <param name="x">position en X</param>
		/// <param name="y">position en Y</param>
		/// <param name="list"></param>
		public Cell(int x, int y, Grid<Cell> list)
			: base(x, y, list, false)
		{
			InnerCell = new GroundCell();

			switch (Rnd.Next(8))
			{
				case 1:
					InnerCell = new Tower1Cell();
					break;
				case 2:
					InnerCell = new FreezeCell();
					break;
				default:
					InnerCell = new GroundCell();
					break;
			}

			if (x == 0)
			{
				if (y == (int)(_s * list.Height))
				{
					InnerCell = new StartCell();
					Start = this;
				}
			}
			else if (x == list.Width - 1)
			{
				if (y == (int)(_f * list.Height))
				{
					InnerCell = new GoalCell();
					Goal = this;
				}
			}
			//else if (x <= 3 && y <= 3)
			//	InnerCell = new GroundCell();
			//else if (x >= list.Width - 4 && y >= list.Height - 4)
			//	InnerCell = new GroundCell();
			//else
			//{
			//	switch (Rnd.Next(8))
			//	{
			//		case 1:
			//			InnerCell = new Tower1Cell();
			//			break;
			//		case 2:
			//			InnerCell = new Tower2Cell();
			//			break;
			//		case 3:
			//			InnerCell = new FreezeCell();
			//			break;
			//		default:
			//			InnerCell = new GroundCell();
			//			break;
			//	}
			//}
		}

		public IBoardCell InnerCell
		{
			get { return _innerCell; }
			set { _innerCell = value; }
		}

		public int CellCost
		{
			get { return InnerCell.CellCost; }
		}

		public bool CanWalk
		{
			get { return InnerCell.CanWalk; }
		}

		//public CircularMenu Menu
		//{
		//	get { return InnerCell.Menu; }
		//}
	}

	public class Menu
	{
		private readonly Tower _texture;
		private readonly Func<Cell, Action> _action;
		private readonly string _text;

		public Menu(Tower texture, Func<Cell, Action> action, string text)
		{
			_texture = texture;
			_action = action;
			_text = text;
		}

		public Tower Texture
		{
			get { return _texture; }
		}

		public Func<Cell, Action> Action
		{
			get { return _action; }
		}

		public string Text
		{
			get { return _text; }
		}
	}


	public interface IBoardCell
	{
		bool CanWalk
		{
			get;
		}

		Tower Texture
		{
			get;
		}

		string Name
		{
			get;
		}

		int CellCost
		{
			get;
		}

		IEnumerable<Menu> Menu { get; }
	}



	public class FreezeCell : IBoardCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;

		static FreezeCell()
		{
			_texture = Tower.Freeze;
			_name = "Freezer";
			_canWalk = true;
			_cellCost = 10;

			_menu = new Menu[]
				{
					new Menu(Tower.Ground, cell => () => cell.InnerCell = new GroundCell(), "Supprimer")
				};
		}

		public bool CanWalk
		{
			get { return _canWalk; }
		}

		public Tower Texture
		{
			get { return _texture; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int CellCost
		{
			get { return _cellCost; }
		}

		public  IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	class Tower1Cell : IBoardCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;

		static Tower1Cell()
		{
			_texture = Tower.Tower1;
			_name = "Tour 1";
			_canWalk = false;
			_cellCost = 0;

			_menu = new Menu[]
				{
					new Menu(Tower.Tower2, cell => () => cell.InnerCell = new Tower2Cell(), "Upgrade"),
					new Menu(Tower.Ground, cell => () => cell.InnerCell = new GroundCell(), "Supprimer")
				};
		}

		public bool CanWalk
		{
			get { return _canWalk; }
		}

		public Tower Texture
		{
			get { return _texture; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int CellCost
		{
			get { return _cellCost; }
		}

		public  IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	class Tower2Cell : IBoardCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;

		static Tower2Cell()
		{
			_texture = Tower.Tower2;
			_name = "Tour 2";
			_canWalk = false;
			_cellCost = 0;

			_menu = new Menu[]
				{
					new Menu(Tower.Ground, cell => () => cell.InnerCell = new GroundCell(), "Supprimer")
				};
		}

		public bool CanWalk
		{
			get { return _canWalk; }
		}

		public Tower Texture
		{
			get { return _texture; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int CellCost
		{
			get { return _cellCost; }
		}

		public  IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	class StartCell : IBoardCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;

		static StartCell()
		{
			_texture = Tower.Start;
			_name = "Départ";
			_canWalk = true;
			_cellCost = 0;

			_menu = new Menu[]
				{
				};
		}

		public  bool CanWalk
		{
			get { return true; }
		}

		public  Tower Texture
		{
			get { return Tower.Start; }
		}

		public  string Name
		{
			get { return "Start"; }
		}

		public  int CellCost
		{
			get { return 0; }
		}

		public  IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	class GoalCell : IBoardCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;

		static GoalCell()
		{
			_texture = Tower.Goal;
			_name = "Arrivée";
			_canWalk = true;
			_cellCost = 0;

			_menu = new Menu[]
				{
				};
		}

		public bool CanWalk
		{
			get { return _canWalk; }
		}

		public Tower Texture
		{
			get { return _texture; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int CellCost
		{
			get { return _cellCost; }
		}

		public IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	class GroundCell : IBoardCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;

		static GroundCell()
		{
			_texture = Tower.Ground;
			_name = "Sol";
			_canWalk = true;
			_cellCost = 0;

			_menu = new Menu[]
				{
					new Menu(Tower.Tower1, cell => () => cell.InnerCell = new Tower1Cell(), "Tour 1"),
					new Menu(Tower.Freeze, cell => () => cell.InnerCell = new FreezeCell(), "Freezer")
				};
		}

		public bool CanWalk
		{
			get { return _canWalk; }
		}

		public Tower Texture
		{
			get { return _texture; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int CellCost
		{
			get { return _cellCost; }
		}

		public  IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}
}