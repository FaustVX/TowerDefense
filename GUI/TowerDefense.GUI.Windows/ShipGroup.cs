using System;
using System.Collections.Generic;
using System.Diagnostics;
using CSharpHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace TowerDefense.GUI.Windows
{
	public class ShipGroup
	{
		private readonly Textures.Ship _ship;
		private readonly IList<Grid.DirectionalCell> _path;
		private readonly int _lenght;
		private static readonly Dictionary<Textures.Ship, Texture2D> _textures;
		private readonly List<Ship> _ships;
		private static readonly float[] speeds;
		private static int speed;
		private static float realSpeed;

		static ShipGroup()
		{
			speeds = new float[]
				{
					0,
					0.5f,
					1f,
					2f,
					5f,
					10f
				};
			speed = 2;
			realSpeed = Speed;

			_textures = new Dictionary<Textures.Ship, Texture2D>();
		}

		public ShipGroup(Textures.Ship ship, IList<Grid.DirectionalCell> path, int lenght, int life, Point starterPosition)
		{
			_ship = ship;
			_path = path;
			_lenght = lenght;
			_ships = new List<Ship>(_lenght);
			for (int i = 0; i < _lenght; i++)
				_ships.Add(new Ship(life, new Vector2(starterPosition.X - ((i + 1) * 0.5f), starterPosition.Y), 0.01f));
		}

		public IEnumerable<Grid.DirectionalCell> Path
		{
			get { return _path; }
		}

		public int Lenght
		{
			get { return _lenght; }
		}

		public static float Speed
		{
			get { return realSpeed; }
		}

		public static int SpeedIndex
		{
			get { return speed; }
			set
			{
				if (value < 0 || value >= speeds.Length)
					return;
				speed = value;
			}
		}

		public static void LoadContent(ContentManager content)
		{
			_textures.Add(Textures.Ship.Ship1, content.Load<Texture2D>(@"Ship/Ship1"));
			_textures.Add(Textures.Ship.Ship2, content.Load<Texture2D>(@"Ship/Ship2"));
		}

		public static void StaticUpdate()
		{
			var v = speeds[speed];
			if (Math.Abs(realSpeed - v) > 0.2f)
			{
				if (realSpeed < v)
					realSpeed += 0.2f;
				else
					realSpeed -= 0.2f;
			}
			else
				realSpeed = v;
		}

		public void Update()
		{
			if (Speed == 0)
				return;
			for (int i = 0; i < _lenght; i++)
			{
				var ship = _ships[i];
				if (ship == null)
					continue;
				if (ship.IsDead)
					_ships[i] = null;

				var speed = ship.Speed * Speed;

#if !DEBUG
				ship.Position = new Vector2(ship.Position.X + speed, ship.Position.Y);
#else
				float offsetx = 0, offsety = 0;
				//Left Right
				switch (ship.Direction)
				{
					case Direction.UpRight:
					case Direction.Right:
					case Direction.DownRight:
					case Direction.Stay:
						offsetx = 0.5f;
						break;
					case Direction.UpLeft:
					case Direction.Left:
					case Direction.DownLeft:
						offsetx = -0.5f;
						break;
				}

				//Up Down
				switch (ship.Direction)
				{
					case Direction.DownLeft:
					case Direction.Down:
					case Direction.DownRight:
						offsety = 0.5f;
						break;
					case Direction.UpLeft:
					case Direction.Up:
					case Direction.UpRight:
						offsety = -0.5f;
						break;
				}

				var cell =
					_path.FirstOrDefault(c =>
										Math.Abs(c.Cell.X - (ship.Position.X)) < 0.5f &&
										Math.Abs(c.Cell.Y - (ship.Position.Y)) < 0.5f);
				if (cell.Cell == null)
				{
					ship.Position = new Vector2(ship.Position.X + speed, ship.Position.Y);
					continue;
				}
				ship.Direction = cell.Direction;
				//Left Right
				switch (ship.Direction)
				{
					case Direction.UpRight:
					case Direction.Right:
					case Direction.DownRight:
					case Direction.Stay:
						ship.Position = new Vector2(ship.Position.X + speed, ship.Position.Y);
						break;
					case Direction.UpLeft:
					case Direction.Left:
					case Direction.DownLeft:
						ship.Position = new Vector2(ship.Position.X - speed, ship.Position.Y);
						break;
				}

				//Up Down
				switch (ship.Direction)
				{
					case Direction.DownLeft:
					case Direction.Down:
					case Direction.DownRight:
						ship.Position = new Vector2(ship.Position.X, ship.Position.Y + speed);
						break;
					case Direction.UpLeft:
					case Direction.Up:
					case Direction.UpRight:
						ship.Position = new Vector2(ship.Position.X, ship.Position.Y - speed);
						break;
				}
				
#endif
			}
		}

		public void Draw(SpriteBatch spritebatch, int size, Point offset)
		{
			for (int i = 0; i < _lenght; i++)
			{
				var ship = _ships[i];
				if (ship.IsDead)
					continue;
				spritebatch.Draw(_textures[_ship],
								new Rectangle((int)(ship.Position.X * size + offset.X),
											(int)(ship.Position.Y * size + offset.Y),
											size, size), null,
								Color.White, ship.Rotation, Vector2.Zero, SpriteEffects.None, 0f);
			}
		}
	}

	public class Ship
	{
		public float Rotation { get; private set; }
		private Direction _direction;
		public int Life { get; private set; }
		public Vector2 Position { get; set; }
		public float Speed { get; private set; }

		public bool IsDead
		{
			get { return Life == 0; }
		}

		public Direction Direction
		{
			get { return _direction; }
			set
			{
				//if (value != _direction)
				//	Debugger.Break();
				_direction = value;
				Rotation = Grid.DirectionalCell.ToRad(_direction);
			}
		}

		public Ship(int life, Vector2 position, float speed)
		{
			Life = life;
			Position = position;
			Speed = speed;
			Direction = Direction.Right;
		}
	}
}
