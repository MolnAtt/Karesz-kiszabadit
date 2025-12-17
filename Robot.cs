using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Karesz
{
	partial class Form1
	{
		class Robot : Test
		{
			#region statikus tulajdonságok
			public static ModuloSzam megfigyeltindex;
			public static Robot akit_kiválasztottak { get => (Robot)Test.lista[megfigyeltindex.ToInt()]; }

			public static readonly Bitmap[] képkészlet_karesz = new Bitmap[4]
			{
				Properties.Resources.Karesz0,
				Properties.Resources.Karesz1,
				Properties.Resources.Karesz2,
				Properties.Resources.Karesz3
			};
			public static readonly Bitmap[] képkészlet_lilesz = new Bitmap[4]
			{
				Properties.Resources.Lilesz0,
				Properties.Resources.Lilesz1,
				Properties.Resources.Lilesz2,
				Properties.Resources.Lilesz3
			};

			public static readonly Bitmap[] képkészlet_maresz = new Bitmap[4]
			{
				Properties.Resources.Maresz0,
				Properties.Resources.Maresz1,
				Properties.Resources.Maresz2,
				Properties.Resources.Maresz3
			};

			public static readonly Bitmap[] képkészlet_gonesz = new Bitmap[4]
			{
				Properties.Resources.Gonesz0,
				Properties.Resources.Gonesz1,
				Properties.Resources.Gonesz2,
				Properties.Resources.Gonesz3
			};
			#endregion
			#region tulajdonságok
			public override bool Ez_egy_robot => true;
			int[] kődb;
			Bitmap[] képkészlet;
			readonly bool uh_engedélyezve;
			readonly bool szuh_engedélyezve;
			
			Action feladat;
			public Action Feladat
			{
				get => feladat;
				set
				{
					feladat = value;
					szál = new Thread(ThreadMain);
				}
			}

			// COOPERATIVE SCHEDULING
			// Analógia: Minden autónak van egy jelzőlámpája és egy dudája
			// Ha a jelzőlámpája zöldre vált, elindul be a kereszteződésbe
			// Ha a kereszteződésen átért, megfújja a dudáját, jelezve, hogy végeztem
			Thread szál;
			AutoResetEvent jelzőlámpa;
			AutoResetEvent duda;
			volatile bool végzett;
			public bool Végzett => végzett;

			#endregion
			#region konstruktorok
			public Robot(string név, Bitmap[] képkészlet, int[] kődb, Vektor h, Vektor v, bool uh_engedélyezve = true, bool szuh_engedélyezve = true) : base(név, h, v)
			{
				this.képkészlet = képkészlet;
				this.kődb = kődb;
				this.uh_engedélyezve = uh_engedélyezve;
				this.szuh_engedélyezve = szuh_engedélyezve;

				// scheduler
				this.jelzőlámpa = new AutoResetEvent(false);
				this.duda = new AutoResetEvent(false);
				this.végzett = false;

				if (0 == Test.lista.Count)
					Robot.megfigyeltindex = new ModuloSzam(0, 1);
				else
					Robot.megfigyeltindex.ModulusNövelése();

			}
			public Robot(string adottnév, int[] indulókövek, Vektor hely, Vektor sebesség, bool uh_engedélyezve = true, bool szuh_engedélyezve = true)
				: this(adottnév, képkészlet_karesz,
						indulókövek,
						hely,
						sebesség,
						uh_engedélyezve,
						szuh_engedélyezve
						)
			{ }
			public Robot(string adottnév, int[] indulókövek, int x, int y, int f, bool uh_engedélyezve = true, bool szuh_engedélyezve = true) :
				this(adottnév, indulókövek, new Vektor(x, y), new Vektor(f), uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév, Bitmap[] képkészlet, int fekete_db, int piros_db, int zöld_db, int sárga_db, int hó_db, int x, int y, int f, bool uh_engedélyezve = true, bool szuh_engedélyezve = true) :
							this(adottnév, képkészlet, new int[] { fekete_db, piros_db, zöld_db, sárga_db, hó_db }, new Vektor(x, y), new Vektor(f), uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév, int fekete_db, int piros_db, int zöld_db, int sárga_db, int hó_db, int x, int y, int f, bool uh_engedélyezve = true, bool szuh_engedélyezve = true) :
							this(adottnév, new int[] { fekete_db, piros_db, zöld_db, sárga_db, hó_db }, new Vektor(x, y), new Vektor(f), uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév, int x, int y, int f, bool uh_engedélyezve = true, bool szuh_engedélyezve = true) :
				this(adottnév, 0, 0, 0, 0, 0, x, y, f, uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév, int x, int y, bool uh_engedélyezve = true, bool szuh_engedélyezve = true) :
				this(adottnév, x, y, 0, uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév) :
				this(adottnév, 5, 28)
			{ }

			#endregion
			#region Játékkezelés
			void ThreadMain()
			{
				jelzőlámpa.WaitOne();

				try
				{
					feladat?.Invoke();
				}
				finally // mert ha a diák kódja rossz, akkor ne akadjon el a scheduler
				{
					végzett = true;
					duda.Set();
				}
			}
			void Indítása_ha_áll()
			{
				if (szál.ThreadState == ThreadState.Unstarted)
					szál.Start();
			}
			public bool Kész { get => szál.ThreadState == ThreadState.Stopped; }
			public bool Vár { get => szál.ThreadState == ThreadState.Suspended; }
			public bool Elindult { get => szál.ThreadState != ThreadState.Unstarted; }
			static void ok_elindítása()
			{
				foreach (Robot robot in Test.lista.OfType<Robot>())
					robot.Indítása_ha_áll();
			}
			public static void Játék()
			{
				Robot.ok_elindítása();

				// első kör: az első robot kapjon engedélyt
				// (vagy mindenki, ha körönként haladunk – itt az egyszerűbb verzió)
				while (Test.lista.OfType<Robot>().Any(r => !r.Végzett))
				{
					foreach (Robot robot in Test.lista.OfType<Robot>().ToList())
					{
						if (!robot.Végzett)
						{
							robot.jelzőlámpa.Set();
							robot.duda.WaitOne();
						}
					}

					Test.ek_léptetése();
					Robot.form.Frissít();

					Thread.Sleep(várakozási_idő);
				}

				SendKeys.Send("%"); // valamilyen misztikus okból kifolyólag nem frissít rendesen az ablak a végén, csak ha valaki az ALT gombot lenyomja...

				//Thread.Sleep(várakozási_idő);
				//while (Robot.lista.Exists(r => !r.Kész))
				//{
				//	if (Robot.lista.TrueForAll(r => r.Kész || r.Vár))
				//	{
				//		Robot.ok_léptetése();
				//		Robot.form.Frissít();
				//		Robot.ok_elindítása();
				//	}
				//	Thread.Sleep(várakozási_idő);
				//}
				//Robot.form.Frissít();
				//SendKeys.Send("%"); // valamilyen misztikus okból kifolyólag nem frissít rendesen az ablak a végén, csak ha valaki az ALT gombot lenyomja...
			}
			public void MarkAsFinished()
			{
				végzett = true;

				// jelezhetünk is, ha épp egy turn-ben halt meg
				duda.Set();
			}
			protected override void Sírkő_letétele()
			{
				Test.pálya.LegyenItt(H, fekete);
			}
			protected override void Eltavolitasa()
			{
				this.Sírkő_letétele();
				if (this.Indexe() < Robot.megfigyeltindex.ToInt())
					--Robot.megfigyeltindex;

				this.MarkAsFinished();   // _finished = true;				
				Test.lista.Remove(this);
				Robot.megfigyeltindex.ModulusCsökkentése();
			}
			void Cselekvés_vége()
			{
				// régi
				//if (!Kész && Elindult)
				//	this.thread.Suspend(); 
				if (Végzett) return;

				duda.Set();   // jelez a schedulernek: „kész a köröm”
				jelzőlámpa.WaitOne();   // és várja a következő engedélyt
			}
			#endregion
			#region Cselekvések
			/// <summary>
			/// Lépteti a testet a megfelelő irányba.
			/// </summary>
			public override void Lépj()
			{
				helyigény = h + v;
				Cselekvés_vége();
			}

			/// <summary>
			/// Elforgatja a robotot a megadott irányban. (Csak normális irányokra reagál.)
			/// </summary>
			/// <param name="forgásirány"></param>
			public void Fordulj(int forgásirány)
			{
				v.Forgat(forgásirány);
				Cselekvés_vége();
			}

			public void Kavicsot_tesz_le(int szín = fekete)
			{
				if (pálya.MiVanItt(H) != üres)
					Mondd("Nem tudom a kavicsot lerakni, mert van lerakva kavics!");
				else if (kődb[szín - 2] <= 0)
					Mondd($"Nem tudom a kavicsot lerakni, mert nincs {színnév[szín]} színű kavicsom!");
				else
				{
					pálya.LegyenItt(H, szín);
					--kődb[szín - 2];
					idő++;
				}
			}

			/// <summary>
			/// Lerakja az adott színű követ a pályán a robot helyére.
			/// </summary>
			/// <param name="szín"></param>
			public void Tegyél_le_egy_kavicsot(int szín = fekete)
			{
				Kavicsot_tesz_le(szín);
				Cselekvés_vége();
			}


			/// <summary>
			/// Felveszi azt, amin éppen áll -- feltéve ha az nem fal, stb.
			/// </summary>
			public void Vegyél_fel_egy_kavicsot()
			{

				if (pálya.MiVanItt(H) > fal)
				{
					++kődb[pálya.MiVanItt(H) - 2];
					pálya.LegyenItt(H, üres);
					idő++;
				}
				else
					Mondd(": Nem tudom a kavicsot felvenni!");

				Cselekvés_vége();
			}
			public void Lőjj()
			{
				if (0 < kődb[hó - 2])
				{
					--kődb[hó - 2];
					Test.Ellövendő_lövedékek.Add((this.H + this.v, this.v));
				}
				else
					Mondd("Nincsen nálam hó!");

				Cselekvés_vége();
			}

			public void Várj() => Cselekvés_vége();
			public void Mondd(string ezt) => MessageBox.Show(Név + ": " + ezt);

			#endregion
			#region Szenzorok

			/// <summary>
			/// Megadja, hogy az adott színből mennyi köve van a robotnak.
			/// </summary>
			/// <param name="szín"></param>
			/// <returns></returns>
			public int Köveinek_száma_ebből(int szín) => kődb[szín - 2];

			/// <summary>
			/// Megadja, hogy kavicson áll-e a robot.
			/// </summary>
			/// <returns></returns>
			public bool Alatt_van_kavics() =>
				pálya.MiVanItt(H) > fal;

			/// <summary>
			/// Megadja, hogy min áll a robot
			/// </summary>
			/// <returns></returns>
			public int Alatt_ez_van() =>
				pálya.MiVanItt(H);

			/// <summary>
			/// Megadja, hogy mi van a robot előtt az adott helyen -- (1 = fal, -1 = kilép)
			/// </summary>
			/// <returns></returns>
			int MiVanElőttem(Vektor Itt) =>
				pálya.BenneVan(Itt) ? pálya.MiVanItt(Itt) : -1;

			/// <summary>
			/// megadja, hogy mi van a robot előtt
			/// </summary>
			/// <returns></returns>
			public int MiVanElőttem() =>
				MiVanElőttem(H + v);

			/// <summary>
			/// Pontosan akkor igaz, ha a robot előtt fal van.
			/// </summary>
			/// <returns></returns>
			public bool Előtt_fal_van() => this.MiVanElőttem() == fal;
			/// <summary>
			/// Pontosan akkor igaz, ha a robot a pálya szélén van és a következő lépéssel kizuhanna a pályáról.
			/// </summary>
			/// <returns></returns>
			public bool Ki_fog_lépni_a_pályáról() => this.MiVanElőttem() == nincs_pálya;

			/// <summary>
			/// megadja, hogy milyen messze van a robot előtti legközelebbi olyan objektum, amely vissza tudja verni a hangot (per pill. másik robot vagy fal)
			/// </summary>
			/// <returns></returns>
			public int UltrahangSzenzor()
			{
				if (!this.uh_engedélyezve)
				{
					Mondd("Súlyos hibát követtem el, nem szabadott volna ultrahangszenzort használnom!");
					Meghal();
					return -1;
				}
				return Akadálytávolság(H, v);
			}
			public (int, int, int) SzélesUltrahangSzenzor()
			{
				if (!this.szuh_engedélyezve)
				{
					Mondd("Súlyos hibát követtem el, nem szabadott volna ultrahangszenzort használnom!");
					Meghal();
					return (-1, -1, -1);
				}
				return (Akadálytávolság(H + v.Forgatott(balra), v), Akadálytávolság(H, v), Akadálytávolság(H + v.Forgatott(jobbra), v));
			}

			public int Hőmérő() => pálya.Hőmérséklet(H);
			HashSet<Vektor> Más_robotok_helyei() => Test.lista.Select(x => x.H).ToHashSet();
			#endregion

			/// <summary>
			/// Visszaadja a sebességvektor számkódját, ami a képek kezeléséhez kell.
			/// </summary>
			/// <returns></returns>
			public override Bitmap Iránykép() => képkészlet[v.ToInt()];

		}
	}
}
