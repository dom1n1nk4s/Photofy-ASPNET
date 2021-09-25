using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photofy.Hubs
{
    public interface IClient
    {
        public Task JoinedNewMember(string id);
        public Task MemberToggleReady(string id);
        public Task MemberDisconnected(string id);
        public Task StartImageActivity();
        public Task StartGame(List<string> images);
    }
}