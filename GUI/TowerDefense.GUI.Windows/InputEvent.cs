using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TowerDefense.GUI.Windows
{
	public enum MouseButton
	{
		Left,
		Middle,
		Right,
		X1,
		X2
	}

	public enum ClickEvent
	{
		OnReleased,
		OnPressed
	}

	public static class InputEvent
	{
		#region Attributs
		private static MouseState mouseState, oldMouseState;
		private static KeyboardState keyboardState, oldKeyboardState;

		public delegate void KeysPressEvent(Keys[] keys);
		public static event KeysPressEvent KeysPressed;
		public static event Action
			LeftMouseClick ,
			RightMouseClick ,
			MiddleMouseClick;
		#endregion

		#region Constructeur
		static InputEvent()
		{
			oldMouseState = Mouse.GetState();
			oldKeyboardState = Keyboard.GetState();
		}

		#endregion

		#region Propriete
		public static MouseState MouseState
		{
			get { return mouseState; }
		}

		public static MouseState OldMouseState
		{
			get { return oldMouseState; }
		}

		public static KeyboardState KeyboardState
		{
			get { return keyboardState; }
		}

		public static KeyboardState OldKeyboardState
		{
			get { return oldKeyboardState; }
		}

		#endregion

		#region Methodes
		public static void Update()
		{
			oldMouseState = MouseState;
			oldKeyboardState = KeyboardState;
			keyboardState = Keyboard.GetState();
			mouseState = Mouse.GetState();

			if (KeysPressed != null && keyboardState.GetPressedKeys().Length != 0)
			{
				List<Keys> keyPressed = new List<Keys>(10);
				keyPressed.AddRange(
					keyboardState.GetPressedKeys().Where(
						key => oldKeyboardState.IsKeyDown(key) && keyboardState.IsKeyUp(key)));
				KeysPressed(keyPressed.ToArray());
			}

			if (OldMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released)
				if (LeftMouseClick != null)
					LeftMouseClick();
			if (OldMouseState.RightButton == ButtonState.Pressed && MouseState.RightButton == ButtonState.Released)
				if (RightMouseClick != null)
					RightMouseClick();
			if (OldMouseState.MiddleButton == ButtonState.Pressed && MouseState.MiddleButton == ButtonState.Released)
				if (MiddleMouseClick != null)
					MiddleMouseClick();
		}

		public static Vector2 MouseMove()
		{
			return new Vector2(OldMouseState.X - MouseState.X, OldMouseState.Y - MouseState.Y);
		}

		public static bool IsKeyDown(Keys key)
		{
			return keyboardState.IsKeyDown(key);
		}

		public static bool IsKeyUp(Keys key)
		{
			return keyboardState.IsKeyUp(key);
		}

		public static bool MouseClick(MouseButton button, ClickEvent clickEvent = ClickEvent.OnReleased)
		{
			ButtonState old, curent;

			switch (button)
			{
				case MouseButton.Left:
					old = oldMouseState.LeftButton;
					curent = mouseState.LeftButton;
				break;
				case MouseButton.Middle:
					old = oldMouseState.MiddleButton;
					curent = mouseState.MiddleButton;
				break;
				case MouseButton.Right:
					old = oldMouseState.RightButton;
					curent = mouseState.RightButton;
				break;
				case MouseButton.X1:
					old = oldMouseState.XButton1;
					curent = mouseState.XButton1;
				break;
				case MouseButton.X2:
					old = oldMouseState.XButton2;
					curent = mouseState.XButton2;
				break;
				default:
					throw new ArgumentOutOfRangeException("button");
			}

			switch (clickEvent)
			{
				case ClickEvent.OnReleased:
					return old == ButtonState.Pressed && curent == ButtonState.Released;
				case ClickEvent.OnPressed:
					return old == ButtonState.Released && curent == ButtonState.Pressed;
				default:
					throw new ArgumentOutOfRangeException("clickEvent");
			}
		}

		public static bool KeyboardClick(Keys key, ClickEvent clickEvent = ClickEvent.OnReleased)
		{
			switch (clickEvent)
			{
				case ClickEvent.OnReleased:
					return oldKeyboardState.IsKeyDown(key) && keyboardState.IsKeyUp(key);
				case ClickEvent.OnPressed:
					return oldKeyboardState.IsKeyUp(key) && keyboardState.IsKeyDown(key);
				default:
					throw new ArgumentOutOfRangeException("clickEvent");
			}
		}

		public static bool HasScrolled()
		{
			return oldMouseState.ScrollWheelValue != mouseState.ScrollWheelValue;
		}

		public static int ScroolValue()
		{
			return mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;
		}

		public static Point MousePosition()
		{
			return new Point(mouseState.X, mouseState.Y);
		}
		#endregion
	}
}