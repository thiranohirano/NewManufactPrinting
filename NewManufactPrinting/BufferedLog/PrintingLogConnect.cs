using NewManufactPrinting.BufferedLog.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewManufactPrinting.BufferedLog
{
    public class PrintingLogConnect
    {
        private PrintingLogContext _context;
        private bool isConnect = false;

        public PrintingLogConnect()
        {
            _context = new PrintingLogContext();
            _context.Database.Log = sql => Console.WriteLine(sql);
        }

        public bool IsConnect()
        {
            return isConnect;
        }

        public ObservableCollection<PrintingLog> GetPrintingLogs()
        {
            try
            {
                DisposeContexts();
                _context = new PrintingLogContext();
                _context.Database.Log = sql => Console.WriteLine(sql); //コンソールに実行SQLを出力する
                _context.PrintingLogs.Load();
                var printingLogs = new ObservableCollection<PrintingLog>(_context.PrintingLogs.ToList());
                isConnect = true;
                return printingLogs;
            }
            catch (Exception)
            {
                isConnect = false;
                return null;
            }
        }

        public void AddPrintingLog(PrintingLog printingLog)
        {
            try
            {
                _context.PrintingLogs.Add(printingLog);
                _context.SaveChanges();
                isConnect = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                isConnect = false;
            }
        }

        public void RemovePrintingLog(PrintingLog printingLog)
        {
            try
            {
                _context.PrintingLogs.Remove(printingLog);
                _context.SaveChanges();
                isConnect = true;
            }
            catch (Exception)
            {
                isConnect = false;
            }
        }

        public void DisposeContexts()
        {
            _context.Dispose();
        }
    }
}
