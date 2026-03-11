using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JanusRequest
{
    /// <summary>
    /// Resolves a media type (Content-Type) to a registered value with fallback rules:
    /// 1) Exact match (normalized)
    /// 2) Structured suffix match (RFC 6839): "application/*+json" -> "+json"
    ///
    /// It also caches redirects: if "application/error+json" falls back to "+json",
    /// the resolver stores that mapping for faster future lookups.
    ///
    /// If a value is later registered for an exact media type, any cached redirect for that
    /// media type is removed automatically.
    /// </summary>
    internal sealed class MediaTypeMap<TValue> : IDictionary<string, TValue>
    {
        private readonly Dictionary<string, string> _redirectCache;
        private readonly Dictionary<string, TValue> _values;
        private readonly ReaderWriterLockSlim _lock;

        public ICollection<string> Keys => _values.Keys;

        public ICollection<TValue> Values => _values.Values;

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        public TValue this[string key]
        {
            get => TryGetValue(key, out var value) ? value : default;
            set => Set(key, value);
        }

        public MediaTypeMap()
        {
            _values = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
            _redirectCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Registers a value for a specific media type key.
        /// Keys may be exact (e.g. "application/json") or suffix keys (e.g. "+json").
        /// </summary>
        public void Set(string key, TValue value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            key = MediaTypeNormalizer.NormalizeMediaType(key);

            _lock.EnterWriteLock();
            try
            {
                _values[key] = value;

                // If someone cached "application/foo+json" -> "+json" and then later registers
                // "application/foo+json" explicitly, remove the redirect so exact wins.
                _redirectCache.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            key = MediaTypeNormalizer.NormalizeMediaType(key);

            _lock.EnterWriteLock();
            try
            {
                _redirectCache.Remove(key);
                return _values.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _values.Clear();
                _redirectCache.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGetValue(string contentTypeOrMediaType, out TValue value)
        {
            if (string.IsNullOrWhiteSpace(contentTypeOrMediaType))
            {
                value = default;
                return false;
            }

            var exact = MediaTypeNormalizer.NormalizeMediaType(contentTypeOrMediaType);
            if (exact.Length == 0)
            {
                value = default;
                return false;
            }

            _lock.EnterReadLock();
            try
            {
                if (_values.TryGetValue(exact, out value))
                    return true;

                if (_redirectCache.TryGetValue(exact, out var cachedKey) && _values.TryGetValue(cachedKey, out value))
                    return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            var suffix = MediaTypeNormalizer.GetStructuredSuffixMediaType(exact);
            if (suffix.Length == 0)
            {
                value = default;
                return false;
            }

            _lock.EnterWriteLock();
            try
            {
                // Re-check inside write lock (avoid races)
                if (_values.TryGetValue(exact, out value))
                    return true;

                if (_values.TryGetValue(suffix, out value))
                {
                    _redirectCache[exact] = suffix;
                    return true;
                }

                value = default;
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool ContainsKey(string key) => TryGetValue(key, out _);

        void IDictionary<string, TValue>.Add(string key, TValue value)
        {
            _values.Add(key, value);
            _redirectCache.Remove(key);
        }

        void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
        {
            _values.Add(item.Key, item.Value);
            _redirectCache.Remove(item.Key);
        }

        bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
            => TryGetValue(item.Key, out _);

        void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {

        }

        bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
            => Remove(item.Key);

        IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _values.ToArray().GetEnumerator() as IEnumerator<KeyValuePair<string, TValue>>;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _values.ToArray().GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
