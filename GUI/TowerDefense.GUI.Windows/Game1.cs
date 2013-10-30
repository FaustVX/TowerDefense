#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CSharpHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TowerDefense.GUI.Windows.Cells;
using TowerDefense.GUI.Windows.Textures;

#endregion

namespace TowerDefense.GUI.Windows
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private readonly Dictionary<Tower, Texture2D> _towerTextures;
		private readonly Dictionary<Textures.Ship, Texture2D> _shipTextures;
		private readonly Grid _board;
		private readonly int _offsetX, _offsetY, _sizeX, _sizeY, _width, _height;
		private Cell _selectedCell;
		private readonly CircularMenu _menu;

		private MouseState _mouseState, _oldMouseState;
		private KeyboardState _keyboardState, _oldKeyboardState;

		private delegate Rectangle CreateRectangle(int x, int y);

		private readonly CreateRectangle _rect;

		private Random _rnd;
		private IEnumerable<Grid<Cell>.DirectionalCell> _path;
		private Func<IEnumerable<Grid<Cell>.DirectionalCell>, Cell, IEnumerable<Grid<Cell>.DirectionalCell>> _changePath;

		private readonly Func<IEnumerable<Grid<Cell>.DirectionalCell>, Cell, IEnumerable<Grid<Cell>.DirectionalCell>>
			_delegateChangePath;

		private bool _running;
		private SpriteFont _font;

		public Game1(int size, bool fullscreen, ArroundSelectMode mode)
		{
			_graphics = new GraphicsDeviceManager(this) {IsFullScreen = fullscreen};
			Content.RootDirectory = "Content";

			_towerTextures = new Dictionary<Tower, Texture2D>();
			_shipTextures = new Dictionary<Textures.Ship, Texture2D>();
			_menu = new CircularMenu(100);

			_rnd = Program.Random;
			if (fullscreen)
			{
				_width = _graphics.GraphicsDevice.DisplayMode.Width;
				_height = _graphics.GraphicsDevice.DisplayMode.Height;
			}
			else
			{
				_width = _graphics.PreferredBackBufferWidth;
				_height = _graphics.PreferredBackBufferHeight;
			}

			_sizeX = _sizeY = size;
			_board = new Grid(_width / _sizeX, _height / _sizeY);
			_offsetX = (_width - _board.Width * _sizeX) / 2;
			_offsetY = (_height - _board.Height * _sizeY) / 2;
			_rect = RectangleFactory(_offsetX, _offsetY, _sizeX, _sizeY);
			_running = true;
			_changePath = _delegateChangePath = (path, cell) =>
				{
					_changePath = (p, c) => p;

					if (Cell.Goal == null || Cell.Start == null)
						return path;

					try
					{
						return _board.AStar(cell, Cell.Goal, (Cell p, Cell c, out int cost) =>
							{
								Point dir = new Point(c.X - p.X, c.Y - p.Y);

								if (dir.X != 0 && dir.Y != 0)
								{
									var c1 = _board[c.X - dir.X, c.Y];
									var c2 = _board[c.X, c.Y - dir.Y];
									bool canWalk = c.InnerCell.CanWalk && (c1.InnerCell.CanWalk && c2.InnerCell.CanWalk);

									cost = c.InnerCell.CellCost + (Math.Max(c1.InnerCell.CellCost, c2.InnerCell.CellCost));
									return canWalk;
								}

								cost = c.InnerCell.CellCost;
								return c.InnerCell.CanWalk;
							}, mode);
					}
					catch (Exception e)
					{
						new Thread(() => System.Windows.Forms.MessageBox.Show(e.Message)).Start();
						return new Grid<Cell>.DirectionalCell[0];
					}
				};
			Exiting += (o, e) => _running = false;

			//new Thread(() =>
			//	{
			//		while (_running)
			//			_path = _changePath(_path);
			//	}).Start();
			//_changePath();
		}

		private static CreateRectangle RectangleFactory(int offsetX, int offsetY, int width, int height)
		{
			return ((x, y) => new Rectangle(x * width + offsetX, y * height + offsetY, width, height));
		}

		protected override void Initialize()
		{
			base.Initialize();

			_oldMouseState = Mouse.GetState();
			_oldKeyboardState = Keyboard.GetState();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_towerTextures.Add(Tower.Ground, Content.Load<Texture2D>("Case"));
			_towerTextures.Add(Tower.Tower1, Content.Load<Texture2D>(@"Tower\Tower1"));
			_towerTextures.Add(Tower.Tower2, Content.Load<Texture2D>(@"Tower\Tower2"));
			_towerTextures.Add(Tower.Freeze, Content.Load<Texture2D>("Freeze"));
			_towerTextures.Add(Tower.Start, Content.Load<Texture2D>("Start"));
			_towerTextures.Add(Tower.Goal, Content.Load<Texture2D>("Goal"));
			_towerTextures.Add(Tower.Direction, Content.Load<Texture2D>("Arrow"));

			_shipTextures.Add(Textures.Ship.Ship1, Content.Load<Texture2D>(@"Ship\Ship1"));
			_shipTextures.Add(Textures.Ship.Ship2, Content.Load<Texture2D>(@"Ship\Ship2"));

			_font = Content.Load<SpriteFont>("Font");

			_menu.LoadContent(Content);

			new Thread(() =>
			{
				while (_running)
					_path = _changePath(_path, Cell.Start);
			}).Start();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			_oldMouseState = _mouseState;
			_oldKeyboardState = _keyboardState;
			_keyboardState = Keyboard.GetState();
			_mouseState = Mouse.GetState();

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
				_keyboardState.IsKeyDown(Keys.Escape))
			{
				_running = false;
				Exit();
			}
			if (!CircularMenu.Opened)
				_selectedCell = _board[(_mouseState.X - _offsetX) / _sizeX, (_mouseState.Y - _offsetY) / _sizeY];

			if (_selectedCell != null)
				if (!CircularMenu.Opened)
				{
					if (_mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
					{
						_menu.Clear();
						_menu.Add(new MenuElement(_towerTextures[Tower.Direction], () => { }, "Retour"));
						_menu.AddRange(
							_selectedCell.InnerCell.Menu.Select(
								menu => new MenuElement(_towerTextures[menu.Texture], menu.Action(_selectedCell), menu.Text)));

						var open = new Point(_mouseState.X, _mouseState.Y);
						if (open.X < _menu.Size / 2)
							open.X = _menu.Size / 2;
						else if (open.X > _width - _menu.Size / 2)
							open.X = _width - _menu.Size / 2;
						if (open.Y < _menu.Size / 2)
							open.Y = _menu.Size / 2;
						else if (open.Y > _height - _menu.Size / 2)
							open.Y = _height - _menu.Size / 2;

						_menu.Open(open);
					}
				}
				else
				{
					if (_mouseState.LeftButton == ButtonState.Pressed && _mouseState.RightButton == ButtonState.Pressed &&
						_oldMouseState.RightButton == ButtonState.Released)
						_menu.Close();
					else if (_mouseState.LeftButton == ButtonState.Released && _oldMouseState.LeftButton == ButtonState.Pressed)
					{
						_menu.Close()();
						_changePath = _delegateChangePath;
					}
				}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			var path = RectangleFactory(_sizeX / 2 + _offsetX, _sizeY / 2 + _offsetY, _sizeX, _sizeY);
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_spriteBatch.Begin((SpriteSortMode)0, null, SamplerState.PointWrap, null, null);

			//_spriteBatch.Draw(_textures[Texture.Ground],
			//				new Rectangle(600, 0, 200, 480),
			//				Color.White);

			for (int x = 0; x < _board.Width; ++x)
				for (int y = 0; y < _board.Height; ++y)
				{
					var cell = _board[x, y];

					_spriteBatch.Draw(_towerTextures[cell.InnerCell.Texture],
									_rect(cell.X, cell.Y),
									(_selectedCell == cell) ? Color.Yellow : Color.White);
				}

			if (_path != null)
				foreach (var c in _path.Where(cell => !Equals(cell, default(Grid<Cell>.DirectionalCell))))
					DrawPath(c, path);

			_menu.Draw(_spriteBatch);

			_spriteBatch.End();
			base.Draw(gameTime);
		}

		private static float DirectionToRad(Direction dir)
		{
			if (dir == Direction.Stay)
				return 0f;
			return (float)((int)dir * (Math.PI / 180));
		}

		private void DrawPath(Grid<Cell>.DirectionalCell c,  CreateRectangle path)
		{
			if (c.Direction == Direction.Stay)
				return;
			Cell cell = c.Cell;

			//if (_selectedCell != null)
			if (_selectedCell == cell)
			{
				Console.WriteLine("X: {0}, Y: {1}, Dir: {2}", cell.X, cell.Y, c.Direction);
			}

			float rotation = DirectionToRad(c.Direction);
			Texture2D texture = _towerTextures[c.Direction == Direction.Stay ? Tower.Ground : Tower.Direction];
			//_spriteBatch.Draw(_textures[texture], new Vector2( _rect(cell.X, cell.Y).X, _rect(cell.X, cell.Y).Y), null, Color.White, rotation, new Vector2(_textures[texture].Width, _textures[texture].Height), 1f, SpriteEffects.None, 0);
			_spriteBatch.Draw(texture,
							path(cell.X, cell.Y),
							null, //texture.Bounds,
							(_selectedCell == cell) ? Color.YellowGreen : Color.White,
							rotation,
							new Vector2(texture.Width / 2,
										texture.Height / 2)
							, SpriteEffects.None, 0f);
		}
	}
}
