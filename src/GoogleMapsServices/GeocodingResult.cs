using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleMapsServices
{
    public class GeocodingResult
    {
        [JsonProperty("address_components")]
        public List<AddressComponent> AddressComponents { get; set; }

        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }
        public Geometry Geometry { get; set; }

        public List<string> Types { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        public IEnumerable<AddressType> AddressTypes
        {
            get 
            { 
                return Types.Select(type => 
                {
                    AddressType addressTypeEnum;

                    if (Enum.TryParse(type, true, out addressTypeEnum))
                    {
                        return addressTypeEnum;
                    }

                    return AddressType.unknown;
                });
            }
        }
    }
}