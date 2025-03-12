using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using door.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using door.Infrastructure;
using door.Domain.DTO;


namespace door.Infrastructure.SQLite
{

    public class DataEntrySQLiteService
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DoorDbContext _context;
        public DataEntrySQLiteService(DoorDbContext context)
        {
            _context = context;
        }
        public async Task<List<DataEntryDTO>> GetDataEntryAsync()
        {

            _logger.Info("DataEntryDTOデータ取得開始");
            // ① Entity を取得
            var dt = await _context.DataEntries
                .Join(
                    _context.MasterDoorStatuses,
                    dt => dt.DoorStatusId,  // `DataEntry` の結合キー
                    status => status.Id,        // `MasterDoorStatus` の結合キー
                    (data, status) => new   
                    {
                        data.Id,
                        data.Date,
                        data.Time,
                        status.DoorStatusName
                    }
                )
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new
                {
                    dt.Id,
                    dt.Date,
                    dt.Time,
                    dt.DoorStatusName
                })
                .ToListAsync();

            // ② DTO に変換 //今DTO無いので　EntityからEntityに変換謎の処理になっている。
            var dataEntryDTO = dt.Select(dt => new DataEntryDTO
            {
                Id = dt.Id,
                Date = dt.Date,
                Time = dt.Time,
                StatusName = dt.DoorStatusName
            }).ToList();

            return dataEntryDTO; // DTO を返す
        }
        public async Task DataEntryInsertAsync(DataEntryRequestDto request)
        {
            var newEntry = new DataEntry(
                //id: 0, // EF Coreが自動設定するので適当な値（0）を入れる
                date: request.Date,
                time: request.Time,
                doorStatusId: request.DoorStatusId
            );

            _context.DataEntries.Add(newEntry);
            await _context.SaveChangesAsync();
        }
    }
}

