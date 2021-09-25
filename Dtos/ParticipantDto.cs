using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photofy.Dtos
{
    public class ParticipantDto
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Boolean IsReady { get; set; }
    }
}