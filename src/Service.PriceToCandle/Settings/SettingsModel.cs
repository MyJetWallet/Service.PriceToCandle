using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.PriceToCandle.Settings
{
    public class SettingsModel
    {
        [YamlProperty("PriceToCandle.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("PriceToCandle.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("PriceToCandle.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("IndexPrices.ServiceBusHostPort")]
        public string ServiceBusHostPort { get; set; }
        
        [YamlProperty("IndexPrices.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
    }
}
