using System.Runtime.Serialization;
using Service.PriceToCandle.Domain.Models;

namespace Service.PriceToCandle.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}