using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaClientApplication.Infrastructure.JsonObjects
{
    public class VerbTreeJson:VerbNode
    {
        public List<VerbTreeJson> Children { get; set; }
    }
}
