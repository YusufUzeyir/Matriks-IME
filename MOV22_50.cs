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
	public class MOV22_50_EMA : Explorer
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.



		[Parameter(22)]
			public int MovPeriod1;

		[Parameter(MovMethod.S)]
			public MovMethod MovMovMethod1;

		[Parameter(50)]
			public int MovPeriod2;

		[Parameter(MovMethod.S)]
			public MovMethod MovMovMethod2;

		[Parameter(50)]
			public int EmaPeriod1;

		MOV mov;
		MOV mov2;
		EMA ema;


		public override void OnInit()
		{
			ema = EMAIndicator(Symbol, SymbolPeriod.Week, OHLCType.Close, EmaPeriod1);
			mov = MOVIndicator(Symbol, SymbolPeriod, OHLCType.Close, MovPeriod1, MovMovMethod1);
			mov2 = MOVIndicator(Symbol, SymbolPeriod, OHLCType.Close, MovPeriod2, MovMovMethod2);


			AddColumns(3);
			SetColumnText(0, "MOV22");
			SetColumnText(1, "MOV50");
			SetColumnText(2, "EMA50");


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

			SetColumn(0, mov.Value[0][mov.CurrentIndex]);
			SetColumn(1, mov2.Value[0][mov2.CurrentIndex]);
			SetColumn(2, ema.Value[0][ema.CurrentIndex]);

			if (CrossBelow(mov, mov2, 0, 0) && ema.Value[0][ema.CurrentIndex] < ohlcData1)
			{
				return true;
			}
			return false;


		}
	}
}
