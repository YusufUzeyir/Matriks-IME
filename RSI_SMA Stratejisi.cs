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
    public class SimpleRSI_SMA : MatriksAlgo
    {
        // Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili bir parametre ise,
        // "SymbolParameter" ile, değilse "Parameter" ile tanımlama yaparız. Parantez içindeki değerler default değerleridir.

        [SymbolParameter("GARAN")]
        public string Symbol;

        [Parameter(SymbolPeriod.Day)]
        public SymbolPeriod SymbolPeriod;

        [Parameter(100)]
        public decimal BuyOrderQuantity;

        [Parameter(100)]
        public decimal SellOrderQuantity;

        [Parameter(10)]
        public int MovPeriod;

        [Parameter(2)]
        public int RsiPeriod;

        RSI rsi;
        SMA sma10;
        SMA sma200;

        [Output]
        public decimal HareketliOrtalama_10;
        [Output]
        public decimal HareketliOrtalama_200;
        [Output]
        public decimal RSI;
         
        public override void OnInit()
        {
            sma10 = SMAIndicator(Symbol, SymbolPeriod, OHLCType.Close, MovPeriod);
            sma200 = SMAIndicator(Symbol, SymbolPeriod, OHLCType.Close, 200);
            rsi = RSIIndicator(Symbol, SymbolPeriod, OHLCType.Close, RsiPeriod);

            AddSymbol(Symbol, SymbolPeriod);

            // Algoritmanın kalıcı veya geçici sinyal ile çalışıp çalışmayacağını belirleyen fonksiyondur.
            // true geçerseniz algoritma sadece yeni bar açılışlarında çalışır, bu fonksiyonu çağırmazsanız veya false geçerseniz her işlem olduğunda algoritma tetiklenir.
            WorkWithPermanentSignal(true);

            //Eger emri bir al bir sat seklinde gonderilmesi isteniyor bu true set edilir. 
            //Alttaki satırı silerek veya false geçerek emirlerin sirayla gönderilmesini engelleyebilirsiniz. 
            SendOrderSequential(true);

            //Alttaki fonksiyon açıldıktan sonra parametre olarak verilen saniyede bir OnTimer fonksiyonu tetiklenir.
            //SetTimerInterval(3);
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
            var close = barData.BarData.Close;

            if (rsi.CurrentValue < 10 && barData.BarData.Close > sma200.CurrentValue)
            {
                SendMarketOrder(Symbol, BuyOrderQuantity, OrderSide.Buy);
                Debug("Close = " + barData.BarData.Close);
                Debug("200 SMA = " + sma200.CurrentValue);
                Debug("rsi = " + rsi.CurrentValue);
                Debug("Alış Emri Gönderildi");
            }

            if (barData.BarData.Close > sma10.CurrentValue)
            {
                SendMarketOrder(Symbol, SellOrderQuantity, OrderSide.Sell);
                Debug("Close = " + barData.BarData.Close);
                Debug("10 SMA = " + sma10.CurrentValue);
                Debug("rsi = " + rsi.CurrentValue);
                Debug("Satış Emri Gönderildi");
            }
            else
            {
                Debug("Beklemede");
                if (rsi.CurrentValue>10)
                {
                    Debug("Rsi ALIS kosulu gerceklesmedi");
                    Debug("RSI = " + rsi.CurrentValue + " > 10");
                }
                if (barData.BarData.Close < sma200.CurrentValue)
                {
                    Debug("SMA ALIS kosulu gerceklesmedi");
                    Debug("Close = " + barData.BarData.Close + " < " + "sma200 = " + sma200.CurrentValue);
                }
                if (barData.BarData.Close < sma10.CurrentValue) //sadece stok varsa olacak
                {
                    Debug("SMA SATIS kosulu gerceklesmedi");
                    Debug("Close = " + barData.BarData.Close + " < " + "10 SMA = " + sma10.CurrentValue);
                }
            }
            HareketliOrtalama_10 = Math.Round(sma10.CurrentValue, 2);
            HareketliOrtalama_200 = Math.Round(sma200.CurrentValue, 2);
            RSI = Math.Round(rsi.CurrentValue, 2);
        }

        /// <summary>
        /// Strateji durdurulduğunda bu fonksiyon tetiklenir.
        /// </summary>
        public override void OnStopped()
        {
        }
    }
}