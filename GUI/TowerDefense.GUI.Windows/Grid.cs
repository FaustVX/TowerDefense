using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CSharpHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TowerDefense.GUI.Windows.Cells;
using TowerDefense.GUI.Windows.Textures;

namespace TowerDefense.GUI.Windows
{
	public class Grid : Grid<Cell>
	{
		private readonly Dictionary<Tower, Texture2D> _towerTextures;
		private int _offsetX, _offsetY, _sizeX, _sizeY;
		private readonly int _width, _height;
		private Cell _selectedCell;
		private IEnumerable<DirectionalCell> _path;
		private Func<IEnumerable<DirectionalCell>, Cell, IEnumerable<DirectionalCell>> _changePath;
		private readonly Func<IEnumerable<DirectionalCell>, Cell, IEnumerable<DirectionalCell>> _delegateChangePath;
		private bool _running;
		private CreateRectangle _rect;

		private delegate Rectangle CreateRectangle(int x, int y);

		/// <summary>
		/// Constructeur de la grille
		/// </summary>
		/// <param name="width">Nombre de cellules horizontalement</param>
		/// <param name="height">Nombre de cellules verticalement</param>
		/// <param name="sizeX"></param>
		/// <param name="sizeY"></param>
		/// <param name="screenWidth"></param>
		/// <param name="screenHeight"></param>
		public Grid(int width, int height, int sizeX, int sizeY, int screenWidth, int screenHeight)
			: base(width, height, (x, y, list) => new Cell(x, y, list))
		{
			_sizeX = sizeX;
			_sizeY = sizeY;

			_towerTextures = new Dictionary<Tower, Texture2D>();
			_offsetX = (screenWidth - Width * _sizeX) / 2;
			_offsetY = (screenHeight - Height * _sizeY) / 2;
			_running = true;
			_changePath = _delegateChangePath = (path, cell) =>
				{
					_changePath = (p, c) => p;

					if (Cell.Goal == null || Cell.Start == null)
						return path;

					try
					{
						return AStar(cell, Cell.Goal, (Cell p, Cell c, out int cost) =>
							{
								Point dir = new Point(c.X - p.X, c.Y - p.Y);

								if (dir.X != 0 && dir.Y != 0)
								{
									var c1 = this[c.X - dir.X, c.Y];
									var c2 = this[c.X, c.Y - dir.Y];
									bool canWalk = c.InnerCell.CanWalk && (c1.InnerCell.CanWalk && c2.InnerCell.CanWalk);

									cost = c.InnerCell.CellCost + (Math.Max(c1.InnerCell.CellCost, c2.InnerCell.CellCost));
									return canWalk;
								}

								cost = c.InnerCell.CellCost;
								return c.InnerCell.CanWalk;
							}, ArroundSelectMode.Round);
					}
					catch (Exception e)
					{
						new Thread(() => System.Windows.Forms.MessageBox.Show(e.Message)).Start();
						return new DirectionalCell[0];
					}
				};

			_rect = RectangleFactory(_offsetX, _offsetY, _sizeX, _sizeY);
		}

		public int SizeX
		{
			get { return _sizeX; }
		}

		public int SizeY
		{
			get { return _sizeY; }
		}

		public bool Running
		{
			get { return _running; }
			set { _running = value; }
		}

		public int OffsetX
		{
			get { return _offsetX; }
			set { _offsetX = value; }
		}

		public int OffsetY
		{
			get { return _offsetY; }
			set { _offsetY = value; }
		}

		public Cell SelectedCell
		{
			get { return _selectedCell; }
		}

		public void LoadContent(ContentManager content)
		{
			_towerTextures.Add(Tower.Ground, content.Load<Texture2D>("Case"));
			_towerTextures.Add(Tower.RangeGround, content.Load<Texture2D>("Range"));
			_towerTextures.Add(Tower.Tower1, content.Load<Texture2D>(@"Tower\Tower1"));
			_towerTextures.Add(Tower.Tower2, content.Load<Texture2D>(@"Tower\Tower2"));
			_towerTextures.Add(Tower.Freeze, content.Load<Texture2D>(@"Tower\Freeze"));
			_towerTextures.Add(Tower.Start, content.Load<Texture2D>("Start"));
			_towerTextures.Add(Tower.Base, content.Load<Texture2D>("Base"));
			_towerTextures.Add(Tower.Direction, content.Load<Texture2D>("Arrow"));

			new Thread(() =>
				{
					while (_running)
						_path = _changePath(_path, Cell.Start);
				}).Start();
		}

