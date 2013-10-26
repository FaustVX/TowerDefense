#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CSharpHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TowerDefense.GUI.Windows.Cells;

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
		private readonly Dictionary<Texture, Texture2D> _textures;
		private readonly Grid _board;
		private readonly int _offsetX, _offsetY, _sizeX, _sizeY;
		private Cell _selectedCell;

		private MouseState _mouseState, _oldMouseState;
		private KeyboardState _keyboardState, _oldKeyboardState;

		private delegate Rectangle CreateRectangle(int x, int y);

		private readonly CreateRectangle _rect;

		private Random _rnd;
		private IEnumerable<Grid<Cell>.DirectionalCell> _path, _mousePath;
		private Func<IEnumerable<Grid<Cell>.DirectionalCell>, Cell, IEnumerable<Grid<Cell>.DirectionalCell>> _changePath;

		private readonly Func<IEnumerable<Grid<Cell>.DirectionalCell>, Cell, IEnumerable<Grid<Cell>.DirectionalCell>>
			_delegateChangePath;

		private readonly object _token;
		private bool _running;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			_textures = new Dictionary<Texture, Texture2D>();
			_rnd = new Random();
			_sizeX = _sizeY = 20;
			_board = new Grid(800 / _sizeX, 480 / _sizeY);
			_offsetX = (800 - _board.Width * _sizeX) / 2;
			_offsetY = (480 - _board.Height * _sizeY) / 2;
			_rect = RectangleFactory(_offsetX, _offsetY, _sizeX, _sizeY);
			_token = new object();
			_running = true;
			_changePath = _delegateChangePath = (path, cell) =>
				{
					_changePath = (p, c) => p;

					if (Cell.Goal == null || Cell.Start == null)
						return path;

					try
					{
						return _board.AStar(cell, Cell.Goal, (c, p) =>
							{
								Point dir = new Point(c.X - p.X, c.Y - p.Y);

								if (dir.X != 0 && dir.Y != 0)
								{
									var c1 = _board[c.X - dir.X, c.Y];
									var c2 = _board[c.X, c.Y - dir.Y];
									return c.InnerCell.CanWalk && (c1.InnerCell.CanWalk || c2.InnerCell.CanWalk);
								}

								return c.InnerCell.CanWalk;
							}, ArroundSelectMode.Round);
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

			_textures.Add(Texture.Ground, Content.Load<Texture2D>("Case"));
			_textures.Add(Texture.Tower1, Content.Load<Texture2D>("Tower1"));
			_textures.Add(Texture.Tower2, Content.Load<Texture2D>("Tower2"));
			_textures.Add(Texture.Freeze, Content.Load<Texture2D>("Freeze"));
			_textures.Add(Texture.Start, Content.Load<Texture2D>("Start"));
			_textures.Add(Texture.Goal, Content.Load<Texture2D>("Goal"));
			_textures.Add(Texture.Direction, Content.Load<Texture2D>("Arrow"));

			new Thread(() =>
			{
				while (_running)
					_path = _changePath(_path, Cell.Start);
			}).Start();

			//new Thread(() =>
			//{
			//	while (_running)
			//		_mousePath = _changePath(_mousePath, _selectedCell);
			//}).Start();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			_mouseState = _oldMouseState;
			_keyboardState = _oldKeyboardState;
			_oldKeyboardState = Keyboard.GetState();
			_oldMouseState = Mouse.GetState();

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
				_keyboardState.IsKeyDown(Keys.Escape))
				Exit();

			_selectedCell = _board[(_mouseState.X - _offsetX) / _sizeX, (_mouseState.Y - _offsetY) / _sizeY];
			if (_selectedCell != null)
			{
				if (_mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
				{
					_selectedCell.SwitchCell();
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

			Func<int, float> degToRad = (deg) => (float)((deg % 360) * (Math.PI / 180));
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_spriteBatch.Begin((SpriteSortMode)0, null, SamplerState.PointWrap, null, null);

			//_spriteBatch.Draw(_textures[Texture.Ground],
			//				new Rectangle(600, 0, 200, 480),
			//				Color.White);

			for (int x = 0; x < _board.Width; ++x)
				for (int y = 0; y < _board.Height; ++y)
				{
					var cell = _board[x, y];

					_spriteBatch.Draw(_textures[cell.InnerCell.Texture],
									_rect(cell.X, cell.Y),
									(_selectedCell == cell) ? Color.Yellow : Color.White);
				}

			if (_path != null)
				foreach (var c in _path.Where(cell => !Equals(cell, default(Grid<Cell>.DirectionalCell))))
					DrawPath(c, degToRad, path);
			if (_mousePath != null)
				foreach (var c in _mousePath.Where(cell => !Equals(cell, default(Grid<Cell>.DirectionalCell))))
					DrawPath(c, degToRad, path);

			_spriteBatch.End();
			base.Draw(gameTime);
		}

		private void DrawPath(Grid<Cell>.DirectionalCell c, Func<int, float> degToRad, CreateRectangle path)
		{
			if (c.Direction == Direction.Stay)
				return;
			Cell cell = c.Cell;

			//if (_selectedCell != null)
			if (_selectedCell == cell)
			{
				Console.WriteLine("X: {0}, Y: {1}, Dir: {2}", cell.X, cell.Y, c.Direction);
			}

			float rotation;
			switch (c.Direction)
			{
				case Direction.Right:
					rotation = degToRad(45 * 0);
					break;
				case Direction.DownRight:
					rotation = degToRad(45 * 1);
					break;
				case Direction.Down:
					rotation = degToRad(45 * 2);
					break;
				case Direction.DownLeft:
					rotation = degToRad(45 * 3);
					break;
				case Direction.Left:
					rotation = degToRad(45 * 4);
					break;
				case Direction.UpLeft:
					rotation = degToRad(45 * 5);
					break;
				case Direction.Up:
					rotation = degToRad(45 * 6);
					break;
				case Direction.UpRight:
					rotation = degToRad(45 * 7);
					break;
				default:
					rotation = 0f;
					break;
			}

			Texture2D texture = _textures[c.Direction == Direction.Stay ? Texture.Ground : Texture.Direction];
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
