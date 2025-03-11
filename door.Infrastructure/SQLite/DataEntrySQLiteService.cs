using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using door.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using door.Infrastructure;

namespace door.Infrastructure.SQLite
{

    class DataEntrySQLiteService
    {
        private readonly DoorDbContext _context;
        public DataEntrySQLiteService(DoorDbContext context)
        {
            _context = context;
        }
        public async Task<List<DataEntry>> GetDataEntryAsync()
        {
            // ① Entity を取得
            var dt = await _context.DataEntries
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new
                {
                    dt.Id,
                    dt.Date,
                    dt.Time,
                    dt.Status
                    //Amount = ed.Amount ?? 0, // Nullable int を int に変換
                    //InputTime = ed.InputTime.HasValue ? ed.InputTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null // Nullable DateTime を string に変換
                })
                .ToListAsync();

            // ② DTO に変換 //今DTO無いので　EntityからEntityに変換謎の処理になっている。
            var dataEntry = dt.Select(dt => new DataEntry
            {
                Id = dt.Id,
                Date = dt.Date,
                Time = dt.Time,
                Status = dt.Status
            }).ToList();

            return dataEntry; // DTO を返す
        }
    }
}

