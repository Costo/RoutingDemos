using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GoogleMapsServices
{
    public class GoogleMaps
    {
        const string GeocodeServiceUrl = "https://maps.googleapis.com/maps/api/geocode/";
        readonly string _apiKey;
        readonly HttpClient _httpClient;

        public GoogleMaps(ILoggerFactory loggerFactory, IMemoryCache memoryCache, string apiKey)
        {
            _apiKey = apiKey;
            var logger = loggerFactory.CreateLogger<GoogleMaps>();

            var handlersChain = new CachingHandler(logger, memoryCache)
            {
                InnerHandler = new HttpClientHandler()
            };
            _httpClient = new HttpClient(handlersChain)
            {
                BaseAddress = new Uri(GeocodeServiceUrl, UriKind.Absolute),
            };

        }

        public async Task<GeocodingResponse> Geocode(string address, string language = "en")
        {
            var qs = QueryString.Create(new Dictionary<string, string>
            {
                ["address"] = address,
                ["language"] = language,
                ["key"] = _apiKey,
            });

            var url = "json" + qs;
            var response = await _httpClient.GetAsync("json" + qs);
                
            var json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GeocodingResponse>(json);
        }
    }



    public class GoogleResult
    {
        public ResultStatus Status { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }
    }

    public enum ResultStatus
    {
        UNKNOWN,
        OK,
        ZERO_RESULTS,
        OVER_QUERY_LIMIT,
        REQUEST_DENIED,
        INVALID_REQUEST
    }

    
}