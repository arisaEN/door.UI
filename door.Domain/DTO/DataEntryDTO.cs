using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace door.Domain.DTO
{
    public class DataEntryDTO
    {
        public int Id { get; set; }
        [Display(Name = "日付")]
        public string Date { get; set; } = null!;
        [Display(Name = "時刻")]
        public string Time { get; set; } = null!;
        [Display(Name = "名前")]
        public string StatusName { get; set; } = null!;
    }
    
    //APIリクエストデータの型定義
    public class DataEntryRequestDto
    {
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int DoorStatusId { get; set; }
    }
}
