using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GoogleMapsServices
{
    public class AddressComponent
    {
        [JsonProperty("long_name")]
        public string LongName { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        public IEnumerable<string> Types { get; set; }

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