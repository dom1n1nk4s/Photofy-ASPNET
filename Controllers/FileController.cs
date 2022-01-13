using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Photofy.Dtos;
using Photofy.Hubs;

namespace Photofy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IHubContext<GameHub, IClient> hubContext;
        private readonly Context context;

        public FileController(IHubContext<GameHub, IClient> hubContext, Context context)
        {
            this.hubContext = hubContext;
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult<ResultDto>> SendImage(FileDto fileDto)
        {
            //should probably check whether all are proper images.
            var user = context.Users.FirstOrDefault(u => u.ConnectionId == fileDto.ConnectionId);
            if (user == null) return BadRequest("Your user is not found. Please restart.");
            user.Image = fileDto.File;
            user.SentImage = true;
            var result = await context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Failed to save database");

            var usersToCall = context.Users.Where(u => u.LobbyId == user.LobbyId);
            if (usersToCall.All(u => u.SentImage))
            {
                await usersToCall.ForEachAsync(u => u.SentImage = false);
                await context.SaveChangesAsync();

                //cannot await this call due to android already awaiting http response
                await hubContext.Clients.Clients(usersToCall.Select(u => u.ConnectionId)).StartGame().ConfigureAwait(false);

                return Ok(new ResultDto("Sent! Starting game..."));
            }
            else
                return Ok(new ResultDto("Sent! Waiting for other players..."));
        }

        [HttpGet("{connectionId}")]
        public ActionResult<List<ImageNodeItemDto>> ReceiveImage(string connectionId)
        {
            var user = context.Users.AsNoTracking().FirstOrDefault(u => u.ConnectionId == connectionId);
            if (user == null) return BadRequest("Your user is not found. Please restart.");

            var images = context.Users.AsNoTracking().Where(u => u.LobbyId == user.LobbyId).Select(u => new ImageNodeItemDto()
            {
                Image = u.Image,
                ConnectionId = u.ConnectionId,
                Title = u.Name
            }).ToList();

            return Ok(images);
        }
    }
}