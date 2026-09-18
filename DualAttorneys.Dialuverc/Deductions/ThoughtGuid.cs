using System.Text.Json;
using System.Text.Json.Serialization;

namespace DualAttorneys.Dialuverc.Deductions
{
    // Coming from Unity, writing out the full readonly struct makes sure this is compatible everywhere.
    [JsonConverter(typeof(ThoughtGuidJsonConverter))]
    public readonly struct ThoughtGuid : IEquatable<ThoughtGuid>
    {
        public readonly Guid Guid;

        public ThoughtGuid()
        {
            Guid = Guid.NewGuid();
        }

        /// <summary>
        /// Used by <see cref="ThoughtGuidJsonConverter"/> to create a <see cref="ThoughtGuid"/> from a serialized representation.
        /// </summary>
        public ThoughtGuid(Guid guid)
        {
            Guid = guid;
        }

        public bool Equals(ThoughtGuid other) => other.Guid == Guid;
        public override bool Equals(object? obj) => obj is ThoughtGuid other && Equals(other);

        public static bool operator ==(ThoughtGuid left, ThoughtGuid right) => left.Equals(right);
        public static bool operator !=(ThoughtGuid left, ThoughtGuid right) => !(left == right);

        public override int GetHashCode() => Guid.GetHashCode();

        public override string ToString() => Guid.ToString();
    }

    public class ThoughtGuidJsonConverter : JsonConverter<ThoughtGuid>
    {
        public override ThoughtGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new ThoughtGuid(reader.GetGuid());
        }

        public override void Write(Utf8JsonWriter writer, ThoughtGuid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Guid);
        }
    }
}
