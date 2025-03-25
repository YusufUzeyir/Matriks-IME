using System;
using System.Collections.Generic;
using System.Linq;
using Matriks.Data.Symbol;
using Matriks.Engines;
using Matriks.Indicators;
using Matriks.Symbols;
using Matriks.AlgoTrader;
using Matriks.Trader.Core;
using Matriks.Trader.Core.Fields;
using Matriks.Trader.Core.TraderModels;
using Matriks.Lean.Algotrader.AlgoBase;
using Matriks.Lean.Algotrader.Models;
using Matriks.Lean.Algotrader.Trading;
using Matriks.Data.Tick;
using Matriks.Enumeration;
using Matriks.IntermediaryInstitutionAnalysis.Enums;
using Newtonsoft.Json;

//===========================================ACIKLAMA===================================================//
// Acceleration Bands indikatörünün lower ve upper çizgilerine bağlıdır.								//
// Fiyat AccBands'in üst bandının üstüne kırarsa al üst bandının altına kırarsa sat sinyali üretilir.	//
// Eğer açığa satış yapılmak istenirse fiyat AccBands'in alt bandının altına kırarsa açığa sat,			//
// alt bandının üstüne kırarsa açığı kapat (buy to cover) sinyali üretilir.								//
// Emirler piyasa fiyatından gönderilecektir.															//
// Emir gönderimi ile birlikte strateji raporunda Debug sekmesine "Alış emri gönderildi."				//
// ve "Satış emri gönderildi." ifadesi yazdırılmaktadır. 												//

namespace Matriks.Lean.Algotrader
{
	public class ACCBandsIndicator : MatriksAlgo
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.

		[SymbolParameter("SEMBOL")]
		public string Symbol;

		[Parameter(SymbolPeriod.Min60)]
		public SymbolPeriod SymbolPeriod;

		[Parameter(1)]
		public decimal OrderQuantity;
		
		[Parameter(false)]
		public bool AcigaSatis;

		[Parameter(10)]
		public int accBandPeriod;

		[Parameter(0.0005)]
		public decimal accBandFactor;

		ACCBands accBands;
		 
		public override void OnInit()
		{
			accBands = ACCBandsIndicator(Symbol, SymbolPeriod, OHLCType.Close, accBandPeriod, accBandFactor);
			AddSymbol(Symbol, SymbolPeriod);
			WorkWithPermanentSignal(true);
			
			//Eger backtestte emri bir al bir sat seklinde gonderilmesi isteniyor bu true set edilir. 
			//Alttaki satırı silerek veya false geçerek emirlerin sirayla gönderilmesini engelleyebilirsiniz. 
			SendOrderSequential(true);
		}


		/// <summary>
		/// Eklenen sembollerin bardata'ları ve indikatorler güncellendikçe bu fonksiyon tetiklenir. 
		/// </summary>
		/// <param name="barData">Bardata ve hesaplanan gerçekleşen işleme ait detaylar</param>
		public override void OnDataUpdate(BarDataEventArgs barData)
		{
			var barDataModel = GetBarData();
			var close = barDataModel.Close[barData.BarDataIndex];

			//Aciga Satis olmayan versiyon
			if (CrossAbove(barDataModel, accBands.Upper, OHLCType.Close))
			{
				SendMarketOrder(Symbol, OrderQuantity, (OrderSide.Buy));
				Debug("Alış emri verildi.");
			}
			if (CrossBelow(barDataModel, accBands.Upper, OHLCType.Close))
			{
				SendMarketOrder(Symbol, OrderQuantity, (OrderSide.Sell));
				Debug("Satış emri verildi.");
			}

			//Aciga Satis
			if (CrossBelow(barDataModel, accBands.Lower, OHLCType.Close) && AcigaSatis==true)
			{
				SendShortSaleMarketOrder(Symbol, OrderQuantity);
				Debug("Satış emri verildi.");
			}
			if (CrossAbove(barDataModel, accBands.Lower, OHLCType.Close) && AcigaSatis==true)
			{
				SendMarketOrder(Symbol, OrderQuantity, (OrderSide.Buy));
				Debug("Alış emri verildi.");
			}
		}

		/// <summary>
		/// Strateji durdurulduğunda bu fonksiyon tetiklenir.
		/// </summary>
		public override void OnStopped()
		{
		}
	}
}