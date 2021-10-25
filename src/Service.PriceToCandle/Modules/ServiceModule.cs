using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.AssetsDictionary.Client;
using SimpleTrading.ServiceBus.PublisherSubscriber.BidAsk;

namespace Service.PriceToCandle.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.ServiceBusHostPort),
                Program.LogFactory);

            var candlePublisher = new BidAskMyServiceBusPublisher(serviceBusClient, "spot-bidask");

            builder
                .RegisterInstance(candlePublisher)
                .As<IPublisher<SimpleTrading.Abstraction.BidAsk.IBidAsk>>()
                .SingleInstance();
            
            builder.RegisterMyServiceBusSubscriberBatch<MyJetWallet.Domain.Prices.BidAsk>(serviceBusClient, "jetwallet-external-prices", "IndexPrices", TopicQueueType.PermanentWithSingleConnection);

            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            
            builder.RegisterAssetsDictionaryClients(noSqlClient);
        }
    }
}