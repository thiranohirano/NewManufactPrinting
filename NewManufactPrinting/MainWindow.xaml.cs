using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
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
using MaterialDialog;
using MyUtilityMethods;

namespace NewManufactPrinting
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        readonly MainWindowViewModel mwvm;
        private DispatcherTimer ConfirmPrintEndTimer;
        private DataTable productSetList;
        private readonly InkJetPrinter inkJetPrinter = new InkJetPrinter();
        private bool IsReadStateAQR;
        private bool isDebug;

        public MainWindow()
        {
            InitializeComponent();
            mwvm = this.FindResource("mwvm") as MainWindowViewModel;
            home_view.clear_btn.Click += Clear_button_Click;
            home_view.printing_btn.Click += Print_button_Click;
            home_view.label_print_btn.Click += LabelPrint_button_Click;
            home_view.complete_btn.Click += Complete_button_Click;
        }

        private async void MainMetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /* アセンブリバージョンが変わるときにユーザ設定を引き継ぐ */
            var assemblyVersion = ApplicationInfo.GetVersionContainsAssembly();
            if (Properties.Settings.Default.AssemblyVersion != assemblyVersion)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.AssemblyVersion = assemblyVersion;
                Properties.Settings.Default.Save();
            }

#if DEBUG
            mwvm.ServerUrl = "http://192.168.100.115:8000/";//"http://nanopi-neo2-server.local:8000/";
#endif
            string api_url = "NONE";
            if (Properties.Settings.Default.ServerUrl == "NONE")
            {
                //bool? re = (bool?)await DialogHostEx.ShowDialog(this, new StartUp());
                //if (re != null && re == true)
                //{
                    if (mwvm.ServerUrl != string.Empty)
                    {
                        api_url = mwvm.ServerUrl;
                    }
                    else
                    {
                        api_url = "NONE";
                    }
                    Console.WriteLine(mwvm.ServerUrl);
#if !DEBUG
                                Properties.Settings.Default.ServerUrl = api_url;
                                Properties.Settings.Default.Save();
#endif
                //}
            }

#if !DEBUG
            this.WindowState = System.Windows.WindowState.Maximized;
