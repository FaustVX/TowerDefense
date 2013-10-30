using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefense.GUI.Windows
{
	public class MenuElement
	{
		private readonly Texture2D _texture;
		private readonly List<MenuElement> _children;
		private readonly Action _action;
		private readonly string _text;

		public MenuElement(Texture2D texture, Action action, string text)
		{
			_texture = texture;
			_action = action;
			_text = text;
			_children = new List<MenuElement>();
		}

		public Texture2D Texture
		{
			get { return _texture; }
		}

		public List<MenuElement> Children
		{
			get { return _children; }
		}

		public Action Action
		{
			get { return _action; }
		}

		public string Text
		{
			get { return _text; }
		}
	}
}