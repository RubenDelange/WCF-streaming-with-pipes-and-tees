using System.IO;
using System.ServiceModel;

namespace contract
{
    [ServiceContract(Name = "IStreaming", SessionMode = SessionMode.Allowed)]
    public interface IStreaming
    {
        [OperationContract]
        void ReceiveStream(Stream stream);
    }
}
