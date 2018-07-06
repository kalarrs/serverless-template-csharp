using System.Collections.Generic;

namespace Kalarrs.NetCore.Util.ServerlessConfigs
{
    public class ServerlessConfig
    {
        public string Service { get; set; }
        public Provider Provider { get; set; }
        public Custom Custom { get; set; }
        public Dictionary<string, Function> Functions { get; set; }
    }
}