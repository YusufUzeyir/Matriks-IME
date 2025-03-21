using System;
using System.Collections.Generic;
using System.Linq;
using Matriks.Data.Symbol;
using Matriks.Data.Tick;
using Matriks.Engines;
using System.Windows.Media;
using Matriks.Indicators;
using Matriks.Symbols;
using Matriks.AlgoTrader;
using Matriks.Trader.Core;
using Matriks.Trader.Core.Fields;
using Matriks.Trader.Core.TraderModels;
using Matriks.Lean.Algotrader.AlgoBase;
using Matriks.Lean.Algotrader.Models;
using Matriks.Lean.Algotrader.Trading;
using Newtonsoft.Json;

namespace Matriks.Lean.Algotrader
{
	public class BrokerageFirmTracking_Kopya : MatriksAlgo
	{
		// Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.

		[SymbolParameter("GARAN")]
		public string Symbol;

		[Parameter(SymbolPeriod.Min)]
		public SymbolPeriod SymbolPeriod;

		[Parameter(365)]//Yatırım Finansman
		public int KurumID;

		[Parameter(BakilacakAlan.Adet)]
		public BakilacakAlan _BakilacakAlan;

		[Parameter(1000)]
		public int Kriter;

		[Parameter(Side.All)]
		public Side IlkEmirYon;

		[Parameter(100)]
		public decimal BuyQuantity;

		[Parameter(100)]
		public decimal SellQuantity;


		public enum BakilacakAlan
		{
			Adet, Hacim
		}
		/// <summary>
		/// Strateji ilk çalıştırıldığında bu fonksiyon tetiklenir. Tüm sembole kayit işlemleri,
		/// indikator ekleme, haberlere kayıt olma işlemleri burada yapılır. 
		/// </summary>
		public override void OnInit()
		{
			AddSymbol(Symbol, SymbolPeriod);

			//Sembole ait gerçekleşen işlemler stratejide kullanılmak isteniyorsa alltaki fonksiyon kullanılmalıdır
			//Bu fonksiyon ile sembolde gerçekleşen her işlemin detaylarını stratejiye çekilebilir 
			//Herhangi kısıtlama olmaksızın strateji çalıştırılmaya başlatılmasıyla sembolde her işlem olduğunda OnTickDataRecieved metodunu tetikler 
			AddSymbolTickData(Symbol);

			//Eger backtestte emri bir al bir sat seklinde gonderilmesi isteniyor bu true set edilir. 
			//Alttaki satırı silerek veya false geçerek emirlerin sirayla gönderilmesini engelleyebilirsiniz. 
			SendOrderSequential(true, IlkEmirYon);
			// Algoritmanın kalıcı veya geçici sinyal ile çalışıp çalışmayacağını belirleyen fonksiyondur.
			// true geçerseniz algoritma sadece yeni bar açılışlarında çalışır, bu fonksiyonu çağırmazsanız veya false geçerseniz her işlem olduğunda algoritma tetiklenir.
			WorkWithPermanentSignal(false);
		}

		/// <summary>
		/// Eklenen sembollerde işlem gerçekleştikçe bu fonksiyon tetiklenir.
		/// </summary>
		/// <param name="tickData">Gerçekleşen işleme dair bilgiler bulundurur</param>
		public override void OnTickDataReceived(AlgoTickData tickData)
		{
			if (tickData.Buyer.Id == KurumID && LastOrderSide.Obj != Side.Buy)
			{
				if ((_BakilacakAlan == BakilacakAlan.Adet && tickData.Transaction.TransactionSize >= Kriter) ||
					(_BakilacakAlan == BakilacakAlan.Hacim && tickData.Transaction.TransactionVolume >= Kriter))
				{
					SendMarketOrder(Symbol, BuyQuantity, OrderSide.Buy);
					Debug("Alış Koşulu Gerçekleşti.");
					Debug($"{tickData.Transaction.TransactionDate.ToLongTimeString()} | {tickData.Transaction.TransactionPrice} | {tickData.Transaction.TransactionSize} | " +
					$"{tickData.Transaction.TransactionVolume} | {tickData.Buyer.Name} | {tickData.Seller.Name}");
				}
			}
			if (tickData.Seller.Id == KurumID && LastOrderSide.Obj != Side.Sell)
			{
				if ((_BakilacakAlan == BakilacakAlan.Adet && tickData.Transaction.TransactionSize >= Kriter) ||
					(_BakilacakAlan == BakilacakAlan.Hacim && tickData.Transaction.TransactionVolume >= Kriter))
				{
					SendMarketOrder(Symbol, SellQuantity, OrderSide.Sell);
					Debug("Satış Koşulu Gerçekleşti.");
					Debug($"{tickData.Transaction.TransactionDate.ToLongTimeString()} | {tickData.Transaction.TransactionPrice} | {tickData.Transaction.TransactionSize} | " +
					$"{tickData.Transaction.TransactionVolume} | {tickData.Buyer.Name} | {tickData.Seller.Name}");
				}

			}
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
		public override void OnDataUpdate(BarDataCurrentValues barDataCurrentValues)
		{

		}

		/// <summary>
		/// Gönderilen emirlerin son durumu değiştikçe bu fonksiyon tetiklenir.
		/// </summary>
		/// <param name="barData">Emrin son durumu</param>
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
