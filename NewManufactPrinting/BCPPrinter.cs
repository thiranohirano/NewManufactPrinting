using System;
using System.Net.NetworkInformation;
using TEC.BCP.Windows.Components;

namespace NewManufactPrinting
{
    public class BCPPrinter : BCPControl
    {
        public const int BA400T_T_Printer = 38;
        public const long ReceivedStatusResult = 0x800A044E;
        public const long CommunicationErrorResult = 0x800A044D;
        public const string EndedIssuingNormallyStatus = "40";
        public const string EndedFeedingeNormallyStatus = "41";

        private bool bStatus;
        private long lngResult;
        private string printStatus;
        private int intIssueCnt;
        private bool isOpen;

        public BCPPrinter()
        {
            this.OnStatus += BCPControl_OnStatus;
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public int GetIssueCount()
        {
            return intIssueCnt;
        }

        public void SetIssueCount(int count)
        {
            intIssueCnt = count;
        }

        public void ResetIssueCount()
        {
            intIssueCnt = 0;
        }

        public long Result()
        {
            return lngResult;
        }

        public bool HasStatusNotification()
        {
            bool re = bStatus;
            bStatus = false;
            return re;
        }

        public bool OpenBA400TTPrinterPort(string sysPath, string ipAddress, int port, out string strStatus)
        {
            return OpenPrinterPort(sysPath, BA400T_T_Printer, ipAddress, port, out strStatus);
        }

        public bool OpenPrinterPort(string sysPath, int usePrinter, string ipAddress, int port, out string strStatus)
        {
            this.SystemPath = sysPath;
            this.UsePrinter = usePrinter;
            this.PortSetting = $"TCP://{ipAddress}:{port}";
            bool isOpenError = false;
            if (isOpen)
            {
                ClosePortWaitingClearBuffer();
            }

            if (!ConfirmPing(ipAddress))
            {
                strStatus = "Pingエラー";
                return false;
            }

            //ポートのオープン（送信完了復帰）
            if (this.OpenPort(1, out lngResult) == false)
            {
                isOpenError = true;
            }
            else
            {
                isOpen = true;
            }


            if (isOpenError)
            {
                strStatus = GetStatusMessage();
                return false;
            }
            else
            {
                strStatus = "接続しました";
            }
            return true;
        }

        public bool ClosePortWaitingClearBuffer()
        {
            isOpen = false;
            int Count;
            do
            {
                this.GetSendBuffCount(out Count, out _);
            } while (Count > 0);

            return this.ClosePort(out lngResult);
        }

        public string GetStatusMessage()
        {
            string strMsg = string.Empty;

            bool bRet = this.GetMessage(lngResult, ref strMsg);
            if (bRet)
            {
                return strMsg;
            }
            else
            {
                string strAbortMsg = String.Empty;
                this.GetMessage("IDS_ISSUE_ABORT", ref strAbortMsg);
                return strAbortMsg;
            }
        }

        //ステータスチェック
        public bool IsContinuableStatus(out string message)
        {
            bool bContinue = false;	//true:継続可能、false:継続不可
            string strMsg = string.Empty;	//メッセージ
            string strStatus;	//プリンタ詳細ステータス

            bool bRet = this.GetMessage(lngResult, ref strMsg);
            if (bRet == false)
            {
                string sErrCodeMsg = String.Empty;
                this.GetMessage("IDS_ERR_ERRCODE", ref sErrCodeMsg);
                strMsg = String.Format(sErrCodeMsg, lngResult);
            }

            if (lngResult == ReceivedStatusResult)
            {
                //プリンタからステータスを受信
                //ここではプリンタからのステータスについては継続可能としていますが、
                //一部エラーについては継続不可です。
                bContinue = true;

                strStatus = printStatus.Substring(0, 2);
                switch (strStatus)
                {
                    case EndedIssuingNormallyStatus: //印刷完了
                    case EndedFeedingeNormallyStatus: //フィード完了
                    case "00": //オンライン
                    case "02": //動作中
                        message = strMsg;
                        return true;
                    default:
                        string strErrStatsuMsg = String.Empty;
                        this.GetMessage("ERR_STATUS", ref strErrStatsuMsg);
                        string strErrRecoveryMsg = String.Empty;
                        this.GetMessage("IDS_REQUEST_RECOVERY", ref strErrRecoveryMsg);
                        strMsg = String.Format(strErrStatsuMsg + "\n" + strErrRecoveryMsg, printStatus);
                        break;
                }
            }

            message = strMsg;

            return bContinue;
        }

        public bool LoadLfmFile(string filePathName)
        {
            return this.LoadLfmFile(filePathName, out lngResult);
        }

        public bool SetObjectData(int objectNo, string objectData)
        {
            return this.SetObjectData(objectNo, objectData, out lngResult);
        }

        public bool InsertData(int position, byte[] binary, int length)
        {
            return this.InsertData(position, binary, length, out lngResult);
        }

        public bool Issue(int issueCnt, int cutInterval, ref string printerStatus)
        {
            return this.Issue(issueCnt, cutInterval, ref printerStatus, out lngResult);
        }

        public bool GetStatus(ref string printerStatus)
        {
            return this.GetStatus(ref printerStatus, out lngResult);
        }

        public string GetCaptionContinueMessage()
        {
            return GetMessage("IDS_CAPTION_CONTINUE");
        }

        public string GetIssueCompleteMessage()
        {
            return GetMessage("IDS_ISSUE_COMPLETE");
        }

        private string GetMessage(string printerStatus)
        {
            string strLabelStatus = string.Empty;
            this.GetMessage(printerStatus, ref strLabelStatus);
            return strLabelStatus;
        }

        private void BCPControl_OnStatus(string PrinterStatus, long Result)
        {
            bStatus = true;
            lngResult = Result;

            //受信ステータス情報の判定
            switch (lngResult)
            {
                case ReceivedStatusResult:
                    printStatus = PrinterStatus;
                    //プリンタからステータスを受信した場合
                    switch (printStatus.Substring(0, 2))
                    {
                        case EndedIssuingNormallyStatus:
                            //ラベル発行が正常に終了した場合
                            intIssueCnt += 1;
                            break;
                        case EndedFeedingeNormallyStatus:
                            //フィードが正常に終了した場合
                            break;
                        default:
                            //その他の場合
                            break;
                    }
                    break;
                case CommunicationErrorResult:
                    //ケーブル切断、電源断などにより通信エラーが発生した場合
                    break;
                default:
                    //ポートへの書き込みに失敗した場合
                    break;
            }
        }

        public bool ConfirmPing(string ipAddress)
        {
            try
            {
                /* Pingで確認 */
                Ping p = new Ping();
                PingReply reply = p.Send(ipAddress, 1000);
                p.Dispose();
                if (reply.Status != IPStatus.Success)
                {
                    throw new Exception("Ping Error");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
