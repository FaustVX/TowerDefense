using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefense.GUI.Windows
{
	public class InformationPanel
	{
		private readonly int _width, _height, _posY;
		private bool _opened;
		private InformationElement _element;
		private Texture2D _pixel;
		private SpriteFont _font;

		public InformationPanel(int width, int posY)
		{
			_width = width;
			_height = 50;
			_posY = posY;
		}

		public int Width
		{
			get { return _width; }
		}

		public int Height
		{
			get { return _height; }
		}

		public bool Opened
		{
			get { return _opened; }
		}

		public void View(InformationElement element)
		{
			_opened = true;
			_element = element;
		}

		public void Close()
		{
			_opened = false;
		}

		public void LoadContent(ContentManager content)
		{
			_pixel = content.Load<Texture2D>(@"Shape\Pixel");
			_font = content.Load<SpriteFont>(@"Font");
		}

		public void Update()
		{
			
		}

		public void Draw(SpriteBatch spritebatch)
		{
			if (!_opened)
				return;
			spritebatch.Draw(_pixel, new Rectangle(0, _posY, _width, _height), Color.Black);

			spritebatch.Draw(_element.Texture, new Rectangle(5, _posY + 5, _height - 10, _height - 10), Color.White);
			spritebatch.DrawString(_font, _element.Name, new Vector2(_height, _posY + 5), Color.Gray);
		}
	}

	public class InformationElement
	{
		public Texture2D Texture { get; private set; }
		public string Name { get; private set; }

		public InformationElement(Texture2D texture, string name)
		{
			Texture = texture;
			Name = name;
		}
	}
}