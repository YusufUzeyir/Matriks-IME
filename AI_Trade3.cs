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

namespace Matriks.Lean.Algotrader
{
	public class a3 : MatriksAlgo
	{
		// Strateji calistirilirken kullanacagimiz parametreler. Eger sembolle ilgili bir parametre ise,
		// "SymbolParameter" ile, degilse "Parameter" ile tanimlama yapariz. Parantez icindeki degerler default degerleridir.

		[SymbolParameter("FXU030N1")]
		public string Symbol;

		[Parameter(SymbolPeriod.Min5)]
		public SymbolPeriod SymbolPeriod;

		[Parameter(1, "Emir Adedi")]
		public decimal Quantity;

		[Parameter(SyntheticOrderPriceType.Percent, "Kar Al Emir Tipi")]
		public SyntheticOrderPriceType TakeProfitPriceType;
		[Parameter(0.3, "Kar Al Stop Seviyesi")]
		public decimal TakeProfitStopLevel;

		[Parameter(SyntheticOrderPriceType.Percent, "Zarar Durdur Emir Tipi")]
		public SyntheticOrderPriceType StopLossPriceType;
		[Parameter(0.3, "Zarar Durdur Stop Seviyesi")]
		public decimal StopLossStopLevel;

		AlgoAiPredictionModel algoAiPredictionModel;
		string AiModelName = "EndeksVadeli_5dk_YukselisModel";


		MOV indexer;

		public override void OnInit()
		{
			AddSymbol(Symbol, SymbolPeriod);
			SetAiPredictionSymbol(Symbol, SymbolPeriod);


			// Bu indikator, backtest esnasinda sembolun son indeksini elde etmek amaciyla kullanilmaktadir.
			// Ayni sembol ve periyod icin eklenilen baska bir indikator de kullanilabilir.
			indexer = MOVIndicator(Symbol, SymbolPeriod, OHLCType.Close);

			SendOrderSequential(true, Side.Buy);
			WorkWithPermanentSignal(true);
		}

		
		public override void OnInitCompleted()
		{
			algoAiPredictionModel = CreateAiPredictionModel(AiModelName, true, Symbol, SymbolPeriod);
			if (algoAiPredictionModel == null) return;

			algoAiPredictionModel.CreatePipeLineRunner();
		}

		
		public override void OnTimer()
		{

		}

		
		/// <param name="newsId">Gelen haberin id'si</param>
		/// <param name="relatedSymbols">Gelen haberin iliskili sembolleri</param>
		public override void OnNewsReceived(int newsId, List<string> relatedSymbols)
		{

		}

		
		/// <param name="barData">Bardata ve hesaplanan gerceklesen isleme ait detaylar</param>
		public override void OnDataUpdate(BarDataEventArgs barData)
		{
			if (barData.SymbolId != GetSymbolId(Symbol)) return;

			var predictedLabel = GetAiPrediction(algoAiPredictionModel) ?? false;

			if (predictedLabel && !LastOrderSide.Obj.Equals(Side.Buy))
			{
				SendMarketOrder(Symbol, Quantity, OrderSide.Buy);
				TakeProfit(Symbol, TakeProfitPriceType, TakeProfitStopLevel);
				StopLoss(Symbol, StopLossPriceType, StopLossStopLevel);
				Debug("Alis emri gonderildi..");
			}
		}

		
		/// <param name="barData">Emrin son durumu</param>
		public override void OnOrderUpdate(IOrder order)
		{
		}

		public bool? GetAiPrediction(AlgoAiPredictionModel algoAiPredictionModel)
		{
			if (algoAiPredictionModel == null) return false;
			var index = indexer.CurrentIndex;
			// Algo kalici sinyalle calismadigi durumda, canli bar icin prediction yapilmadigindan index 1 azaltilir
			if (LiveMode && !GetWorkWithPermanentSignal())
			{
				index = indexer.CurrentIndex - 1;
			}

			// Hesaplama icin yeterli bar yoksa prediction null donecektir
			var prediction = algoAiPredictionModel.GetPrediction(index);
			if (prediction != null)
			{
				Debug(algoAiPredictionModel.ModelName + " PredictedLabel: " + prediction.PredictedLabel.ToString() + ", " +
					  "Time: " + prediction.Times.ToString());
			}

			// Dogru tahmin oranini doner
			var predictionAccuracy = Math.Round(algoAiPredictionModel.GetPredictionAccuracy(), 2);
			if (predictionAccuracy != 0)
				Debug(algoAiPredictionModel.ModelName + " PredictionAccuracy: " + predictionAccuracy);
			var bardata = GetBarData(Symbol, SymbolPeriod);
			if (bardata != null)
			{
				var indexDateTime = bardata.Time[index];
				// Onceden yapilmis bir tahminin hedef bari kapanmissa dogrulugunu doner
				var isPredictionCorrect = algoAiPredictionModel.IsPredictionCorrect(index);
				if (isPredictionCorrect.Result.HasValue)
				{
					Debug(algoAiPredictionModel.ModelName + " IsPredictionCorrect: " +
							isPredictionCorrect.Result + ", PredictionTime: " +
							isPredictionCorrect.PredictionDateTime + ", TargetTime: " + indexDateTime);
				}
			}

			return prediction? .PredictedLabel;
		}

	
		public override void OnStopped()
		{

		}
	}
}
