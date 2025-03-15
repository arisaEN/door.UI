using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using door.Domain.DTO;
using door.Domain.Entities;

namespace door.Domain.Repositories
{
    public interface IDataEntrySQLiteService
    {
        /// <summary>
        /// door.msからデータ参照用
        /// </summary>
        /// <returns></returns>
        /// 
        Task GetDataEntryAsync();
        /// <summary>
        /// door.msからデータ挿入用
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task DataEntryInsertAsync(DataEntryRequestDto request);
        /// <summary>
        /// discordからデータ挿入用
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task DataEntryReqTempJointAsync(DataEntryRequestDto request);
    }
}
