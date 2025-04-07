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
	public class a2 : MatriksAlgo
	{

		[SymbolParameter("FSASA")]
		public string Symbol;

		[Parameter(SymbolPeriod.Min5)]
		public SymbolPeriod SymbolPeriod;

		[Parameter(1, "Emir Adedi")]
		public decimal Quantity;

		[Parameter(true, "Hisse senetleri icin aciga satis emri gonderilmesi istenmiyorsa bu parametrenin secimi kaldirilmalidir.\nDiger piyasalarda bu parametrenin bir etkisi yoktur.")]
		public bool ShortSaleForBist;

		[Parameter(SyntheticOrderPriceType.Percent, "Kar Al Emir Tipi")]
		public SyntheticOrderPriceType TakeProfitPriceType;
		[Parameter(0.3, "Kar Al Stop Seviyesi")]
		public decimal TakeProfitStopLevel;

		[Parameter(SyntheticOrderPriceType.Percent, "Zarar Durdur Emir Tipi")]
		public SyntheticOrderPriceType StopLossPriceType;
		[Parameter(0.3, "Zarar Durdur Stop Seviyesi")]
		public decimal StopLossStopLevel;

		AlgoAiPredictionModel algoAiPredictionModel;
		string AiModelName = "PayVadeli_5dk_DususModeli";


		MOV indexer;

		public override void OnInit()
		{
			AddSymbol(Symbol, SymbolPeriod);
			SetAiPredictionSymbol(Symbol, SymbolPeriod);


			indexer = MOVIndicator(Symbol, SymbolPeriod, OHLCType.Close);

			SendOrderSequentialForShort(false);
			SendOrderSequential(true, Side.Sell);
			WorkWithPermanentSignal(true);
		}

	
		public override void OnInitCompleted()
		{
			algoAiPredictionModel = CreateAiPredictionModel(AiModelName, true, Symbol, SymbolPeriod);
			if (algoAiPredictionModel == null) return;

			algoAiPredictionModel.CreatePipeLineRunner();

			//Aciga satis kontrolu
			ShortSaleForBist = ShortSaleForBist && (GetSymbolDetail(Symbol).ExchangeDetail.ExchangeID == 4);
		}

		
		/// <param name="barData">Bardata ve hesaplanan gerceklesen isleme ait detaylar</param>
		public override void OnDataUpdate(BarDataEventArgs barData)
		{
			if (barData.SymbolId != GetSymbolId(Symbol) || algoAiPredictionModel == null) return;

			var predictedLabel = GetAiPrediction(algoAiPredictionModel) ?? false;

			if (predictedLabel && !LastOrderSide.Obj.Equals(Side.Sell))
			{
				//Hisse senetleri icin aciga satis emri gonderimi
				if (ShortSaleForBist)
				{
					SendShortSaleMarketOrder(Symbol, Quantity);
					LastOrderSide.Obj = Side.Sell;
					Debug("Aciga satis emri gonderildi..");
				}
				else
				{
					SendMarketOrder(Symbol, Quantity, OrderSide.Sell);
					Debug("Satis emri gonderildi..");
				}
				TakeProfit(Symbol, TakeProfitPriceType, TakeProfitStopLevel);
				StopLoss(Symbol, StopLossPriceType, StopLossStopLevel);
			}
		}

		public override void OnSyntheticOrderTriggered(SyntheticAlgoOrder sOrder)
		{
			if (sOrder.IsTriggered)
			{
				Debug("Sentetik emir tetiklendi..");
				LastOrderSide.Obj = Side.All;
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
