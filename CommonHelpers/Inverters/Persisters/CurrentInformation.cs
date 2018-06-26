using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Inverters.Persisters
{
    public class CurrentInformation
    {
        public List<int> CurrentProductions { get; private set; }
        public List<int> CurrentPowers { get; private set; }
        public int CurrentProduction { get; set; }
        public int CurrentDay { get; set; }
        public List<float> Temps { get; private set; }
        public List<DateTime> Taketimes { get; private set; }
        public CurrentInformation()
        {
            CurrentPowers = new List<int>();
            Temps = new List<float>();
            Taketimes = new List<DateTime>();
            CurrentProductions = new List<int>();
        }
        public void Clear()
        {
            CurrentPowers.Clear();
            Temps.Clear();
            Taketimes.Clear();
            CurrentProductions.Clear();
        }
        public float AverangeTemps
        {
            get
            {
                if (Temps.Count == 0) return 0;
                float tot = 0;
                lock (this)
                {
                    Temps.ForEach(delegate(float v) { tot += v; });
                }

                return (float)tot / (float)Temps.Count;
            }
        }
        public int AverangePower
        {
            get
            {
                if (CurrentPowers.Count == 0) return 0;
                int tot = 0;
                lock (this)
                {
                    CurrentPowers.ForEach(delegate(int v) { tot += v; });
                }

                return (int)tot / CurrentPowers.Count;
            }
        }

        public int MaxPower
        {
            get
            {
                if (CurrentPowers.Count == 0) return 0;
                int max = Int32.MinValue;
                lock (this)
                {
                    CurrentPowers.ForEach(delegate(int v) { if (v > max)max = v; });
                }

                return max;
            }
        }


    }

}
