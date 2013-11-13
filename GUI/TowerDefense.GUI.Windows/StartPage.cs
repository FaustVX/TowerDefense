using System;
using System.Threading;
using System.Windows.Forms;
using CSharpHelper;

namespace TowerDefense.GUI.Windows
{
	public partial class StartPage : Form
	{
		private readonly Action<string, int, int, bool, ArroundSelectMode, bool> _start;
		private ArroundSelectMode _mode;
		private bool _fullscreen;
		private bool _fancy;
		private readonly string _text;

		public StartPage(Action<string, int, int, bool, ArroundSelectMode, bool> start)
		{
			_start = start;
			_mode = ArroundSelectMode.Round;
			_fullscreen = false;
			_fancy = true;
			_text = "Taille Cellules";

			InitializeComponent();

			button3.Enabled = !_fullscreen;
			button4.Enabled = _fullscreen;

			button5.Enabled = _fancy;
			button6.Enabled = !_fancy;

			AcceptButton = button1;
			var rnd = new Random();
			var length = rnd.Next(5, 20);
			char[] text = new char[length];
			for (int i = 0; i < length; i++)
				text[i] = (char)rnd.Next(97, 123);
			textBox1.Text = new string(text);
			label3.Text = _text + " (" + trackBar1.Value + ")";

		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(!Enum.TryParse(numericUpDown1.Text, out _mode))
			{
				_mode= ArroundSelectMode.Round;
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			int size = trackBar1.Value;
			string name = textBox1.Text;
			int money = (int)numericUpDown1.Value;
			Close();

			new Thread(() => _start(name, money, size, _fullscreen, _mode, _fancy)).Start();
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

		private void button_FancyMouse_Click(object sender, EventArgs e)
		{
			_fancy = !_fancy;
			button5.Enabled = _fancy;
			button6.Enabled = !_fancy;
		}
	}
}
