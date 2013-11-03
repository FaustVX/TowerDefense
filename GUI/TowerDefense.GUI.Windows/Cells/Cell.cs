using System;
using System.Collections.Generic;
using System.Linq;
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
		private static readonly double _s, _f;
		public static Cell Start, Goal;

		private BoardCell _innerCell;
		private readonly ICollection<Cell> _towersOwner;

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
			_towersOwner = new List<Cell>();
			InnerCell = new GroundCell(this);

			//switch (Rnd.Next(8))
			//{
			//	case 1:
			//		InnerCell = new Tower1Cell(this);
			//		break;
			//	case 2:
			//		InnerCell = new FreezeCell(this);
			//		break;
			//	default:
			//		InnerCell = new GroundCell(this);
			//		break;
			//}

			if (x == 0)
			{
				if (y == (int)(_s * list.Height))
				{
					InnerCell = new StartCell(this);
					Start = this;
				}
			}
			else if (x == list.Width - 1)
			{
				if (y == (int)(_f * list.Height))
				{
					InnerCell = new GoalCell(this);
					Goal = this;
				}
			}
		}

		public BoardCell InnerCell
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

		public IEnumerable<Cell> Owners
		{
			get { return _towersOwner; }
		}

		public bool HasOwner
		{
			get { return _towersOwner.Count > 0; }
		}

		public void AddTowerOwner(Cell tower)
		{
			_towersOwner.Add(tower);
		}

		public void RemoveTowerOwner(Cell tower)
		{
			_towersOwner.Remove(tower);
		}
	}

	public class Information
	{
		public Information()
		{
		}
	}

	public class Menu
	{
		private readonly Tower _texture;
		private readonly Func<Cell, Player, Action> _action;
		private readonly string _text;
		private readonly int _money;

		public Menu(Tower texture, Func<Cell, Player, Action> action, int money, string text)
		{
			_texture = texture;
			_action = action;
			_text = text;
			_money = money;
		}

		public Tower Texture
		{
			get { return _texture; }
		}

		public Func<Cell, Player, Action> Action
		{
			get { return _action; }
		}

		public string Text
		{
			get { return _text; }
		}

		public int Money
		{
			get { return _money; }
		}
	}

	public abstract class BoardCell
	{
		private readonly Cell _cell;

		protected BoardCell(Cell cell)
		{
			_cell = cell;
		}

		public Cell Cell
		{
			get { return _cell; }
		}
		public abstract bool CanWalk { get; }
		public abstract Tower Texture { get; }
		public abstract string Name { get; }
		public abstract int CellCost { get; }
		public abstract IEnumerable<Menu> Menu { get; }
	}

	public class FreezeCell : TowerCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly Information _info;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;
		private static readonly int _range;

		static FreezeCell()
		{
			_texture = Tower.Freeze;
			_name = "Freezer";
			_canWalk = true;
			_cellCost = 10;
			_range = 2;

			_info = new Information();

			_menu = new Menu[]
				{
					new Menu(Tower.Ground, (cell, player) => () =>
						{
							foreach (Cell c in ((TowerCell)cell.InnerCell).Cells)
								c.RemoveTowerOwner(cell);
							cell.InnerCell = new GroundCell(cell);
						}, 20, "Supprimer")
				};
		}

		public FreezeCell(Cell cell)
			: base(cell, _range)
		{
		}

		public override bool CanWalk
		{
			get { return _canWalk; }
		}

		public override Tower Texture
		{
			get { return _texture; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override int CellCost
		{
			get { return _cellCost; }
		}

		public override Information Information
		{
			get { return _info; }
		}

		public override IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	public abstract class TowerCell : BoardCell
	{
		private static readonly bool _canWalk;
		private static readonly int _cellCost;
		private readonly int _range;
		private readonly IEnumerable<Cell> _cells;

		static TowerCell()
		{
			_canWalk = false;
			_cellCost = 0;
		}

		protected TowerCell(Cell cell, int range) : base(cell)
		{
			_range = range;
			_cells = Cell.Range(range);

			foreach (var cell1 in _cells.Select(c=>c.InnerCell))
			{
				cell1.Cell.AddTowerOwner(cell);
			}
		}

		public override bool CanWalk
		{
			get { return _canWalk; }
		}

		public override int CellCost
		{
			get { return _cellCost; }
		}

		public int Range
		{
			get { return _range; }
		}

		public IEnumerable<Cell> Cells
		{
			get { return _cells; }
		}

		public abstract Information Information
		{
			get;
		}
	}

	public class Tower1Cell : TowerCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly Information _info;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _range;

		static Tower1Cell()
		{
			_texture = Tower.Tower1;
			_name = "Tour 1";
			_range = 1;

			_info = new Information();

			_menu = new Menu[]
				{
					new Menu(Tower.Tower2, (cell, player) => () =>
						{
							foreach (Cell c in ((TowerCell)cell.InnerCell).Cells)
								c.RemoveTowerOwner(cell);
							cell.InnerCell = new Tower2Cell(cell);
						}, -30, "Upgrade"),
					new Menu(Tower.Ground, (cell, player) => () =>
						{
							foreach (Cell c in ((TowerCell)cell.InnerCell).Cells)
								c.RemoveTowerOwner(cell);
							cell.InnerCell = new GroundCell(cell);
						}, 10, "Supprimer")
				};
		}

		public Tower1Cell(Cell cell)
			: base(cell, _range)
		{
		}

		public override Tower Texture
		{
			get { return _texture; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}

		public override Information Information
		{
			get { return _info; }
		}
	}

	public class Tower2Cell : TowerCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly Information _info;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _range;

		static Tower2Cell()
		{
			_texture = Tower.Tower2;
			_name = "Tour 2";
			_range = 3;

			_info = new Information();

			_menu = new Menu[]
				{
					new Menu(Tower.Ground, (cell, player) => () =>
						{
							foreach (Cell c in ((TowerCell)cell.InnerCell).Cells)
								c.RemoveTowerOwner(cell);
							cell.InnerCell = new GroundCell(cell);
						}, 25, "Supprimer")
				};
		}

		public Tower2Cell(Cell cell)
			: base(cell, _range)
		{
		}

		public override Tower Texture
		{
			get { return _texture; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}

		public override Information Information
		{
			get { return _info; }
		}
	}

	public class StartCell : BoardCell
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

		public StartCell(Cell cell)
			: base(cell)
		{
		}

		public override bool CanWalk
		{
			get { return _canWalk; }
		}

		public override Tower Texture
		{
			get { return _texture; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override int CellCost
		{
			get { return _cellCost; }
		}

		public override IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	public class GoalCell : BoardCell
	{
		private static readonly IEnumerable<Menu> _menu;
		private static readonly bool _canWalk;
		private static readonly Tower _texture;
		private static readonly string _name;
		private static readonly int _cellCost;

		static GoalCell()
		{
			_texture = Tower.Base;
			_name = "Arrivée";
			_canWalk = true;
			_cellCost = 0;

			_menu = new Menu[]
				{
				};
		}

		public GoalCell(Cell cell)
			: base(cell)
		{
		}

		public override bool CanWalk
		{
			get { return _canWalk; }
		}

		public override Tower Texture
		{
			get { return _texture; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override int CellCost
		{
			get { return _cellCost; }
		}

		public override IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}

	public class GroundCell : BoardCell
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
					new Menu(Tower.Tower1, (cell, player) => () => cell.InnerCell = new Tower1Cell(cell), -15, "Tour 1"),
					new Menu(Tower.Freeze, (cell, player) => () => cell.InnerCell = new FreezeCell(cell), -20, "Freezer")
				};
		}

		public GroundCell(Cell cell)
			: base(cell)
		{
		}

		public override bool CanWalk
		{
			get { return _canWalk; }
		}

		public override Tower Texture
		{
			get { return Cell.HasOwner ? Tower.RangeGround : _texture; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override int CellCost
		{
			get { return _cellCost; }
		}

		public override IEnumerable<Menu> Menu
		{
			get { return _menu; }
		}
	}
}