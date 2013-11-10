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
		private int _sizeName;
		private readonly int _lifeBarSize;
		private int _money;
		private float _life;

		public StatusBar(int height, int width, Player player)
		{
			_height = height;
			_width = width;
			_player = player;
			_lifeBarSize = 150;
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
			_sizeName = (int)_font.MeasureString(_player.Name).X;
		}

		public void Update()
		{
			if (_money != _player.Money)
				if (_money < _player.Money)
					_money++;
				else
					_money--;
			if (Math.Abs(_life - _player.Life) > 0.1f)
				if (_life < _player.Life)
					_life+=0.5f;
				else
					_life-=0.5f;
		}

		public void Draw(SpriteBatch spritebatch)
		{
			spritebatch.Draw(_pixel, _rectangle, Color.Black);
			spritebatch.DrawString(_font, _player.Name, Vector2.Zero, Color.Gray);

			spritebatch.Draw(_pixel, new Rectangle(_sizeName + 10, 5, _lifeBarSize, _height - 10), Color.Peru);
			spritebatch.Draw(_pixel,
							new Rectangle(_sizeName + 10, 5, (int)(_life / _player.MaxLife * _lifeBarSize), _height - 10)
							, Color.Red);
			spritebatch.DrawString(_font, _life.ToString("#"), new Vector2(_sizeName + 10 + _life / _player.MaxLife * _lifeBarSize, 2), Color.Gray);

			spritebatch.DrawString(_font, string.Format("{0}{1}", _money, _player.Currency), new Vector2(_sizeName + _lifeBarSize + 60, 2), Color.Gray);

		}
	}
}