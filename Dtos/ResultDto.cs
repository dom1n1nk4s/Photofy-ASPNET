using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photofy.Dtos
{
    public class ResultDto
    {
        public string Result { get; set; }
        public ResultDto(string r) { Result = r; }
    }
}