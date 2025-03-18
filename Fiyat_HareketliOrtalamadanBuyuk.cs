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
	public class Fiyat_HareketliOrtalamadanBuyuk : Explorer
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.



		[Parameter(22)]
			public int MovPeriod1;

		[Parameter(MovMethod.S)]
			public MovMethod MovMovMethod1;

		MOV mov;



		public override void OnInit()
		{
			mov = MOVIndicator(Symbol, SymbolPeriod, OHLCType.Close, MovPeriod1, MovMovMethod1);


			AddColumns(2);
			SetColumnText(0, "Fiyat");
			SetColumnText(1, "Hareketl Ortalama");


		}


		/// <summary>
		/// Eklenen sembollerin bardata'ları ve indikatorler güncellendikçe bu fonksiyon tetiklenir. 
		/// </summary>
		/// <param name="barData">Bardata ve hesaplanan gerçekleşen işleme ait detaylar</param>
		public override bool OnExplorer(List<BarDataEventArgs> bardatas)
		{
			var barData1 = GetBarData(Symbol, SymbolPeriod);
			var ohlcData1 = GetSelectedValueFromBarData(barData1, OHLCType.Close);

			var bardata = bardatas.FirstOrDefault() ? .BarData;
			SetColumn(0, bardata.Close);
			SetColumn(1, mov.Value[0][mov.CurrentIndex]);

			if (mov.Value[0][mov.CurrentIndex] < ohlcData1)
			{
				return true;
			}
			return false;


		}
	}
}
