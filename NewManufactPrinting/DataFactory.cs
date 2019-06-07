using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.ComponentModel;
using System.Windows;

namespace NewManufactPrinting
{
    class DataFactory
    {
        public static byte[] ConfirmLabelNumCmd = { 0x1B, 0x69, 0x58, 0x43, 0x31, 0x00, 0x00 };
        
        //印字データ2()
        public static byte[] MakeSendDataPrint(string model, string printLot)
        {
            byte[] sendData = new byte[0];

            byte[] Headder = { };
            byte[] IDENTIFIER = { 0xE3 };
            byte[] LENGTH_Data = { 0x00, 0x00 };
            //ヘッダー作成
            byte[] messagenumber = { };
            byte[] title = {
                           };
            byte[] ingecater = { 0x40, 0x00 };
            byte[] massageparameter = { };
            byte[] multiTOP = { };
            byte[] Encodervalue = { };
            byte[] beforemarjin = { };
            byte[] afteremarjin = { };
            byte[] sebraltimesinterval = { };
            byte[] spi = { };
            byte[] algorithm = { };
            byte[] counterType = { };
            byte[] counterUP = { };
            byte[] counterSTART = {
                                  };
            byte[] counterFINAL = {
                                  };
            byte[] counterSTEP = { };
            byte[] batchcounter = { };

            Headder = messagenumber.Concat(title).ToArray();
            Headder = Headder.Concat(title).ToArray();
            Headder = Headder.Concat(ingecater).ToArray();
            Headder = Headder.Concat(massageparameter).ToArray();
            Headder = Headder.Concat(multiTOP).ToArray();
            Headder = Headder.Concat(Encodervalue).ToArray();
            Headder = Headder.Concat(beforemarjin).ToArray();
            Headder = Headder.Concat(afteremarjin).ToArray();
            Headder = Headder.Concat(sebraltimesinterval).ToArray();
            Headder = Headder.Concat(spi).ToArray();
            Headder = Headder.Concat(algorithm).ToArray();
            Headder = Headder.Concat(counterType).ToArray();
            Headder = Headder.Concat(counterUP).ToArray();
            Headder = Headder.Concat(counterSTART).ToArray();
            Headder = Headder.Concat(counterFINAL).ToArray();
            Headder = Headder.Concat(counterSTEP).ToArray();
            Headder = Headder.Concat(batchcounter).ToArray();

            //テキストデータ生成
            byte[] Linestart = { 0x0A };
            byte[] position1 = { 0x80, 0x09 };
            //byte[] position2 = { };
            //byte[] position3 = { };
            byte[] Fontsize1 = { 0x60 };
            //byte[] Fontsize2 = { };
            //byte[] Fontsize3 = { };
            byte[] Bold = { 0x01 };
            byte[] textstart = { 0x10 };
            byte[] textData1 = System.Text.Encoding.ASCII.GetBytes(model);
            byte[] textSP = { 0x20 };
            byte[] textend = { 0x10 };
            byte[] LineEND = { 0x0D };

            //テキストブロック１生成(型式）
            byte[] textBlock1 = { };
            textBlock1 = textBlock1.Concat(Linestart).ToArray();
            textBlock1 = textBlock1.Concat(position1).ToArray();
            textBlock1 = textBlock1.Concat(Fontsize1).ToArray();
            textBlock1 = textBlock1.Concat(Bold).ToArray();
            textBlock1 = textBlock1.Concat(textstart).ToArray();
            textBlock1 = textBlock1.Concat(textData1).ToArray();
            textBlock1 = textBlock1.Concat(textSP).ToArray();

            //テキストブロック２生成(ロット番号)
            byte[] textBlock2 = { };

            if (printLot != string.Empty)
            {
                //pd._lot = lotnum;
                textBlock2 = textBlock2.Concat
                      (System.Text.Encoding.ASCII.GetBytes
                      (printLot)).ToArray();
                textBlock2 = textBlock2.Concat(textSP).ToArray();
            }
            else
            {
                //pd._lot = " ";
            }

            //印字データ作成

            //テキストブロック３生成(社名)
            byte[] textBlock3 = { };

            textBlock3 = textBlock3.Concat
                (System.Text.Encoding.ASCII.GetBytes(" Diatrend Corp.")).ToArray();
            textBlock3 = textBlock3.Concat(textSP).ToArray();

            textBlock3 = textBlock3.Concat(textend).ToArray();
            textBlock3 = textBlock3.Concat(Bold).ToArray();
            textBlock3 = textBlock3.Concat(Fontsize1).ToArray();
            textBlock3 = textBlock3.Concat(position1).ToArray();
            textBlock3 = textBlock3.Concat(LineEND).ToArray();

            byte[] TypeOfWriting = { 0x01 };//書き込みタイプ

            byte[] printData = { };
            printData = printData.Concat(Headder).ToArray();
            printData = printData.Concat(textBlock1).ToArray();
            printData = printData.Concat(textBlock2).ToArray();
            printData = printData.Concat(textBlock3).ToArray();

            LENGTH_Data[1] =
                BitConverter.GetBytes(printData.Length)[0];

            byte[] FinallyData = { };

            FinallyData = FinallyData.Concat(IDENTIFIER).ToArray();
            FinallyData = FinallyData.Concat(LENGTH_Data).ToArray();
            FinallyData = FinallyData.Concat(printData).ToArray();

            sendData = sendData.Concat(FinallyData).ToArray();
            sendData = sendData.Concat(CalcCheckSum(sendData)).ToArray();

            return sendData;
        }


