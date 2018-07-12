namespace Kalarrs.NetCore.Util.ServerlessConfigs
{
    public class FunctionEvent
    {
        public HttpEvent Http { get; set; }
        public ScheduleEvent Schedule { get; set; }
    }
}