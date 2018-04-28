namespace Kalarrs.Sreverless.NetCore
{
    public class HttpEvent
    {
        public string Handler { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public bool Cors { get; set; }
    }
}