using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WicNet.Utilities;

namespace WicNetExplorer.Utilities
{
    public abstract class Serializable<T> : INotifyPropertyChanged where T : new()
    {
        private readonly ConcurrentDictionary<string, object?> _values = new(StringComparer.OrdinalIgnoreCase);

        public event PropertyChangedEventHandler? PropertyChanged;

        public static async Task<T?> DeserializeAsync(string filePath, bool returnDefaultIfError = false)
        {
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                return new T();

            try
            {
                using var stream = File.OpenRead(filePath);
                return await DeserializeAsync(stream, returnDefaultIfError).ConfigureAwait(false);
            }
            catch
            {
                if (returnDefaultIfError)
                    return default;

                return new T();
            }
        }

        public static async Task<T?> DeserializeAsync(Stream stream, bool returnDefaultIfError = false)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                return await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
            }
            catch
            {
                if (returnDefaultIfError)
                    return default;

                return new T();
            }
        }

        public static T? Deserialize(string filePath, bool returnDefaultIfError = false)
        {
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));

            if (!IOUtilities.PathIsFile(filePath))
                return new T();

            try
            {
                using var stream = File.OpenRead(filePath);
                return Deserialize(stream, returnDefaultIfError);
            }
            catch
            {
                if (returnDefaultIfError)
                    return default;

                return new T();
            }
        }

        public static T? DeserializeFromString(string json, bool returnDefaultIfError = false)
        {
            if (json is null)
                throw new ArgumentNullException(nameof(json));

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                if (returnDefaultIfError)
                    return default;

                return new T();
            }
        }

        public static T? Deserialize(Stream stream, bool returnDefaultIfError = false)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                var json = new StreamReader(stream, Encoding.UTF8).ReadToEnd();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                if (returnDefaultIfError)
                    return default;

                return new T();
            }
        }

        public virtual string Serialize() => JsonSerializer.Serialize((object)this, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        }); // note: you *must* cast as object

        public virtual void Serialize(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var writer = new Utf8JsonWriter(stream);
            JsonSerializer.Serialize(writer, (object)this);
        }

        public virtual void Serialize(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            IOUtilities.FileEnsureDirectory(filePath);
            IOUtilities.FileDelete(filePath);
            using var writer = File.OpenWrite(filePath);
            Serialize(writer);
        }

        public virtual async Task SerializeAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            await JsonSerializer.SerializeAsync(stream, (object)this).ConfigureAwait(false); // note: you *must* cast as object
        }

        public virtual async Task SerializeAsync(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            IOUtilities.FileEnsureDirectory(filePath);
            IOUtilities.FileDelete(filePath);
            using var writer = File.OpenWrite(filePath);
            await SerializeAsync(writer).ConfigureAwait(false);
        }

        protected void OnPropertyChanged(string name) => OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);

        protected Tv? GetPropertyValue<Tv>(Tv? defaultValue = default, [CallerMemberName] string? propertyName = null)
        {
            if (!TryGetPropertyValue(out var value, propertyName))
                return defaultValue;

            if (!Conversions.TryChangeType<Tv>(value, out var convertedValue))
                return defaultValue;

            return convertedValue;
        }

        protected virtual bool TryGetPropertyValue(out object? value, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            return _values.TryGetValue(propertyName, out value);
        }

        protected virtual bool SetPropertyValue(object? value, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var changed = true;
            _values.AddOrUpdate(propertyName, value, (k, o) =>
            {
                changed = !Equals(value, o);
                return value;
            });

            if (changed)
            {
                OnPropertyChanged(propertyName);
            }
            return changed;
        }

        public virtual void CopyFrom(Serializable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var frozen = other._values.ToArray();
            foreach (var kv in frozen)
            {
                SetPropertyValue(kv.Value, kv.Key);
            }
        }

        protected virtual T CreateNew() => new();
        public virtual T Clone()
        {
            var clone = CreateNew()!;
            ((Serializable<T>)(object)clone).CopyFrom(this);
            return clone;
        }

        public static string BackupDirectoryName => "bak";
        public static void Backup(string filePath, TimeSpan? maxDuration = null)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                BackupPrivate(filePath, maxDuration);
            }
            catch
            {
                // do nothing, will work next time probably...
            }
        }

        private static void BackupPrivate(string filePath, TimeSpan? maxDuration = null)
        {
            if (!IOUtilities.PathIsFile(filePath))
                return;

            // *warning* if you change format here, change parsing code below
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var bakPath = Path.Combine(Path.GetDirectoryName(filePath)!, BackupDirectoryName, string.Format("{0:yyyy}_{0:MM}_{0:dd}.{1}.{2}.json", DateTime.Now, Environment.TickCount, fileName));
            IOUtilities.FileEnsureDirectory(bakPath);
            IOUtilities.FileOverwrite(filePath, bakPath, true, false);
            if (maxDuration.HasValue)
            {
                var dir = Path.GetDirectoryName(bakPath)!;
                foreach (var file in Directory.GetFiles(dir))
                {
                    if (string.Compare(file, bakPath, StringComparison.OrdinalIgnoreCase) == 0)
                        continue;

                    var name = Path.GetFileNameWithoutExtension(file);
                    var tick = name.IndexOf('.');
                    if (tick < 0)
                        continue;

                    var dateName = name[..tick].Split('_');
                    if (dateName.Length != 3)
                        continue;

                    var month = 0;
                    var day = 0;
                    if (!int.TryParse(dateName[0], out var year) &&
                        !int.TryParse(dateName[1], out month) &&
                        !int.TryParse(dateName[2], out day))
                        continue;

                    DateTime dt;
                    try
                    {
                        dt = new DateTime(year, month, day);
                    }
                    catch
                    {
                        continue;
                    }

                    if ((DateTime.Now - dt) > maxDuration.Value)
                    {
                        IOUtilities.FileDelete(file, true, false);
                    }
                }
            }
        }
    }
}
