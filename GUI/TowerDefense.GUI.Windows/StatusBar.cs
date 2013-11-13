using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefense.GUI.Windows
{
	public class StatusBar
	{
		private readonly int _height, _width;
		private readonly Player _player;
		private Texture2D _pixel;
		private SpriteFont _font;
		private readonly Rectangle _rectangle;
		private int _startName, _startLifeBar, _startLife, _startMoney, _startSpeed;
		private readonly int _lifeBarSize;
		private int _money;
		private int _life;
		private readonly int _space;

		public StatusBar(int height, int width, Player player)
		{
			_height = height;
			_width = width;
			_player = player;
			_lifeBarSize = 150;
			_space = 10;
			_money = 0;
			_life = 0;
			_rectangle = new Rectangle(0, 0, _width, _height);
		}

		public int Height
		{
			get { return _height; }
		}

		public int Width
		{
			get { return _width; }
		}

		public void LoadContent(ContentManager content, GraphicsDevice device)
		{
			_pixel = new Texture2D(device, 1, 1);
			_pixel.SetData(new Color[] {Color.White});
			_font = content.Load<SpriteFont>("Font");

			_startName = 5;
			_startLifeBar = _startName + (int)_font.MeasureString(_player.Name).X + _space;
			_startLife = _startLifeBar + _lifeBarSize;
			_startMoney = _startLife + _space + (int)_font.MeasureString(_player.Life.ToString()).X + _space;
			_startSpeed = _startMoney + (int)_font.MeasureString((_player.Money * 10).ToString()).X + _space;
		}

		public void Update()
		{
			if (_money != _player.Money)
				if (_money < _player.Money)
					_money++;
				else
					_money--;
			if (_life != _player.Life)
				if (_life < _player.Life)
					_life += 1;
				else
					_life -= 1;

			_startLife = _startLifeBar + (int)((float)_life / _player.MaxLife * _lifeBarSize);
		}

		public void Draw(SpriteBatch spritebatch)
		{
			//BackGround
			spritebatch.Draw(_pixel, _rectangle, Color.Black);

			//Name
			spritebatch.DrawString(_font, _player.Name, new Vector2(_startName, 0), Color.Gray);
			//BackGroung LifeBar
			spritebatch.Draw(_pixel, new Rectangle(_startLifeBar, 5, _lifeBarSize, _height - 10), Color.Red * 0.3f);
			//ForeGroundLifeBar
			spritebatch.Draw(_pixel,
							new Rectangle(_startLifeBar, 5, (int)((float)_life / _player.MaxLife * _lifeBarSize), _height - 10)
							, Color.Red);
			//Life
			spritebatch.DrawString(_font, _life.ToString("0"), new Vector2(_startLife, 2), Color.Gray);
			//Money
			spritebatch.DrawString(_font, string.Format("{0}{1}", _money, _player.Currency), new Vector2(_startMoney, 2), Color.Gray);
			//Speed
			spritebatch.DrawString(_font, string.Format("{0}{1}", ShipGroup.Speed.ToString("0.##"), "x"), new Vector2(_startSpeed, 2), Color.Gray);
		}
	}
}