using System;

public class ConfigItem
{
    public string Key { get; set; }
    public Type type { get; set; }
    public object Value { get; set; }
    public object DefaultValue { get; set; }
    public string Tab { get; set; }
    public string Description { get; set; }
    public object MinValue { get; set; }
    public object MaxValue { get; set; }
    public bool Sync { get; set; }
    public bool Hidden { get; set; }
    public bool Optional { get; set; }
    public bool RequireRestart { get; set; }


    public ConfigItem(string key, object defaultValue, string tab, string description, object minValue = null, object maxValue = null, bool sync = true, bool optional = false, bool hidden = false, bool requireRestart = false)
    {
        Key = key;
        DefaultValue = defaultValue;
        type = defaultValue.GetType();
        Tab = tab;
        Description = description;
        if (minValue != null && maxValue != null)
        {
            if (minValue.GetType() == type && maxValue.GetType() == type)
            {
                MinValue = minValue;
                MaxValue = maxValue;
            }
        }
        else
        {
            MinValue = null;
            MaxValue = null;
        }
        Sync = sync;
        Optional = optional;
        Hidden = hidden;
        Value = DefaultValue;
        RequireRestart = requireRestart;
    }
}