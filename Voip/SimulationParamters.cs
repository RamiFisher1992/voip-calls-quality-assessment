using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voip
{
    public class SimulationParamters
    {
        //RealTime
        public string choosenAlgorithem { get; set; }
        public int numOfCalls { get; set; }
        public int numOfSamples { get; set; }
        public double validationPercent { get; set; }
        public string runnningMode { get; set; }
        public string includeUnknown { get; set; }
        public List<string>features { get; set; }
        //PCA
        public string viewerAlgorithem { get; set; }
        public int viewerNumOfCalls { get; set; }
        public int viewerNumOfSamples { get; set; }
        public string pcaIncludeUnknown { get; set; }
        //Robustness 
        public double robustValidationPercent { get; set; }
        public int robustNumOfCalls { get; set; }
        public int robustNumOfSamples { get; set; }
        public SimulationParamters()
        { }
    }
}
