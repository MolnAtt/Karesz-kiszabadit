using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Karesz
{
	/// <summary>
	/// Egészek fölötti kétdimenziós vektorok (Z^2)
	/// </summary>
	struct Vektor
	{
		public static Vektor Észak { get => new Vektor(0, -1); }
		public static Vektor Dél { get => new Vektor(0, 1); }
		public static Vektor Kelet { get => new Vektor(1, 0); }
		public static Vektor Nyugat { get => new Vektor(-1, 0); }

		#region tulajdonságok

		public int X, Y;

        #endregion

        #region konstruktorok

        public Vektor(int x, int y) =>
			(X, Y) = (x, y);
		public Vektor(Vektor V) :
			this(V.X, V.Y)
		{ }

		/// <summary>
		/// 0:É, 1:NY, 2:D, 3:K, ...
		/// </summary>
		/// <param name="f"></param>
		public Vektor(int f)
		{
			if (3 < f) f %= 4;
			if (f < 0) f *= -1;

			switch (f)
			{
				case 0:
					(X, Y) = (0, -1);
					break;
				case 1:
					(X, Y) = (1, 0);
					break;
				case 2:
					(X, Y) = (0, 1);
					break;
				case 3:
					(X, Y) = (-1, 0);
					break;
				default:
					(X, Y) = (0, 0);
					break;
			}
		}

		#endregion

		#region operátorok (+,-,*,/,==,!=)

		public static Vektor operator +(Vektor u, Vektor v) =>
			new Vektor(u.X + v.X, u.Y + v.Y);
		public static Vektor operator -(Vektor u, Vektor v) =>
			new Vektor(u.X - v.X, u.Y - v.Y);
		public static int operator *(Vektor u, Vektor v) =>
			u.X * v.X + u.Y * v.Y;
		public static Vektor operator *(Vektor u, int a) =>
			new Vektor(u.X * a, u.Y * a);
		public static Vektor operator *(int a, Vektor u) =>
			u * a;
		public static Vektor operator /(Vektor u, int a) =>
			new Vektor(u.X / a, u.Y / a);

		public static bool operator ==(Vektor u, Vektor v) =>
			u.X == v.X && u.Y == v.Y;
		public static bool operator !=(Vektor u, Vektor v) =>
			!(u == v);

		#endregion

		#region egyéb műveletek

		public override bool Equals(object obj)
		{
			if (!(obj is Vektor))
				return false;

			Vektor other = (Vektor)obj;
			return this == other;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + X;
				hash = hash * 31 + Y;
				return hash;
			}
		}


		public int HosszN() =>
			X * X + Y * Y;
		public void Forgat(int i) =>
			(X, Y) = (-i * Y, i * X); // fordított a koordinátarendszer!
		/// <summary>
		/// A négy égtáj fele mutató irányvektort lekódoljuk egy int-be. 
		/// 0: észak
		/// 1: kelet
		/// 2: dél
		/// 3: nyugat
		/// </summary>
		/// <returns></returns>
		public int ToInt() =>
			Y == -1 ? 0 : (X == 1 ? 1 : (Y == 1 ? 2 : 3));
		public int TavN(Vektor Q) =>
			(this - Q).HosszN();
		public Vektor Balra() =>
			new Vektor(X - 1, Y);
		public Vektor Jobbra() =>
			new Vektor(X + 1, Y);
		public Vektor Fent() =>
			new Vektor(X, Y + 1);
		public Vektor Lent() =>
			new Vektor(X, Y - 1);

		public Vektor Forgatott(int i)
		{
			Vektor v = new Vektor(this);
			v.Forgat(i);
			return v;
		}

		public override string ToString() => $"{X}, {Y}";

		static int intervallumba(int b, int x, int j) => Math.Min(Math.Max(b, x), j);
		public static Vektor dobozba(Vektor ba, Vektor v, Vektor jf) => new Vektor(intervallumba(ba.X, v.X, jf.X), intervallumba(ba.Y, v.Y, jf.Y));

		public bool Téglalapban_van(Vektor bf, Vektor jl) => bf.X <= this.X && this.X <= jl.X && bf.Y <= this.Y && this.Y <= jl.Y;

		public static List<Vektor> Rács(Vektor bf, Vektor jl)
		{
			Vektor átló = jl - bf;
			List<Vektor> mezők = new List<Vektor>((átló.X + 1) * (átló.Y + 1));

			for (int x = 0; x <= 40; x++)
			{
				for (int y = 0; y <= 30; y++)
				{
					mezők.Add(new Vektor(x, y));
				}
			}

			return mezők;
		}

		public static List<Vektor> Cikk(Vektor innen, Vektor ide)
		{
			Vektor vx = new Vektor(Math.Sign(ide.X - innen.X), 0);
			Vektor vy = new Vektor(0, Math.Sign(ide.Y - innen.Y));
			List<Vektor> útvonal = new List<Vektor>();
			Vektor itt = new Vektor(innen);
			while (itt.Y != ide.Y)
			{
				útvonal.Add(new Vektor(itt));
				itt += vy;
			}
			while (itt.X != ide.X)
			{
				útvonal.Add(new Vektor(itt));
				itt += vx;
			}
			útvonal.Add(new Vektor(itt));
			return útvonal;
		}

		public static List<Vektor> Cakk(Vektor innen, Vektor ide)
		{
			Vektor vx = new Vektor(ide.X - innen.X, 0);
			Vektor vy = new Vektor(0, ide.Y - innen.Y);
			List<Vektor> útvonal = new List<Vektor>();
			Vektor itt = new Vektor(innen);
			while (itt.X != ide.X)
			{
				útvonal.Add(new Vektor(itt));
				itt += vx;
			} 
			while (itt.Y != ide.Y)
			{
				útvonal.Add(new Vektor(itt));
				itt += vy;
			}
			return útvonal;
		}

		#endregion
	}
}
