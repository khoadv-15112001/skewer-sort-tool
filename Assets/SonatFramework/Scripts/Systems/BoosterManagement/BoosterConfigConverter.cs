using System;
using Newtonsoft.Json;
using Sonat.Enums;

namespace SonatFramework.Systems.BoosterManagement
{
    public class BoosterConfigConverter : JsonConverter<BoosterConfig>
    {
        public override void WriteJson(JsonWriter writer, BoosterConfig value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.booster.ToString());
            writer.WriteValue(value.levelUnlock);
            writer.WriteValue(value.priceCurrency.ToString());
            writer.WriteValue(value.price);
            writer.WriteValue(value.defaultValue);
            writer.WriteValue(value.value);
            writer.WriteEndArray();
        }

        public override BoosterConfig ReadJson(
            JsonReader reader,
            Type objectType,
            BoosterConfig existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var token = Newtonsoft.Json.Linq.JToken.Load(reader);
            var result = new BoosterConfig();

            // Support both array format and object format
            if (token is Newtonsoft.Json.Linq.JArray arr)
            {
                // Array format: ["BoosterType", levelUnlock, "Currency", price, defaultValue, value]
                result.booster = arr[0].ToObject<GameResource>();
                result.levelUnlock = arr[1].ToObject<int>();
                result.priceCurrency = arr[2].ToObject<GameResource>();
                result.price = arr[3].ToObject<int>();
                result.defaultValue = arr[4].ToObject<int>();
                result.value = arr[5].ToObject<int>();
            }
            else if (token is Newtonsoft.Json.Linq.JObject obj)
            {
                // Object format: {"booster": "X", "levelUnlock": 1, ...}
                result = obj.ToObject<BoosterConfig>();
            }

            return result;
        }

    }
}