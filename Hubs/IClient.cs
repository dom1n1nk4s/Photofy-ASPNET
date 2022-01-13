using System.Collections.Generic;
using System.Threading.Tasks;
using Photofy.Dtos;

namespace Photofy.Hubs
{
    public interface IClient
    {
        public Task JoinedNewMember(string name, bool isReady, string id);
        public Task MemberToggleReady(string id);
        public Task MemberDisconnected(string id);
        public Task StartResultActivity(List<GuessDto> choices);
        public Task StartImageActivity();
        public Task StartGame();
    }
}