﻿using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Point = Microsoft.Xna.Framework.Point;

namespace TowerDefense.GUI.Windows
{
	public class CircularMenu
	{
		private readonly int _itemSize;

		#region Attributs
		private Color _backgroundColor, _foregroundColor, _selectedColor;
		private readonly List<MenuElement> _elements;
		private Texture2D _circleTexture;
		private static bool _opened;
		private Point _openningPoint;
		private readonly int _size;
		private int _selectedItem;
		private static readonly Random Rnd;
		private Texture2D _lineTexture;
		private int _count;
		private SpriteFont _font;

		#endregion

		#region Constructeur
		static CircularMenu()
		{
			Rnd = Program.Random;
		}

		public CircularMenu(int itemSize)
		{
			_itemSize = itemSize;
			_size = 250;
			_opened = false;
			_selectedItem = 0;
			_elements = new List<MenuElement>();
			_backgroundColor = new Color(75, 75, 75, 170);
			_foregroundColor = Color.LightGray;
			_selectedColor = new Color(169, 169, 169, 170);
		}
		#endregion

		#region Propriete
		public Color BackgroundColor
		{
			get { return _backgroundColor; }
			set { _backgroundColor = value; }
		}
		public Color ForegroundColor
		{
			get { return _foregroundColor; }
			set { _foregroundColor = value; }
		}
		public Color SelectedColor
		{
			get { return _selectedColor; }
			set { _selectedColor = value; }
		}

		public List<MenuElement> Elements
		{
			get { return _elements; }
		}

		public static bool Opened
		{
			get { return _opened; }
		}

		public Point OpenningPoint
		{
			get { return _openningPoint; }
		}

		public int Size
		{
			get { return _size; }
		}

		#endregion

		#region Methodes
		public void Add(MenuElement element)
		{
			_elements.Add(element);
			_count = _elements.Count;
		}

		public void AddRange(IEnumerable<MenuElement> elements)
		{
			_elements.AddRange(elements);
			_count = _elements.Count;
		}

		public void Remove(MenuElement element)
		{
			_elements.Remove(element);
			_count = _elements.Count;
		}

		public void Clear()
		{
			_count = 0;
			_elements.Clear();
		}

		public void LoadContent(ContentManager content)
		{
			_circleTexture = content.Load<Texture2D>(@"Shape\Circle");
			_lineTexture = content.Load<Texture2D>(@"Shape\Line");
			_font = content.Load<SpriteFont>("Font");
		}

		public void Draw(SpriteBatch spritebatch)
		{
			if (!Opened)
				return;
			spritebatch.Draw(_circleTexture,
							new Rectangle(OpenningPoint.X - Size / 2, OpenningPoint.Y - Size / 2, Size, Size),
							_backgroundColor);

			var mouse = Mouse.GetState();
			var angle = Vector.AngleBetween(new Vector(Size, 0),
											new Vector(OpenningPoint.X - mouse.X, OpenningPoint.Y - mouse.Y))
						* (Math.PI / 180) + Math.PI;
			Rectangle line = new Rectangle(OpenningPoint.X, OpenningPoint.Y, Size / 2, 1);

			for (int i = 0; i < _count; ++i)
			{
				float a1 = (MathHelper.TwoPi / _count) * i;
				float a2 = (MathHelper.TwoPi / _count) * (i + 1);

				if ((angle > a1 && angle < a2) || (angle > -a1 && angle < -a2))
				{
					_selectedItem = i;

					for (float a = a1; a < a2; a += 0.005f)
					{
						spritebatch.Draw(_lineTexture, line, null,
								_selectedColor, a,
								new Vector2(0f, 0.5f),
								SpriteEffects.None, 0f);
					}
				}

				spritebatch.Draw(_lineTexture, line, null,
								_foregroundColor, a1,
								new Vector2(0f, 0.5f),
								SpriteEffects.None, 0f);
			}

			//spritebatch.Draw(_lineTexture, line,
			//				null, _foregroundColor,
			//				(float)angle,
			//				new Vector2(0f, 0.5f), SpriteEffects.None, 0f);

			spritebatch.Draw(_elements[_selectedItem].Texture,
							new Rectangle(OpenningPoint.X - _itemSize / 2, OpenningPoint.Y - _itemSize / 2, _itemSize, _itemSize),
							Color.White);

			spritebatch.DrawString(_font, _elements[_selectedItem].Text,
									new Vector2((OpenningPoint.X - _itemSize / 2) + 2, (OpenningPoint.Y + _itemSize / 2) + 2),
									Color.Gray);
			spritebatch.DrawString(_font, _elements[_selectedItem].Text,
									new Vector2(OpenningPoint.X - _itemSize / 2, OpenningPoint.Y + _itemSize / 2),
									Color.White);
		}

		public void Open(Point openningPoint)
		{
			if (_opened)
				return;
			_opened = true;
			_openningPoint = openningPoint;
		}

		public Action Close()
		{
			if (!_opened)
				return () => { };
			_opened = false;
			return _elements[_selectedItem].Action;
		}
		#endregion

	}
}