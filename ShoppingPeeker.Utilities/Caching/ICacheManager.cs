using System.Collections.Generic;
 namespace ShoppingPeeker.Utilities.Caching
{
    /// <summary>
    /// Cache manager interface
    /// ��ʱֻʵ�������õ�Cache���������
    /// MemCache Redis δ����ʵ��
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        T Get<T>(string key);

        /// <summary>
        /// ������ݵ�������
        /// </summary>
        /// <param name="key">��</param>
        /// <param name="data">����</param>
        /// <param name="cacheTime">����ʱ�䣨��λ���룩��</param>
        void Set(string key, object data, int cacheTime = CacheConfigFactory.DefaultTimeOut);

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        bool IsHasSet(string key);

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        void Remove(string key);

        ///// <summary>
        ///// Removes items by pattern
        ///// </summary>
        ///// <param name="pattern">pattern</param>
        //void RemoveByPattern(string pattern);

        /// <summary>
        /// Clear all cache data
        /// </summary>
        void Clear();

        ///// <summary>
        ///// get the cache manager all keys 
        ///// </summary>
        ///// <returns></returns>
        //IEnumerable<string> GetCacheKeys();
    }
}
