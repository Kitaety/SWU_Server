using Microsoft.AspNetCore.SignalR;
using SWU_Web.SystemServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Hubs
{
    public class NotificationHub:Hub
    {
        public static IHubContext<NotificationHub> Current { get; set; }
        
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("LoadSystems", SystemDbContoller.Current.GetSystems());
        }

        public async Task UpdateSystemValue(HubPackage package)
        {
            await Clients.All.SendAsync("UpdateSystemValue", package);
        }

        public async Task UpdateSystemStatus(int idSystem, int status)
        {
            await Clients.All.SendAsync("UpdateSystemStatus", new { id=idSystem,status=status});
        }
    }
}
