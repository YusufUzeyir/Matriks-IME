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
	public class allMost : Explorer
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.



		[Parameter(3)]
			public int MostPeriod1;

		[Parameter(2)]
			public decimal MostPercentage1;

		[Parameter(MovMethod.E)]
			public MovMethod MostMovMethod1;

		MOST most;



		public override void OnInit()
		{
			most = MOSTIndicator(Symbol, SymbolPeriod, OHLCType.Close, MostPeriod1, MostPercentage1, MostMovMethod1);


			AddColumns(4);
			SetColumnText(0, "fiyat");
			SetColumnText(1, "exmov");
			SetColumnText(2, "most");
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
			SetColumn(1, most.Value[1][most.CurrentIndex]);
			SetColumn(2, most.Value[0][most.CurrentIndex]);

			if (CrossAbove(most, most, 1, 0) || CrossBelow(most, most, 1, 0))
			{
				if(CrossAbove(most, most, 1, 0))
				{
					SetColumn(3,"AL");
				}
				else if(CrossBelow(most, most, 1, 0))
				{
					SetColumn(3,"SAT");
				}
				return true;
			}
			return false;


		}
	}
}
