using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Karesz
{
	public partial class Form1 : Form
	{
		static Random r = new Random();
		string betöltendő_pálya = "palya32.txt";
		void TANÁR_ROBOTJAI()
		{
			Robot karesz = new Robot("Karesz", 0, 0, 0, 0, 0, 5, 28, 0, false, false);
			Betölt(betöltendő_pálya);
			Frissít();
		}
	}
}