#endif

            try
            {
                mwvm.SetApiServer(api_url);
                await ConnectServer();
            }
            catch (Exception)
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Error", "無効なURLです。");
            }

            string ipAddress = Properties.Settings.Default.IpAddress;
            if(ipAddress == "NONE")
            {
                ipAddress = string.Empty;
            }
            mwvm.IpAddress = ipAddress;
            mwvm.Port = Properties.Settings.Default.Port;

            mwvm.SelectedBarcodeComPort = Properties.Settings.Default.BarcodeCOM;
            mwvm.BarcodeSerialPort.Encoding = Encoding.GetEncoding(932);
            mwvm.BarcodeSerialPort.DataReceived += BarcodeSerialPort_DataReceived;

            mwvm.SelectedLabelPrinterComPort = Properties.Settings.Default.LabelPrinterCOM;
            mwvm.LabelPrinterSerialPort.Encoding = Encoding.GetEncoding(932);
            mwvm.LabelPrinterSerialPort.BaudRate = 115200;

            mwvm.CableDataFilePath = Properties.Settings.Default.CableDataCsvPath;
            mwvm.ProductSetListFilePath = Properties.Settings.Default.ProductSetListCsvPath;

            //印字終了確認処理を行うタイマー
            ConfirmPrintEndTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            ConfirmPrintEndTimer.Tick += ConfirmPrintEndTimer_Tick;

            await ConnectPrinter();
            mwvm.GetPortName();
            await mwvm.OpenBarcodeSerialPort();
            mwvm.SelectBarcodeComPort = mwvm.SelectedBarcodeComPort;
            await mwvm.OpenLabelPrinterSerialPort();
            mwvm.SelectLabelPrinterComPort = mwvm.SelectedLabelPrinterComPort;
        }

        private void MainMetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string ipAddress = Properties.Settings.Default.IpAddress;
            if (ipAddress == string.Empty)
            {
                ipAddress = "NONE";
            }
            Properties.Settings.Default.IpAddress = ipAddress;
            mwvm.CloseBarcodeSerialPort();
            mwvm.CloseLabelPrinterSerialPort();
            inkJetPrinter.Close();
        }

        /// <summary>
        /// 印字開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Print_button_Click(object sender, RoutedEventArgs e)
        {
            string printLot = MakePrintLot(mwvm.Lot);
            bool sendSuccess = false;
            await Task.Run(() =>
            {
                //印字データをプリンターに送信
                sendSuccess = inkJetPrinter.SendPrinting(mwvm.Model, printLot);
            });

            if(!sendSuccess && !isDebug)
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Warning", "送信できませんでした。");
                return;
            }

            mwvm.CompleteButtonEnabled = true;

            inkJetPrinter.ClearRePrintTimes();
            mwvm.RedoPrintingTimes = inkJetPrinter.GetRePrintTimes();

            if(!isDebug)
            {
                ConfirmPrintEndTimer.Start();
            }
        }

        /// <summary>
        /// クリアボタン処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Clear_button_Click(object sender, RoutedEventArgs e)
        {
            await DoClearTask();
        }

        /// <summary>
        /// 印字完了ボタンの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Complete_button_Click(object sender, RoutedEventArgs e)
        {
            MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(this, "ロギング中...");
            await mwvm.WriteCablePrintingLog();

            if (mwvm.IsConnect == false)
            {　　
                //ロギングに失敗した場合
                //MessageBox.Show("ロギングに失敗しました。内部バッファにデータを保存します。");
                //Googleにアクセスできないときの処理
            }

            controller.setMessage("完了処理中です...");
            //表示データと取得データのクリア
            await DoClearTask();
            controller.Close();
        }

        /// <summary>
        /// ラベルの印刷処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LabelPrint_button_Click(object sender, RoutedEventArgs e)
        {
            if (!mwvm.LabelPrinterSerialPort.IsOpen)
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Warning", "ラベルプリンターのCOMポートが開いていません");
                return;
            }

            //コマンドを送って応答があるかで、電源が入っているか確認
            mwvm.LabelPrinterSerialPort.Write(DataFactory.ConfirmLabelNumCmd, 0, DataFactory.ConfirmLabelNumCmd.Length);
            await Task.Delay(500);
            if (mwvm.LabelPrinterSerialPort.BytesToRead > 0)
            {
                mwvm.LabelPrinterSerialPort.DiscardInBuffer();
                string print_lot = string.Empty;
                if (mwvm.Lot != string.Empty)
                {
                    print_lot = String.Format(" Lot.{0}", mwvm.Lot);
                }
                MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(this, "ラベル印刷中...");
                bool sendSuccess = false;
                await Task.Run(() =>
                {
                    try
                    {
                        //印刷コマンドを送信
                        byte[] sendData = DataFactory.MakeSendDataForLabel(mwvm.Model, print_lot, mwvm.LabelPrintingQuantity);
                        mwvm.LabelPrinterSerialPort.Write(sendData, 0, sendData.Length);
                        sendSuccess = true;
                    }
                    catch (Exception)
                    {
                    }
                });
                if (!sendSuccess)
                {
                    controller.Close();
                    await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Warning", "ラベルプリンタとの通信に失敗しました");
                    return;
                }

                await Task.Delay(2000);

                controller.Close();

                mwvm.CompleteButtonEnabled = true;
                mwvm.LabelPrintingTimes += mwvm.LabelPrintingQuantity;
            }
            else
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Caution", "ラベルプリンターの電源が入っていることを確認して下さい");
            }
        }

        //QRコードを受信したときの処理
        private async void BarcodeSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = sender as SerialPort;
            if (mwvm.CompleteButtonEnabled)
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Caution", "印字が完了していません\n印字完了ボタンを押すか、クリアボタンを押してから読み込んで下さい");
                serialPort.DiscardInBuffer();
                return;
            }

            string buffer = serialPort.ReadExisting();

            ExecQrRead(buffer);

            serialPort.DiscardInBuffer();//受信バッファの中のデータ
        }

        private void Model_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            mwvm.PrintingContent = MakePrintContent(mwvm.Model, mwvm.Lot);
        }

        private void Lot_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            mwvm.PrintingContent = MakePrintContent(mwvm.Model, mwvm.Lot);
        }

        /// <summary>
        /// サーバーとの接続処理
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        public async Task ConnectServer(bool reset = true)
        {
            if (reset)
            {
                mwvm.ResetAllQRInfo();
            }
            var controller = await MaterialDialogUtil.ShowMaterialProgressDialog(this, "Now Loading...");
            await mwvm.LoadLogClasses();
            controller.Close();
            if (!mwvm.IsConnect)
            {
                await ShowDisconnectDialog();
            }
        }

        public async Task<bool> ConnectPrinter()
        {
            string result = string.Empty;
            MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(this, "インクジェットプリンターに接続中...");
            bool success = false;
            await Task.Run(() =>
            {
                inkJetPrinter.SetIpAddress(Properties.Settings.Default.IpAddress);
                inkJetPrinter.SetPort(Properties.Settings.Default.Port);
                success = inkJetPrinter.Connect();
            });
            controller.Close();
            if(!success)
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Warning", "インクジェットプリンターに接続できませんでした");
            }

            return success;
        }

        //タイマー1(印字終了確認と印字回数の更新)
        private void ConfirmPrintEndTimer_Tick(object sender, EventArgs e)
        {
            inkJetPrinter.ConfirmPrintEnd(mwvm.RedoPrintingMode);
            mwvm.PrintingTimes = inkJetPrinter.GetPrintTimes();
            int rePrintTimes = inkJetPrinter.GetRePrintTimes();
            if (mwvm.RedoPrintingMode && rePrintTimes > mwvm.RedoPrintingTimes)
            {
                mwvm.RedoPrintingTimes = rePrintTimes;
                mwvm.RedoPrintingMode = false;
            }
        }

        /// <summary>
        /// AのQRコードを読んだ後の処理
        /// </summary>
        /// <param name="aqr"></param>
        private async void ExecAQrRead(string aqr)
        {
            await ExecAQrReadLog(aqr);
            IsReadStateAQR = false;
        }

        private async Task ExecAQrReadLog(string aqr)
        {
            mwvm.ShowAQR(aqr);

            //セット品型式である場合に付属品そのものの型式に変換し、
            //ロット番号を付加するかどうかを決定。
            await Task.Run(() =>
            {
                productSetList = DataFactory.GetProductSetList();
            });
            PrintContent pc = DataFactory.GetConvertedPrintContent(mwvm.Model, productSetList);

            string lot = string.Empty;
            if (pc.HasAddedLot)
            {
                lot = DateTime.Now.ToString("ddMMyy");
            }
            string printStr = MakePrintContent(mwvm.Model, lot);
            mwvm.Lot = lot;
            mwvm.PrintingContent = printStr;
        }

        private async Task DoClearTask()
        {
            await Task.Run(() =>
            {
                //印字データ破棄の送信
                inkJetPrinter.SendClear();
            });

            //表示データの消去
            mwvm.ClearInfos();

            mwvm.RedoPrintingMode = false;

            mwvm.PrintingButtonEnabled = false;
            mwvm.RedoPrintingButtonEnabled = false;
            mwvm.CompleteButtonEnabled = false;

            //印字終了確認処理のタイマーを停止
            ConfirmPrintEndTimer.Stop();

            //checkbox:枚数入力
            mwvm.EditEnable = false;

            //印字終了確認処理の回数をクリア
            inkJetPrinter.ClearTimerCounterForPrint();
            //やり直し回数のクリア
            inkJetPrinter.ClearRePrintTimes();

            isDebug = false;
        }

        /// <summary>
        /// QRコード別の処理
        /// </summary>
        /// <param name="qr_str"></param>
        private void ExecQrRead(string qr_str)
        {
            string buffer = qr_str;
            buffer = buffer.Replace("\r", string.Empty).Replace("\n", string.Empty);
            if (buffer != string.Empty && buffer != "\r\n")
            {
                string QRresult = buffer;
                string[] qr = LogDefineMethod.leaveDelimiterSplitQR(QRresult);
                if (qr.Length > 5)
                {
                    switch (qr[1])
                    {
                        case "A":
                            if (!IsReadStateAQR && qr.Length > 6)
                            {
                                IsReadStateAQR = true;
                                this.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    ExecAQrRead(QRresult);
                                }));
                            }
                            break;
                        case "B":
                            this.Dispatcher.BeginInvoke(new ShowDelegate(mwvm.ShowBQR), QRresult);
                            break;
                    }
                }
            }
        }

        private string MakePrintLot(string lot)
        {
            string printLot = string.Empty;
            if (lot != string.Empty)
            {
                printLot = String.Format(" Lot.{0} ", lot);
            }
            return printLot;
        }

        private string MakePrintContent(string model, string lot)
        {
            if (model == string.Empty) return string.Empty;
            string printLot = MakePrintLot(lot);
            return String.Format("{0} {1} Diatrend Corp.", model, printLot);
        }

        /// <summary>
        /// サーバー接続失敗時のメッセージ表示
        /// </summary>
        /// <returns></returns>
        private async Task ShowDisconnectDialog()
        {
            await MaterialDialogUtil.ShowMaterialMessageDialog(this, "Warning", "サーバーへの接続が失敗しました。\nネットワーク接続の確認と\nサーバー管理者に確認してください。");
        }

        private delegate void ShowDelegate(string data);

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
                        if (!IsReadStateAQR)
                        {
                            IsReadStateAQR = true;
                            string Aresult = QRresult;

                            ExecAQrRead(Aresult);
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
                ReadDebug("RORANI,A,000000-2,2013/03/11,2013/03/13,ひらの,DXCAB-CY(4M),5,BBBBBBBBBBBBBBB,1,10,0,0,0,0,あああああああ注釈だよおおおおおお\r\n");
                mwvm.Member = "平野卓次";
                mwvm.LabelPrintingTimes = 5;
                isDebug = true;
            }
# endif
        }
    }
}
