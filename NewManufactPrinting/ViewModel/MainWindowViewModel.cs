using JanCodeApiClient.Models;
using ManufactApiClient.Models;
using MaterialDesignThemes.Wpf;
using MaterialDialog;
using MyUtilityMethods;
using NewManufactPrinting.BufferedLog;
using NewManufactPrinting.BufferedLog.Entities;
using NewManufactPrinting.ViewModel;
using NewManufactPrinting.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TecLabelPrinter;

namespace NewManufactPrinting
{
    public class MainWindowViewModel : BindableClasses
    {
        public string Title
        {
            get { return Application.ResourceAssembly.GetName().Name; }
        }

        public string Version
        {
            get
            {
                Assembly mainAssembly = Assembly.GetEntryAssembly();
                AssemblyName mainAssemName = mainAssembly.GetName();
                // バージョン名（AssemblyVersion属性）を取得
                Version appVersion = mainAssemName.Version;
                string v = appVersion.Major.ToString() + "." +
              appVersion.Minor.ToString() + "." +
              appVersion.Build.ToString() + "." +
              appVersion.Revision.ToString();
                return "Version:" + v;
            }
        }

        #region Home

        public BaseInfoLog BaseInfoLog;

        private string _OrderNumber = string.Empty;
        public string OrderNumber
        {
            get { return _OrderNumber; }
            set {
                if(value != string.Empty && Member != string.Empty)
                {
                    PrintingButtonEnabled = true;
                    RedoPrintingButtonEnabled = true;
                    LabelPrintingButtonEnabled = true;
                    LabelPrintingTimes = 0;
                }
                this.SetProperty(ref _OrderNumber, value);
            }
        }

        private int? _Quantity;
        /// <summary>
        /// 本数
        /// </summary>
        public int? Quantity
        {
            get { return _Quantity; }
            set { this.SetProperty(ref _Quantity, value); }
        }

        private string _Model = string.Empty;
        /// <summary>
        /// 型式
        /// </summary>
        public string Model
        {
            get { return _Model; }
            set
            {
                PrintingContent = MakePrintContent(value, Lot);
                this.SetProperty(ref _Model, value);
            }
        }

        private string _Member = string.Empty;
        /// <summary>
        /// 製作者名
        /// </summary>
        public string Member
        {
            get { return _Member; }
            set
            {
                if (value != string.Empty && OrderNumber != string.Empty)
                {
                    PrintingButtonEnabled = true;
                    RedoPrintingButtonEnabled = true;
                    LabelPrintingButtonEnabled = true;
                    LabelPrintingTimes = 0;
                }
                this.SetProperty(ref _Member, value);
            }
        }

        private string _Plate = string.Empty;
        /// <summary>
        /// 板番号
        /// </summary>
        public string Plate
        {
            get { return _Plate; }
            set { this.SetProperty(ref _Plate, value); }
        }

        private string CableNumber;
        private string _Lot = string.Empty;
        /// <summary>
        /// Lot番号
        /// </summary>
        public string Lot
        {
            get { return _Lot; }
            set
            {
                PrintingContent = MakePrintContent(Model, value);
                this.SetProperty(ref _Lot, value);
            }
        }

        private string _PrintingContent = string.Empty;
        /// <summary>
        /// 印字内容
        /// </summary>
        public string PrintingContent
        {
            get { return _PrintingContent; }
            set { this.SetProperty(ref _PrintingContent, value); }
        }

        private int? _PrintingTimes;
        /// <summary>
        /// 印字回数
        /// </summary>
        public int? PrintingTimes
        {
            get { return _PrintingTimes; }
            set { this.SetProperty(ref _PrintingTimes, value); }
        }

        private bool _PrintingButtonEnabled = false;
        public bool PrintingButtonEnabled
        {
            get { return _PrintingButtonEnabled; }
            set { this.SetProperty(ref _PrintingButtonEnabled, value); }
        }

        private int? _RedoPrintingTimes;
        /// <summary>
        /// やり直し回数
        /// </summary>
        public int? RedoPrintingTimes
        {
            get { return _RedoPrintingTimes; }
            set { this.SetProperty(ref _RedoPrintingTimes, value); }
        }

        private bool _RedoPrintingMode = false;
        /// <summary>
        /// やり直しモード
        /// </summary>
        public bool RedoPrintingMode
        {
            get { return _RedoPrintingMode; }
            set { this.SetProperty(ref _RedoPrintingMode, value); }
        }

        private bool _RedoPrintingButtonEnabled = false;
        public bool RedoPrintingButtonEnabled
        {
            get { return _RedoPrintingButtonEnabled; }
            set
            {
                this.SetProperty(ref _RedoPrintingButtonEnabled, value);
            }
        }

        private bool _IsRunningStopWatch;
        public bool IsRunningStopWatch
        {
            get { return _IsRunningStopWatch; }
            set { this.SetProperty(ref _IsRunningStopWatch, value); }
        }

        private string _StopWatchText = STOPWATCH_RESET_TEXT;
        public string StopWatchText
        {
            get { return _StopWatchText; }
            set { this.SetProperty(ref _StopWatchText, value); }
        }

        private string _LabelModel;
        public string LabelModel
        {
            get { return _LabelModel; }
            set { this.SetProperty(ref _LabelModel, value); }
        }

        private DateTime? _LabelDate = DateTime.Today;
        public DateTime? LabelDate
        {
            get { return _LabelDate; }
            set { this.SetProperty(ref _LabelDate, value); }
        }

        private JanModelDetail _JanModel;
        public JanModelDetail JanModel
        {
            get { return _JanModel; }
            set { this.SetProperty(ref _JanModel, value); }
        }

        private bool _LabelPrintingButtonEnabled = false;
        public bool LabelPrintingButtonEnabled
        {
            get { return _LabelPrintingButtonEnabled; }
            set { this.SetProperty(ref _LabelPrintingButtonEnabled, value); }
        }

        private int? _LabelPrintingTimes;
        /// <summary>
        /// ラベル印刷回数
        /// numOfPrintLabelのbinding対象
        /// </summary>
        public int? LabelPrintingTimes
        {
            get { return _LabelPrintingTimes; }
            set { this.SetProperty(ref _LabelPrintingTimes, value); }
        }

