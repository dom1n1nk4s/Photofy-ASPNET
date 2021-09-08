using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Photofy.Hubs
{
    public class GameHub : Hub
    {
        private readonly Context context;
        public GameHub(Context context)
        {
            this.context = context;
        }
        public string Test()
        {
            Console.WriteLine("Test");
            return "Hello World";
        }
        // public async Task NewUser (){
        //     return await 
        // }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            await base.OnConnectedAsync();
        }
    }
}