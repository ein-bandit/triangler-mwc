namespace MobileWebControl
{
    public interface IWebServer
    {
        string GetPublicIPAddress();
        void CloseConnection();
    }
}
