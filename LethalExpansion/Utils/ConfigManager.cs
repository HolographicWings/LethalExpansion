using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace LethalExpansion.Utils
{
    public class ConfigManager
    {
        private static ConfigManager _instance;
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                }
                return _instance;
            }
        }

        private List<ConfigItem> items = new List<ConfigItem>();
        private List<ConfigEntryBase> entries = new List<ConfigEntryBase>();

        public void AddItem(ConfigItem item)
        {
            try
            {
                items.Add(item);
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        
        public void ReadConfig()
        {
            items = items.OrderBy(item => item.Tab).ThenBy(item => item.Key).ToList();
            try
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!items[i].Hidden)
                    {
                        string _tmpDesc = items[i].Description;
                        _tmpDesc += "\nNetwork synchronization: " + (items[i].Sync ? "Yes" : "No");
                        _tmpDesc += "\nMod required by clients: " + (items[i].Optional ? "No" : "Yes");
                        switch (items[i].type.Name)
                        {
                            case "Int32":
                                entries.Add(LethalExpansion.config.Bind(items[i].Tab, items[i].Key, (int)items[i].DefaultValue, new ConfigDescription(_tmpDesc, new AcceptableValueRange<int>((int)items[i].MinValue, (int)items[i].MaxValue))));
                                break;
                            case "Single":
                                entries.Add(LethalExpansion.config.Bind(items[i].Tab, items[i].Key, (float)items[i].DefaultValue, new ConfigDescription(_tmpDesc, new AcceptableValueRange<float>((float)items[i].MinValue, (float)items[i].MaxValue))));
                                break;
                            case "Boolean":
                                entries.Add(LethalExpansion.config.Bind(items[i].Tab, items[i].Key, (bool)items[i].DefaultValue, new ConfigDescription(_tmpDesc)));
                                break;
                            case "String":
                                entries.Add(LethalExpansion.config.Bind(items[i].Tab, items[i].Key, (string)items[i].DefaultValue, new ConfigDescription(_tmpDesc)));
                                break;
                            default:
                                break;
                        }
                        if (!items[i].Sync)
                        {
                            items[i].Value = entries.Last().BoxedValue;
                        }
                    }
                    else
                    {
                        entries.Add(default);
                    }
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
        public object FindItemValue(string key)
        {
            try
            {
                return items.First(item => item.Key == key).Value;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public object FindItemValue(int index)
        {
            try
            {
                return items[index].Value;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public object FindEntryValue(string key)
        {
            try
            {
                return entries.First(item => item.Definition.Key == key).BoxedValue;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public object FindEntryValue(int index)
        {
            try
            {
                return entries[index].BoxedValue;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public bool RequireRestart(string key)
        {
            try
            {
                return items.First(item => item.Key == key).RequireRestart;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return default;
            }
        }
        public bool RequireRestart(int index)
        {
            try
            {
                return items[index].RequireRestart;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return default;
            }
        }
        public string FindDescription(string key)
        {
            try
            {
                return items.First(item => item.Key == key).Description;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public string FindDescription(int index)
        {
            try
            {
                return items[index].Description;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public (bool,bool) FindNetInfo(string key)
        {
            try
            {
                ConfigItem i = items.First(item => item.Key == key);
                return (i.Sync, i.Optional);
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return (default, default);
            }
        }
        public (bool, bool) FindNetInfo(int index)
        {
            try
            {
                return (items[index].Sync, items[index].Optional);
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return (default, default);
            }
        }
        public object FindDefaultValue(string key)
        {
            try
            {
                return items.First(item => item.Key == key).DefaultValue;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public object FindDefaultValue(int index)
        {
            try
            {
                return items[index].DefaultValue;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public bool MustBeSync(string key)
        {
            try
            {
                return items.First(item => item.Key == key).Sync;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return true;
            }
        }
        public bool MustBeSync(int index)
        {
            try
            {
                return items[index].Sync;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return true;
            }
        }
        public bool SetItemValue(string key, object value)
        {
            ConfigItem configItem = items.First(item => item.Key == key);
            if (!items.First(item => item.Key == key).RequireRestart)
            {
                try
                {
                    configItem.Value = value;
                    return true;
                }
                catch (Exception ex)
                {
                    LethalExpansion.Log.LogError(ex.Message);
                    return false;
                }
            }
            return false;
        }
        public bool SetEntryValue(string key, object value)
        {
            try
            {
                entries.First(item => item.Definition.Key == key).BoxedValue = value;
                return true;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return false;
            }
        }
        public bool SetItemValue(int index, string value, char type)
        {
            if (!items[index].RequireRestart)
            {
                try
                {
                    switch (type)
                    {
                        case 'i':
                            items[index].Value = int.Parse(value, CultureInfo.InvariantCulture);
                            break;
                        case 'f':
                            items[index].Value = float.Parse(value, CultureInfo.InvariantCulture);
                            break;
                        case 'b':
                            items[index].Value = bool.Parse(value);
                            break;
                        case 's':
                            items[index].Value = value;
                            break;
                        default:
                            break;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LethalExpansion.Log.LogError(ex.Message);
                    return false;
                }
            }
            return false;
        }
        public bool SetEntryValue(int index, string value, char type)
        {
            try
            {
                switch (type)
                {
                    case 'i':
                        entries[index].BoxedValue = int.Parse(value);
                        break;
                    case 'f':
                        entries[index].BoxedValue = float.Parse(value);
                        break;
                    case 'b':
                        entries[index].BoxedValue = bool.Parse(value);
                        break;
                    case 's':
                        entries[index].BoxedValue = value;
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return false;
            }
        }
        public (object,int) FindValueAndIndex(string key)
        {
            try
            {
                return (items.First(item => item.Key == key).Value, items.FindIndex(item => item.Key == key));
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return (null,-1);
            }
        }
        public int FindIndex(string key)
        {
            try
            {
                return (items.FindIndex(item => item.Key == key));
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return -1;
            }
        }

        public T FindItemValue<T>(string key)
        {
            try
            {
                ConfigItem configItem = items.First(item => item.Key == key);
                if (configItem != null && configItem.Value is T)
                {
                    return (T)configItem.Value;
                }
                else
                {
                    LethalExpansion.Log.LogError("Key not found or value is of incorrect type");
                    throw new InvalidOperationException("Key not found or value is of incorrect type");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return default(T);
            }
        }
        public T FindItemValue<T>(int index)
        {
            try
            {
                ConfigItem configItem = items[index];
                if (configItem != null && configItem.Value is T)
                {
                    return (T)configItem.Value;
                }
                else
                {
                    LethalExpansion.Log.LogError("Key not found or value is of incorrect type");
                    throw new InvalidOperationException("Key not found or value is of incorrect type");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return default(T);
            }
        }
        public T FindEntryValue<T>(string key)
        {
            try
            {
                ConfigEntryBase configItem = entries.First(item => item.Definition.Key == key);
                if (configItem != null && configItem.BoxedValue is T)
                {
                    return (T)configItem.BoxedValue;
                }
                else
                {
                    LethalExpansion.Log.LogError("Key not found or value is of incorrect type");
                    throw new InvalidOperationException("Key not found or value is of incorrect type");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return default(T);
            }
        }
        public T FindEntryValue<T>(int index)
        {
            try
            {
                ConfigEntryBase configItem = entries[index];
                if (configItem != null && configItem.BoxedValue is T)
                {
                    return (T)configItem.BoxedValue;
                }
                else
                {
                    LethalExpansion.Log.LogError("Key not found or value is of incorrect type");
                    throw new InvalidOperationException("Key not found or value is of incorrect type");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return default(T);
            }
        }
        public bool SetItemValue<T>(string key, T value)
        {
            ConfigItem configItem = items.First(item => item.Key == key);
            if (!configItem.RequireRestart)
            {
                try
                {
                    if (configItem != null && configItem.Value is T)
                    {
                        configItem.Value = (T)value;
                        return true;
                    }
                    else
                    {
                        LethalExpansion.Log.LogError("Key not found or value is of incorrect type");
                        throw new InvalidOperationException("Key not found or value is of incorrect type");
                    }
                }
                catch (Exception ex)
                {
                    LethalExpansion.Log.LogError(ex.Message);
                    return false;
                }
            }
            return false;
        }
        public bool SetEntryValue<T>(string key, T value)
        {
            try
            {
                ConfigEntryBase configItem = entries.First(item => item.Definition.Key == key);
                if (configItem != null && configItem.BoxedValue is T)
                {
                    configItem.BoxedValue = (T)value;
                    return true;
                }
                else
                {
                    LethalExpansion.Log.LogError("Key not found or value is of incorrect type");
                    throw new InvalidOperationException("Key not found or value is of incorrect type");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return false;
            }
        }
        public bool SetItemValue<T>(int index, T value)
        {
            if (!items[index].RequireRestart)
            {
                try
                {
                    items[index].Value = (T)value;
                    return true;
                }
                catch (Exception ex)
                {
                    LethalExpansion.Log.LogError(ex.Message);
                    return false;
                }
            }
            return false;

        }
        public bool SetEntryValue<T>(int index, T value)
        {
            try
            {
                entries[index].BoxedValue = (T)value;
                return true;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return false;
            }
        }
        public (T,int) FindValueAndIndex<T>(string key)
        {
            try
            {
                ConfigItem configItem = items.First(item => item.Key == key);
                if (configItem != null && configItem.Value is T)
                {
                    return ((T)configItem.Value, items.FindIndex(item => item.Key == key));
                }
                else
                {
                    LethalExpansion.Log.LogError("Key not found or value is of incorrect type");
                    throw new InvalidOperationException("Key not found or value is of incorrect type");
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return (default(T),-1);
            }
        }

        public List<ConfigItem> GetAll()
        {
            try
            {
                return items;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public int GetCount()
        {
            try
            {
                return items.Count;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return -1;
            }
        }
        public int GetEntriesCount()
        {
            try
            {
                return entries.Count;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return -1;
            }
        }


        public object ReadConfigValue(string key)
        {
            try
            {
                return entries[FindIndex(key)].BoxedValue;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return null;
            }
        }
        public T ReadConfigValue<T>(string key)
        {
            try
            {
                return (T)entries[FindIndex(key)].BoxedValue;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return default(T);
            }
        }
        public bool WriteConfigValue(string key, object value)
        {
            try
            {
                int index = FindIndex(key);
                SetItemValue(index, value);
                entries[index].BoxedValue = value;
                return true;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return false;
            }
        }
        public bool WriteConfigValue<T>(string key, T value)
        {
            try
            {
                int index = FindIndex(key);
                SetItemValue(index, (T)value);
                entries[index].BoxedValue = (T)value;
                return true;
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
                return false;
            }
        }
    }
}
