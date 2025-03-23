using System;
using System.Collections.Generic;
using System.Linq;
using Matriks;
using Matriks.Data.Symbol;
using Matriks.Data.Tick;
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
using Matriks.Enumeration;
using Matriks.IntermediaryInstitutionAnalysis.Enums;
using Newtonsoft.Json;

namespace Matriks.Lean.Algotrader
{
	public class bolingerRSI : MatriksAlgo
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.


		[SymbolParameter("GARAN")]
			public string Symbol1;


		[Parameter(SymbolPeriod.Min60)]
			public SymbolPeriod SymbolPeriod1;


		[Parameter(Side.Buy)]
			public Side orderSide;



		[Parameter(14)]
			public int RsiPeriod1;

		[Parameter(20)]
			public int BollingerPeriod1;

		[Parameter(2)]
			public decimal BollingerStandardDeviation1;

		[Parameter(MovMethod.S)]
			public MovMethod BollingerMovMethod1;

		[Parameter(false)]
			public bool BollingerStdevKullanma1;

		[Parameter(1)]
			public decimal OrderQuantity;

		[Parameter(3)]
			public decimal karAl;

		[Parameter(1)]
			public decimal zararDurdur;

		RSI rsi;
		BOLLINGER bollinger;




		public override void OnInit()
		{

			rsi = RSIIndicator(Symbol1, SymbolPeriod1, OHLCType.Close, RsiPeriod1);
			bollinger = BollingerIndicator(rsi, BollingerPeriod1, BollingerStandardDeviation1, BollingerMovMethod1, BollingerStdevKullanma1);


			SendOrderSequential(true, orderSide);
			WorkWithPermanentSignal(true);

			//Alttaki fonksiyon açıldıktan sonra parametre olarak verilen saniyede bir OnTimer fonksiyonu tetiklenir.
			// SetTimerInterval(3600);

			//Alttaki fonksiyon ile tanımlanan sembol ile ilgili haber geldiğinde OnNewsReceived fonksiyonu tetiklenir.
			//AddNewsSymbol(Symbol);

			//Alttaki fonksiyon ile tanımlanan anahtar kelime ile ilgili haber geldiğinde OnNewsReceived fonksiyonu tetiklenir.
			//AddNewsKeyword("KAP");
		}

		/// <summary>
		/// Init islemleri tamamlaninca, bardatalar kullanmaya hazir hale gelince bu fonksiyon tetiklenir. Data uzerinde bir defa yapilacak islemler icin kullanilir
		/// </summary>
		public override void OnInitCompleted()
		{

		}

		/// <summary>
		/// SetTimerInterval fonksiyonu ile belirtilen sürede bir bu fonksiyon tetiklenir.
		/// </summary>
		public override void OnTimer()
		{

		}

		/// <summary>
		/// AddNewsSymbol ve AddNewsKeyword ile haberlere kayit olunmuşsa bu fonksiyon tetiklenir.
		/// </summary>
		/// <param name="newsId">Gelen haberin id'si</param>
		/// <param name="relatedSymbols">Gelen haberin ilişkili sembolleri</param>
		public override void OnNewsReceived(int newsId, List<string> relatedSymbols)
		{

		}

		/// <summary>
		/// Eklenen sembollerin bardata'ları ve indikatorler güncellendikçe bu fonksiyon tetiklenir. 
		/// </summary>
		/// <param name="barData">Bardata ve hesaplanan gerçekleşen işleme ait detaylar</param>
		public override void OnDataUpdate(BarDataEventArgs barData)
		{
			SendMarketOrder(Symbol1, OrderQuantity, OrderSide.Buy, includeAfterSession:false);

			if (CrossAbove(rsi, bollinger, 0, 2))
			{
				SendMarketOrder(Symbol1, OrderQuantity, OrderSide.Buy, includeAfterSession:false);
			}
			if (CrossBelow(rsi, bollinger, 0, 0))
			{
				SendMarketOrder(Symbol1, OrderQuantity, OrderSide.Sell, includeAfterSession:false);
			}

		}

		/// <summary>
		/// Gönderilen emirlerin son durumu değiştikçe bu fonksiyon tetiklenir.
		/// </summary>
		/// <param name="barData">Emrin son durumu</param>
		public override void OnOrderUpdate(IOrder order)
		{
			if (order.OrdStatus.Obj == OrdStatus.Filled)
			{
				if (order.Side.Obj == Side.Buy)// alış emri gerçekleşmesi
				{
					TakeProfit(Symbol1, SyntheticOrderPriceType.Percent, karAl, includeAfterSession:false);
					StopLoss(Symbol1, SyntheticOrderPriceType.Percent, zararDurdur, includeAfterSession:false);
					Debug("TakeProfit ve StopLoss Tanımlandı.");
				}

				if (order.Side.Obj == Side.Sell)// satış emri gerçekleşmesi 
				{

				}
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
