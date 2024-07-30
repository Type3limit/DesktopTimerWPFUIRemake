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
