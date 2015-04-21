﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimberWinR;
using TimberWinR.Parser;
using TimberWinR.Diagnostics;

namespace TestConsole
{
    class Program
    {
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static Manager _timberWinR;

        public static Diagnostics Diagnostics { get; set; }

        static void Main(string[] args)
        {
            _timberWinR = new TimberWinR.Manager("default.json", "Debug", "D:\\logs", true, _cancellationTokenSource.Token, false);
            //_timberWinR.OnConfigurationProcessed += TimberWinROnOnConfigurationProcessed;
            _timberWinR.Start(_cancellationTokenSource.Token);
            Diagnostics = new Diagnostics(_timberWinR, _cancellationTokenSource.Token, 5141);
            Console.ReadKey();
        }
    }
}
