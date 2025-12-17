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
using static System.Net.Mime.MediaTypeNames;

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
			public static Robot Get(string n) => (Robot) Test.lista.First(x => x.Ez_egy_robot && x.Név == n);
			protected int Indexe() => Test.lista.FindIndex(r => r == this);
			#endregion
			#region Instanciák tulajdonságai
			public string Név { get; private set; }
			public Vektor h;
			public Vektor H { get => h; }
			protected Vektor helyigény;
			protected Vektor v;
			public virtual bool Ez_egy_robot => false;
			public virtual bool Ez_egy_lövedék => false;

			public virtual Bitmap Iránykép() { return null; }

			#endregion
			public override string ToString() => $"{this.Név} ({this.H})";
			#region Konstruktorok

			public Test(string név, Vektor h, Vektor v)
			{
				this.Név = név;
				this.h = h;
				this.v = v;
				this.helyigény = h;
				Test.lista.Add(this);
			}
			public Test(string adottnév, int x, int y, int f) : this(adottnév, new Vektor(x, y), new Vektor(f)){}
			public Test(string adottnév, int x, int y) : this(adottnév, x, y, 0){ }
			public Test(string adottnév) : this(adottnév, 5, 28){ }
			#endregion
			#region Játékkezelés
			protected static void ek_léptetése()
			{
				Test.Új_lövedékek_létrehozása();

				foreach (Lövedék lövedék in Test.lista.OfType<Lövedék>())
					lövedék.Lépj();

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
					Lövedék golyesz = new Lövedék($"Golyesz-{golyeszcount++}", h, v);
                }
				Test.Ellövendő_lövedékek.Clear();
            }

			protected virtual void Eltavolitasa(){}

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
			protected virtual void Sírkő_letétele(){}

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
			public virtual void Lépj(){}

			protected static HashSet<(Vektor, Vektor)> Ellövendő_lövedékek = new HashSet<(Vektor, Vektor)>();

			bool Más_test_van_itt(Vektor v) => -1 < Test.lista.FindIndex(r => r.H == v);

			protected int Akadálytávolság(Vektor hely, Vektor sebesség)
			{
				int d = 1;
				Vektor J = new Vektor(hely + sebesség);
				while (pálya.BenneVan(J) && !(pálya.MiVanItt(J) == 1 || Más_test_van_itt(J)))
				{
					J += sebesség;
					d++;
				}
				return pálya.BenneVan(J) ? d : -1;
			}
			#endregion

			public void Megy()
			{
				h += v;
			}
			public void Forog(int irany)
			{
				v.Forgat(irany);
			}

		}
	}
}