        private bool _EditEnable;
        public bool EditEnable
        {
            get { return _EditEnable; }
            set
            {
                if(value)
                {
                    LabelPrintingQuantity = 1;
                } else
                {
                    LabelPrintingQuantity = Quantity.HasValue ? Quantity.Value : 0;
                }
                this.SetProperty(ref _EditEnable, value);
            }
        }

        private int _LabelPrintingQuantity = 0;
        /// <summary>
        /// 枚数を表示するラベル
        /// numOfLabelsのbinding
        /// </summary>
        public int LabelPrintingQuantity
        {
            get { return _LabelPrintingQuantity; }
            set
            {
                if(value == 0)
                { 
                    LabelPrintingButtonEnabled = false;
                }
                this.SetProperty(ref _LabelPrintingQuantity, value);
            }
        }

        private bool _IsCutOneByOne;
        public bool IsCutOneByOne
        {
            get { return _IsCutOneByOne; }
            set { this.SetProperty(ref _IsCutOneByOne, value); }
        }

        private bool _CompleteButtonEnabled = false;
        public bool CompleteButtonEnabled
        {
            get { return _CompleteButtonEnabled; }
            set { this.SetProperty(ref _CompleteButtonEnabled, value); }
        }

        #endregion

        #region BufferedLogs

        private ObservableCollection<PrintingLog> _BufferedPrintingLogs = new ObservableCollection<PrintingLog>();
        public ObservableCollection<PrintingLog> BufferedPrintingLogs
        {
            get { return _BufferedPrintingLogs; }
            set
            {
                int? count = null;
                if (value != null)
                {
                    if (value.Count > 0)
                    {
                        count = value.Count;
                    }
                }
                BufferedPrintingLogsLength = count;
                BufferedLoggingButtonEnabled = count == null ? false : true;
                this.SetProperty(ref _BufferedPrintingLogs, value);
            }
        }

        private int? _BufferedPrintingLogsLength;
        public int? BufferedPrintingLogsLength
        {
            get { return _BufferedPrintingLogsLength; }
            set { this.SetProperty(ref _BufferedPrintingLogsLength, value); }
        }

        private bool _BufferedLoggingButtonEnabled = false;
        public bool BufferedLoggingButtonEnabled
        {
            get { return _BufferedLoggingButtonEnabled; }
            set { this.SetProperty(ref _BufferedLoggingButtonEnabled, value); }
        }

        #endregion

        #region Settings

        private Dictionary<string, string> _PortsTable = new Dictionary<string, string>(); //COMポート群
        public Dictionary<string, string> PortsTable
        {
            get { return _PortsTable; }
            set { this.SetProperty(ref _PortsTable, value); }
        }

        private string _SelectedBarcodeComPort; //バーコードリーダー通信用COMポート
        public string SelectedBarcodeComPort
        {
            get { return _SelectedBarcodeComPort; }
            set
            {
                if (value == null)
                    value = SERIALPORT_NONE;
                this.SetProperty(ref _SelectedBarcodeComPort, value);
            }
        }

        private string _SelectBarcodeComPort = SERIALPORT_NONE; //バーコードリーダー通信用COMポート(設定)
        public string SelectBarcodeComPort
        {
            get { return _SelectBarcodeComPort; }
            set
            {
                if (value == null)
                    value = SERIALPORT_NONE;
                this.SetProperty(ref _SelectBarcodeComPort, value);
            }
        }

        private string _SelectedLabelPrinterComPort; //ラベルプリンター通信用COMポート
        public string SelectedLabelPrinterComPort
        {
            get { return _SelectedLabelPrinterComPort; }
            set
            {
                if (value == null)
                    value = SERIALPORT_NONE;
                this.SetProperty(ref _SelectedLabelPrinterComPort, value);
            }
        }

        private string _SelectLabelPrinterComPort = SERIALPORT_NONE; //ラベルプリンター通信用COMポート(設定)
        public string SelectLabelPrinterComPort
        {
            get { return _SelectLabelPrinterComPort; }
            set
            {
                if (value == null)
                    value = SERIALPORT_NONE;
                this.SetProperty(ref _SelectLabelPrinterComPort, value);
            }
        }

        private string _CableDataFilePath = string.Empty;
        public string CableDataFilePath
        {
            get { return _CableDataFilePath; }
            set { this.SetProperty(ref _CableDataFilePath, value); }
        }

        private string _ProductSetListFilePath = string.Empty;
        public string ProductSetListFilePath
        {
            get { return _ProductSetListFilePath; }
            set { this.SetProperty(ref _ProductSetListFilePath, value); }
        }

        private string _IpAddress = string.Empty;
        public string IpAddress
        {
            get { return _IpAddress; }
            set { this.SetProperty(ref _IpAddress, value); }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { this.SetProperty(ref _Port, value); }
        }

        private string _TecIpAddress = string.Empty;
        public string TecIpAddress
        {
            get { return _TecIpAddress; }
            set { this.SetProperty(ref _TecIpAddress, value); }
        }

        private int _TecPort;
        public int TecPort
        {
            get { return _TecPort; }
            set { this.SetProperty(ref _TecPort, value); }
        }

        private int _CutPosition = 0;
        public int CutPosition
        {
            get { return _CutPosition; }
            set { this.SetProperty(ref _CutPosition, value); }
        }

        #endregion

        #region Help

        private bool _IsEnableReadMe;
        public bool IsEnableReadMe
        {
            get { return _IsEnableReadMe; }
            set { this.SetProperty(ref _IsEnableReadMe, value); }
        }

        #endregion

        private int _SelectedMenuIndex = 0;
        public int SelectedMenuIndex
        {
            get { return _SelectedMenuIndex; }
            set { this.SetProperty(ref _SelectedMenuIndex, value); }
        }

        private bool _IsConnect = false;
        public bool IsConnect
        {
            get { return _IsConnect; }
            set { this.SetProperty(ref _IsConnect, value); }
        }

        private bool _InkJetIsConnect = false;
        public bool InkJetIsConnect
        {
            get { return _InkJetIsConnect; }
            set { this.SetProperty(ref _InkJetIsConnect, value); }
        }

