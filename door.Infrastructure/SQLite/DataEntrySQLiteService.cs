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
        /// <summary>
        /// APIリクエストからDBインサート
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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


        /////////////////未完成↓


        /// <summary>
        /// discordAPIリクエストからRDS内でテーブル結合し、結果を返す
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<DataEntryDTO>> DataEntryReqTempJointAsync(DataEntryRequestDto request)
        {

            //現状reqと同じデータを明細から出力して、それとマスタを結合して渡している。　
            var dataEntryDTOs = await _context.DataEntries
                .Where(de => de.Date == request.Date && de.Time == request.Time && de.DoorStatusId == request.DoorStatusId)
                .Join(
                    _context.MasterDoorStatuses,
                    de => de.DoorStatusId,
                    status => status.Id,
                    (de, status) => new DataEntryDTO
                    {
                        Id = de.Id,
                        Date = de.Date,
                        Time = de.Time,
                        StatusName = status.DoorStatusName
                    }
                )
                .OrderByDescending(de => de.Id)
                .ToListAsync();

            return dataEntryDTOs;
        }
    }
}

