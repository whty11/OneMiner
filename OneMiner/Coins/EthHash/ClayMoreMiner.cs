﻿using OneMiner.Core;
using OneMiner.Core.Interfaces;
using OneMiner.Model;
using OneMiner.Model.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace OneMiner.Coins.EthHash
{
    /// <summary>
    /// this class does not represent a miner program. coz this contains specif info like batfilepath etc
    /// this represents a miner program inside a configured miner. there could be many miners of the same type. eg ethereum, ethereum_sia
    /// for the real representation fo a miner program, look at JsonData.MinerProgram
    /// </summary>
    class ClaymoreMiner:IMinerProgram
    {

        private const string MINERURL = "https://github.com/nanopool/Claymore-Dual-Miner/releases/download/v10.0/Claymore.s.Dual.Ethereum.Decred_Siacoin_Lbry_Pascal.AMD.NVIDIA.GPU.Miner.v10.0.zip";
        private const string EXENAME = "EthDcrMiner64.exe";
        public  string Script { get; set; }
        public IOutputReader Reader { get; set; }

        public string MinerFolder { get; set; }
        public string MinerEXE { get; set; }
        public string BATFILE  { get; set; }
        public bool AutomaticScriptGeneration { get; set; }


        public MinerProgramState MinerState { get; set; }

        public IMiner Miner { get; set; }
        public ICoin MainCoin { get; set; }
        public ICoin DualCoin { get; set; }

        public ICoinConfigurer MainCoinConfigurer { get; set; }
        public ICoinConfigurer DualCoinConfigurer { get; set; }
        public bool DualMining { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }//claymore ccminer etc


        public ClaymoreMiner(ICoin mainCoin, bool dualMining, ICoin dualCoin, string minerName,IMiner miner)
        {

            MinerState = MinerProgramState.Stopped;

            MainCoin = mainCoin;
            MainCoinConfigurer = mainCoin.SettingsScreen;
            DualCoin = dualCoin;
            if (DualCoin!=null)
                DualCoinConfigurer = DualCoin.SettingsScreen;
            DualMining = dualMining;
            Name = minerName;
            Miner = miner;
            AutomaticScriptGeneration = true;
            Type = "Claymore";

        }



        public bool MiningScriptsPresent()
        {
            if ( BATFILE == null || BATFILE == "")
                return false;
            FileInfo script = new FileInfo(BATFILE);
            if (script.Exists)
                return true;
            return ProgramPresent();
        }
        public bool ProgramPresent()
        {
            if (MinerEXE == null || MinerEXE == "")
                return false;
            FileInfo miner = new FileInfo(MinerEXE);
            if (miner.Exists)
                return true;
            return false;
        }
        public void SaveProgramToDB()
        {
            if (ProgramPresent())
            {
                Config model = Factory.Instance.Model;
                model.AddMinerProgram(this);

            }
        }
        public void SaveScriptToDB()
        {
            if (MiningScriptsPresent())
            {
                Config model = Factory.Instance.Model;
                model.AddMinerScript(this, Miner);

            }
        }
        public  void DownloadProgram()
        {
            if (!ProgramPresent())
            {
                MinerState = MinerProgramState.Downloading;
                MinerDownloader downloader = new MinerDownloader(MINERURL, EXENAME);

                MinerFolder = downloader.DownloadFile();
                MinerEXE = MinerFolder + @"\" + EXENAME;
                SaveProgramToDB();

            }
            BATFILE = MinerFolder + @"\" + Name + ".bat";
            ConfigureMiner();
            SaveScriptToDB();
            MinerState = MinerProgramState.Stopped;

        }

        public void  StartMining()
        {
            FileInfo file = new FileInfo(BATFILE);
            try
            {
                MinerState = MinerProgramState.Running;
                ProcessStartInfo info = new ProcessStartInfo();
                info.UseShellExecute = false;
                info.FileName = BATFILE;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.WorkingDirectory = file.DirectoryName + "\\";

                Process objProcess = new Process();
                objProcess.StartInfo = info;
                objProcess.Start();
                objProcess.WaitForExit();
            }
            catch (Exception e)
            {
            }
            finally
            {
                MinerState = MinerProgramState.Stopped;

            }

        }
        public void KillMiner()
        {
            //Todo:
        }
        public  string GenerateScript()
        {
            try
            {
                //generate script and write to folder
                string command = EXENAME + " -epool " + MainCoinConfigurer.Pool;
                command += " -ewal " + MainCoinConfigurer.Wallet;
                command += " -epsw x ";
                if(DualCoin!=null)
                {
                    command += " -dpool " + DualCoinConfigurer.Pool;
                    command += " -dwal " + MainCoinConfigurer.Wallet;
                    command += " -ftime 10 ";

                }

                Script = SCRIPT1+command;
                return Script;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public void ModifyScript(string script)
        {
            Script = script;
            SaveToBAtFile();
            AutomaticScriptGeneration = false;

        }
        private void ConfigureMiner()
        {
            try
            {
                GenerateScript();
                SaveToBAtFile();

            }
            catch (Exception e)
            {
            }
        }
        private void SaveToBAtFile()
        {
            try
            {
                FileStream stream = File.Open(BATFILE, FileMode.Create);
                StreamWriter sw = new StreamWriter(stream);
                sw.Write(Script);
                sw.Flush();
                sw.Close();
                //generate script and write to folder

            }
            catch (Exception e)
            {
            }
        }



        private const string SCRIPT1 = 
@"setx GPU_FORCE_64BIT_PTR 0
setx GPU_MAX_HEAP_SIZE 100
setx GPU_USE_SYNC_OBJECTS 1
setx GPU_MAX_ALLOC_PERCENT 100
setx GPU_SINGLE_ALLOC_PERCENT 100
";

    }
}
