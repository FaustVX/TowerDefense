#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
		private readonly bool _fancyMouse;
		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private readonly Dictionary<Tower, Texture2D> _towerTextures;
		private readonly Grid _board;
		private readonly int _width, _height;

		private readonly CircularMenu _menu;
		private readonly StatusBar _statusBar;
		private readonly InformationPanel _infoPanel;

		private Random _rnd;
		private SpriteFont _font;
		private Texture2D _informationTexture;
		private readonly Player _player;

		public Game1(string name, int money, int size, bool fullscreen, bool fancyMouse)
		{
			_fancyMouse = fancyMouse;
			_graphics = new GraphicsDeviceManager(this) {IsFullScreen = fullscreen};
			Content.RootDirectory = "Content";
			if(fancyMouse)
			{
				IsMouseVisible = false;
				var cursor = new Cursor(this, 40)
					{
						BorderColor = Color.White,
						BorderSize = 2,
						FillColor = Color.GreenYellow,
						StartScale = 0.25f,
						EndScale = 0f,
						TrailNodeMass=12f
					};
				Components.Add(cursor);
			}

			_towerTextures = new Dictionary<Tower, Texture2D>();
			_menu = new CircularMenu(size * 2);

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
			_board = new Grid(30, 20, size, size, _width, _height);

			_player = new Player(name, 100, money);
			_statusBar = new StatusBar(30, _width, _player);
			_infoPanel = new InformationPanel(_width, _statusBar.Height);
			Exiting += (o, e) => _board.Running = false;
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_board.LoadContent(Content);

			_towerTextures.Add(Tower.Ground, Content.Load<Texture2D>("Case"));
			_towerTextures.Add(Tower.RangeGround, Content.Load<Texture2D>("Range"));
			_towerTextures.Add(Tower.Tower1, Content.Load<Texture2D>(@"Tower/Tower1"));
			_towerTextures.Add(Tower.Tower2, Content.Load<Texture2D>(@"Tower/Tower2"));
			_towerTextures.Add(Tower.Freeze, Content.Load<Texture2D>(@"Tower/Freeze"));
			_towerTextures.Add(Tower.Start, Content.Load<Texture2D>("Start"));
			_towerTextures.Add(Tower.Base, Content.Load<Texture2D>("Base"));
			_towerTextures.Add(Tower.Direction, Content.Load<Texture2D>("Arrow"));

			_font = Content.Load<SpriteFont>("Font");
			_informationTexture = Content.Load<Texture2D>("Information");
			ShipGroup.LoadContent(Content);

			_menu.LoadContent(Content, _graphics.GraphicsDevice);
			_statusBar.LoadContent(Content, _graphics.GraphicsDevice);
			_infoPanel.LoadContent(Content, _graphics.GraphicsDevice);
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			InputEvent.Update();

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
				InputEvent.KeyboardClick(Keys.Escape))
			{
				if (CircularMenu.Opened)
					_menu.Close();
				else
				{
					_board.Running = false;
					Exit();
				}
			}

			//Stop
			if (InputEvent.KeyboardClick(Keys.OemQuotes, ClickEvent.OnPressed))
				ShipGroup.SpeedIndex = 0;
				//Speed 0.5 -> 10
			else if (InputEvent.KeyboardClick(Keys.D1))
				ShipGroup.SpeedIndex = 1;
			else if (InputEvent.KeyboardClick(Keys.D2))
				ShipGroup.SpeedIndex = 2;
			else if (InputEvent.KeyboardClick(Keys.D3))
				ShipGroup.SpeedIndex = 3;
			else if (InputEvent.KeyboardClick(Keys.D4))
				ShipGroup.SpeedIndex = 4;
			else if (InputEvent.KeyboardClick(Keys.D5))
				ShipGroup.SpeedIndex = 5;
				//Speed - 1
			else if (InputEvent.KeyboardClick(Keys.D6))
				ShipGroup.SpeedIndex--;
				//Speed + 1
			else if (InputEvent.KeyboardClick(Keys.D7))
				ShipGroup.SpeedIndex++;

			if (InputEvent.KeyboardClick(Keys.F10))
				if (_fancyMouse)
					IsMouseVisible = !IsMouseVisible;

			ShipGroup.StaticUpdate();
			_board.Update();

			if (_board.SelectedCell != null)
				if (_infoPanel.Opened)
				{
					if (InputEvent.MouseClick(MouseButton.Left))
						_infoPanel.Close();
				}
				else if (!CircularMenu.Opened)
				{
					if (InputEvent.MouseClick( MouseButton.Left, ClickEvent.OnPressed))
					{
						_menu.Clear();
						_menu.Add(new MenuElement(_towerTextures[Tower.Direction], () => { }, "Retour", ""));
						_menu.AddRange(_board.SelectedCell.InnerCell.Menu.Select(
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
									menu.Action(_board.SelectedCell, _player)();
								}, menu.Text, menu.Money + _player.Currency)));
						if (_board.SelectedCell.InnerCell is TowerCell)
						{
							var cell = _board.SelectedCell.InnerCell as TowerCell;
							_menu.Add(new MenuElement(_informationTexture,
													() => _infoPanel.View(new InformationElement(_towerTextures[cell.Texture], cell.Name))
													, "Information", ""));
						}

						var open = InputEvent.MousePosition();
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
					if (InputEvent.MouseState.LeftButton == ButtonState.Pressed && InputEvent.MouseClick(MouseButton.Right))
						_menu.Close();
					else if (InputEvent.MouseClick(MouseButton.Left))
					{
						_menu.Close()();
						_board.RecalculatePath();
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
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_spriteBatch.Begin((SpriteSortMode)0, null, SamplerState.PointWrap, null, null);

			_board.Draw(_spriteBatch);
			_statusBar.Draw(_spriteBatch);
			_infoPanel.Draw(_spriteBatch);
			_menu.Draw(_spriteBatch);

			_spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
