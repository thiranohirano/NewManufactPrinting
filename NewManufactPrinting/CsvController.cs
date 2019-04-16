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
using ManufactApiClient.Models;

namespace NewManufactPrinting
{
    public class CsvController
    {
        public static string GetPlate(string cableNumber, string csvFilePath)
        {
            string boardNumber = string.Empty;
            try
            {
                // csvファイルを開く
                using (var sr = new StreamReader(csvFilePath))
                {
                    // ストリームの末尾まで繰り返す
                    while (!sr.EndOfStream)
                    {
                        // ファイルから一行読み込む
                        var line = sr.ReadLine();
                        string[] data = line.Split(',');

                        if (data[0].ToString() == cableNumber)
                        {
                            Console.Write("{0}\n ", data[2]);
                            boardNumber = data[2];
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                Console.WriteLine(e.Message);
            }
            return boardNumber;
        }
        public static DataTable GetProductSetListDataTable(string csvDir, string csvFileName)
        {
            DataTable dt = new DataTable();
            try
            {
                //接続文字列
                string conString = "Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq="
                    + csvDir + ";Extensions=asc,csv,tab,txt;";
                System.Data.Odbc.OdbcConnection con =
                    new System.Data.Odbc.OdbcConnection(conString);

                string commText = "SELECT * FROM [" + csvFileName + "]";
                System.Data.Odbc.OdbcDataAdapter da =
                    new System.Data.Odbc.OdbcDataAdapter(commText, con);

                //DataTableに格納する
                da.Fill(dt);
            }
            catch
            {
            }
            return dt;
        }
    }
}
