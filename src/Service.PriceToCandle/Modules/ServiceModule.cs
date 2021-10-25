using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.AssetsDictionary.Client;
using Service.PriceToCandle.Job;
using SimpleTrading.ServiceBus.Models;
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

            builder.RegisterMyServiceBusPublisher<SimpleTrading.ServiceBus.Models.BidAskServiceBusModel>(serviceBusClient, "spot-bidask", false);
            
            builder.RegisterMyServiceBusSubscriberBatch<MyJetWallet.Domain.Prices.BidAsk>(serviceBusClient, "jetwallet-external-prices", "PriceToCandle", TopicQueueType.PermanentWithSingleConnection, 1000);

            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            
            builder.RegisterAssetsDictionaryClients(noSqlClient);

            builder.RegisterType<CandlePriceWriter>().SingleInstance().AutoActivate();
        }
    }
}