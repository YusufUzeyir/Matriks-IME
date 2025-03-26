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

//===================================================================ACIKLAMA=======================================================================//
// Carpanli bir ATR hesaplamasiyla beraber onceki degerlerini takip eder. Eger kapanis VE onceki kapanis, onceki trail degerinden buyukse           //
// Trail = Maximum(OncekiTrail, (Kapanis - finalATR)) olarak hesaplanir. Eger kapanis VE onceki kapanis, onceki trail degerinden kucukse            //
// Trail = Minimum(OncekiTrail, (Kapanis + finalATR)) olarak hesaplanir. Daha sonra bu hesaplanan trail degerleri bir indikatore beslenerek         //
// (cross fonksiyonu icerisinde kullanilabilmesi icin) kapanis ile cross etmesi beklenir. Eger kapanis trail'i yukari kirarsa alis, asagi kirarsa   //
// satis emri gonderilir. NOT: Cross fonksiyonu onceki degerlerle kiyaslama yaptigi icin 2 tane sabit degeri kabul edemez. Sabit degerlerden birini //
// bir indikatore atarak (bu durumda onceki degerleri de barindiran bir array gibi calismaktadir) digerini de bardata ile alarak cross fonksiyonu   //
// icerisinde kullanmamiza olanak saglanmistir. Bu bakımdan önemli bir örnek stratejidir.                                                           //

namespace Matriks.Lean.Algotrader
{
	public class ATRPrevTrail_Kopya : MatriksAlgo
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.

		[SymbolParameter("GARAN")]
		public string Symbol;

		[Parameter(SymbolPeriod.Min5)]
		public SymbolPeriod SymbolPeriod;

		[Parameter(100)]
		public int Quantity;

		[Parameter(5)]
		public int AtrPeriyod;

		[Parameter(2)]
		public int AtrFactor;

		ATR atr;
		MOV trailAsIndicator;
		bool firstrun = true;
		decimal Trail = 0;

		public override void OnInit()
		{
			atr = ATRIndicator(Symbol, SymbolPeriod, OHLCType.Close, 14);
			trailAsIndicator = new MOV(1, MovMethod.Simple);

			AddSymbol(Symbol, SymbolPeriod);

			//Eger backtestte emri bir al bir sat seklinde gonderilmesi isteniyor bu true set edilir. 
			//Alttaki satırı silerek veya false geçerek emirlerin sirayla gönderilmesini engelleyebilriz. 
			SendOrderSequential(true);
			WorkWithPermanentSignal(true);
		}

		/// <summary>
		/// Init islemleri tamamlaninca, bardatalar kullanmaya hazir hale gelince bu fonksiyon tetiklenir. Data uzerinde bir defa yapilacak islemler icin kullanilir
		/// </summary>
		public override void OnInitCompleted()
		{

		}

		/// <summary>
		/// Eklenen sembollerin bardata'ları ve indikatorler güncellendikçe bu fonksiyon tetiklenir. 
		/// </summary>
		/// <param name="barData">Bardata ve hesaplanan gerçekleşen işleme ait detaylar</param>
		public override void OnDataUpdate(BarDataEventArgs barData)
		{
			var barDataModel = GetBarData();
			var finalATR = AtrFactor * atr.CurrentValue;
			//var Prev = Ref(atr,1);
			var Close = barDataModel.Close[barData.BarDataIndex - 1]; //barData.BarDataIndex yazarsak yeni baslayan barin acilisini verir, -1 bar yeni acildiginda bir onceki barin close'u
			var PrevClose = barDataModel.Close[barData.BarDataIndex - 2];

			decimal PrevTrail = firstrun ? 0 : Trail; //ilk calismada PrevTrail degeri olmayacagi icin 0 atiyoruz. ilk calisma degilse trail henuz guncellenmedigi icin bir onceki calismadaki degerini aliyor
			firstrun = false;

			string header, line1, line2, line3, header_ohlc, line1_ohlc, line2_ohlc;
			header = String.Format("{0,-15} {1,-25} {2,-25} {3,-25} {4,-25}", " ", "Close", "PrevTrail", "PrevClose", "Trail");
			line1 = String.Format("{0,-15} {1,-25} {2,-25} {3,-25} {4,-25}", "LAST:", Close, Math.Round(PrevTrail, 2), PrevClose, Math.Round(Trail, 2));
			Debug(header);
			Debug(line1);

			if (Close > PrevTrail && PrevClose > PrevTrail)
			{
				Trail = Maximum(PrevTrail, (Close - finalATR));
				Debug($"Close({Close}) is > than PrevTrail({Math.Round(PrevTrail, 2)}) & PrevClose({PrevClose}) is > than PrevTrail({Math.Round(PrevTrail, 2)})");
				Debug("New Trail = " + Math.Round(Trail, 2));
			}
			else if (Close < PrevTrail && PrevClose < PrevTrail)
			{
				Trail = Minimum(PrevTrail, (Close + finalATR));
				Debug($"Close({Close}) is > than PrevTrail({Math.Round(PrevTrail, 2)}) & PrevClose({PrevClose}) is > than PrevTrail({Math.Round(PrevTrail, 2)})");
				Debug("New Trail = " + Math.Round(Trail, 2));
			}

			trailAsIndicator.Update(Trail, barData.BarDataIndex, barData.BarData.Dtime); //Trail burada guncellenmis durumda ve trailAsIndicator indikator objesine besliyoruz
			//Bu indikator aslinda 1 periyotluk bir moving average oldugundan Trail degerlerini tutan bir array gibi calismaktadir

			if (CrossAbove(barDataModel, trailAsIndicator, OHLCType.Close))
			{
				SendMarketOrder(Symbol, Quantity, OrderSide.Buy);
				Debug(Quantity + " adet alış emri iletildi");
			}
			else if (CrossBelow(barDataModel, trailAsIndicator, OHLCType.Close))
			{
				SendMarketOrder(Symbol, Quantity, OrderSide.Sell);
				Debug(Quantity + " adet satış emri iletildi");
			}
		}

		/// <summary>
		/// Gönderilen emirlerin son durumu değiştikçe bu fonksiyon tetiklenir.
		/// </summary>
		/// <param name="order">Emrin son durumu</param>
		public override void OnOrderUpdate(IOrder order)
		{
			if (order.OrdStatus.Obj == OrdStatus.Filled)
			{

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
