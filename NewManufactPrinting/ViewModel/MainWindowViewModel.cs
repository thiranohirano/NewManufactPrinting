using ManufactApiClient.Models;
using MaterialDialog;
using MyUtilityMethods;
using NewManufactPrinting.BufferedLog;
using NewManufactPrinting.BufferedLog.Entities;
using NewManufactPrinting.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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

        private string _Member = string.Empty;
        /// <summary>
        /// 製作者名
        /// </summary>
        public string Member
        {
            get
            {
                return _Member;
            }
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

        private string _Model = string.Empty;
        /// <summary>
        /// 型式
        /// </summary>
        public string Model
        {
            get
            {
                return _Model;
            }
            set
            {
                this.SetProperty(ref _Model, value);
            }
        }

        private int? _quantity;
        /// <summary>
        /// 本数
        /// </summary>
        public int? Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                this.SetProperty(ref _quantity, value);
            }
        }

        private string _Plate = string.Empty;
        /// <summary>
        /// 板番号
        /// </summary>
        public string Plate
        {
            get
            {
                return _Plate;
            }
            set
            {
                this.SetProperty(ref _Plate, value);
            }
        }

        private string CableNumber;
        private string _Lot = string.Empty;
        /// <summary>
        /// Lot番号
        /// </summary>
        public string Lot
        {
            get
            {
                return _Lot;
            }
            set
            {
                this.SetProperty(ref _Lot, value);
            }
        }

        private string _PrintingContent = string.Empty;
        /// <summary>
        /// 印字内容
        /// </summary>
        public string PrintingContent
        {
            get
            {
                return _PrintingContent;
            }
            set
            {
                this.SetProperty(ref _PrintingContent, value);
            }
        }

        private int? _PrintingTimes;
        /// <summary>
        /// 印字回数
        /// </summary>
        public int? PrintingTimes
        {
            get
            {
                return _PrintingTimes;
            }
            set
            {
                this.SetProperty(ref _PrintingTimes, value);
            }
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
            get
            {
                return _RedoPrintingTimes;
            }
            set
            {
                this.SetProperty(ref _RedoPrintingTimes, value);
            }
        }

        private bool _RedoPrintingMode = false;
        /// <summary>
        /// やり直しモード
        /// </summary>
        public bool RedoPrintingMode
        {
            get { return _RedoPrintingMode; }
            set
            {
                if (value)
                {
                    RedoPrintingButtonText = RedoDuringString;
                    //RePrintButtonEnabled = false;
                }
                else
                {
                    RedoPrintingButtonText = RedoString;
                    //RePrintButtonEnabled = true;
                }
                _RedoPrintingMode = value;
            }
        }

        private static string RedoString = "やり直し";
        private static string RedoDuringString = "やり直し中";

        private string _RedoPrintingButtonText = RedoString;
        public string RedoPrintingButtonText
        {
            get { return _RedoPrintingButtonText; }
            set { this.SetProperty(ref _RedoPrintingButtonText, value); }
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
            get
            {
                return _LabelPrintingTimes;
            }
            set
            {
                this.SetProperty(ref _LabelPrintingTimes, value);
            }
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
            get
            {
                return _LabelPrintingQuantity;
            }
            set
            {
                if(value == 0)
                { 
                    LabelPrintingButtonEnabled = false;
                }
                this.SetProperty(ref _LabelPrintingQuantity, value);
            }
        }

        private bool _CompleteButtonEnabled = false;
        public bool CompleteButtonEnabled
        {
            get { return _CompleteButtonEnabled; }
            set { this.SetProperty(ref _CompleteButtonEnabled, value); }
        }

        private string _StartStopButtonText = START_TEXT;
        public string StartStopButtonText
        {
            get { return _StartStopButtonText; }
            set { this.SetProperty(ref _StartStopButtonText, value); }
        }

        private string _StopWatchText = STOPWATCH_RESET_TEXT;
        public string StopWatchText
        {
            get { return _StopWatchText; }
            set { this.SetProperty(ref _StopWatchText, value); }
        }

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

        private string _ServerUrl = string.Empty;
        public string ServerUrl
        {
            get { return _ServerUrl; }
            set { this.SetProperty(ref _ServerUrl, value); }
        }

        private bool _IsConnect = false;
        public bool IsConnect
        {
            get { return _IsConnect; }
            set
            {
                ConnectState = value ? "接続成功" : "接続エラー";
                ConnectStateColor = value ? Brushes.LightGreen : Brushes.Red;
                this.SetProperty(ref _IsConnect, value);
            }
        }

        private string _ConnectState = "接続エラー";
        public string ConnectState
        {
            get { return _ConnectState; }
            set { this.SetProperty(ref _ConnectState, value); }
        }

        private Brush _ConnectStateColor = Brushes.Red;
        public Brush ConnectStateColor
        {
            get { return _ConnectStateColor; }
            set { this.SetProperty(ref _ConnectStateColor, value); }
        }

        private bool _InkJetIsConnect = false;
        public bool InkJetIsConnect
        {
            get { return _InkJetIsConnect; }
            set
            {
                InkJetConnectState = value ? "接続成功" : "接続エラー";
                InkJetConnectStateColor = value ? Brushes.LightGreen : Brushes.Red;
                this.SetProperty(ref _InkJetIsConnect, value);
            }
        }

        private string _InkJetConnectState = "接続エラー";
        public string InkJetConnectState
        {
            get { return _InkJetConnectState; }
            set { this.SetProperty(ref _InkJetConnectState, value); }
        }

        private Brush _InkJetConnectStateColor = Brushes.Red;
        public Brush InkJetConnectStateColor
        {
            get { return _InkJetConnectStateColor; }
            set { this.SetProperty(ref _InkJetConnectStateColor, value); }
        }

        private ObservableCollection<PrintingLog> _BufferedPrintingLogs = new ObservableCollection<PrintingLog>();
        public ObservableCollection<PrintingLog> BufferedPrintingLogs
        {
            get { return _BufferedPrintingLogs; }
            set {
                int? count = null;
                if(value != null)
                {
                    if(value.Count > 0)
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


        public bool DialogIsOpen
        {
            get; set;
        }

        public string[] ServerUrls { get { return new string[] { "http://192.168.100.50/", "http://192.168.100.51/" }; } }

        private const string START_TEXT = "START";
        private const string STOP_TEXT = "STOP";
        private const string STOPWATCH_RESET_TEXT = "00:00:00";
        private const string SERIALPORT_NONE = "NONE";
        private const string CABLE_DATA_CSV_FILE = "ケーブル番号ー外形.csv";
        private const string PRODUCT_SET_LIST_CSV_FILE = "セット品型式リスト.csv";
        private MainWindow mainWindow;
        private ManufactApiConnect.CablePrintingManufactApiConnect manufactApiConnect;
        private PrintingLogConnect printingLogConnect;
        private Stopwatch stopwatch = new Stopwatch();
        public SerialPort BarcodeSerialPort = new SerialPort(SERIALPORT_NONE); //バーコードリーダー通信シリアルポート
        public SerialPort LabelPrinterSerialPort = new SerialPort(SERIALPORT_NONE); //ラベルプリンター通信シリアルポート
        private string AQRCode;

        public  ICommand RedoPrint { get; private set; }
        public ICommand Plus { get; private set; }
        public ICommand Minus { get; private set; }

        public ICommand StopWatchStartStop { get; private set; }
        public ICommand StopWatchReset { get; private set; }

        public ICommand ServerUrlSettingApply { get; private set; }

        public ICommand InkJetPrinterSettingApply { get; private set; }

        public ICommand CableDataFilePathDesktopSet { get; private set; }
        public ICommand ProductSetListFilePathDesktopSet { get; private set; }

        public ICommand CableDataFilePathSettingApply { get; private set; }
        public ICommand ProductSetListFilePathSettingApply { get; private set; }

        public ICommand PortsReflesh { get; private set; }
        public ICommand BarcodeSerialPortSettingApply { get; private set; }
        public ICommand LabelPrinterSerialPortSettingApply { get; private set; }

        public MainWindowViewModel()
        {
            mainWindow = (MainWindow)Application.Current.MainWindow as MainWindow;
            printingLogConnect = new PrintingLogConnect();
            RedoPrint = new DelegateCommand((o) =>
            {
                RedoPrintingMode = !RedoPrintingMode;
            });

            Plus = new DelegateCommand((o) =>
            {
                LabelPrintingQuantity++;
            });

            Minus = new DelegateCommand((o) =>
            {
                if (LabelPrintingQuantity > 1) LabelPrintingQuantity--; 
            });

            StopWatchStartStop = new DelegateCommand((o) =>
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                    StartStopButtonText = START_TEXT;
                }
                else
                {
                    stopwatch.Start();
                    StartStopButtonText = STOP_TEXT;
                }
            });

            StopWatchReset = new DelegateCommand((o) =>
            {
                stopwatch.Reset();
                StartStopButtonText = START_TEXT;
            });

            ServerUrlSettingApply = new DelegateCommand(async (o) =>
            {
#if !DEBUG
                string api_url = "NONE";
                if(ServerUrl != "NONE")
                {
                    api_url = ServerUrl;
                }
                Properties.Settings.Default.ServerUrl = api_url;
                Properties.Settings.Default.Save();
#endif

                IsConnect = false;
                SetApiServer(ServerUrl);
                await mainWindow.ConnectServer(false);
                if(IsConnect)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "サーバーに接続できました");
                }
            });

            InkJetPrinterSettingApply = new DelegateCommand(async (o) =>
            {
                if (IpAddress == string.Empty) IpAddress = "NONE";
                Properties.Settings.Default.IpAddress = IpAddress;
                Properties.Settings.Default.Port = Port;
                Properties.Settings.Default.Save();
                bool success = await mainWindow.ConnectPrinter();
                if(success)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", "インクジェットプリンターに接続できました");
                }
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

            LabelPrinterSerialPortSettingApply = new DelegateCommand(async (o) =>
            {
                SelectedLabelPrinterComPort = SelectLabelPrinterComPort;
                await OpenLabelPrinterSerialPort();
                if (SelectedLabelPrinterComPort != SERIALPORT_NONE)
                {
                    await MaterialDialogUtil.ShowMaterialMessageDialog(mainWindow, "Info", $"{SelectedLabelPrinterComPort}で開きました");
                }
            });

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
        }

        public void SetApiServer(string server)
        {
            manufactApiConnect = new ManufactApiConnect.CablePrintingManufactApiConnect(server);
        }

        public async Task LoadLogClasses()
        {
            string PrintingClassName = "印字";
            string[] classes = { PrintingClassName };
            await manufactApiConnect.LoadLogClasses(classes);
            IsConnect = manufactApiConnect.IsConnect();
            Console.WriteLine(manufactApiConnect.IsConnect());
        }

        public async Task WriteCablePrintingLog(DateTime created)
        {
            ManufactLog manufactLog = new ManufactLog()
            {
                created = created,
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

        public async Task WriteBufferedLog(PrintingLog printingLog)
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

        public async Task<string> GetUpdateUrl()
        {
            Console.WriteLine("GetUpdateUrl");
            Console.WriteLine(ApplicationInfo.GetVersionContainsAssembly());
            var ca = await manufactApiConnect.GetConfirmApplication(Title);
            if (ca != null)
            {
                if (new Version(ca.version).CompareTo(new Version(ApplicationInfo.GetVersionContainsAssembly())) > 0)
                {
                    return ServerUrl + ca.download_url;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 製品QRコードからデータ読み込み
        /// </summary>
        /// <param name="aqr"></param>
        public void ShowAQR(string aqr)
        {
            AQRCode = aqr;
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
        public void ResetAQR()
        {
            string Aresult = "0,,,,,,,,,,,,,,,,";
            ShowAQR(Aresult);
        }

        /// <summary>
        /// 社員QRコードからデータ読み込み
        /// </summary>
        /// <param name="bqr"></param>
        public void ShowBQR(string bqr)
        {
            string[] qrB = new string[10];
            qrB = bqr.Split(',');
            Member = qrB[5];
        }

        /// <summary>
        /// 社員QRコードの読み込みリセット
        /// </summary>
        public void ResetBQR()
        {
            string Bresult = "0,,,,,,,,";
            ShowBQR(Bresult);
        }

        /// <summary>
        /// すべてのQR情報リセット
        /// </summary>
        public void ResetAllQRInfo()
        {
            ResetAQR();
            ResetBQR();
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

        public async Task OpenLabelPrinterSerialPort()
        {
            await OpenSerialPort(LabelPrinterSerialPort, SelectedLabelPrinterComPort, "ラベルプリンター");
            SelectedLabelPrinterComPort = LabelPrinterSerialPort.PortName;
            SelectLabelPrinterComPort = SelectedLabelPrinterComPort;
        }

        public void CloseLabelPrinterSerialPort()
        {
            Properties.Settings.Default.LabelPrinterCOM = CloseSerialPort(LabelPrinterSerialPort);
            Properties.Settings.Default.Save();
        }

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

        public void ClearInfos()
        {
            ResetAllQRInfo();
            PrintingContent = string.Empty;
            PrintingTimes = null;
            RedoPrintingTimes = null;
            LabelPrintingTimes = null;
            Lot = string.Empty;
        }

        public void LoadPrintingLogs()
        {
            BufferedPrintingLogs = printingLogConnect.GetPrintingLogs();
        }

        public void AddPrintingLog(DateTime created)
        {
            var pl = new PrintingLog()
            {
                OrderNumber = BaseInfoLog.order_number,
                OrderDate = BaseInfoLog.order_date,
                DeliveryDate = BaseInfoLog.delivery_date,
                Customer = BaseInfoLog.customer,
                Model = BaseInfoLog.model,
                Quantity = BaseInfoLog.quantity,
                Created = created,
                Member = Member,
                PrintingTimes = PrintingTimes ?? 0,
                RedoTimes = RedoPrintingTimes ?? 0,
                LabelPrintingTimes = LabelPrintingTimes ?? 0,
                CableNumber = CableNumber
            };
            printingLogConnect.AddPrintingLog(pl);
        }

        public void RemovePrintingLog(PrintingLog printingLog)
        {
            printingLogConnect.RemovePrintingLog(printingLog);
        }
    }
}
