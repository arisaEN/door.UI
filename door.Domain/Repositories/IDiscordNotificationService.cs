using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using door.Domain.DTO;
using door.Domain.Events;


namespace door.Domain.Repositories
{

    public interface IDiscordNotificationService
    {
        /// <summary>
        /// 状態変化検知用
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        Task HandleDoorStateChange(string stateMessage);
        /// <summary>
        /// discord通知
        /// /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task NotificationStateChange(StateChangedEvent domainEvent);
        

    }
}
