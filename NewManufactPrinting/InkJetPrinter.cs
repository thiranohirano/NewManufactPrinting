using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NewManufactPrinting
{
    public class InkJetPrinter
    {
        TcpClient client = null;
        string ipAddress;
        int port;
        private int TimerCounter;
        private int PrintingTimes;
        private int RedoPrintingTimes;

        public InkJetPrinter()
        {

        }

        public void SetIpAddress(string ipAddress)
        {
            this.ipAddress = ipAddress;
        }

        public void SetPort(int port)
        {
            this.port = port;
        }

        public bool Connect()
        {
            client = new TcpClient();

            try
            {
                client.Connect(ipAddress, port);
                SendIssuePrintEnd();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Close()
        {
            if (client == null) return;
            try
            {
                client.Close();
            }
            catch (Exception)
            {
            }
        }

        public bool SendPrinting(string model, string printLot)
        {
            byte[] senddata = DataFactory.MakeSendDataPrint(model, printLot);
            return SendDataToPrinter(senddata);
        }

        public void SendClear()
        {
            byte[] senddata = DataFactory.MakeSendDataForClear();
            SendDataToPrinter(senddata);
        }

        //印字終了確認リクエストを送信(このリクエストを印字前に送信すると
        //印字終了時にプリンターから印字終了通知(ヘッダーがE7のデータ)
        //がくる。ただし、印字を行うたびに印字終了通知を送ってもらうためには
        //毎回印字前にこのリクエストを送らなければならない。)
        public void SendAcknowledgeEnd()
        {
            //印字終了確認リクエストデータを生成
            byte[] senddata = DataFactory.MakeSendDataForAcknowledgeEnd();
            //印字終了確認リクエストを送信
            SendDataToPrinter(senddata);
        }

        //プリンターに印字終了確認リクエストを送ると、
        //印字終了時に印字終了通知を送ってもらうように依頼する
        //リクエストを送信。
        public void SendIssuePrintEnd()
        {
            //印字終了通知を送ってもらえるようにするリクエストデータを生成
            byte[] senddata = DataFactory.MakeSendDataForIssuePrintEnd();
            SendDataToPrinter(senddata);
        }

        //印字終了かどうかを確認 
        public void ConfirmPrintEnd(bool rePrintMode)
        {
            if (TimerCounter == 0)
            {
                SendAcknowledgeEnd();
            }
            try
            {
                if (client.Available >= 2)
                {
                    Byte[] dat = new Byte[client.Available];
                    var stream = client.GetStream();
                    stream.Read(dat, 0, dat.GetLength(0));
                    if (dat.Length >= 2)
                    {
                        if (dat[1] == 0xE7)
                        {
                            //受け取ったデータが終了通知である場合
                            PrintingTimes += 1;
                            if (rePrintMode)
                            {
                                RedoPrintingTimes += 1;
                            }
                            SendAcknowledgeEnd();
                        }
                    }
                }
            }
            catch { }
            TimerCounter += 1;
        }

        //印字回数を取得
        public int GetPrintTimes()
        {
            return PrintingTimes;
        }

        //再印字回数を取得
        public int GetRePrintTimes()
        {
            return RedoPrintingTimes;
        }

        public void ClearRePrintTimes()
        {
            RedoPrintingTimes = 0;
        }

        //印字回数をクリア
        public void ClearPrintTimes()
        {
            PrintingTimes = 0;
        }

        //タイマーカウントをクリア
        public void ClearTimerCounterForPrint()
        {
            TimerCounter = 0;
        }

        //プリンターにデータ送信
        private bool SendDataToPrinter(byte[] data)
        {
            if (client == null) return false;
            try
            {
                var stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
