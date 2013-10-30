using System;
using System.Threading;
using System.Windows.Forms;
using CSharpHelper;

namespace TowerDefense.GUI.Windows
{
	public partial class StartPage : Form
	{
		private readonly Action<int, int, bool, ArroundSelectMode> _start;
		private ArroundSelectMode _mode;
		private int _seed;
		private bool _fullscreen;
		private readonly string _text;

		public StartPage(Action<int, int, bool, ArroundSelectMode> start)
		{
			_start = start;
			_seed = Environment.TickCount;
			_mode = ArroundSelectMode.Round;
			_fullscreen = false;
			_text = "Taille Cellules";

			InitializeComponent();

			button3.Enabled = !_fullscreen;
			button4.Enabled = _fullscreen;
			textBox1.Text = _seed.ToString();

			AcceptButton = button1;
			comboBox1.SelectedIndex = 0;
			label3.Text = _text + " (" + trackBar1.Value + ")";

			ShowDialog();

		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(!Enum.TryParse(comboBox1.Text, out _mode))
			{
				_mode= ArroundSelectMode.Round;
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
			int size = trackBar1.Value;

			new Thread(() => _start(_seed, size, _fullscreen, _mode)).Start();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			if (textBox1.Text == string.Empty)
				_seed = Environment.TickCount;
			else if (!int.TryParse(textBox1.Text, out _seed))
				_seed = textBox1.Text.GetHashCode();
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			label3.Text = _text + " (" + trackBar1.Value + ")";
		}

		private void button_fullscreen_Click(object sender, EventArgs e)
		{
			_fullscreen = !_fullscreen;
			button3.Enabled = !_fullscreen;
			button4.Enabled = _fullscreen;
		}
	}
}
