using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PCL.Core.ProgramSetup.SourceManage;

public interface IFileSerializer<in TSerialized>
{
    void Deserialize(Stream source, TSerialized result);
    void Serialize(TSerialized input, Stream destination);
}

public class IniDictSerializer : IFileSerializer<IDictionary<string, string>>
{
    public static readonly IniDictSerializer Instance = new();

    public void Deserialize(Stream source, IDictionary<string, string> result)
    {
        result.Clear();
        using var reader = new StreamReader(source);
        while (reader.ReadLine() is { } line)
        {
            var splitPos = line.IndexOf(':');
            if (splitPos == -1)
                continue;
            var key = line[..splitPos];
            var value = line[(splitPos + 1)..];
            result.Add(key, value);
        }
    }

    public void Serialize(IDictionary<string, string> input, Stream destination)
    {
        using var writer = new StreamWriter(destination);
        foreach (var pair in input)
            writer.WriteLine("{0}:{1}", pair.Key, pair.Value);
    }
}

public class JsonDictSerializer : IFileSerializer<IDictionary<string, string>>
{
    public static readonly JsonDictSerializer Instance = new();
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public void Deserialize(Stream source, IDictionary<string, string> result)
    {
        result.Clear();
        if (source.Length == 0)
            return;
        if (JsonSerializer.Deserialize<Dictionary<string, string>>(source) is { } dict)
            foreach (var pair in dict)
                result.Add(pair.Key, pair.Value);
    }

    public void Serialize(IDictionary<string, string> input, Stream destination)
    {
        JsonSerializer.Serialize(destination, input, _serializerOptions);
    }
}