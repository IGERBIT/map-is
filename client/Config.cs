using System.Globalization;
using Newtonsoft.Json;

namespace client;

public class Config
{
    public string Path { get; }

    private Dictionary<string, string> _data = new Dictionary<string, string>();
    
    public Config(string path)
    {
        Path = path;
    }

    public void Load()
    {
        CreateIfNeeded(Path);

        var text = File.ReadAllText(Path);

        if (JsonConvert.DeserializeObject<Dictionary<string, string>>(text) is { } data)
        {
            _data = data;
        }
    }

    private static void CreateIfNeeded(string path)
    {
        if (!File.Exists(path)) File.CreateText(JsonConvert.SerializeObject(new { }, Formatting.Indented));
    }
    
    public void Save()
    {
        File.WriteAllText(Path, JsonConvert.SerializeObject(_data, Formatting.Indented));
    }

    public string GetString(string key)
    {
        return _data.GetValueOrDefault(key, null);
    }
    
    public void SetString(string key, string value)
    {
        _data[key] = value;
    }
    
    public int GetInt(string key)
    {
        return int.Parse(_data.GetValueOrDefault(key, null));
    }
    
    public void SetInt(string key, int value)
    {
        _data[key] = value.ToString();
    }
    
    public double GetDouble(string key)
    {
        return double.Parse(_data.GetValueOrDefault(key, null));
    }
    
    public void SetDouble(string key, double value)
    {
        _data[key] = value.ToString(CultureInfo.InvariantCulture);
    }
    
    public bool GetBool(string key)
    {
        return bool.Parse(_data.GetValueOrDefault(key, null));
    }
    
    public void SetBool(string key, bool value)
    {
        _data[key] = value ? bool.TrueString : bool.FalseString;
    }

    public string this[string key]
    {
        get => GetString(key);
        set => SetString(key, value);
    }
}

