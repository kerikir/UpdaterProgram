using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdaterFile
{
    public interface IDownload
    {
        public string Speed { get; set; }
        public float Progress { get; set; }
        void SpeedCalculate(long bytes);
    }
}
