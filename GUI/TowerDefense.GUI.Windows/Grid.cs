using CSharpHelper;
using TowerDefense.GUI.Windows.Cells;

namespace TowerDefense.GUI.Windows
{
	public class Grid: Grid<Cell>
	{
		/// <summary>
		/// Constructeur de la grille
		/// </summary>
		/// <param name="width">Nombre de cellules horizontalement</param>
		/// <param name="height">Nombre de cellules verticalement</param>
		public Grid(int width, int height)
			: base(width, height, (x, y, list) => new Cell(x, y, list))
		{

		}
	}
}