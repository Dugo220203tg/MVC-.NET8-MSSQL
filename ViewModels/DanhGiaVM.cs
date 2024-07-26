﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;

namespace TDProjectMVC.ViewModels
{
    public class DanhGiaVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDG { get; set; }
        public string MaKH { get; set; }
        public int Sao { get; set; }
        public int MaHH { get; set; }
        public string NoiDung { get; set; }
        public DateOnly Ngay { get; set; }
        public double TrungBinhSao { get; set; }
        public double MotSao { get; set; }
        public double HaiSao { get; set; }
        public double BaSao { get; set; }
        public double BonSao { get; set; }
        public double NamSao { get; set; }

    }
}
