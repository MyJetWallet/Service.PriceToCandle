using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.ServiceBus;
using Serilog;
using Service.AssetsDictionary.Client;
using SimpleTrading.Abstraction.BidAsk;
using BidAsk = MyJetWallet.Domain.Prices.BidAsk;

namespace Service.PriceToCandle.Job
{
    public class CandlePriceWriter
    {
        private readonly ISubscriber<IReadOnlyList<BidAsk>> _subscriber;
        private readonly IServiceBusPublisher<SimpleTrading.ServiceBus.Models.BidAskServiceBusModel> _candlePublisher;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionaryClient;
        private readonly ILogger<CandlePriceWriter> _logger;

        public CandlePriceWriter(
            ISubscriber<IReadOnlyList<MyJetWallet.Domain.Prices.BidAsk>> subscriber,
            IServiceBusPublisher<SimpleTrading.ServiceBus.Models.BidAskServiceBusModel> candlePublisher,
            ISpotInstrumentDictionaryClient instrumentDictionaryClient,
            ILogger<CandlePriceWriter> logger
            )
        {
            _subscriber = subscriber;
            _candlePublisher = candlePublisher;
            _instrumentDictionaryClient = instrumentDictionaryClient;
            _logger = logger;
            
            subscriber.Subscribe(HandlePrices);
        }

        private async ValueTask HandlePrices(IReadOnlyList<BidAsk> data)
        {
            _logger.LogInformation("Receive {count} prices", data.Count);
            
            var instruments = _instrumentDictionaryClient
                .GetAllSpotInstruments()
                .GroupBy(e => $"{e.ConvertSourceExchange}||{e.ConvertSourceMarket}")
                .Select(e => e.First())
                .ToDictionary(e => $"{e.ConvertSourceExchange}||{e.ConvertSourceMarket}");
            
            var list = new List<SimpleTrading.ServiceBus.Models.BidAskServiceBusModel>();
            
            foreach (var item in data)
            {
                if (instruments.TryGetValue($"{item.LiquidityProvider}||{item.Id}", out var instr))
                {
                    list.Add( new SimpleTrading.ServiceBus.Models.BidAskServiceBusModel()
                    {
                        Id = instr.Symbol,
                        DateTime = item.DateTime,
                        Ask = (double) item.Ask,
                        Bid = (double) item.Bid
                    });
                }
            }

            if (list.Any())
            {
                try
                {
                    await _candlePublisher.PublishAsync(list);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot publish prices to candle service bus");
                }
            }

            _logger.LogInformation("Publish {count} prices", list.Count);
        }
    }
}