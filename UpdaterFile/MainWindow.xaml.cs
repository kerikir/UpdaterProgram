using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UpdaterFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionPath;
        private string zipPath;

        private string currentVersion;
        private string newVersion;

        WebClient client;

        Download download;
        CancellationTokenSource cancelSource;
        CancellationToken token;

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionPath = Path.Combine(rootPath, "Version.txt");
            currentVersion = File.ReadAllText(versionPath);
            zipPath = Path.Combine(rootPath, "WpfTemperature.zip");
            download = new Download();
            DataContext = download;
            client = new WebClient();
            cancelSource = new CancellationTokenSource();
            token = cancelSource.Token;

            tblcurrentVersion.Text = currentVersion;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                currentVersion = File.ReadAllText(versionPath);
                newVersion = wc.DownloadString("https://drive.google.com/uc?export=download&id=1-kJ9TdrQuDCu41jKASzLHpMALdCNESz3");

                if (int.Parse(newVersion.Replace(".", "")) > int.Parse(currentVersion.Replace(".", "")))
                {
                    var result = MessageBox.Show($"Доступна версия {newVersion}. Обновить?", "", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        pbUploading.Visibility = Visibility.Visible;
                        btnDelete.Visibility = Visibility.Visible;
                        tbUploadingSpeed.Visibility = Visibility.Visible;
                        btnUpdate.IsEnabled = false;
                        Upload();
                    }
                    else
                    {
                        //Process.Start("WpfTemperature.exe");
                        Process.GetCurrentProcess().Kill();
                    }

                }
                else
                {
                    MessageBox.Show($"Версия актуальна");
                }

            }
        }

        public void Upload()
        {
            client = new WebClient();
            btnDelete.Visibility = Visibility.Visible;
            client.DownloadProgressChanged += (s, e) =>
            {
                    download.Progress = e.ProgressPercentage;
                    download.SpeedCalculate(e.BytesReceived);
            };
            client.DownloadFileCompleted += (s, e) =>
            {
                if (cancelSource.IsCancellationRequested)
                {
                    btnDelete.Visibility = Visibility.Hidden;
                    //UnupdateForm();
                    return;
                }
                btnDelete.Visibility = Visibility.Hidden;
                UploadCompleted();
                MessageBox.Show("Загрузка завершена");
                UpdateForm();
                FormCloses();
                btnUpdate.IsEnabled = true;
            };



            //client.DownloadFileAsync(new System.Uri("https://drive.google.com/uc?export=download&id=1Dv0O0PJL3YvQPsvyNNWxWcRfuIo7B7a8"), zipPath, token);
            client.DownloadFileAsync(new System.Uri("https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-archive"), zipPath,token);
        }

        public void UnupdateForm()
        {
            btnDelete.Visibility = Visibility.Hidden;
            pbUploading.Visibility = Visibility.Hidden;
            tbUploadingSpeed.Visibility = Visibility.Hidden;
            download.Progress = 0;
            download.Speed = "Скорость: 0.00 МБ/сек";
            btnUpdate.IsEnabled = true;
        }

        //public async Task Down(CancellationToken tokens)
        //{
        //    Task task = Task.Factory.StartNew(() =>
        //    {
        //        client.DownloadProgressChanged += (s, e) =>
        //        {
        //            download.Progress = e.ProgressPercentage;
        //            download.SpeedCalculate(e.BytesReceived);
        //            tbUploadingSpeed.Text = download.Speed;
        //        };
        //        client.DownloadFileCompleted += (s, e) =>
        //        {

        //            btnDelete.Visibility = Visibility.Hidden;
        //            UploadCompleted();
        //            MessageBox.Show("Загрузка завершена");
        //            UpdateForm();
        //            FormCloses();
        //            btnUpdate.IsEnabled = true;
        //        };

        //        client.DownloadFileAsync(new System.Uri("https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-archive"), zipPath, token);
        //    }, token);
        //}

        //public async void Upload()
        //{
        //    client.DownloadProgressChanged += (s, e) =>
        //    {
        //        download.Progress = e.ProgressPercentage;
        //        download.SpeedCalculate(e.BytesReceived);
        //    };
        //    client.DownloadFileCompleted += (s, e) =>
        //    {
        //        btnDelete.Visibility = Visibility.Hidden;
        //        UploadCompleted();
        //        MessageBox.Show("Загрузка завершена");
        //        UpdateForm();
        //        FormCloses();
        //    };

        //    //client.DownloadFileAsync(new System.Uri("https://drive.google.com/uc?export=download&id=1Dv0O0PJL3YvQPsvyNNWxWcRfuIo7B7a8"), zipPath);

        //    Task task = Task.Factory.StartNew(() =>
        //    {
        //        client.DownloadFile(new System.Uri("https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-archive"), zipPath);
        //        token.ThrowIfCancellationRequested();  // Проверяем запрос отмены
        //    }, token);
        //}


        public void UploadCompleted()
        {
            var proc = Process.GetProcessesByName("WpfTemperature");
            //var proc2 = Process.GetProcesses();
            foreach(var item in proc)
            {
                item.Kill();
            }
            if(!File.Exists(zipPath))
            {
                ZipFile.ExtractToDirectory(zipPath, rootPath, true);
                File.Delete(zipPath);
                File.WriteAllText(versionPath, newVersion);
            }
            
        }

        public void UpdateForm()
        {
            tblcurrentVersion.Text = "Текущая версия: " + newVersion;
            pbUploading.Visibility = Visibility.Hidden;
            tbUploadingSpeed.Text = "";
        }

        public void FormCloses()
        {
            Process.Start("WpfTemperature.exe");
            Process.GetCurrentProcess().Kill();
        }

        private void btnStopDownload_Click(object sender, RoutedEventArgs e)
        {
            cancelSource.Cancel();
            UnupdateForm();
            MessageBox.Show("Скачивание отменено");
        }
    }
}
