using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Karesz
{
	partial class Form1
	{
		class Robot
		{
			#region statikus tulajdonságok
			public static int várakozási_idő = 100;
			public static Form1 form;
			static Pálya pálya { get => Robot.form.pálya; }
			public static List<Robot> lista = new List<Robot>();
			static HashSet<Robot> halállista = new HashSet<Robot>();
			public static int ok_száma { get => Robot.lista.Count; }
			public static ModuloSzam megfigyeltindex;
			public static Robot akit_kiválasztottak { get => lista[megfigyeltindex.ToInt()]; }
			public static int uh(Robot r) => r.Akadálytávolság(r.H, r.v);
			#endregion
			#region statikus metódusok
			public static Robot Get(string n) => Robot.lista.First(x => x.Név == n);
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
			static readonly Bitmap[] képkészlet_golyesz = new Bitmap[4]
			{
				Properties.Resources.golyesz_up,
				Properties.Resources.golyesz_right,
				Properties.Resources.golyesz_down,
				Properties.Resources.golyesz_left
			};
			
			int Indexe() => Robot.lista.FindIndex(r => r == this);
			public static bool ok_közül_valaki_még_dolgozik() => -1 < Robot.lista.FindIndex(r => !r.Kész);
			#endregion
			#region Instanciák tulajdonságai
			public string Név { get; private set; }
			Bitmap[] képkészlet;
			public Vektor h;
			public Vektor H { get => h; }
			Vektor helyigény;
			Vektor v;
			int[] kődb;
			Action feladat;
			bool uh_engedélyezve;
			bool szuh_engedélyezve;
			// cooperative scheduling
			Thread szál;

			// Analógia: Minden autónak van egy jelzőlámpája és egy dudája
			// Ha a jelzőlámpája zöldre vált, elindul be a kereszteződésbe
			// Ha a kereszteződésen átért, megfújja a dudáját, jelezve, hogy végeztem
			AutoResetEvent jelzőlámpa;
			AutoResetEvent duda;
			volatile bool végzett;

			public bool Végzett => végzett;


			public Action Feladat
			{
				get => feladat;
				set
				{
					feladat = value;
					szál = new Thread(ThreadMain);
				}
			}

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

			#endregion
			public override string ToString() => $"{this.Név} ({this.H})";
			#region Konstruktorok

			/// <summary>
			/// Teljes konstruktor: Létrehoz egy új robotot a megadott névvel, képkészlettel, pozícióval, iránnyal és induló kövek számával.
			/// </summary>
			/// <param name="név">A robot neve</param>
			/// <param name="képkészlet">A képkészlet a Resources mappából</param>
			/// <param name="H">indulási pozíció</param>
			/// <param name="I">kezdőirány</param>
			/// <param name="kődb">induláskor a zsebeiben lévő kövek száma</param>
			/// <param name="szülőform">az eredeti form, a visszahivatkozáshoz kell</param>
			/// <param name="pálya">a pálya, amin a robot mozog</param>
			public Robot(string név, Bitmap[] képkészlet, int[] kődb, Vektor h, Vektor v, bool uh_engedélyezve=true, bool szuh_engedélyezve=true)
			{
				this.Név = név;
				this.h = h;
				this.v = v;
				this.képkészlet = képkészlet;
				this.kődb = kődb;
				this.helyigény = h;
				this.uh_engedélyezve = uh_engedélyezve;
				this.szuh_engedélyezve = szuh_engedélyezve;

				// scheduler
				this.jelzőlámpa = new AutoResetEvent(false);
				this.duda = new AutoResetEvent(false);
				this.végzett = false;

				if (0 == Robot.lista.Count)
					Robot.megfigyeltindex = new ModuloSzam(0,1);
				else
					Robot.megfigyeltindex.ModulusNövelése();

				Robot.lista.Add(this);
			}
			public Robot(string adottnév, int[] indulókövek, Vektor hely, Vektor sebesség,bool uh_engedélyezve= true, bool szuh_engedélyezve = true)
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
							this(adottnév, new int[] { fekete_db, piros_db, zöld_db, sárga_db, hó_db }, new Vektor(x, y), new Vektor(f),uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév, int x, int y, int f, bool uh_engedélyezve = true, bool szuh_engedélyezve = true) :
				this(adottnév, 0, 0, 0, 0, 0, x, y, f, uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév, int x, int y, bool uh_engedélyezve= true, bool szuh_engedélyezve = true) :
				this(adottnév, x, y, 0, uh_engedélyezve, szuh_engedélyezve)
			{ }
			public Robot(string adottnév) :
				this(adottnév, 5, 28)
			{ }
            #endregion
            #region Játékkezelés
            static void ok_elindítása()
			{
				foreach (Robot robot in Robot.lista)
					robot.Indítása_ha_áll();
			}
			public static void Játék()
			{

				Robot.ok_elindítása();

				// első kör: az első robot kapjon engedélyt
				// (vagy mindenki, ha körönként haladunk – itt az egyszerűbb verzió)
				while (lista.Exists(r => !r.Végzett))
				{
					foreach (Robot robot in lista)
					{
						if (!robot.Végzett)
						{
							robot.jelzőlámpa.Set();
							robot.duda.WaitOne();
						}
					}

					Robot.ok_léptetése();
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
			static void ok_léptetése()
			{
				Robot.Új_lövedékek_létrehozása();
				Robot.holtak_összegyűjtése();
				Robot.holtak_eltávolítása();
				foreach (Robot robot in Robot.lista)
					robot.h = robot.helyigény;
			}

            private static void Új_lövedékek_létrehozása()
            {
                foreach ((Vektor, Vektor) p in Robot.Ellövendő_lövedékek)
                {
					(Vektor h, Vektor v) = p;
					Robot golyesz = new Robot("Golyesz", képkészlet_golyesz, new int[] { 0, 0, 0, 0, 0 }, h, v);
					golyesz.Feladat = delegate ()
					{
                        while (true)
							golyesz.Lépj();
					};
                }
				Robot.Ellövendő_lövedékek.Clear();
            }

			public void MarkAsFinished()
			{
				végzett = true;

				// jelezhetünk is, ha épp egy turn-ben halt meg
				duda.Set();
			}

			static void holtak_eltávolítása()
			{
				foreach (Robot robot in Robot.halállista)
				{
					robot.Sírkő_letétele();
					if (robot.Indexe() < megfigyeltindex.ToInt())
						--megfigyeltindex;

					robot.MarkAsFinished();   // _finished = true;

					Robot.lista.Remove(robot);
					megfigyeltindex.ModulusCsökkentése();


				}
				Robot.halállista.Clear();
			}

			static void holtak_összegyűjtése()
			{
				Robot.Halállistához(r => pálya.MiVanItt(r.helyigény) == fal); // falba lép
				Robot.Halállistához(r => !pálya.BenneVan(r.helyigény)); // kiesik a pályáról
				Robot.Halállistához((r1, r2) => r1.helyigény == r2.helyigény); // egy helyre léptek
				Robot.Halállistához((r1, r2) => r1.helyigény == r2.H && r2.helyigény == r1.H); // átmentek egymáson / megpróbáltak helyet cserélni
			}
			void Sírkő_letétele()
			{
				if (Név == "Golyesz")
					pálya.LegyenItt(H, hó);
				else
					pálya.LegyenItt(H, fekete);
			}

			void Meghal() => Robot.halállista.Add(this);
			static void Halállistához(Func<Robot, bool> predikátum)
			{
				foreach (Robot robot in Robot.lista)
					if (predikátum(robot))
						robot.Meghal();
			}
			static void Halállistához(Func<Robot, Robot, bool> predikátum)
			{
				for (int i = 0; i < Robot.lista.Count; i++)
					for (int j = i+1; j < Robot.lista.Count; j++)
						if (predikátum(Robot.lista[i], Robot.lista[j]))
						{
							lista[i].Meghal();
							lista[j].Meghal();
						}
			}
			//void Start_or_Resume()
			//{
			//	if (this.thread.ThreadState == ThreadState.Unstarted)
			//		this.thread.Start();
			//	else if (this.Vár)
			//		this.thread.Resume();
			//}
			#endregion
			#region Motorok
			/// <summary>
			/// Elhelyezi a Robotot a megadott helyre.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			public void Teleport(int x, int y)
			{
				(h.X, h.Y) = (x, y);
				(helyigény.X, helyigény.Y) = (x, y);
			}
			/// <summary>
			/// Lépteti a robotot a megfelelő irányba.
			/// </summary>
			public void Lépj()
			{
				helyigény = h+v;
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
			/// <summary>
			/// Lerakja az adott színű követ a pályán a robot helyére.
			/// </summary>
			/// <param name="szín"></param>
			public void Tegyél_le_egy_kavicsot(int szín = fekete)
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
					Robot.Ellövendő_lövedékek.Add((this.H + this.v, this.v));
				}
				else
					Mondd("Nincsen nálam hó!");

				Cselekvés_vége();
			}

			static HashSet<(Vektor, Vektor)> Ellövendő_lövedékek = new HashSet<(Vektor, Vektor)>();

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


			int Akadálytávolság(Vektor hely, Vektor sebesség)
			{
				int d = 1;
				Vektor J = new Vektor(hely + sebesség);
				while (pálya.BenneVan(J) && !(pálya.MiVanItt(J) == 1 || Más_robot_van_itt(J)))
				{
					J += sebesség;
					d++;
				}
				return pálya.BenneVan(J) ? d : -1;
			}
			bool Más_robot_van_itt(Vektor v) => -1 < Robot.lista.FindIndex(r => r.H == v);
			HashSet<Vektor> Más_robotok_helyei() => Robot.lista.Select(x => x.H).ToHashSet();
			public int Hőmérő() =>
				pálya.Hőmérséklet(H);
			#endregion
			#region Formkezeléshez és szálkezeléshez szolgáló metódusok

			/// <summary>
			/// Visszaadja a sebességvektor számkódját, ami a képek kezeléséhez kell.
			/// </summary>
			/// <returns></returns>
			public Bitmap Iránykép() => képkészlet[v.ToInt()];
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
		}
	}
}