using DailyPriceFetch;
using System;

namespace RunLambdaService
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Test Start");
			var function = new Function();
			var result = function.FunctionHandler();
			Console.WriteLine("Test end");
			Console.WriteLine(result);
		}
	}
}
