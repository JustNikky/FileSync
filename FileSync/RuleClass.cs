using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSync
{
    class RuleClass
    {
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int NumNewFolders { get; set; }
        public int NumNewFiles { get; set; }
        public int NumOverwrittenFiles { get; set; }
    }
}
