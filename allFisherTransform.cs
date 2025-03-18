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
	public class allFisherTransform : Explorer
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.



		[Parameter(10)]
			public int FtPeriod1;

		FT ft;



		public override void OnInit()
		{
			ft = FisherTransformationIndicator(Symbol, SymbolPeriod, OHLCType.Close, FtPeriod1);


			AddColumns(4);
			SetColumnText(0, "fiyat");
			SetColumnText(1, "ftfish");
			SetColumnText(2, "fttrigger");
			SetColumnText(3, "Sinyal");

		}


		/// <summary>
		/// Eklenen sembollerin bardata'ları ve indikatorler güncellendikçe bu fonksiyon tetiklenir. 
		/// </summary>
		/// <param name="barData">Bardata ve hesaplanan gerçekleşen işleme ait detaylar</param>
		public override bool OnExplorer(List<BarDataEventArgs> bardatas)
		{

			var bardata = bardatas.FirstOrDefault() ? .BarData;
			SetColumn(0, bardata.Close);
			SetColumn(1, ft.Value[0][ft.CurrentIndex]);
			SetColumn(2, ft.Value[1][ft.CurrentIndex]);

			if (CrossAbove(ft, ft, 0, 1) || CrossBelow(ft, ft, 0, 1))
			{
				if (CrossAbove(ft, ft, 0, 1))
				{
					SetColumn(3, "AL");
				}
				else if (CrossBelow(ft, ft, 0, 1))
				{
					SetColumn(3, "SAT");
				}
				return true;
			}
			return false;


		}
	}
}
