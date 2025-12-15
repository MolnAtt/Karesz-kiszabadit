using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karesz
{
	partial class Form1
	{
		class Lövedék : Test
		{
			#region statikus tulajdonságok

			#endregion
			#region tulajdonságok
			public override bool Ez_egy_lövedék => true;
			Bitmap[] képkészlet;
			#endregion
			#region konstruktorok
			public Lövedék(string név, Bitmap[] képkészlet, Vektor h, Vektor v) : base(név, h, v)
			{
				this.képkészlet = képkészlet;
			}
			public Lövedék(string név, Vektor h, Vektor v) : base(név, h, v)
			{
				this.képkészlet = new Bitmap[4]
				{
					Properties.Resources.golyesz_up,
					Properties.Resources.golyesz_right,
					Properties.Resources.golyesz_down,
					Properties.Resources.golyesz_left
				};
			}
			#endregion

			protected override void Eltavolitasa()
			{
				this.Sírkő_letétele();
				Test.lista.Remove(this);
			}
			protected override void Sírkő_letétele()
			{
				pálya.LegyenItt(H, hó);
			}
			/// <summary>
			/// Visszaadja a sebességvektor számkódját, ami a képek kezeléséhez kell.
			/// </summary>
			/// <returns></returns>
			public override Bitmap Iránykép() => képkészlet[v.ToInt()];

			public override void Lépj()
			{
				helyigény = h + v;
			}

		}
	}
}
