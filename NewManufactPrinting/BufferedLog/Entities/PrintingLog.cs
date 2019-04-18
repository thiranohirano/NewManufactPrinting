using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewManufactPrinting.BufferedLog.Entities
{
    [Table("printing_log")]
    public class PrintingLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_number")]
        public string OrderNumber { get; set; }

        [Column("order_date")]
        public DateTime? OrderDate { get; set; }

        [Column("delivery_date")]
        public DateTime? DeliveryDate { get; set; }

        [Column("customer")]
        public string Customer { get; set; }

        [Column("model")]
        public string Model { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("member")]
        public string Member { get; set; }

        [Column("printing_times")]
        public int PrintingTimes { get; set; }

        [Column("redo_times")]
        public int RedoTimes { get; set; }

        [Column("label_printing_time")]
        public int LabelPrintingTimes { get; set; }

        [Column("cable_number")]
        public string CableNumber { get; set; }
    }
}
