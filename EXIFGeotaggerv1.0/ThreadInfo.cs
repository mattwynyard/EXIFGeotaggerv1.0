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
        //private ProgressForm progressForm;
        public ThreadInfo()
        {

        }

        public ConcurrentQueue<string> JobQueue { get; set; }

        public string OutPath { get; set; }

        public int Length { get; set; }

        public ProgressForm ProgressForm { get; set; }

        public CancellationToken Token { get; set; }

        public IProgress<int> ProgressHandler { get; set; }

    }
}
