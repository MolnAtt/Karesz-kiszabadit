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
		void Türelmesen_Lépj(Robot r, int db)
		{
			while(0 < db)
			{
				if (1 != r.UltrahangSzenzor())
				{
					r.Lépj();
					db--;
				}
				else
				{
					r.Várj();
				}
			}
		}
		void Körbemegy(Robot r)
		{
			for (int i = 0; i < 4; i++)
			{
				Türelmesen_Lépj(r, 3);
				r.Fordulj(jobbra);
				Türelmesen_Lépj(r, 3);
			}
		}
		void Félkör(Robot r)
		{
			Türelmesen_Lépj(r, 18);
			r.Fordulj(balra);
			Körbemegy(r);
			r.Fordulj(balra);
		}
		void TANÁR_ROBOTJAI()
		{
			Betölt(betöltendő_pálya);
			List<Vektor> alsókocka = Vektor.Rács(new Vektor(33, 23), new Vektor(40, 30));
			List<Vektor> jobbalsóháromszög = alsókocka.Where(p => p.X + p.Y >= 64).ToList();
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
			foreach (Vektor p in Vektor.Cikk(móló_vége, út_vége))
			{
				pálya.LegyenItt(p, fekete);
			}

			Robot karesz = new Robot("Karesz", 0, 0, 0, 0, 0, 39 + r.Next(3) - 1, 29 + r.Next(3) - 1, 0, true, false);
			Frissít();
			Robot őrvezető = new Robot("Őrvezető", 0, 0, 0, 0, 0, 19, 15, 3, true, false);
			//őrvezető.Feladat = delegate () { };
			őrvezető.Feladat = delegate ()
			{
				Türelmesen_Lépj(őrvezető, 8);
				őrvezető.Fordulj(balra);
				Körbemegy(őrvezető);
				őrvezető.Fordulj(balra);
				while (true)
				{
					Félkör(őrvezető);
				}
			};
			Robot lilesz = new Robot("Lilesz", 0, 0, 0, 0, 0, 20, 15, 3, true, false);
			//lilesz.Feladat = delegate () { };
			lilesz.Feladat = delegate ()
			{
				Türelmesen_Lépj(lilesz, 9);
				lilesz.Fordulj(balra);
				Körbemegy(lilesz);
				lilesz.Fordulj(balra);
				int db = 0;
				while (!(Robot.lista.Count==2 && Robot.lista.Contains(lilesz) && Robot.lista.Contains(karesz) && db%2==1))
				{
					Félkör(lilesz);
					db++;
				}

				lilesz.Mondd("De hiszen már nem őriz senki! Szabad vagyok!");
				Türelmesen_Lépj(lilesz, 9);
				lilesz.Fordulj(jobbra);
				Türelmesen_Lépj(lilesz, 12);
				lilesz.Fordulj(balra);
				Türelmesen_Lépj(lilesz, 2);
				lilesz.Fordulj(jobbra);
				Türelmesen_Lépj(lilesz, 2);
				lilesz.Fordulj(balra);
				Türelmesen_Lépj(lilesz, 100);
			};
			Robot közlegény = new Robot("Közlegény", 0, 0, 0, 0, 0, 21, 15, 3, true, false);
			//közlegény.Feladat = delegate () { };
			közlegény.Feladat = delegate ()
			{
				Türelmesen_Lépj(közlegény, 10);
				közlegény.Fordulj(balra);
				Körbemegy(közlegény);
				közlegény.Fordulj(balra);
				while (true)
				{
					Félkör(közlegény);
				}
			};

		}
	}
}