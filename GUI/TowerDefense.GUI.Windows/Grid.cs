using CSharpHelper;
using TowerDefense.GUI.Windows.Cells;

namespace TowerDefense.GUI.Windows
{
	public class Grid: Grid<Cell>
	{
		private readonly int _sizeX;
		private readonly int _sizeY;

		/// <summary>
		/// Constructeur de la grille
		/// </summary>
		/// <param name="width">Nombre de cellules horizontalement</param>
		/// <param name="height">Nombre de cellules verticalement</param>
		public Grid(int width, int height, int sizeX, int sizeY)
			: base(width, height, (x, y, list) => new Cell(x, y, list))
		{
			_sizeX = sizeX * Width;
			_sizeY = sizeY * Height;
		}

		public int SizeX
		{
			get { return _sizeX; }
		}

		public int SizeY
		{
			get { return _sizeY; }
		}
	}
}