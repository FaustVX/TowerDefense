using Microsoft.Xna.Framework;

namespace TowerDefense.GUI.Windows
{
	public class ShipGroup<TShip>: DrawableGameComponent
		where TShip: Ship
	{
		public ShipGroup(Game game) : base(game)
		{
		}
	}

	public class Ship
	{
	}
}