		private static CreateRectangle RectangleFactory(int offsetX, int offsetY, int width, int height)
		{
			return ((x, y) => new Rectangle(x * width + offsetX, y * height + offsetY, width, height));
		}

		public void Update()
		{
			if (!CircularMenu.Opened)
			{
				if (InputEvent.MouseState.RightButton == ButtonState.Pressed ||
					InputEvent.MouseState.MiddleButton == ButtonState.Pressed)
				{
					var move = InputEvent.MouseMove();
					_offsetX -= (int)move.X;
					_offsetY -= (int)move.Y;
					_rect = RectangleFactory(_offsetX, _offsetY, _sizeX, _sizeY);
				}
				if (InputEvent.HasScrolled())
				{
					var dir = (InputEvent.ScroolValue() > 0) ? 1 : -1;

					//_offsetX += dir * (_offsetX - _board.Width);
					//_offsetY += dir;

					_sizeX += dir;
					_sizeY += dir;
					if (_sizeX < 1)
						_sizeX = 1;
					if (_sizeY < 1)
						_sizeY = 1;

					_rect = RectangleFactory(_offsetX, _offsetY, _sizeX, _sizeY);
				}
				if (InputEvent.MousePosition().X - _offsetX < 0 || InputEvent.MousePosition().Y - _offsetY < 0)
					_selectedCell = null;
				else
					_selectedCell =
						this[(InputEvent.MousePosition().X - _offsetX) / _sizeX, (InputEvent.MousePosition().Y - _offsetY) / _sizeY];
			}
		}

		public void Draw(SpriteBatch spritebatch)
		{
			var path = RectangleFactory(_sizeX / 2 + _offsetX, _sizeY / 2 + _offsetY, _sizeX, _sizeY);
			for (int x = 0; x < Width; ++x)
				for (int y = 0; y < Height; ++y)
				{
					var cell = this[x, y];
					if (cell.InnerCell is GoalCell || cell.InnerCell is StartCell)
						spritebatch.Draw(_towerTextures[Tower.Ground],
										_rect(cell.X, cell.Y),
										(_selectedCell == cell) ? Color.Yellow : Color.White);
					spritebatch.Draw(_towerTextures[cell.InnerCell.Texture],
									_rect(cell.X, cell.Y),
									(_selectedCell == cell) ? Color.Yellow : Color.White);
				}

			if (_path != null)
				foreach (var c in _path.Where(cell => !Equals(cell, default(DirectionalCell))))
					DrawPath(c, path, spritebatch);
		}

		private static float DirectionToRad(Direction dir)
		{
			if (dir == Direction.Stay)
				return 0f;
			return (float)((int)dir * (Math.PI / 180));
		}

		private void DrawPath(DirectionalCell c, CreateRectangle path, SpriteBatch spritebatch)
		{
			if (c.Direction == Direction.Stay)
				return;
			Cell cell = c.Cell;

#if DEBUG
			if (_selectedCell == cell)
			{
				Console.WriteLine("X: {0}, Y: {1}, Dir: {2}", cell.X, cell.Y, c.Direction);
			}
#endif


			float rotation = DirectionToRad(c.Direction);
			Texture2D texture = _towerTextures[c.Direction == Direction.Stay ? Tower.Ground : Tower.Direction];
			spritebatch.Draw(texture,
							path(cell.X, cell.Y),
							null,
							(_selectedCell == cell) ? Color.YellowGreen : Color.White,
							rotation,
							new Vector2(texture.Width / 2,
										texture.Height / 2)
							, SpriteEffects.None, 0f);
#if DEBUG
			//var pos = _rect(cell.X, cell.Y);
			//_spriteBatch.DrawString(_font, c.Value.ToString(), new Vector2(pos.X, pos.Y), Color.Red);
#endif

		}

		public void RecalculatePath()
		{
			_changePath = _delegateChangePath;
		}
	}
}