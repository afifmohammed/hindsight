using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using MediatR.Extras.Logging;
using Newtonsoft.Json;

namespace MediatR.Extras
{
    public delegate string AppSettingsProvider(string key);

    public delegate string ConnectionStringProvider(string key);

    public class Configured<TKey> : Configured<TKey, string>
    { }

    public class Configured<TKey, TValue> : IRequest<TValue>
    {
        public string Key { get { return typeof(TKey).Name; } }

        private TValue @default;
        private bool hasDefault;
        public TValue Default
        {
            get { return this.@default; }
            set
            {
                this.hasDefault = !ReferenceEquals(value, null);
                this.@default = value;
            }
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TValue>.Default.GetHashCode(Default);
        }

        public static bool operator ==(Configured<TKey, TValue> left, Configured<TKey, TValue> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Configured<TKey, TValue> left, Configured<TKey, TValue> right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;

            var other = obj as Configured<TKey, TValue>;

            if (other == null) return false;

            if (!other.hasDefault && !this.hasDefault) return true;

            if (this.hasDefault) return this.@default.Equals(other.@default);

            return false;
        }

        public override string ToString()
        {
            IDictionary<string, object> list = new ExpandoObject();
            list.Add("Key", this.Key);
            if (this.hasDefault)
                list.Add("Default", this.Default);

            return JsonConvert.SerializeObject(list);
        }
    }

    public class ConnectionString<TKey> : IRequest<string>
    {
        public string Key { get { return typeof(TKey).Name; } }

        private string @default;
        private bool hasDefault;
        public string Default
        {
            get { return this.@default; }
            set
            {
                this.hasDefault = !ReferenceEquals(value, null);
                this.@default = value;
            }
        }

        public override int GetHashCode()
        {
            return EqualityComparer<string>.Default.GetHashCode(Default);
        }

        public static bool operator ==(ConnectionString<TKey> left, ConnectionString<TKey> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ConnectionString<TKey> left, ConnectionString<TKey> right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;

            var other = obj as ConnectionString<TKey>;

            if (other == null) return false;

            if (!other.hasDefault && !this.hasDefault) return true;

            if (this.hasDefault) return this.@default.Equals(other.@default);

            return false;
        }

        public override string ToString()
        {
            IDictionary<string, object> list = new ExpandoObject();
            list.Add("Key", this.Key);
            if (this.hasDefault)
                list.Add("Default", this.Default);

            return JsonConvert.SerializeObject(list);
        }
    }

    class LookupConnectionStringFor<TKey> : IRequestHandler<ConnectionString<TKey>, string>
    {
        private readonly ConnectionStringProvider provider;
        private static readonly ILog Log = LogProvider.GetLogger(typeof(LookupConnectionStringFor<TKey>).CSharpName());

        public LookupConnectionStringFor(ConnectionStringProvider provider)
        {
            this.provider = provider;
        }

        public string Handle(ConnectionString<TKey> message)
        {
            var noDefault = new ConnectionString<TKey>();

            try
            {
                var value = provider(message.Key);
                if (ReferenceEquals(value, null))
                    throw new ConfigurationErrorsException(string.Format("No Connection string defined for {0}", message.Key));
                return value;
            }
            catch (Exception)
            {
                if (noDefault == message) throw;
                Log.Log(LogLevel.Debug, () => "No {TypeOfKey} entry found for {Key}, falling back on the default value of {Default}", null, "ConnectionString", message.Key, message.Default);
                return message.Default;
            }
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }

    class LookupAppSettingsFor<TKey> : IRequestHandler<Configured<TKey>, string>
    {
        private readonly AppSettingsProvider provider;
        private static readonly ILog Log = LogProvider.GetLogger(typeof (LookupAppSettingsFor<TKey>).CSharpName());

        public LookupAppSettingsFor(AppSettingsProvider provider)
        {
            this.provider = provider;
        }

        public string Handle(Configured<TKey> message)
        {
            var noDefault = new Configured<TKey>();

            try
            {
                var value = provider(message.Key);
                if (ReferenceEquals(value, null))
                    throw new ConfigurationErrorsException(string.Format("No Appsettings key defined for {0}", message.Key));
                return value;
            }
            catch (Exception)
            {
                if (noDefault == message) throw;
                Log.Log(LogLevel.Debug, () => "No {TypeOfKey} entry found for {Key}, falling back on the default value of {Default}", null, "AppSettings", message.Key, message.Default);
                return message.Default;
            }
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }

    class LookupAppSettingsFor<TKey, TValue> : IRequestHandler<Configured<TKey, TValue>, TValue>
    {
        private readonly AppSettingsProvider provider;

        private static readonly ILog Log = LogProvider.GetLogger(typeof (LookupAppSettingsFor<TKey, TValue>).CSharpName());

        public LookupAppSettingsFor(AppSettingsProvider provider)
        {
            this.provider = provider;
        }

        public TValue Handle(Configured<TKey, TValue> message)
        {
            var noDefault = new Configured<TKey, TValue>();

            try
            {
                var value = provider(message.Key);
                if (ReferenceEquals(value, null))
                    throw new ConfigurationErrorsException(string.Format("No Appsettings key defined for {0}", message.Key));
                try
                {
                    return (TValue)Convert.ChangeType(value, typeof(TValue));
                }
                catch (Exception)
                {
                    if (noDefault == message) throw;
                    Log.Log(LogLevel.Warn, () => "Couldn't parse {TypeOfKey} value {Value} for key {Key} to {TypeOfValue}, falling back on the default value of {Default}",
                        null,
                        "AppSettings",
                        value,
                        message.Key,
                        typeof(TValue).CSharpName(),
                        message.Default);

                    return message.Default;
                }
            }
            catch (Exception)
            {
                if (noDefault == message) throw;
                Log.Log(LogLevel.Debug, () => "No {TypeOfKey} entry found for {Key}, falling back on the default value of {Default}", null, "AppSettings", message.Key, message.Default);
                return message.Default;
            }
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }
}