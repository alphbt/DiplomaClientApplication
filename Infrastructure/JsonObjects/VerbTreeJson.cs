using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaClientApplication.Infrastructure.JsonObjects
{
    public class VerbTreeJson<T>:VerbNode 
        where T:VerbNode
    {
        public List<T> Children { get; set; }
    }
}
