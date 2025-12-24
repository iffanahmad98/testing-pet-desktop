using Newtonsoft.Json;
using System;

public class TimeSpanConverter : JsonConverter<TimeSpan>
{
    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString()); // "01:23:45.0000000"
    }

    public override TimeSpan ReadJson(
        JsonReader reader,
        Type objectType,
        TimeSpan existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        return TimeSpan.Parse((string)reader.Value);
    }
}
