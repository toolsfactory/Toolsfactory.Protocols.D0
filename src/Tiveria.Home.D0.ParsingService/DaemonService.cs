using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tiveria.Common.Logging;

namespace Tiveria.Home.D0.SampleApp
{
    internal partial class DaemonService : BackgroundService
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IOptions<DaemonConfig> _options;
        private readonly CancellationToken _token = new CancellationToken();
        private readonly IHexDumpLogger _hexLogger;
        private readonly ILogManager _logManager;

        private D0SerialTransport _transport;
        private double _lastValue = -1;

        public DaemonService(ILoggerFactory loggerFactory, IOptions<DaemonConfig> options, IConfiguration config, IHostApplicationLifetime hostApplicationLifetime)
        {
            loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _config = config;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = loggerFactory.CreateLogger<DaemonService>();
            _logManager = new LoggingExtensionsLogManager(loggerFactory);
            _hexLogger = new Tiveria.Common.Logging.HexDumpLogger(new LoggingExtensionsLogger(_logger));
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing DaemonService...");
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"DaemonService is starting.");

            stoppingToken.Register(() =>
                _logger.LogDebug("DeamonService stop request received."));

            await Task.Delay(1000, stoppingToken);
            _logger.LogInformation($"Delay configured: {_options.Value.DelaySec} sec");
    
            if (!TestTransportAndPort(_options.Value.SerialPort))
                _hostApplicationLifetime.StopApplication();
            else
                await RunLoopAsync(stoppingToken);

            await Task.Delay(1000);
            _logger.LogDebug($"DaemonService is finished.");
        }

        private async Task RunLoopAsync(CancellationToken stoppingToken)
        {
            Int64 counter = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Beginning with loop {counter++}");
                    _transport.Open();
                    var parser = CreateParser(stoppingToken);
                    var ok = await parser.ReadAndParseAsync();
                    _transport.Close();
                    Thread.Sleep(5 * 60 * 1000);
                }
                catch (Exception e)
                {
                    _hexLogger.Flush();
                    _logger.LogError("Exception in read loop", e);
                    Thread.Sleep(5000);
                }
                finally
                {
                    if (_transport.IsOpen)
                        _transport.Close();
                }
            }
        }

        private D0SimpleStreamParser CreateParser(CancellationToken stoppingToken)
        {
            var parser = new D0SimpleStreamParser(_logManager, _hexLogger, _transport, stoppingToken, false);
            parser.VendorMessageEvent += Parser_VendorMessageEvent;
            parser.ObisDataEvent += Parser_ObisDataEvent;
            return parser;
        }

        private bool TestTransportAndPort(string serialDev)
        {
            _transport = new D0SerialTransport(_logManager, serialDev);
           try
            {
                _transport.Open();
                _transport.Close();
                _logger.LogInformation("Serial device sucessfully tested");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Serial device could not be opened!", e);
                return false;
            }
        }


        private async Task RunAsync()
        {
            if (TestTransportAndPort(_options.Value.SerialPort))
            {
                Int64 counter = 0;

                var parser = new D0SimpleStreamParser(_logManager, _hexLogger, _transport, _token, false);
                parser.VendorMessageEvent += Parser_VendorMessageEvent;
                parser.ObisDataEvent += Parser_ObisDataEvent;
                _logger.LogInformation("Starting reading and parsing loop ...");
                while (true)
                {
                    try
                    {
                        _logger.LogInformation($"Beginning with loop {counter++}");
                        _transport.Open();
                        var ok = await parser.ReadAndParseAsync().ConfigureAwait(false);
                        _transport.Close();
                        Thread.Sleep(5 * 60 * 1000);
                    }
                    catch (Exception e)
                    {
                        _hexLogger.Flush();
                        _logger.LogError("Exception in read loop", e);
                        Thread.Sleep(5000);
                    }
                    finally
                    {
                        if (_transport.IsOpen)
                            _transport.Close();
                    }
                }
            }
        }

        private void Parser_ObisDataEvent(object sender, ObisDataEventArgs e)
        {
            _logger.LogInformation($"Obis Data - Code: {e.Code} - Value: {e.Value} - Unit: {e.Unit}");
            if (e.Code == "1.8.0")
                ReactOn180ObisDataAsync(e.Value);
        }

        private void Parser_VendorMessageEvent(object sender, VendorMessageEventArgs e)
        {
            _logger.LogInformation($"Vendor Info - Name: {e.Vendor} - Indentification: {e.Identification} - BaudChar: {e.BaudrateCharacter} ({e.Baudrate})");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:System-URI-Objekte anstelle von Zeichenfolgen übergeben", Justification = "<Ausstehend>")]
        private async Task ReactOn180ObisDataAsync(string value)
        {
            if (double.TryParse(value, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            {
                SendAsync("http://192.168.2.150:8080/rest/items/Strom_Verbrauch_Total/state", value);
                SendAsync("http://192.168.2.150:8080/rest/items/Strom_Verbrauch_lastupdate/state", DateTime.Now.ToString("s"));
                if (_lastValue > -1 && _lastValue <= val)
                {
                    var period = (((val - _lastValue) * 1000).ToString(CultureInfo.InvariantCulture));
                    _logger.LogInformation($"Calculating Period: ({val} - {_lastValue}) * 1000 = {period}");
                    SendAsync("http://192.168.2.150:8080/rest/items/Strom_Verbrauch_Period/state", period);
                }
                _lastValue = val;
            }
        }

        private async void SendAsync(string endpoint, string value)
        {
            try
            {
                _logger.LogInformation($"Sending '{value}' to Endpoint '{endpoint}'");
                using var client = new System.Net.Http.HttpClient();
                using var content = new StringContent(value);
                await client.PutAsync(endpoint, content).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed sending message to endpoint '{endpoint}'", e);
            }
        }
    }
}
