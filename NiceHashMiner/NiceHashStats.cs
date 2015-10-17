﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace NiceHashMiner
{
    class NiceHashStats
    {
#pragma warning disable 649
        class nicehash_global_stats
        {
            public double profitability_above_ltc;
            public double price;
            public double profitability_ltc;
            public int algo;
            public double speed;
        }

        public class nicehash_stats
        {
            public double balance;
            public double accepted_speed;
            public double rejected_speed;
            public int algo;
        }

        public class nicehash_sma_stats
        {
            public int port;
            public string name;
            public int algo;
            public double paying;
        }

        public class nicehash_result_2
        {
            public nicehash_sma_stats[] simplemultialgo;
        }

        public class nicehash_json_2
        {
            public nicehash_result_2 result;
            public string method;
        }

        class nicehash_result<T>
        {
            public T[] stats;
        }
        
        class nicehash_json<T>
        {
            public nicehash_result<T> result;
            public string method;
        }

        class nicehash_error
        {
            public string error;
            public string method;
        }

        class nicehashminer_version
        {
            public string version;
        }
#pragma warning restore 649


        public static double[] GetAlgorithmRates()
        {
            string r1 = GetNiceHashAPIData("https://www.nicehash.com/api?method=simplemultialgo.info");
            if (r1 == null) return null;

            nicehash_json_2 nhjson_current;
            try
            {
                nhjson_current = JsonConvert.DeserializeObject<nicehash_json_2>(r1);
                double[] outval = new double[nhjson_current.result.simplemultialgo.Length];
                for (int i = 0; i < nhjson_current.result.simplemultialgo.Length; i++)
                {
                    outval[i] = nhjson_current.result.simplemultialgo[i].paying;
                }

                return outval;
            }
            catch
            {
                return null;
            }
        }


        public static nicehash_stats GetStats(string btc, int location, int algo)
        {
            if (location > 1) location = 1;
            string r1 = GetNiceHashAPIData("https://www.nicehash.com/api?method=stats.provider&location=" + location.ToString() + "&addr=" + btc);
            if (r1 == null) return null;

            nicehash_json<nicehash_stats> nhjson_current;
            try
            {
                nhjson_current = JsonConvert.DeserializeObject<nicehash_json<nicehash_stats>>(r1);
                for (int i = 0; i < nhjson_current.result.stats.Length; i++)
                {
                    if (nhjson_current.result.stats[i].algo == algo)
                        return nhjson_current.result.stats[i];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }


        public static double GetBalance(string btc)
        {
            double balance = 0;

            for (int l = 0; l < 2; l++)
            {
                string r1 = GetNiceHashAPIData("https://www.nicehash.com/api?method=stats.provider&location=" + l.ToString() + "&addr=" + btc);
                if (r1 == null) break;

                nicehash_json<nicehash_stats> nhjson_current;
                try
                {
                    nhjson_current = JsonConvert.DeserializeObject<nicehash_json<nicehash_stats>>(r1);
                    for (int i = 0; i < nhjson_current.result.stats.Length; i++)
                        balance += nhjson_current.result.stats[i].balance;
                }
                catch
                {
                    break;
                }
            }

            return balance;
        }


        public static string GetVersion()
        {
            string r1 = GetNiceHashAPIData("https://www.nicehash.com/nicehashminer?method=version");
            if (r1 == null) return null;

            nicehashminer_version nhjson;
            try
            {
                nhjson = JsonConvert.DeserializeObject<nicehashminer_version>(r1);
                return nhjson.version;
            }
            catch
            {
                return null;
            }
        }


        private static string GetNiceHashAPIData(string URL)
        {
            string ResponseFromServer;
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(URL);
                WR.Timeout = 5000;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 5000;
                StreamReader Reader = new StreamReader(SS);
                ResponseFromServer = Reader.ReadToEnd();
                if (ResponseFromServer.Length == 0 || ResponseFromServer[0] != '{')
                    throw new Exception("Not JSON!");
                Reader.Close();
                Response.Close();
            }
            catch
            {
                return null;
            }

            return ResponseFromServer;
        }
    }
}