using Autofac;
using Service.PriceToCandle.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.PriceToCandle.Client
{
    public static class AutofacHelper
    {
        public static void RegisterPriceToCandleClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new PriceToCandleClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
