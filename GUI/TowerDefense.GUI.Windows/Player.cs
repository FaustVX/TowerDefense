namespace TowerDefense.GUI.Windows
{
	public class Player
	{
		private readonly string _name;
		private int _maxLife;
		private int _life;
		private int _money;
		private readonly string _currency;

		public Player(string name, int maxLife, int money)
		{
			_name = name;
			_maxLife = maxLife;
			_money = money;
			_life = _maxLife;
			_currency = "$";
		}

		public string Name
		{
			get { return _name; }
		}

		public int Money
		{
			get { return _money; }
			set { _money = value; }
		}

		public string Currency
		{
			get { return _currency; }
		}

		public int MaxLife
		{
			get { return _maxLife; }
			set { _maxLife = value; }
		}

		public int Life
		{
			get { return _life; }
			set
			{
				if (value < 0)
					_life = 0;
				else if (value > _maxLife)
					_life = _maxLife;
				else
					_life = value;
			}
		}

		public bool CanWithDraw(int value)
		{
			return value <= Money;
		}

		public void WithDraw(int value)
		{
			Money -= value;
		}

		public void Put(int money)
		{
			Money += money;
		}
	}
}