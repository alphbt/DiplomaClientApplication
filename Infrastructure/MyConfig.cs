using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaClientApplication.Infrastructure
{
    public class MyConfig
    {
        public string PathToDictionary { get; set; }
        public string PathToCrossLexica { get; set; }

        public string PythonDll { get; set; }

        public string InstallingPipPath { get; set; }
        public string InstallingPipDirectory { get; set; }
    }
}
