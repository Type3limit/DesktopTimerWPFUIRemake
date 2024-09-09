using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DesktopTimer.Helpers
{
    public class TypeJsonConverter:JsonConverter<Type>
    {
        public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? typeName = reader.GetString();
            return typeName == null ? null : Type.GetType(typeName);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.AssemblyQualifiedName);
        }
    }

    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? colorName = reader.GetString();
            return (Color) ColorConverter.ConvertFromString(colorName);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var timeSpanValue = reader.GetDouble();
            return TimeSpan.FromMilliseconds(timeSpanValue);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.TotalMilliseconds);
        }
    }

    public class FontFamilyJsonConverter : JsonConverter<FontFamily>
    {
        public override FontFamily? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? fontFamilyName = reader.GetString();
            return fontFamilyName != null ? new FontFamily(fontFamilyName) : null;
        }

        public override void Write(Utf8JsonWriter writer, FontFamily value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Source);
        }
    }
}
