namespace Ntk8.Tests
{
    public interface IAppSettings
    {
        string DefaultConnection { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        public string DefaultConnection { get; set; }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }
}