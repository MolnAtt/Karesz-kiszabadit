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

		class Test
		{
			#region statikus tulajdonságok
			public static int várakozási_idő = 100;
			public static Form1 form;
			protected static Pálya pálya { get => Test.form.pálya; }
			public static List<Test> lista = new List<Test>();
			static HashSet<Test> halállista = new HashSet<Test>();
			public static int ek_száma { get => Test.lista.Count; }
			public static int uh(Test r) => r.Akadálytávolság(r.H, r.v);
			#endregion
			#region statikus metódusok
			public static Test Get(string n) => Test.lista.First(x => x.Név == n);
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
			
			protected int Indexe() => Test.lista.FindIndex(r => r == this);
			#endregion
			#region Instanciák tulajdonságai
			public string Név { get; private set; }
			Bitmap[] képkészlet;
			public Vektor h;
			public Vektor H { get => h; }
			protected Vektor helyigény;
			protected Vektor v;
			public virtual bool Ez_egy_robot => false;

			#endregion
			public override string ToString() => $"{this.Név} ({this.H})";
			#region Konstruktorok

			public Test(string név, Bitmap[] képkészlet, Vektor h, Vektor v)
			{
				this.Név = név;
				this.h = h;
				this.v = v;
				this.képkészlet = képkészlet;
				this.helyigény = h;
				Test.lista.Add(this);
			}
			public Test(string adottnév, Vektor hely, Vektor sebesség): this(adottnév, képkészlet_karesz, hely, sebesség) { }
			public Test(string adottnév, int x, int y, int f) : this(adottnév, new Vektor(x, y), new Vektor(f)){}
			public Test(string adottnév, Bitmap[] képkészlet, int x, int y, int f) : this(adottnév, képkészlet, new Vektor(x, y), new Vektor(f)) {}
			public Test(string adottnév, int x, int y) : this(adottnév, x, y, 0){ }
			public Test(string adottnév) : this(adottnév, 5, 28){ }
			#endregion
			#region Játékkezelés



			protected static void ek_léptetése()
			{
				Test.Új_lövedékek_létrehozása();
				Test.holtak_összegyűjtése();
				Test.holtak_eltávolítása();
				foreach (Test test in Test.lista)
					test.h = test.helyigény;
			}

			static int golyeszcount = 0;

			private static void Új_lövedékek_létrehozása()
            {
                foreach ((Vektor, Vektor) p in Test.Ellövendő_lövedékek)
                {
					(Vektor h, Vektor v) = p;
					Test golyesz = new Test($"Golyesz-{golyeszcount++}", képkészlet_golyesz, h, v);
                }
				Test.Ellövendő_lövedékek.Clear();
            }

			void Eltavolitasa()
			{
				Test.lista.Remove(this);
			}

			static void holtak_eltávolítása()
			{
				foreach (Test test in Test.halállista)
				{
					test.Eltavolitasa();
				}
				Test.halállista.Clear();
			}

			static void holtak_összegyűjtése()
			{
				Test.Halállistához(t => pálya.MiVanItt(t.helyigény) == fal); // falnak ütközik
				Test.Halállistához(t => pálya.MiVanItt(t.helyigény) == láva && t.Ez_egy_robot); // robotként lávába lép
				Test.Halállistához(t => !pálya.BenneVan(t.helyigény)); // kiesik a pályáról
				Test.Halállistához((t1, t2) => t1.helyigény == t2.helyigény); // egy helyre léptek
				Test.Halállistához((t1, t2) => t1.helyigény == t2.H && t2.helyigény == t1.H); // átmentek egymáson / megpróbáltak helyet cserélni
			}
			void Sírkő_letétele()
			{
				pálya.LegyenItt(H, hó);
			}

			public void Meghal() => Test.halállista.Add(this);
			static void Halállistához(Func<Test, bool> predikátum)
			{
				foreach (Test test in Test.lista)
					if (predikátum(test))
						test.Meghal();
			}
			static void Halállistához(Func<Test, Test, bool> predikátum)
			{
				for (int i = 0; i < Test.lista.Count; i++)
					for (int j = i+1; j < Test.lista.Count; j++)
						if (predikátum(Test.lista[i], Test.lista[j]))
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
			/// Elhelyezi a Testet a megadott helyre.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			public void Teleport(int x, int y)
			{
				(h.X, h.Y) = (x, y);
				(helyigény.X, helyigény.Y) = (x, y);
			}
			/// <summary>
			/// Lépteti a testet a megfelelő irányba.
			/// </summary>
			public void Lépj()
			{
				helyigény = h+v;
			}

			protected static HashSet<(Vektor, Vektor)> Ellövendő_lövedékek = new HashSet<(Vektor, Vektor)>();


			protected int Akadálytávolság(Vektor hely, Vektor sebesség)
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
			#endregion
			#region Formkezeléshez és szálkezeléshez szolgáló metódusok

			/// <summary>
			/// Visszaadja a sebességvektor számkódját, ami a képek kezeléséhez kell.
			/// </summary>
			/// <returns></returns>
			public Bitmap Iránykép() => képkészlet[v.ToInt()];


			#endregion
		}
	}
}