        private bool _TecPrinterIsConnect = false;
        public bool TecPrinterIsConnect
        {
            get { return _TecPrinterIsConnect; }
            set { this.SetProperty(ref _TecPrinterIsConnect, value); }
        }

        private string _TecPrinterStatus;
        public string TecPrinterStatus
        {
            get { return _TecPrinterStatus; }
            set { this.SetProperty(ref _TecPrinterStatus, value); }
        }

        private string _ServerUrl = string.Empty;
        public string ServerUrl
        {
            get { return _ServerUrl; }
            set { this.SetProperty(ref _ServerUrl, value); }
        }

        public string[] ServerUrls { get { return new string[] { "http://192.168.100.50/", "http://192.168.100.51/" }; } }

        private ConfirmApplication _ConfirmApplication;
        public ConfirmApplication ConfirmApplication
        {
            get { return _ConfirmApplication; }
            set
            {
                IsEnableReadMe = value != null;
                this.SetProperty(ref _ConfirmApplication, value);
            }
        }

        public bool DialogIsOpen
        {
            get; set;
        }

        private const string STOPWATCH_RESET_TEXT = "00:00:00";
        private const string SERIALPORT_NONE = "NONE";
        private const string CABLE_DATA_CSV_FILE = "ケーブル番号ー外形.csv";
        private const string PRODUCT_SET_LIST_CSV_FILE = "セット品型式リスト.csv";
        private const string ADDRESS_NONE = "NONE";
        private MainWindow mainWindow;
        private ManufactApiConnect.CablePrintingManufactApiConnect manufactApiConnect;
        private JanCodeApiConnect.JanCodeApiConnect janCodeApiConnect;
        private PrintingLogConnect printingLogConnect = new PrintingLogConnect();
        private TecLabelPrinter.TecLabelPrinter tecLabelPrinter = new TecLabelPrinter.TecLabelPrinter();
        private Stopwatch stopwatch = new Stopwatch();
        private readonly SerialPort BarcodeSerialPort = new SerialPort(SERIALPORT_NONE); //バーコードリーダー通信シリアルポート
        private readonly SerialPort LabelPrinterSerialPort = new SerialPort(SERIALPORT_NONE); //ラベルプリンター通信シリアルポート
        private readonly InkJetPrinter inkJetPrinter = new InkJetPrinter();
        public DispatcherTimer ConfirmPrintEndTimer;
        public bool isDebug;
        public bool IsReadStateAQR;

        public ICommand Clear { get; private set; }
        public ICommand Print { get; private set; }
        public  ICommand RedoPrint { get; private set; }
        public ICommand StopWatchStartStop { get; private set; }
        public ICommand StopWatchReset { get; private set; }
        public ICommand Plus { get; private set; }
        public ICommand Minus { get; private set; }
        public ICommand LabelPrint { get; private set; }
        public ICommand Complete { get; private set; }

        public ICommand BufferedLogging { get; private set; }

        public ICommand InkJetPrinterSettingApply { get; private set; }
        public ICommand TecPrinterSettingApply { get; private set; }
        public ICommand TecPrinterLabelSizeSend { get; private set; }

        public ICommand CableDataFilePathDesktopSet { get; private set; }
        public ICommand ProductSetListFilePathDesktopSet { get; private set; }

        public ICommand CableDataFilePathSettingApply { get; private set; }
        public ICommand ProductSetListFilePathSettingApply { get; private set; }

        public ICommand PortsReflesh { get; private set; }
        public ICommand BarcodeSerialPortSettingApply { get; private set; }
        //public ICommand LabelPrinterSerialPortSettingApply { get; private set; }

        public ICommand ReadMeOpen { get; private set; }

        public MainWindowViewModel()
        {
            mainWindow = (MainWindow)Application.Current.MainWindow as MainWindow;

            #region HomeView Commands

            Clear = new DelegateCommand(async (o) =>
            {
                await DoClearTask();
            });

            Print = new DelegateCommand(async (o) =>
            {
                string printLot = MakePrintLot(Lot);
                bool sendSuccess = false;
                await Task.Run(() =>
                {
                    //印字データをプリンターに送信
                    sendSuccess = inkJetPrinter.SendPrinting(Model, printLot);
                });

                if (!sendSuccess && !isDebug)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", "送信できませんでした。");
                    return;
                }

                CompleteButtonEnabled = true;

                inkJetPrinter.ClearPrintTimes();
                inkJetPrinter.ClearRePrintTimes();
                RedoPrintingTimes = inkJetPrinter.GetRePrintTimes();

                if (!isDebug)
                {
                    ConfirmPrintEndTimer.Start();
                }
                else
                {
                    PrintingTimes = inkJetPrinter.GetPrintTimes();
                }
            });

            RedoPrint = new DelegateCommand((o) =>
            {
                RedoPrintingMode = !RedoPrintingMode;
            });

            StopWatchStartStop = new DelegateCommand((o) =>
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
                else
                {
                    stopwatch.Start();
                }
                IsRunningStopWatch = stopwatch.IsRunning;
            });

            StopWatchReset = new DelegateCommand((o) =>
            {
                stopwatch.Reset();
                IsRunningStopWatch = stopwatch.IsRunning;
            });

            Plus = new DelegateCommand((o) =>
            {
                LabelPrintingQuantity++;
            });

            Minus = new DelegateCommand((o) =>
            {
                if (LabelPrintingQuantity > 1) LabelPrintingQuantity--; 
            });

            LabelPrint = new DelegateCommand(async (o) =>
            {
                if(!TecPrinterIsConnect)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", "TECラベルプリンターは接続されていません");
                    return;
                }

                bool IssueResult = await Issue();
                
                if (IssueResult)
                {
                    var controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "書込み中...");
                    //controller.SetMessage("書込み中...");
                    string printContent = $"{(LabelDate.HasValue ? LabelDate.Value.ToShortDateString() : null)}で{LabelPrintingQuantity}枚印刷";
                    await AddPrintingLog(printContent, LabelModel);
                    await controller.CloseWait(300);
                    CompleteButtonEnabled = true;
                    LabelPrintingTimes += LabelPrintingQuantity;
                }
                else
                {
                    TecPrinterIsConnect = false;
                }

