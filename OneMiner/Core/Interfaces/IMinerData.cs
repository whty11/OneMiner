﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneMiner.Core.Interfaces
{
    public interface IMinerScript
    {
        string ProgramType { get; set; }//eg:mvidia or AMD
        string BATfile { get; set; }
        bool AutomaticScriptGeneration { get; set; }
    }
    public interface IMinerData
    {
        string Id { get; set; }
        string Name { get; set; }
        string Algorithm { get; set; }
        //string BATFileName { get; set; }
        string MainCoin { get; set; }
        string MainCoinPool { get; set; }
        string MainCoinWallet { get; set; }
        bool DualMining { get; set; }
        string DualCoin { get; set; }
        string DualCoinPool { get; set; }
        string DualCoinWallet { get; set; }
        List<IMinerScript> MinerScripts { get; set; }

    }
}
