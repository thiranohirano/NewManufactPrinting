using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using ManufactApiClient.Models;
using MaterialDesignThemes.Wpf;
using MaterialDialog;
using MyUtilityMethods;
using NewManufactPrinting.BufferedLog.Entities;
using NewManufactPrinting.Views;

namespace NewManufactPrinting
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private const string NoneUrlSetting = "NONE";
        readonly MainWindowViewModel mwvm;

        public MainWindow()
        {
            InitializeComponent();
            mwvm = this.FindResource("mwvm") as MainWindowViewModel;
        }

        private async void MainMetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                /* アセンブリバージョンが変わるときにユーザ設定を引き継ぐ */
                var assemblyVersion = ApplicationInfo.GetVersionContainsAssembly();
                if (Properties.Settings.Default.AssemblyVersion != assemblyVersion)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.AssemblyVersion = assemblyVersion;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                bool? result = await MaterialDialogUtil.ShowMaterialMessageYesNoDialog(this, "Warning", "設定ファイルが壊れています。\n初期化or前バージョンの設定で再起動しますか？");
                string filePath = ((ConfigurationErrorsException)ex.InnerException).Filename;
                File.Delete(filePath);
                if (result.HasValue && result.Value == true)
                {
                    string appName = Application.ResourceAssembly.GetName().Name;
                    Process.Start(appName + ".exe");
                    this.Close();
                    return;
                }
                else
                {
                    this.Close();
                    return;
                }
            }

#if DEBUG
            mwvm.ServerUrl = "http://nanopi-neo2-server.local:8000/";
#endif
            string apiUrl = NoneUrlSetting;
            if (Properties.Settings.Default.ServerUrl == NoneUrlSetting)
            {
                apiUrl = await ShowStartUpDialog(apiUrl);
            }
            else
            {
                apiUrl = Properties.Settings.Default.ServerUrl;
                mwvm.ServerUrl = apiUrl;
            }

#if !DEBUG
            this.WindowState = System.Windows.WindowState.Maximized;
#endif

            await mwvm.ConfirmApiUrlConnection(apiUrl);

#if !DEBUG
            await mwvm.ConnectPrinter();
#endif
            await mwvm.OpenTecPrinter();

            mwvm.GetPortName();
            await mwvm.OpenBarcodeSerialPort();
            mwvm.SelectBarcodeComPort = mwvm.SelectedBarcodeComPort;
            //await mwvm.OpenLabelPrinterSerialPort();
            //mwvm.SelectLabelPrinterComPort = mwvm.SelectedLabelPrinterComPort;
        }

        private async void MainMetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mwvm.CloseBarcodeSerialPort();
            //mwvm.CloseLabelPrinterSerialPort();
            mwvm.ClosePrinter();
            await mwvm.CloseTecPrinter();
        }
        /// <summary>
        /// 再接続メニューボタンのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Reconnect_mi_Click(object sender, RoutedEventArgs e)
        {
            if (mwvm.DialogIsOpen) return;
            mwvm.IsReadStateAQR = true;
            await mwvm.ConnectServer(reset: false);
            if (mwvm.IsConnect)
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Info", "接続成功しました。");
            }
            mwvm.IsReadStateAQR = false;
        }

        /// <summary>
        /// 環境設定メニューボタンのクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Env_setting_mi_Click(object sender, RoutedEventArgs e)
        {
            if (mwvm.DialogIsOpen) return;
            string apiUrl = string.Empty;
            if (Properties.Settings.Default.ServerUrl != "NONE")
            {
                apiUrl = Properties.Settings.Default.ServerUrl;
            }
            else
            {
#if DEBUG
                apiUrl = "http://nanopi-neo2-server.local:8000/";
#endif
            }

            mwvm.ServerUrl = apiUrl;
            apiUrl = await ShowStartUpDialog(apiUrl);

            mwvm.IsConnect = false;
            await mwvm.ConfirmApiUrlConnection(apiUrl, isShowResultDialog: true);
        }

        private async void Check_update_mi_Click(object sender, RoutedEventArgs e)
        {
            await mwvm.ConfirmUpdate(isShowResultDialog: true);
        }

        /// <summary>
        /// デバッグ用
        /// </summary>
        /// <param name="qrtext"></param>
        private void ReadDebug(string qrtext)
        {
            string buffer = qrtext;
            buffer = buffer.Replace("\r", string.Empty).Replace("\n", string.Empty);

            if (buffer != string.Empty && buffer != "\r\n")
            {
                string QRresult = buffer;

                string[] qr = LogDefineMethod.leaveDelimiterSplitQR(QRresult);
                if (qr.Length > 5)
                {
                    if (qr[1] == "A")
                    {
                        if (!mwvm.IsReadStateAQR)
                        {
                            mwvm.IsReadStateAQR = true;
                            string Aresult = QRresult;

                            mwvm.ExecAQrRead(Aresult);
                        }
                    }
                }
            }
        }

        private void MainMetroWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
#if DEBUG
            if (e.Key == System.Windows.Input.Key.F6)
            {
                ReadDebug("RORANI,A,000000-8,2019/04/18,2019/04/19,ひらの,DXCAB-CY(4M),3,BBBBBBBBBBBBBBB,1,10,0,0,0,0,あああああああ注釈だよおおおおおお\r\n");
                mwvm.Member = "平野卓次";
                mwvm.LabelPrintingTimes = 5;
                mwvm.isDebug = true;
            }
#endif
        }

        private async Task<string> ShowStartUpDialog(string apiUrl)
        {
            bool? re = (bool?)await DialogHostEx.ShowDialog(this, new StartUp());
            if (re.HasValue && re.Value == true)
            {
                if (mwvm.ServerUrl != string.Empty)
                {
                    apiUrl = mwvm.ServerUrl;
                }
                else
                {
                    apiUrl = NoneUrlSetting;
                }
                Console.WriteLine(mwvm.ServerUrl);
#if !DEBUG
                    Properties.Settings.Default.ServerUrl = apiUrl;
                    Properties.Settings.Default.Save();
#endif
            }

            return apiUrl;
        }
    }
}
