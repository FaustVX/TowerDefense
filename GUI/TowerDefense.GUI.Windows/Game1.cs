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
		private int _offsetX, _offsetY, _sizeX, _sizeY;
		private readonly int _width, _height;
		private Cell _selectedCell;

		private readonly CircularMenu _menu;
		private readonly StatusBar _statusBar;
		private readonly InformationPanel _infoPanel;

		private MouseState _mouseState, _oldMouseState;
		private KeyboardState _keyboardState, _oldKeyboardState;

		private delegate Rectangle CreateRectangle(int x, int y);

		private CreateRectangle _rect;

		private Random _rnd;
		private IEnumerable<Grid<Cell>.DirectionalCell> _path;
		private Func<IEnumerable<Grid<Cell>.DirectionalCell>, Cell, IEnumerable<Grid<Cell>.DirectionalCell>> _changePath;

		private readonly Func<IEnumerable<Grid<Cell>.DirectionalCell>, Cell, IEnumerable<Grid<Cell>.DirectionalCell>>
			_delegateChangePath;

		private bool _running;
		private SpriteFont _font;
		private Texture2D _pixel;
		private Texture2D _informationTexture;
		private readonly Player _player;

		public Game1(string name, int size, bool fullscreen, ArroundSelectMode mode)
		{
			_graphics = new GraphicsDeviceManager(this) {IsFullScreen = fullscreen};
			Content.RootDirectory = "Content";

			_towerTextures = new Dictionary<Tower, Texture2D>();
			_shipTextures = new Dictionary<Textures.Ship, Texture2D>();
			_menu = new CircularMenu(size * 2);

			_rnd = Program.Random;

			_sizeX = _sizeY = size;
			_board = new Grid(30, 20, _sizeX, _sizeY);
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
			_offsetX = (_width - _board.Width * _sizeX) / 2;
			_offsetY = (_height - _board.Height * _sizeY) / 2;

			_rect = RectangleFactory(_offsetX, _offsetY, _sizeX, _sizeY);
			_player = new Player(name, 100);
			_statusBar = new StatusBar(30, _width, _player);
			_infoPanel = new InformationPanel(_width, _statusBar.Height);
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
			_towerTextures.Add(Tower.RangeGround, Content.Load<Texture2D>("Range"));
			_towerTextures.Add(Tower.Tower1, Content.Load<Texture2D>(@"Tower\Tower1"));
			_towerTextures.Add(Tower.Tower2, Content.Load<Texture2D>(@"Tower\Tower2"));
			_towerTextures.Add(Tower.Freeze, Content.Load<Texture2D>(@"Tower\Freeze"));
			_towerTextures.Add(Tower.Start, Content.Load<Texture2D>("Start"));
			_towerTextures.Add(Tower.Base, Content.Load<Texture2D>("Base"));
			_towerTextures.Add(Tower.Direction, Content.Load<Texture2D>("Arrow"));

			_shipTextures.Add(Textures.Ship.Ship1, Content.Load<Texture2D>(@"Ship\Ship1"));
			_shipTextures.Add(Textures.Ship.Ship2, Content.Load<Texture2D>(@"Ship\Ship2"));

			_font = Content.Load<SpriteFont>("Font");
			_pixel = Content.Load<Texture2D>(@"Shape\Pixel");
			_informationTexture = Content.Load<Texture2D>("Information");

			_menu.LoadContent(Content);
			_statusBar.LoadContent(Content);
			_infoPanel.LoadContent(Content);

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
			{
				if (_mouseState.RightButton == ButtonState.Pressed || _mouseState.MiddleButton == ButtonState.Pressed)
				{
					_offsetX += _mouseState.X - _oldMouseState.X;
					_offsetY += _mouseState.Y - _oldMouseState.Y;
					_rect = RectangleFactory(_offsetX, _offsetY, _sizeX, _sizeY);
				}
				if (_mouseState.ScrollWheelValue - _oldMouseState.ScrollWheelValue != 0)
				{
					var dir = ((_mouseState.ScrollWheelValue - _oldMouseState.ScrollWheelValue) > 0) ? 1 : -1;

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

				if (_mouseState.X - _offsetX < 0 || _mouseState.Y - _offsetY < 0)
					_selectedCell = null;
				else
					_selectedCell = _board[(_mouseState.X - _offsetX) / _sizeX, (_mouseState.Y - _offsetY) / _sizeY];
			}

			if (_selectedCell != null)
				if (_infoPanel.Opened)
				{
					if (_mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
						_infoPanel.Close();
				}
				else if (!CircularMenu.Opened)
				{
					if (_mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
					{
						_menu.Clear();
						_menu.Add(new MenuElement(_towerTextures[Tower.Direction], () => { }, "Retour", ""));
						_menu.AddRange(_selectedCell.InnerCell.Menu.Select(
							menu => new MenuElement(_towerTextures[menu.Texture], () =>
								{
									if (menu.Money < 0)
									{
										if (!_player.CanWithDraw(-menu.Money))
											return;
										_player.WithDraw(-menu.Money);
									}
									else
										_player.Put(menu.Money);
									//_player.Life += _rnd.Next(-20, 20);
									menu.Action(_selectedCell, _player)();
								}, menu.Text, menu.Money + _player.Currency)));
						if (_selectedCell.InnerCell is TowerCell)
						{
							var cell = _selectedCell.InnerCell as TowerCell;
							_menu.Add(new MenuElement(_informationTexture,
													() => _infoPanel.View(new InformationElement(_towerTextures[cell.Texture], cell.Name))
													, "Information", ""));
						}

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
			_statusBar.Update();

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

			_statusBar.Draw(_spriteBatch);
			_infoPanel.Draw(_spriteBatch);

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

		private void DrawPath(Grid<Cell>.DirectionalCell c, CreateRectangle path)
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
			_spriteBatch.Draw(texture,
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
	}
}
