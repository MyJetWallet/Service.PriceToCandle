using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.PriceToCandle.Grpc;

namespace Service.PriceToCandle.Client
{
    [UsedImplicitly]
    public class PriceToCandleClientFactory: MyGrpcClientFactory
    {
        public PriceToCandleClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
