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
		string betöltendő_pálya = "kiszabadit.txt";
		void TANÁR_ROBOTJAI()
		{
			Betölt(betöltendő_pálya);
			List<Vektor> alsókocka = Vektor.Rács(new Vektor(33, 23), new Vektor(40, 30));
			List<Vektor> jobbalsóháromszög = alsókocka.Where(p => p.X + p.Y >= 63).ToList();
			jobbalsóháromszög.Shuffle();
			foreach (Vektor p in jobbalsóháromszög.Take(2))
			{
				pálya.LegyenItt(p, hó);
			}

			int x = r.Next(18, 23);
			Vektor ablak = new Vektor(x, 16);
			pálya.LegyenItt(ablak, üres);


			Vektor mag = new Vektor(37, 25);
			List<Vektor> lehetséges_útvégek = new List<Vektor>();
			while (mag.Y < 30)
			{
				lehetséges_útvégek.Add(mag);
				mag += new Vektor(-1, 1);
			}
			lehetséges_útvégek.Shuffle();
			Vektor út_vége = lehetséges_útvégek.First();
			Vektor móló_vége = new Vektor(x, 21);
			foreach (Vektor p in Vektor.FüggőlegesenVízszintesen(móló_vége, út_vége))
			{
				pálya.LegyenItt(p, fekete);
			}

			Robot karesz = new Robot("Karesz", 0, 0, 0, 0, 0, 5, 28, 0, false, false);
			Frissít();
		}
	}
}