using System;
using System.Collections.Generic;
using System.Linq;
using Matriks.Data.Symbol;
using Matriks.Engines;
using Matriks.Indicators;
using Matriks.Symbols;
using Matriks.Trader.Core;
using Matriks.Trader.Core.Fields;
using Matriks.Lean.Algotrader.AlgoBase;
using Matriks.Lean.Algotrader.Models;
using Matriks.Lean.Algotrader.Trading;
using Matriks.AI;
using Matriks.AI.AiParameters;
using Matriks.AI.Data;
using Matriks.Trader.Core.TraderModels;

namespace Matriks.Lean.Algotrader
{
	public class MostFisherTransform : Explorer
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.



		[Parameter(3)]
			public int MostPeriod1;

		[Parameter(2)]
			public decimal MostPercentage1;

		[Parameter(MovMethod.E)]
			public MovMethod MostMovMethod1;

		[Parameter(10)]
			public int FtPeriod1;

		FT ft;

		MOST most;



		public override void OnInit()
		{
			most = MOSTIndicator(Symbol, SymbolPeriod, OHLCType.Close, MostPeriod1, MostPercentage1, MostMovMethod1);
			ft = FisherTransformationIndicator(Symbol, SymbolPeriod, OHLCType.Close, FtPeriod1);

			AddColumns(8);
			SetColumnText(0, "fiyat");
			SetColumnText(1, "exmov");
			SetColumnText(2, "most");
			SetColumnText(3, "ftfish");
			SetColumnText(4, "fttrigger");
			SetColumnText(5, "Most Sinyal");
			SetColumnText(6, "Ft Sinyal");
			SetColumnText(7, "Güçlü Sinyal");
		}


		/// <summary>
		/// Eklenen sembollerin bardata'ları ve indikatorler güncellendikçe bu fonksiyon tetiklenir. 
		/// </summary>
		/// <param name="barData">Bardata ve hesaplanan gerçekleşen işleme ait detaylar</param>
		public override bool OnExplorer(List<BarDataEventArgs> bardatas)
		{

			var bardata = bardatas.FirstOrDefault() ? .BarData;
			SetColumn(0, bardata.Close);
			SetColumn(1, most.Value[1][most.CurrentIndex]);
			SetColumn(2, most.Value[0][most.CurrentIndex]);
			SetColumn(3, ft.Value[0][ft.CurrentIndex]);
			SetColumn(4, ft.Value[1][ft.CurrentIndex]);

			if (CrossAbove(most, most, 1, 0) || CrossBelow(most, most, 1, 0) || CrossAbove(ft, ft, 0, 1) || CrossBelow(ft, ft, 0, 1))
			{

				//Most indikatörünün al-sat sinyalinin yazıdırlması
				if (CrossAbove(most, most, 1, 0))
				{
					SetColumn(5, "AL");
				}
				else if (CrossBelow(most, most, 1, 0))
				{
					SetColumn(5, "SAT");
				}
				//Ft indikatörünün al-sat sinyalinin yazıdrılması.
				if (CrossAbove(ft, ft, 0, 1))
				{
					SetColumn(6, "AL");
				}
				else if (CrossBelow(ft, ft, 0, 1))
				{
					SetColumn(6, "SAT");
				}
				// Güçlü sinyalin yazdırılması.
				if (CrossBelow(most, most, 1, 0) && CrossBelow(ft, ft, 0, 1))
				{
					SetColumn(7, "SAT");
				}
				else if (CrossAbove(ft, ft, 0, 1) && CrossAbove(most, most, 1, 0))
				{
					SetColumn(7, "AL");
				}

				return true;
			}
			return false;


		}
	}
}
