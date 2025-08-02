using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UpdaterFile
{
    public class Download : DependencyObject, IDownload
    {
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(float), typeof(Download));
        public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register("Speed", typeof(string), typeof(Download));

        public float Progress
        {
            get { return (float)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, (float)value); }
        }

        public string Speed
        {
            get { return (string)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, (string)value); }
        }

        private DateTime lastUpdate;
        private long lastBytes = 0;
        public void SpeedCalculate(long bytes)
        {
            if (lastBytes == 0)
            {
                lastUpdate = DateTime.Now;
                lastBytes = bytes;
                return;
            }

            var now = DateTime.Now;
            var timeSpan = now - lastUpdate;
            if (timeSpan.Milliseconds != 0)
            {
                var bytesChange = bytes - lastBytes;
                var bytesPerMillisecond = ((bytesChange / timeSpan.Milliseconds) / 1024d / 1024d * 1000).ToString("0.00");
                Speed = $"Скорость: {bytesPerMillisecond} МБ/сек";
            }
            lastBytes = bytes;
            lastUpdate = now;
        }
    }
}
