using System.Net;

namespace WebApplication1.Interfaces;

public interface IKinesis
{
    Task<HttpStatusCode> CreateStream(string name);
    Task<List<string>> ListStreams();
    Task<HttpStatusCode> RemoveStream(string name);
    Task<string> AddDataToStream(string name, string message);
    Task<List<string>> ReadFromStream(string name);
}