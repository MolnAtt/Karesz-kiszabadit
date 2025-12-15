using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Karesz
{
	partial class Form1
	{
		#region Robotokra visszavezetett parancsok

		void Lépj() =>
			Test.akit_kiválasztottak.Lépj();
		void Várj() =>
			Test.akit_kiválasztottak.Várj();
		/*
		void Fordulj_jobbra() => 
			Robot.akit_kiválasztottak.Fordulj(jobbra);
		void Fordulj_balra() => 
			Robot.akit_kiválasztottak.Fordulj(balra);
		*/
		void Fordulj(int irány) => 
			Test.akit_kiválasztottak.Fordulj(irány);
		int Köveinek_száma_ebből(int szín) => 
			Test.akit_kiválasztottak.Köveinek_száma_ebből(szín);
		void Vegyél_fel_egy_kavicsot() => 
			Test.akit_kiválasztottak.Vegyél_fel_egy_kavicsot();
		void Tegyél_le_egy_kavicsot(int szín = fekete) => 
			Test.akit_kiválasztottak.Tegyél_le_egy_kavicsot(szín);
		bool Van_e_itt_Kavics() => 
			Test.akit_kiválasztottak.Alatt_van_kavics();
		int Mi_van_alattam() => 
			Test.akit_kiválasztottak.Alatt_ez_van();
		bool Van_e_előttem_fal() =>
			Test.akit_kiválasztottak.Előtt_fal_van();
		bool Kilépek_e_a_pályáról() =>
			Test.akit_kiválasztottak.Ki_fog_lépni_a_pályáról();
		int Ultrahang() =>
			Test.akit_kiválasztottak.UltrahangSzenzor();
		(int,int,int) SzélesUltrahang() =>
			Test.akit_kiválasztottak.SzélesUltrahangSzenzor();
		int Hőmérséklet() =>
			Test.akit_kiválasztottak.Hőmérő();
		void Mondd(string s) =>
			Test.akit_kiválasztottak.Mondd(s);

		#endregion

		#region Pályára visszavezetett parancsok

		void Betölt(string path) => pálya.Betölt(path);

        #endregion
	}
}