                //if (!LabelPrinterSerialPort.IsOpen)
                //{
                //    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", "ラベルプリンターのCOMポートが開いていません");
                //    return;
                //}

                ////コマンドを送って応答があるかで、電源が入っているか確認
                //LabelPrinterSerialPort.Write(DataFactory.ConfirmLabelNumCmd, 0, DataFactory.ConfirmLabelNumCmd.Length);
                //await Task.Delay(500);
                //if (LabelPrinterSerialPort.BytesToRead > 0)
                //{
                //    LabelPrinterSerialPort.DiscardInBuffer();
                //    string print_lot = string.Empty;
                //    if (Lot != string.Empty)
                //    {
                //        print_lot = String.Format(" Lot.{0}", Lot);
                //    }
                //    else
                //    {
                //        print_lot = " ";
                //    }
                //    MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "ラベル印刷中...");
                //    bool sendSuccess = false;
                //    await Task.Run(() =>
                //    {
                //        try
                //        {
                //            //印刷コマンドを送信
                //            byte[] sendData = DataFactory.MakeSendDataForLabel(Model, print_lot, LabelPrintingQuantity);
                //            LabelPrinterSerialPort.Write(sendData, 0, sendData.Length);
                //            sendSuccess = true;
                //        }
                //        catch (Exception)
                //        {
                //        }
                //    });
                //    if (!sendSuccess)
                //    {
                //        controller.Close();
                //        await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", "ラベルプリンタとの通信に失敗しました");
                //        return;
                //    }

                //    await Task.Delay(2000);

                //    controller.Close();

