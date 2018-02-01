﻿
using System;
using ShoppingPeeker.Utilities;

namespace ShoppingPeeker.Utilities.Caching
{
    /// <summary>
    /// 缓存组件类型
    /// </summary>
    public enum CacheManagerType
    {
        /// <summary>
        /// .net  自身的Cache 
        /// </summary>
        Default,
 
        /// <summary>
        /// Redis Cache
        /// </summary>
        Redis
    }

    /// <summary>
    /// 缓存组件工厂
    /// </summary>
    public class CacheConfigFactory
    {
        private const string ConfigNodeName = "CacheConfig";
        private static ICacheManager _cacheManager = null;

        /// <summary>
        /// 默认的过期的秒数
        /// </summary>
        public const int DefaultTimeOut = 30;

        /// <summary>
        /// 获取缓存管理组件
        /// 默认是 系统内存缓存
        /// 配置文件 CacheConfig 节点 。默认、MemCahed、Redis 
        /// </summary>
        /// <returns></returns>
        public static ICacheManager GetCacheManager()
        {
            if (null!=_cacheManager)
            {
                return _cacheManager;//单例模式
            }

            var managerType = CacheManagerType.Default;

            var config = ConfigHelper.AppSettingsConfiguration.GetConfig(ConfigNodeName);
            if (!string.IsNullOrEmpty(config))
            {
                try
                {
                    //首字母 大写转换后 尝试转换为对应的枚举
                    string titledConfig = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(config);
                    managerType = (CacheManagerType)Enum.Parse(typeof(CacheManagerType), titledConfig);
                }
                catch { }
            }


            ICacheManager result = null;
            switch (managerType)
            {
              
      
                case CacheManagerType.Redis:
                    result = RedisCacheManager.Current;
                    break;
                case CacheManagerType.Default:
                default:
                    result=  NativeCacheManager.Current;
                    break;
            }

            return result;

        }
    }
}
