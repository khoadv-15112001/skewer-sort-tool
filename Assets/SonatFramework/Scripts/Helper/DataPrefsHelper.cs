using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SonatFramework.Systems;
using SonatFramework.Systems.GameDataManagement;

namespace SonatFramework.Scripts.Helper
{
    public class IntDataPref
    {
        private static readonly Service<DataService> DataService = new();

        private readonly string _name;
        public Action<int> onChanged;

        public IntDataPref(string name)
        {
            _name = name;
        }

        public IntDataPref(string name, int defaultValue)
        {
            if (!DataService.Instance.HasKey(name))
                DataService.Instance.SetInt(name, defaultValue);
            _name = name;
        }

        public int Value
        {
            get => DataService.Instance.GetInt(_name, 0);
            set
            {
                if (value != Value)
                {
                    DataService.Instance.SetInt(_name, value);
                    onChanged?.Invoke(value);
                }
            }
        }

        public bool BoolValue
        {
            get => DataService.Instance.GetInt(_name, 0) != 0;
            set
            {
                if (value == (Value != 0)) return;
                DataService.Instance.SetInt(_name, value ? 1 : 0);
                onChanged?.Invoke(value ? 1 : 0);
            }
        }


        public bool HasKey()
        {
            return DataService.Instance.HasKey(_name);
        }
    }


    public class LongDataPref
    {
        private static readonly Service<DataService> DataService = new();


        private readonly string _name;

        private long _currentValue;
        public Action<long> OnChanged;

