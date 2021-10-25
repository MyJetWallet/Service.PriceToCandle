using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using Serilog;
using Service.AssetsDictionary.Client;
using SimpleTrading.Abstraction.BidAsk;
using BidAsk = MyJetWallet.Domain.Prices.BidAsk;

namespace Service.PriceToCandle.Job
{
    public class CandlePriceWriter
    {
        private readonly ISubscriber<IReadOnlyList<BidAsk>> _subscriber;
        private readonly IPublisher<IBidAsk> _candlePublisher;
        private readonly ISpotInstrumentDictionaryClient _instrumentDictionaryClient;
        private readonly ILogger<CandlePriceWriter> _logger;

        public CandlePriceWriter(
            ISubscriber<IReadOnlyList<MyJetWallet.Domain.Prices.BidAsk>> subscriber,
            IPublisher<SimpleTrading.Abstraction.BidAsk.IBidAsk> candlePublisher,
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
            
            var list = new List<SimpleTrading.Abstraction.BidAsk.IBidAsk>();
            
            foreach (var item in data)
            {
                if (instruments.ContainsKey($"{item.LiquidityProvider}||{item.Id}"))
                {
                    list.Add((SimpleTrading.Abstraction.BidAsk.IBidAsk) new SimpleTrading.Abstraction.BidAsk.BidAsk()
                    {
                        Id = item.Id,
                        DateTime = item.DateTime,
                        Ask = (double) item.Ask,
                        Bid = (double) item.Bid
                    });
                }
            }
            
            try
            {
                var taskList = list.Select(e => _candlePublisher.PublishAsync(e).AsTask()).ToArray();

                await Task.WhenAll(taskList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot publish prices to candle service bus");
            }
            
            _logger.LogInformation("Publish {count} prices", list.Count);
        }
    }
}