        //印字データの破棄のためのリクエストデータ
        public static byte[] MakeSendDataForClear()
        {
            byte[] sendData = new byte[]{  0xE3,0x00,0x11,0x40,0x00,0x0A,0x80,
                                              0x09,0x0D,0x01,0x10,0x20,0x20,0x20,
                                              0x10,0x01,0x0D,0x80,0x09,0x0D
                                               };
            sendData = sendData.Concat(CalcCheckSum(sendData)).ToArray();

            return sendData;
        }

        public static byte[] MakeSendDataForAcknowledgeEnd()
        {
            //印字終了確認リクエストデータを生成
            byte[] sendData = { 0x41, 0x00, 0x01, 0x01 };
            sendData = sendData.Concat(DataFactory.CalcCheckSum(sendData)).ToArray();
            return sendData;
        }
        public static byte[] MakeSendDataForIssuePrintEnd()
        {
            //印字終了通知を送ってもらえるようにする
            //リクエストデータを生成
            byte[] sendData = { 0xE7, 0x00, 0x01, 0x01 };
            sendData = sendData.Concat(DataFactory.CalcCheckSum(sendData)).ToArray();
            return sendData;
        }

        public static string MakeSaveDataString(string data1, string data2, string[] data3)
        {
            string saveData = "";
            saveData += "*";
            saveData += data1;
            saveData += ",";
            saveData += data2;

            for (int i = 0; i < data3.Length; i++)
            {
                saveData += "," + data3[i];
            }
            return saveData;
        }

        //ラベルプリンタ用送信データ文字列作成(ラベル印刷依頼)
        public static byte[] MakeSendDataForLabel(string model, string lot, int num_of_label)
        {
            byte[] send = new byte[0];
            byte[] period = new byte[1] { 0x09 };

            string num_str = "^CN";
            string init_str = "^II";
            string model_str = model;
            string trigger_str = "^FF";

            num_str += String.Format("{0:D3}", num_of_label);

            send = System.Text.Encoding.ASCII.GetBytes(init_str + num_str + model_str);
            send = send.Concat(period).ToArray();

            byte[] bt = System.Text.Encoding.ASCII.GetBytes(lot + trigger_str);
            send = send.Concat(bt).ToArray();

            return send;
        }

        //セット品型式を付属品そのものの型式に変換
        //( 例:DIFC-U4(S75)の付属品の場合
        //    　　　S75　　　　　→　　DAD01R4H  のように変換
        //    (QRコード上の型式)　　 (セット品そのものの型式)
        //  のように変換)
        public static PrintContent GetConvertedPrintContent(string model, DataTable productSetList)
        {
            int lt = model.Length;
            PrintContent printContent = new PrintContent
            {
                Model = model,
                HasAddedLot = true
            };
            try
            {
                var setproduct = from data in productSetList.AsEnumerable()
                             where model.Contains((string)data.ItemArray[1])
                             select data;
            
                if (setproduct.Count() > 0)
                {
                    object[] objs = setproduct.ToArray()[0].ItemArray;
                    string conversionModel = objs[0].ToString();
                    string containModel = objs[1].ToString();
                    int len = containModel.Length;
                    string forwardMatchModel = model.Substring(0, len);

                    if (forwardMatchModel == containModel)
                    {
                        printContent.HasAddedLot = false;

                        if (conversionModel == "*")
                        {
                            printContent.Model = model;
                        }
                        else
                        {
                            printContent.Model = conversionModel;
                        }
                    }
                }
            }
            catch
            {
            }
            return printContent;
        }

        public static DataTable GetProductSetList()
        {
            string path = Properties.Settings.Default.ProductSetListCsvPath;
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            return CsvController.GetProductSetListDataTable(dir, fileName);
        }

        public static string GetPlate(string cableNumber)
        {
            return CsvController.GetPlate(cableNumber, Properties.Settings.Default.CableDataCsvPath);
        }

        //データのチェックサムを計算
        private static byte[] CalcCheckSum(byte[] date)
        {
            int exclusive_or = 0x00;
            byte[] checksum = { 0x00 };
            for (int i = 0; i < date.Length; i++)
            {
                exclusive_or ^= date[i];
            }

            checksum[0] = BitConverter.GetBytes(exclusive_or)[0];
            return checksum;
        }

    }

    public struct PrintContent
    {
        public string Model;
        public bool HasAddedLot;
    }
}
