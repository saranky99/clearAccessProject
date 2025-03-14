namespace clearAccess.Service
{
    public interface IRefreshHandler
    {
         Task<string> GenerateToken(string username);
    }
}
