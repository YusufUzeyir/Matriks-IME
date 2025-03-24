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
    public class MOSTRSIStratejisi : MatriksAlgo
    //strateji ismini burada deklare ediyoruz. Dosyada ki isimle stratejide yazılan
    // isim tamamen aynı olmalıdır. (küçük büyük harf duyarlı)
    {
        // Strateji çalıştırılırken kullanacağımız parametreler. Eğer sembolle ilgili
        // bir parametre ise, "SymbolParameter" ile, değilse "Parameter" ile tanımlama
        //yaparız. Parantez içindeki değerler default değerleridir.
        [SymbolParameter("GARAN")]
        public string Symbol;//Sembol ismi

        [Parameter(SymbolPeriod.Day)]
        public SymbolPeriod SymbolPeriod;
        //Stratejiyi çalıştırmak istediğimiz bar periyodu

        [Parameter(100)]
        public int BuyOrderCount;
        //alım miktarı için kullanacağımız parametre

        [Parameter(100)]
        public int SellOrderCount;
        //satım miktarı için kullanacağımız parametre

        [Parameter(14)]
        public int periodRsi;
        //RSI periyodu için kullanacağımız parametre

        [Parameter(3)]
        public int periodMost;
        //MOST periyodu için kullanacağımız parametre

        [Parameter(2)]
        public decimal percentage;
        //MOST yüzde paremetresi için kullanacağımız parametre
        
        //Kullanacağımız indikatör obje tanımları
        RSI rsi;
        MOST most;

        // Strateji ilk çalıştırıldığında bu fonksiyon tetiklenir. Tüm sembole kayit
        //işlemleri,indikator ekleme, haberlere kayıt olma işlemleri burada yapılır.
        public override void OnInit()
        {
            //tanımladığımız objelere indikatör tanımlarını ve gerekli değerleri
            //atıyoruz
            rsi = RSIIndicator(Symbol, SymbolPeriod, OHLCType.Close, periodRsi);
            most = MOSTIndicator(rsi, periodMost, percentage, MovMethod.Exponential);

            //Sembol ve periyoduna kayıt
            AddSymbol(Symbol, SymbolPeriod);

            // Algoritmanın kalıcı veya geçici sinyal ile çalışıp çalışmayacağını
            //belirliyoruz. true değer, algoritmanın sadece yeni bar açılışlarında
            //çalışmasını sağlar, bu fonksiyonu çağırmazsak veya false olarak
            //belirlersek her işlem olduğunda algoritma
            //tetiklenecektir.
            WorkWithPermanentSignal(true);

            //Eger backtestte emri bir al bir sat seklinde gonderilmesi isteniyor bu
            //true set edilir.
            //Alttaki satırı silerek veya false geçerek emirlerin sirayla
            //gönderilmesini engelleyebilirsiniz.
            SendOrderSequential(true);
        }

        // Eklenen sembollerin bardata'ları ve indikatorler güncellendikçe bu
        //fonksiyon tetiklenir.
        //Dolayısıyla asıl al/sat stratejisini yazacağımız bölümdür
        public override void OnDataUpdate(BarDataEventArgs barData)
        {
            //Bu koşul alım emri içindir. Eğer grafikte MOST'un EXMOV bandı
            //most bandını yukarı kırarsa al emri gönderilecek.
            if (CrossAbove(most.CurrentValue, most.ExMOV))
            {
                //Parametrelerde belirlenen sembolden, belirlenen miktarda, piyasa
                //fiyatından alış emri gönderir
                SendMarketOrder(Symbol, BuyOrderCount, (OrderSide.Buy));

                //"" içerisinde bulunan ifadeyi debug penceresine basar
                Debug("Alış Emri Gönderildi");

                //EXMOV değerini debug penceresine basar
                Debug("exmov:" + Math.Round(most.ExMOV.CurrentValue, 2));

                //MOST değerini debug penceresine basar
                Debug("most:" + Math.Round(most.CurrentValue, 2));
            }

            //Bu koşul satım emri içindir. Eğer grafikte MOST'un EXMOV bandı
            //most bandını aşağı kırarsa sat emri gönderilecek.
            if (CrossBelow(most.CurrentValue, most.ExMOV))
            {
                //Parametrelerde belirlenen sembolden, belirlenen miktarda, piyasa
                //fiyatından satış emri gönderir
                SendMarketOrder(Symbol, SellOrderCount, (OrderSide.Sell));

                //"" içerisinde bulunan ifadeyi debug penceresine basar
                Debug("Satış Emri Gönderildi");

                //EXMOV değerini debug penceresine basar
                Debug("exmov:" + Math.Round(most.ExMOV.CurrentValue, 2));

                //MOST değerini debug penceresine basar
                Debug("most:" + Math.Round(most.CurrentValue, 2));
            }
        }
    }
}