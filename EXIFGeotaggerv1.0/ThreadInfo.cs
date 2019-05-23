using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EXIFGeotagger
{
    class ThreadInfo
    {
        public ThreadInfo()
        {

        }

        public string File { get; set; }
        public string OutPath { get; set; }

        public int Length { get; set; }

        public Record Record { get; set; }

        //public ProgressForm ProgressForm { get; set; }

        public IProgress<int> ProgressHandler { get; set; }

    }
}