                //    CompleteButtonEnabled = true;
                //    LabelPrintingTimes += LabelPrintingQuantity;
                //}
                //else
                //{
                //    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Caution", "ラベルプリンターの電源が入っていることを確認して下さい");
                //}
            });

            Complete = new DelegateCommand(async (o) =>
            {
                if (DialogIsOpen) return;
                MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "ロギング中...");
                DateTime created = DateTime.Now;
                await WriteCablePrintingLog(created);

                if (IsConnect == false)
                {
                    //ロギングに失敗した場合
                    controller.Close();
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Caution", "ロギングに失敗しました\n内部バッファにデータを保存します");
                    controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "保存中...");
                    await Task.Run(() =>
                    {
                        AddPrintingLog(created);
                    });
                    await Task.Run(() =>
                    {
                        LoadPrintingLogs();
                    });
                }

                controller.SetMessage("完了処理中です...");
                await Task.Delay(2000);
                //表示データと取得データのクリア
                await DoClearTask();
                await controller.CloseWait();
            });

            #endregion

            #region BufferedLogsView Commands

            BufferedLogging = new DelegateCommand(async (o) =>
            {
                await DoBufferedLogging();
            });

            #endregion

            #region SettingsView Commands

            InkJetPrinterSettingApply = new DelegateCommand(async (o) =>
            {
                if (IpAddress == string.Empty) IpAddress = ADDRESS_NONE;
                Properties.Settings.Default.IpAddress = IpAddress;
                Properties.Settings.Default.Port = Port;
                Properties.Settings.Default.Save();
                bool success = await ConnectPrinter();
                if(success)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "インクジェットプリンターに接続できました");
                }
            });

            TecPrinterSettingApply = new DelegateCommand(async (o) =>
            {
                if (TecIpAddress == string.Empty) TecIpAddress = ADDRESS_NONE;
                Properties.Settings.Default.TecIpAddress = TecIpAddress;
                Properties.Settings.Default.TecPort = TecPort;
                Properties.Settings.Default.CutPosition = CutPosition;
                Properties.Settings.Default.Save();
                bool success = await OpenTecPrinter();
                //await CloseTecPrinterAsync();
                if (success)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "TECラベルプリンターに接続できました");
                }
                else
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", "TECラベルプリンターに接続できませんでした");
                }
            });

            TecPrinterLabelSizeSend = new DelegateCommand(async (o) =>
            {
                if (!TecPrinterIsConnect) return;

                var controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "送信中...");
                await Task.Run(() =>
                {
                    try
                    {
                        int Gap = 30;
                        int LabelHeight = 250;
                        int LabelWidth = 700;
#if DEBUG
                        Gap = 20;
                        LabelHeight = 740;
                        LabelWidth = 1057;
#endif
                        int pitch = Gap + LabelHeight;

                        tecLabelPrinter.SendLabelSizeCommand(pitch, LabelWidth, LabelHeight);
                    }
                    catch (Exception)
                    {

                    }
                });
                await controller.CloseWait(200);
            });

            CableDataFilePathDesktopSet = new DelegateCommand((o) =>
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                CableDataFilePath = Path.Combine(desktopPath, CABLE_DATA_CSV_FILE);
            });

            ProductSetListFilePathDesktopSet = new DelegateCommand((o) =>
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ProductSetListFilePath = Path.Combine(desktopPath, PRODUCT_SET_LIST_CSV_FILE);
            });

            CableDataFilePathSettingApply = new DelegateCommand(async (o) =>
            {
                Properties.Settings.Default.CableDataCsvPath = CableDataFilePath;
                Properties.Settings.Default.Save();
                await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "適用しました");
            });

            ProductSetListFilePathSettingApply = new DelegateCommand(async (o) =>
            {
                Properties.Settings.Default.ProductSetListCsvPath = ProductSetListFilePath;
                Properties.Settings.Default.Save();
                await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "適用しました");
            });

            PortsReflesh = new DelegateCommand((o) =>
            {
                GetPortName();
            });

            BarcodeSerialPortSettingApply = new DelegateCommand(async (o) =>
            {
                SelectedBarcodeComPort = SelectBarcodeComPort;
                await OpenBarcodeSerialPort();
                if(SelectedBarcodeComPort != SERIALPORT_NONE)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", $"{SelectedBarcodeComPort}で開きました");
                }
            });

            //LabelPrinterSerialPortSettingApply = new DelegateCommand(async (o) =>
            //{
            //    SelectedLabelPrinterComPort = SelectLabelPrinterComPort;
            //    await OpenLabelPrinterSerialPort();
            //    if (SelectedLabelPrinterComPort != SERIALPORT_NONE)
            //    {
            //        await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", $"{SelectedLabelPrinterComPort}で開きました");
            //    }
            //});

            #endregion

            #region HelpView Commands

            ReadMeOpen = new DelegateCommand((o) =>
            {
                if (ConfirmApplication != null)
                {
                    Process.Start(new ProcessStartInfo(ConfirmApplication.read_me_url_full));
                }
            });

            #endregion

            /* ストップウォッチ用描画タイマー */
            DispatcherTimer stopwatchTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            stopwatchTimer.Tick += (s, e) =>
            {
                TimeSpan ts = stopwatch.Elapsed;
                StopWatchText = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            };
            stopwatchTimer.Start();

            string ipAddress = Properties.Settings.Default.IpAddress;
            if (ipAddress == ADDRESS_NONE)
            {
                ipAddress = string.Empty;
            }
            IpAddress = ipAddress;
            Port = Properties.Settings.Default.Port;

            string tecIpAddress = Properties.Settings.Default.TecIpAddress;
            if (tecIpAddress == ADDRESS_NONE)
            {
                tecIpAddress = string.Empty;
            }
            TecIpAddress = tecIpAddress;
            TecPort = Properties.Settings.Default.TecPort;
            CutPosition = Properties.Settings.Default.CutPosition;

            SelectedBarcodeComPort = Properties.Settings.Default.BarcodeCOM;
            BarcodeSerialPort.Encoding = Encoding.GetEncoding(932);
            BarcodeSerialPort.DataReceived += BarcodeSerialPort_DataReceived;

            SelectedLabelPrinterComPort = Properties.Settings.Default.LabelPrinterCOM;
            LabelPrinterSerialPort.Encoding = Encoding.GetEncoding(932);
            LabelPrinterSerialPort.BaudRate = 115200;

            CableDataFilePath = Properties.Settings.Default.CableDataCsvPath;
            ProductSetListFilePath = Properties.Settings.Default.ProductSetListCsvPath;

            //印字終了確認処理を行うタイマー
            ConfirmPrintEndTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            ConfirmPrintEndTimer.Tick += ConfirmPrintEndTimer_Tick;
        }

        public async Task ConfirmApiUrlConnection(string apiUrl, bool isShowResultDialog = false)
        {
            try
            {
                manufactApiConnect = new ManufactApiConnect.CablePrintingManufactApiConnect(apiUrl);
                janCodeApiConnect = new JanCodeApiConnect.JanCodeApiConnect(apiUrl);
                await ConnectServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Error", "無効なURLです。");
                return;
            }
            if (isShowResultDialog)
            {
                if (IsConnect)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "接続成功しました。");
                }
            }
            await ConfirmUpdate();
        }

        public async Task ConnectServer(bool reset = true)
        {
            if (reset)
            {
                ResetAllQRInfo();
            }
            var controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "サーバー接続中...");
            await LoadLogClasses();
            controller.SetMessage("バッファデータ確認中...");
            await Task.Run(() =>
            {
                LoadPrintingLogs();
            });
            await controller.CloseWait();
            if (!IsConnect)
            {
                await ShowDisconnectDialog();
            }
            else if (BufferedPrintingLogsLength > 0)
            {
                await DoBufferedLogging();
            }
        }

        public async Task DoBufferedLogging()
        {
            MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "バッファロギング中...");

            foreach (var item in BufferedPrintingLogs)
            {
                await WriteBufferedLog(item);
                if (!IsConnect)
                {
                    break;
                }
                else
                {
                    RemovePrintingLog(item);
                }
            }
            await Task.Run(() =>
            {
                LoadPrintingLogs();
            });
            await controller.CloseWait(300);
        }

        public async Task ConfirmUpdate(bool isShowResultDialog = false)
        {
            if (!IsConnect) return;
            var controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "アップデートを確認中...");
            string downloadUrl = await GetUpdateUrl();
            Console.WriteLine(downloadUrl);
            await controller.CloseWait();
            if (downloadUrl != null)
            {
                if (downloadUrl != string.Empty)
                {
                    Update update = new Update();
                    bool? re = (bool?)await DialogHostEx.ShowDialog(mainWindow, update);
                    if (re.HasValue && re.Value == true)
                    {
                        Process.Start(new ProcessStartInfo(downloadUrl));
                    }
                }
                else if (isShowResultDialog)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "最新のバージョンです");
                }
            }
            else if (IsConnect)
            {
                if (isShowResultDialog)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "このアプリケーションのバージョンはサーバーで管理されていません");
                }
            }
            if (!IsConnect)
            {
                await ShowDisconnectDialog();
            }
        }

        private async Task<string> GetUpdateUrl()
        {
            var ca = await manufactApiConnect.GetConfirmApplication(Title);
            ConfirmApplication = ca;
            if (ca != null)
            {
                try
                {
                    if (new Version(ca.version).CompareTo(new Version(ApplicationInfo.GetVersionContainsAssembly())) > 0)
                    {
                        return ca.download_url_full;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private async Task LoadLogClasses()
        {
            string PrintingClassName = "印字";
            string[] classes = { PrintingClassName };
            await manufactApiConnect.LoadLogClasses(classes);
            IsConnect = manufactApiConnect.IsConnect();
            Console.WriteLine(manufactApiConnect.IsConnect());
        }

        private async Task WriteCablePrintingLog(DateTime created)
        {
            ManufactLog manufactLog = new ManufactLog()
            {
                created = WebApiClient.WebApiClientUtil.ConvertTimestamp(created),
                member = Member
            };
            CablePrintingLog cablePrintingLog = new CablePrintingLog()
            {
                printing_times = PrintingTimes ?? 0,
                redo_times = RedoPrintingTimes ?? 0,
                label_printing_times = LabelPrintingTimes ?? 0
            };
            IsConnect = await manufactApiConnect.WriteCablePrintingLog(BaseInfoLog, manufactLog, cablePrintingLog, CableNumber);
        }

        private async Task WriteBufferedLog(PrintingLog printingLog)
        {
            BaseInfoLog baseInfoLog = new BaseInfoLog()
            {
                order_number = printingLog.OrderNumber,
                order_date = printingLog.OrderDate,
                delivery_date = printingLog.DeliveryDate,
                customer = printingLog.Customer,
                model = printingLog.Model,
                quantity = printingLog.Quantity,
            };
            ManufactLog manufactLog = new ManufactLog()
            {
                created = printingLog.Created,
                member = printingLog.Member
            };
            CablePrintingLog cablePrintingLog = new CablePrintingLog()
            {
                printing_times = printingLog.PrintingTimes,
                redo_times = printingLog.RedoTimes,
                label_printing_times = printingLog.LabelPrintingTimes
            };
            IsConnect = await manufactApiConnect.WriteCablePrintingLog(baseInfoLog, manufactLog, cablePrintingLog, printingLog.CableNumber);
        }

        private async Task<JanModelDetail> FindJanModel(string model)
        {
            if (model == null || model == string.Empty) return null;
            var jm = await janCodeApiConnect.FindJanModel(model);
            IsConnect = janCodeApiConnect.IsConnect();
            return jm;
        }

        private async Task AddPrintingLog(string printingContent, string printingModel)
        {
            var pl = janCodeApiConnect.MakeLabelPrintingLog(DateTime.Now, printingContent, printingModel);
            if (JanModel != null)
            {
                await janCodeApiConnect.AddLabelPrintingLog(pl, JanModel.id);
            }
            else
            {
                await janCodeApiConnect.AddLabelPrintingLog(pl);
            }
            IsConnect = janCodeApiConnect.IsConnect();
        }

        /// <summary>
        /// 製品QRコードからデータ読み込み
        /// </summary>
        /// <param name="aqr"></param>
        private void ShowAQR(string aqr)
        {
            string[] split_qr = LogDefineMethod.leaveDelimiterSplitQR(aqr);
            var lbi = new BaseInfoLog();
            for (int i = 0; i < 6; i++)
            {
                string data = split_qr[i + 2];

                switch (i)
                {
                    case 0:
                        lbi.order_number = data;
                        break;
                    case 1:
                        DateTime orderdate;
                        if (DateTime.TryParse(data, out orderdate))
                        {
                            lbi.order_date = orderdate;
                        }
                        else { lbi.order_date = null; }
                        break;
                    case 2:
                        DateTime deliverydate;
                        if (DateTime.TryParse(data, out deliverydate))
                        {
                            lbi.delivery_date = deliverydate;
                        }
                        else { lbi.delivery_date = null; }
                        break;
                    case 3:
                        lbi.customer = data;
                        break;
                    case 4:
                        lbi.model = data;
                        break;
                    case 5:
                        int qt;
                        if (int.TryParse(data, out qt))
                        {
                            lbi.quantity = qt;
                        }
                        else
                        {
                            lbi.quantity = null;
                        }
                        break;
                }
            }
            BaseInfoLog = lbi;
            OrderNumber = lbi.order_number;
            Quantity = lbi.quantity;
            Model = lbi.model;
            CableNumber = split_qr[10];
            Plate = DataFactory.GetPlate(CableNumber);
            LabelPrintingQuantity = lbi.quantity ?? 0;
        }

        /// <summary>
        /// 製品QRコード読み込みリセット
        /// </summary>
        private void ResetAQR()
        {
            string Aresult = "0,,,,,,,,,,,,,,,,";
            ShowAQR(Aresult);
        }

        /// <summary>
        /// 社員QRコードからデータ読み込み
        /// </summary>
        /// <param name="bqr"></param>
        private void ShowBQR(string bqr)
        {
            string[] qrB = new string[10];
            qrB = bqr.Split(',');
            Member = qrB[5];
        }

        /// <summary>
        /// 社員QRコードの読み込みリセット
        /// </summary>
        private void ResetBQR()
        {
            string Bresult = "0,,,,,,,,";
            ShowBQR(Bresult);
        }

        /// <summary>
        /// すべてのQR情報リセット
        /// </summary>
        private void ResetAllQRInfo()
        {
            ResetAQR();
            ResetBQR();
        }

        //QRコードを受信したときの処理
        private void BarcodeSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);
            SerialPort serialPort = sender as SerialPort;
            string buffer = serialPort.ReadExisting();

            if (DialogIsOpen || SelectedMenuIndex != 0)
            {
                if (serialPort.BytesToRead > 0)
                {
                    serialPort.DiscardInBuffer();//受信バッファの中のデータ
                }
                return;
            }

            ExecQrRead(buffer);

            if (serialPort.BytesToRead > 0)
            {
                serialPort.DiscardInBuffer();//受信バッファの中のデータ
            }
        }

        /// <summary>
        /// AのQRコードを読んだ後の処理
        /// </summary>
        /// <param name="aqr"></param>
        public async void ExecAQrRead(string aqr)
        {
            await ExecAQrReadLog(aqr);
            IsReadStateAQR = false;
        }

        private async Task ExecAQrReadLog(string aqr)
        {
            ShowAQR(aqr);

            //セット品型式である場合に付属品そのものの型式に変換し、
            //ロット番号を付加するかどうかを決定。
            DataTable productSetList = new DataTable();
            await Task.Run(() =>
            {
                productSetList = DataFactory.GetProductSetList();
            });
            PrintContent pc = DataFactory.GetConvertedPrintContent(Model, productSetList);

            string lot = string.Empty;
            if (pc.HasAddedLot)
            {
                lot = DateTime.Now.ToString("ddMMyy");
            }
            string printStr = MakePrintContent(Model, lot);
            Lot = lot;
            PrintingContent = printStr;
            await ExecAQrReadJanCode(Model);
        }

        private async Task ExecAQrReadJanCode(string model)
        {
            var controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "JANコード検索...");
            var jm = await FindJanModel(model);
            controller.Close();

            if (!IsConnect)
            {
                await ShowDisconnectDialog();
            }
            else
            {
                if (jm != null)
                {
                    LabelModel = jm.model;
                    JanModel = jm;
                }
                else
                {
                    LabelModel = model;
                    JanModel = null;
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Caution", $"{Model}はJANコード登録されていません");
                }
            }
        }

        /// <summary>
        /// QRコード別の処理
        /// </summary>
        /// <param name="qr_str"></param>
        private void ExecQrRead(string qr_str)
        {
            if (CompleteButtonEnabled)
            {
                mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Caution", "印字が完了していません\n印字完了ボタンを押すか、クリアボタンを押してから読み込んで下さい");
                }));
                return;
            }
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
                                mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    ExecAQrRead(QRresult);
                                }));
                            }
                            break;
                        case "B":
                            mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                ShowBQR(QRresult);
                            }));
                            break;
                    }
                }
            }
        }

        public async Task<bool> ConnectPrinter()
        {
            string result = string.Empty;
            MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "インクジェットプリンターに接続中...");
            bool success = false;
            await Task.Run(() =>
            {
                inkJetPrinter.Close();
                inkJetPrinter.SetIpAddress(Properties.Settings.Default.IpAddress);
                inkJetPrinter.SetPort(Properties.Settings.Default.Port);
                success = inkJetPrinter.Connect();
            });
            await controller.CloseWait(300);
            InkJetIsConnect = success;
            if (!success)
            {
                await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", "インクジェットプリンターに接続できませんでした");
            }

            return success;
        }

        public void ClosePrinter()
        {
            inkJetPrinter.Close();
        }

        public async Task<bool> OpenTecPrinter()
        {
            bool success = false;
            string status = string.Empty;
            //MaterialProgressDialogController controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "TECラベルプリンターに接続中...");
            await Task.Run(() =>
            {
                string sysPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TecSys");
                success = tecLabelPrinter.OpenPrinterPort(TecIpAddress, TecPort, out status);
            });
            //await controller.CloseWait(300);
            TecPrinterIsConnect = success;

            TecPrinterStatus = status;
            return success;
        }

        public void CloseTecPrinter()
        {
            tecLabelPrinter.ClosePort();
        }

        public async Task CloseTecPrinterAsync()
        {
            await Task.Run(() =>
            {
                tecLabelPrinter.ClosePort();
            });
        }

        private async Task<bool> Issue()
        {
            var controller = await MaterialDialogUtil.ShowMaterialProgressDialog(mainWindow, "プリンターの状態確認...");
            bool isStatusError = false;
            string printerStatus = string.Empty;
            await Task.Run(() =>
            {
                if (tecLabelPrinter.GetStatus(ref printerStatus) == false)
                {
                    isStatusError = true;
                }
                Console.WriteLine(printerStatus);
            });

            if (isStatusError)
            {
                controller.Close();
                //mwvm.Status = "送信失敗";

                await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Error", "送信失敗");
                return false;
            }

            controller.SetMessage("ラベル印刷中...");
            string printStatus = string.Empty;  //プリンタステータス文字列
            bool isError = false;
            tecLabelPrinter.ResetIssueCount();
            int intTotalCnt = 1;

            await Task.Run(() =>
            {
                try
                {
                    int Gap = 30;
                    int LabelHeight = 250;
                    int LabelWidth = 700;
#if DEBUG
                    Gap = 20;
                    LabelHeight = 740;
                    LabelWidth = 1057;
#endif
                    int feed = (Gap + LabelHeight) * 2;
#if DEBUG
                    feed = (Gap + LabelHeight);
#endif
                    int pitch = Gap + LabelHeight;

                    tecLabelPrinter.SendLabelSizeCommand(pitch, LabelWidth, LabelHeight);
                    tecLabelPrinter.SendPrintingPositionMinorAdjustment(0, CutPosition, 0);
                    //tecLabelPrinter.SendReverseFeedCommand(feed);
                    tecLabelPrinter.SendImageBufferClearCommand();

                    tecLabelPrinter.SendStringCommand("PC000;0053,0166,10,10,X,+00,00,B,J0000,+0000000000,P1");
                    if (JanModel != null)
                    {
                        tecLabelPrinter.SendStringCommand("XB00;0314,0084,5,1,04,0,0110,+0000000000,020,1,00");
                    }
                    tecLabelPrinter.SendBitmapFontFormatCommand(3, 350, 70, 1.0f, 1.0f, "j", TecLabelPrinterUtils.StringRotate.Rotate0, "B", Jkkll: "J0101", Pq: "P2");

                    if (LabelDate.HasValue)
                    {
                        tecLabelPrinter.SendBitmapFontDataCommand(0, LabelDate.Value.ToShortDateString());
                    }
                    if (JanModel != null)
                    {
                        tecLabelPrinter.SendBarcodeDataCommand(0, JanModel.jan_code.code);
                    }
                    tecLabelPrinter.SendBitmapFontDataCommand(3, LabelModel);

                    int cut = LabelPrintingQuantity;
                    if(IsCutOneByOne)
                    {
                        cut = 1;
                    }
                    tecLabelPrinter.HasStatusNotification();
                    tecLabelPrinter.SendIssueCommand(LabelPrintingQuantity, cut, TecLabelPrinterUtils.SensorType.Type2,
                        TecLabelPrinterUtils.IssueMode.ModeC, TecLabelPrinterUtils.IssueSpeed.Speed4ips,
                        TecLabelPrinterUtils.Ribbon.Ribbon1, TecLabelPrinterUtils.TagRotation.HipOut,
                        TecLabelPrinterUtils.StatusResponseType.HasResponse);
                    //tecLabelPrinter.SendForwardFeedCommand(feed);

                    //ステータスチェック
                    if (tecLabelPrinter.HasStatusNotification() == true)
                    {
                        Console.WriteLine("HasStatusNotification");
                        if (tecLabelPrinter.IsContinuableStatus(out string message) == false)
                        {   //復帰不可
                            isError = true;
                        }
                        else if (message != string.Empty)
                        {
                            //mwvm.Status = message;
                        }
                    }
                }
                catch (Exception)
                {
                    isError = true;
                }
            });

            if (!isError)
            {
                while (tecLabelPrinter.GetIssueCount() < intTotalCnt)
                {
                    await Task.Delay(10);
                    //ステータスチェック
                    if (tecLabelPrinter.HasStatusNotification() == true)
                    {
                        Console.WriteLine("HasStatusNotification");
                        if (tecLabelPrinter.IsContinuableStatus(out string message) == false)
                        {   //復帰不可
                            isError = true;
                            break;
                        }
                        else if (message != string.Empty)
                        {
                            //mwvm.Status = message;
                        }
                    }
                }
            }

            await controller.CloseWait(300);
            if (isError)
            {
                //mwvm.Status = tecLabelPrinter.GetReceiveStatus();
                await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", $"印刷エラー({tecLabelPrinter.GetReceiveStatus()})");
                return false;
            }

            return true;
        }

        //タイマー1(印字終了確認と印字回数の更新)
        private void ConfirmPrintEndTimer_Tick(object sender, EventArgs e)
        {
            inkJetPrinter.ConfirmPrintEnd(RedoPrintingMode);
            PrintingTimes = inkJetPrinter.GetPrintTimes();
            int rePrintTimes = inkJetPrinter.GetRePrintTimes();
            if (RedoPrintingMode && rePrintTimes > RedoPrintingTimes)
            {
                RedoPrintingTimes = rePrintTimes;
                RedoPrintingMode = false;
            }
        }

        private async Task DoClearTask()
        {
            Console.WriteLine("DoClear");
            await Task.Run(() =>
            {
                //印字データ破棄の送信
                inkJetPrinter.SendClear();
            });

            //表示データの消去
            ClearInfos();

            RedoPrintingMode = false;

            PrintingButtonEnabled = false;
            RedoPrintingButtonEnabled = false;
            CompleteButtonEnabled = false;

            //印字終了確認処理のタイマーを停止
            ConfirmPrintEndTimer.Stop();

            //checkbox:枚数入力
            EditEnable = false;

            //印字終了確認処理の回数をクリア
            inkJetPrinter.ClearTimerCounterForPrint();
            //やり直し回数のクリア
            inkJetPrinter.ClearRePrintTimes();

            isDebug = false;
        }

        public async Task OpenBarcodeSerialPort()
        {
            await OpenSerialPort(BarcodeSerialPort, SelectedBarcodeComPort, "バーコードリーダー");
            SelectedBarcodeComPort = BarcodeSerialPort.PortName;
            SelectBarcodeComPort = SelectedBarcodeComPort;
        }

        public void CloseBarcodeSerialPort()
        {
            Properties.Settings.Default.BarcodeCOM = CloseSerialPort(BarcodeSerialPort);
            Properties.Settings.Default.Save();
        }

        //public async Task OpenLabelPrinterSerialPort()
        //{
        //    await OpenSerialPort(LabelPrinterSerialPort, SelectedLabelPrinterComPort, "ラベルプリンター");
        //    SelectedLabelPrinterComPort = LabelPrinterSerialPort.PortName;
        //    SelectLabelPrinterComPort = SelectedLabelPrinterComPort;
        //}

        //public void CloseLabelPrinterSerialPort()
        //{
        //    Properties.Settings.Default.LabelPrinterCOM = CloseSerialPort(LabelPrinterSerialPort);
        //    Properties.Settings.Default.Save();
        //}

        private async Task OpenSerialPort(SerialPort serialPort, string selectSerialPort, string serialPortName)
        {
            try
            {
                serialPort.Close();
            }
            catch { }

            if (!serialPort.IsOpen)
            {
                if (PortsTable.ContainsKey(selectSerialPort) && selectSerialPort != SERIALPORT_NONE)
                {
                    try
                    {
                        serialPort.PortName = selectSerialPort;
                        serialPort.Open();
                    }
                    catch
                    {
                        serialPort.PortName = SERIALPORT_NONE;
                        await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", $"{serialPortName}のCOMポートが開けません");
                    }
                }
                else
                {
                    serialPort.PortName = SERIALPORT_NONE;
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", $"{serialPortName}のCOMポートが設定されていません");
                }
            }
        }

        private string CloseSerialPort(SerialPort serialPort)
        {
            string re_portName = SERIALPORT_NONE;
            if (serialPort.IsOpen)
            {
                re_portName = serialPort.PortName;
            }
            try
            {
                serialPort.Close();
            }
            catch { }

            return re_portName;
        }

        /// <summary>
        /// COMポート名取得
        /// </summary>
        public void GetPortName()
        {
            Dictionary<string, string> ports = new Dictionary<string, string>();

            ports.Add(SERIALPORT_NONE, "設定なし");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);

                ports.Add((string)s, "Serial Port(" + s + ")");
            }
            PortsTable = ports;
        }

        private void ClearInfos()
        {
            ResetAllQRInfo();
            PrintingContent = string.Empty;
            PrintingTimes = null;
            RedoPrintingTimes = null;
            LabelPrintingTimes = null;
            Lot = string.Empty;
            JanModel = null;
            LabelModel = string.Empty;
        }

        private void LoadPrintingLogs()
        {
            BufferedPrintingLogs = printingLogConnect.GetPrintingLogs();
        }

        private void AddPrintingLog(DateTime created)
        {
            var pl = new PrintingLog()
            {
                OrderNumber = BaseInfoLog.order_number,
                OrderDate = BaseInfoLog.order_date,
                DeliveryDate = BaseInfoLog.delivery_date,
                Customer = BaseInfoLog.customer,
                Model = BaseInfoLog.model,
                Quantity = BaseInfoLog.quantity,
                Created = WebApiClient.WebApiClientUtil.ConvertTimestamp(created),
                Member = Member,
                PrintingTimes = PrintingTimes ?? 0,
                RedoTimes = RedoPrintingTimes ?? 0,
                LabelPrintingTimes = LabelPrintingTimes ?? 0,
                CableNumber = CableNumber
            };
            printingLogConnect.AddPrintingLog(pl);
        }

        private void RemovePrintingLog(PrintingLog printingLog)
        {
            printingLogConnect.RemovePrintingLog(printingLog);
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
            await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Warning", "サーバーへの接続が失敗しました\nネットワーク接続の確認と\nサーバー管理者に確認してください");
        }
    }
}
