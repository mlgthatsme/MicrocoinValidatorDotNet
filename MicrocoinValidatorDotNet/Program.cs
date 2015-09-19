using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace MicrocoinValidatorDotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            CoinValidator validator = new CoinValidator();
            validator.Open();

            validator.OnCoinValidate += Validator_OnCoinValidate;
            
            Console.Read();

            validator.Close();
        }

        private static void Validator_OnCoinValidate(object sender, CoinAcceptEventArgs e)
        {
            Console.WriteLine("Inserted a coin value of ${0} on channel {1}", e.GetCurrencyValue(CoinCurrency.AUD).ToString(), (int)e.Channel);
        }
    }
}
