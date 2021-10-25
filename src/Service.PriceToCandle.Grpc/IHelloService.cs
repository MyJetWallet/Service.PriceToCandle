using System.ServiceModel;
using System.Threading.Tasks;
using Service.PriceToCandle.Grpc.Models;

namespace Service.PriceToCandle.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}