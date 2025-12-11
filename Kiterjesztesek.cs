using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karesz
{
	public static class Kiterjesztesek
	{
		private static Random r = new Random();

		/// <summary>
		/// Fisher-Yates-Knuth keverés
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">tároló</param>
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = r.Next(n + 1);
				(list[n], list[k]) = (list[k], list[n]);
			}
		}

	}
}
