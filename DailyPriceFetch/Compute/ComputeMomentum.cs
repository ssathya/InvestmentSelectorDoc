using Models.AppModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DailyPriceFetch.Compute
{
	public static class ComputeMomentum
	{

		#region Public Methods

		public static double CalculateRsi(IEnumerable<double> closePrices)
		{
			double Tolerance = 10e-20;
			var prices = closePrices as double[] ?? closePrices.ToArray();

			double sumGain = 0;
			double sumLoss = 0;
			for (int i = 1; i < prices.Length; i++)
			{
				var difference = prices[i] - prices[i - 1];
				if (difference >= 0)
				{
					sumGain += difference;
				}
				else
				{
					sumLoss -= difference;
				}
			}

			if (sumGain == 0) return 0;
			if (Math.Abs(sumLoss) < Tolerance) return 100;

			var relativeStrength = sumGain / sumLoss;

			return 100.0 - (100.0 / (1 + relativeStrength));
		}

		public static double ComputeMomentumValue(HistoricPrice historicPrice)
		{
			var logPrices = historicPrice.Candles.Select(r => Math.Log((double)r.Close));
			var count = logPrices.Count();
			var xAxis = new List<double>();
			for (int i = 1; i <= count; i++)
			{
				xAxis.Add(i);
			}
			LinearRegression(xAxis, logPrices, out double rSquared, out double yIntercept, out double slope);
			//Annualized percent
			var annualizedSlope = (Math.Pow(Math.Exp(slope), logPrices.Count()) - 1) * 100;
			//Adjust for fitness
			var score = annualizedSlope * rSquared;
			return score;
		}

		#endregion Public Methods


		#region Private Methods

		private static void LinearRegression(
			IEnumerable<double> inputXVals,
			IEnumerable<double> inputYVals,
			out double rSquared,
			out double yIntercept,
			out double slope)
		{
			var xVals = inputXVals.ToArray();
			var yVals = inputYVals.ToArray();

			if (xVals.Count() != yVals.Count())
			{
				throw new Exception("Input values should be with the same length.");
			}

			double sumOfX = 0;
			double sumOfY = 0;
			double sumOfXSq = 0;
			double sumOfYSq = 0;
			double sumCodeviates = 0;

			for (var i = 0; i < xVals.Count(); i++)
			{
				var x = xVals[i];
				var y = yVals[i];
				sumCodeviates += x * y;
				sumOfX += x;
				sumOfY += y;
				sumOfXSq += x * x;
				sumOfYSq += y * y;
			}

			var count = xVals.Length;
			var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
			var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

			var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
			var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
			var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

			var meanX = sumOfX / count;
			var meanY = sumOfY / count;
			var dblR = rNumerator / Math.Sqrt(rDenom);

			rSquared = dblR * dblR;
			yIntercept = meanY - ((sCo / ssX) * meanX);
			slope = sCo / ssX;
		}

		#endregion Private Methods
	}
}