        public LongDataPref(string name)
        {
            _name = name;
            _currentValue = 0;
            try
            {
                if (!string.IsNullOrEmpty(DataService.Instance.GetString(_name, "")))
                    _currentValue = long.Parse(DataService.Instance.GetString(_name, ""));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public long Value
        {
            get => _currentValue;
            set
            {
                if (value != Value)
                {
                    _currentValue = value;
                    DataService.Instance.SetString(_name, value.ToString());
                    OnChanged?.Invoke(value);
                }
            }
        }
    }


    public class StringDataPref
    {
        private static readonly Service<DataService> DataService = new();

        private readonly string _name;

        private readonly string _default;

        public StringDataPref(string name, string defaultValue)
        {
            _name = name;
            _default = defaultValue;
        }

        public bool Exist => DataService.Instance.HasKey(_name) &&
                             !string.IsNullOrEmpty(DataService.Instance.GetString(_name, _default));

        public string Value
        {
            get => DataService.Instance.GetString(_name, _default);
            set
            {
                if (value != Value)
                    DataService.Instance.SetString(_name, value);
            }
        }

        public void Clear()
        {
            DataService.Instance.DeleteKey(_name);
        }
    }

    public class ListDataPref<T>
    {
        private static readonly Service<DataService> DataService = new();

        private readonly string _name;
        private List<T> _current;
        public ListDataPref(string name)
        {
            _name = name;
            _current = new List<T>();
            var value = DataService.Instance.GetString(_name, "");
            if (string.IsNullOrEmpty(value))
            {
                _current = new List<T>();
                return;
            }
            else
            {
                var temp = value.Split(',');
                _current = temp.Select(x => (T)Convert.ChangeType(x, typeof(T))).ToList();
            }
        }

        public List<T> Value
        {
            get => _current;
            set
            {
                _current = value;
                var temp = string.Join(",", value.Select(x => x.ToString()));
                if (string.IsNullOrEmpty(temp) == false)
                {
                    DataService.Instance.SetString(_name, temp);
                }
                else{
                    DataService.Instance.DeleteKey(_name);
                }
            }
        }

        public void Add(T idx)
        {
            _current.Add(idx);
            Value = _current;
        }

        public void Remove(T idx)
        {
            _current.Remove(idx);
            Value = _current;
        }

        public void Clear()
        {
            _current = new List<T>();
            Value = _current;
        }

        public bool Contains(T idx)
        {
            return _current.Contains(idx);
        }

        public int Count => Value.Count;
    }

    public class DictionaryDataPref<TKey, TValue>
    {
        private static readonly Service<DataService> DataService = new();
        private readonly string _name;
        private readonly Dictionary<TKey, TValue> _default;
        private Dictionary<TKey, TValue> _cache;

        public Action<Dictionary<TKey, TValue>> OnChanged;

        public DictionaryDataPref(string name, Dictionary<TKey, TValue> defaultValue = null)
        {
            _name = name;
            _default = defaultValue ?? new Dictionary<TKey, TValue>();

            Load();
        }

        public bool Exist =>
            DataService.Instance.HasKey(_name) &&
            !string.IsNullOrEmpty(DataService.Instance.GetString(_name, string.Empty));

        public Dictionary<TKey, TValue> Value
        {
            get => _cache;
            set
            {
                if (value == null) value = new Dictionary<TKey, TValue>();
                _cache = value;
                Save();
                OnChanged?.Invoke(_cache);
            }
        }

        public void Clear()
        {
            _cache = new Dictionary<TKey, TValue>();
            DataService.Instance.DeleteKey(_name);
            OnChanged?.Invoke(_cache);
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_cache);
                DataService.Instance.SetString(_name, json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error saving {_name}: {e}");
            }
        }

        private void Load()
        {
            try
            {
                string str = DataService.Instance.GetString(_name, "");
                if (string.IsNullOrEmpty(str))
                {
                    _cache = _default != null ? Clone(_default) : new Dictionary<TKey, TValue>();
                    Save();
                }
                else
                {
                    _cache = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(str);
                    if (_cache == null) _cache = new Dictionary<TKey, TValue>();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error loading {_name}: {e}");
                _cache = new Dictionary<TKey, TValue>();
            }
        }

        private Dictionary<TKey, TValue> Clone(Dictionary<TKey, TValue> source)
        {
            try
            {
                var json = JsonConvert.SerializeObject(source);
                return JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
            }
            catch
            {
                return new Dictionary<TKey, TValue>();
            }
        }

        // Dictionary functions
        public void Add(TKey key, TValue value)
        {
            _cache[key] = value;
            Save();
            OnChanged?.Invoke(_cache);
        }

        public bool Remove(TKey key)
        {
            bool result = _cache.Remove(key);
            if (result)
            {
                Save();
                OnChanged?.Invoke(_cache);
            }
            return result;
        }

        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public int Count => _cache.Count;

        public ICollection<TKey> Keys => _cache.Keys;

        public ICollection<TValue> Values => _cache.Values;

        public TValue this[TKey key]
        {
            get => _cache[key];
            set
            {
                _cache[key] = value;
                Save();
                OnChanged?.Invoke(_cache);
            }
        }
    }

    public class ClassDataPref<T> where T : class, new()
    {
        private static readonly Service<DataService> DataService = new();
        private readonly string _name;
        private readonly T _default;
        private T _cache;

        public Action<T> OnChanged;

        public ClassDataPref(string name, T defaultValue = null)
        {
            _name = name;
            _default = defaultValue ?? new T();

            Load();
        }

        public bool Exist =>
            DataService.Instance.HasKey(_name) &&
            !string.IsNullOrEmpty(DataService.Instance.GetString(_name, string.Empty));

        public T Value
        {
            get => _cache;
            set
            {
                if (value == null) value = new T();
                _cache = value;
                Save();
                OnChanged?.Invoke(_cache);
            }
        }

        public void Clear()
        {
            _cache = new T();
            DataService.Instance.DeleteKey(_name);
            OnChanged?.Invoke(_cache);
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_cache);
                DataService.Instance.SetString(_name, json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error saving {_name}: {e}");
            }
        }

        private void Load()
        {
            try
            {
                string str = DataService.Instance.GetString(_name, "");
                if (string.IsNullOrEmpty(str))
                {
                    _cache = _default != null ? Clone(_default) : new T();
                    Save();
                }
                else
                {
                    _cache = JsonConvert.DeserializeObject<T>(str);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error loading {_name}: {e}");
                _cache = new T();
            }
        }

        private T Clone(T source)
        {
            try
            {
                var json = JsonConvert.SerializeObject(source);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return new T();
            }
        }
    }

    /// <summary>
    /// ListJsonPref - A list data preference that uses JSON serialization.
    /// Use this for complex types (classes, structs) instead of ListDataPref.
    /// </summary>
    public class ListJsonPref<T>
    {
        private static readonly Service<DataService> DataService = new();

        private readonly string _name;
        private List<T> _current;

        public Action<List<T>> OnChanged;

        public ListJsonPref(string name)
        {
            _name = name;
            Load();
        }

        public List<T> Value
        {
            get => _current;
            set
            {
                _current = value ?? new List<T>();
                Save();
                OnChanged?.Invoke(_current);
            }
        }

        public void Add(T item)
        {
            _current.Add(item);
            Save();
            OnChanged?.Invoke(_current);
        }

        public void Remove(T item)
        {
            _current.Remove(item);
            Save();
            OnChanged?.Invoke(_current);
        }

        public void Clear()
        {
            _current = new List<T>();
            DataService.Instance.DeleteKey(_name);
            OnChanged?.Invoke(_current);
        }

        public bool Contains(T item)
        {
            return _current.Contains(item);
        }

        public int Count => _current.Count;

        public T this[int index]
        {
            get => _current[index];
            set
            {
                _current[index] = value;
                Save();
                OnChanged?.Invoke(_current);
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_current);
                DataService.Instance.SetString(_name, json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ListJsonPref] Error saving {_name}: {e}");
            }
        }

        private void Load()
        {
            try
            {
                string str = DataService.Instance.GetString(_name, "");
                if (string.IsNullOrEmpty(str))
                {
                    _current = new List<T>();
                }
                else
                {
                    _current = JsonConvert.DeserializeObject<List<T>>(str);
                    if (_current == null) _current = new List<T>();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ListJsonPref] Error loading {_name}: {e}");
                _current = new List<T>();
            }
        }
    }

}