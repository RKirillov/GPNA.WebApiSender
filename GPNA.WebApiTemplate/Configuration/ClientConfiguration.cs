using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPNA.gRPCClient.Configuration
{
    public class ClientConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public int BatchCount { get; private set; } = 10000;
        /// <summary>
        /// 
        /// </summary>
        public int DeadLineSec { get; private set; } = 60;
        /// <summary>
        /// 
        /// </summary>
        public bool WithWaitForReady { get; private set; } = true;
    }
}
