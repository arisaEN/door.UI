using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using door.Domain.DTO;



namespace door.Domain.Repositories
{

    public interface INotificationService
    {
        /// <summary>
        /// discord通知
        /// /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task NotificationStateChange(string stateMessage);
        

    }
}
