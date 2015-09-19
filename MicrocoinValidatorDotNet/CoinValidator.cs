using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace MicrocoinValidatorDotNet
{
    /// <summary>
    /// Gives access to a Microcoin QL Coin Validator.
    /// </summary>
    class CoinValidator
    {
        private SerialPort m_Serial;
        private string _serialMessage;
        private Thread _queryThread;

        public event EventHandler<CoinAcceptEventArgs> OnCoinValidate;

        public int QueryDelay = 10;

        public void Open(string ComPort)
        {
            m_Serial = new SerialPort(ComPort);
            m_Serial.Open();
            Enable();
        }


        void ParseSerialData(string s)
        {
            for(int i = 0; i < 6;i++)
            {
                int coinCount = int.Parse(s[i].ToString());

                for (int c = 0; c < coinCount; c++)
                {
                    if (OnCoinValidate != null)
                    {
                        OnCoinValidate(this, new CoinAcceptEventArgs() { Channel = (CoinChannel)(i+1) });
                    }
                }
            }
        }

        void Query()
        {
            m_Serial.Write
                ("?\r\n");
        }

        public void Open()
        {
            m_Serial = new SerialPort(SerialPort.GetPortNames()[0]);
            m_Serial.Open();
            Enable();
        }

        public void Close()
        {
            Disable();
            m_Serial.Close();
        }

        void Enable()
        {
            m_Serial.Write("EV\r\n");

            _queryThread = new Thread(new ThreadStart(QueryThreadWork));
            _queryThread.Start();
        }

        void QueryThreadWork()
        {
            while (m_Serial.IsOpen)
            {
                Thread.Sleep(QueryDelay);
                Query();
                while (m_Serial.BytesToRead > 0)
                {
                    char c = (char)m_Serial.ReadByte();

                    if (c == '\r')
                    {
                        ParseSerialData(_serialMessage);
                        _serialMessage = "";
                    }
                    else
                        _serialMessage += c;
                }
            }
        }

        void Disable()
        {
            m_Serial.Write("DV\r\n");
        }
    }

    public enum CoinChannel : int
    {
        Channel1 = 0,
        Channel2 = 1,
        Channel3 = 2,
        Channel4 = 3,
        Channel5 = 4,
        Channel6 = 5,
        Channel7 = 6,
        Channel8 = 7,
        Channel9 = 8,
        Channel10 = 9,
        Channel11 = 10,
        Channel12 = 11,
        ChannelCount
    }

    public enum CoinCurrency : int
    {
        AUD = 0,
        CurrencyCount
    }

    public class CoinCurrencyHelper
    {
        public static decimal GetCoinValueFromChannel(CoinCurrency currency, CoinChannel channel)
        {
            decimal[,] currencyTable = new decimal[(int)CoinCurrency.CurrencyCount,12];

            // AUD
            currencyTable[0, 1] = 0.10m; // 10 Cents
            currencyTable[0, 2] = 0.20m; // 20 Cents
            currencyTable[0, 3] = 0.50m; // 50 Cents
            currencyTable[0, 4] = 1m;    // 1 Dollar
            currencyTable[0, 5] = 2m;    // 2 Dollars

            return currencyTable[(int)currency, (int)channel - 1];
        }
    }

    class CoinAcceptEventArgs : EventArgs
    {
        public CoinChannel Channel
        {
            get; set;
        }

        public decimal GetCurrencyValue(CoinCurrency currency)
        {
            return CoinCurrencyHelper.GetCoinValueFromChannel(currency, Channel);
        }
    }
}
