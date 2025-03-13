using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using door.Domain.DTO;


namespace door.Domain.Repositories
{

    public interface ICameraNotification
    {
        Task DataEntryInsert(DataEntryRequestDto request);
        Task NotificationStateChange(string message);
    }
}
