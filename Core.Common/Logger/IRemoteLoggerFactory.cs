namespace Core.Common.Logger;

public interface IRemoteLoggerFactory
{
    Task<IRemoteLogger> CreateAsync();
}