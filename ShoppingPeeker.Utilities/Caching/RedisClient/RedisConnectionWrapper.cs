﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using StackExchange.Redis;

using ShoppingPeeker.Utilities.Ioc;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace ShoppingPeeker.Utilities.Caching
{
    /// <summary>
    /// Redis 的连接管理
    /// Redis connection wrapper implementation
    /// </summary>
    public class RedisConnectionWrapper
    {

        private static object _RedisConnectionWrapperLock = new object();
        public static RedisConnectionWrapper Current
        {

            get
            {
                RedisConnectionWrapper connectionWrapper;
                //互斥锁定写入资源
                lock (_RedisConnectionWrapperLock)
                {
                    connectionWrapper = Singleton<RedisConnectionWrapper>.Instance;
                    if (null == connectionWrapper)
                    {
                        connectionWrapper = new RedisConnectionWrapper();
                        Singleton<RedisConnectionWrapper>.Instance = connectionWrapper;
                    }
                }

                return connectionWrapper;
            }
        }

        #region Fields

        private readonly Lazy<string> _connectionString;

        private volatile ConnectionMultiplexer _connection=null;
        private volatile RedLockFactory _redisLockFactory=null;
        private readonly object _lock = new object();

        #endregion

        #region Ctor



        public RedisConnectionWrapper(string connString = null)
        {
            this._connectionString = new Lazy<string>(
                () =>
                {
                    if (string.IsNullOrEmpty(connString))
                    {
                        connString = GetConnectionString();
                    }
                    return connString;
                }
                );

        }
        #endregion

        #region Utilities

        /// <summary>
        /// 从配置加载Redis 的连接字符串
        /// 连接字符串配置格式： ConnectionMultiplexer.Connect("mycache.redis.cache.windows.net,abortConnect=false,ssl=true,password=...");
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            try
            {

                string redisHost = ConfigHelper.HostingConfiguration.GetConfig("redisHost");
                string redisPort = ConfigHelper.HostingConfiguration.GetConfig("redisPort");
                string redisPassword = ConfigHelper.HostingConfiguration.GetConfig("redisPwd");
                return GetConnectionString(redisHost, redisPort, redisPassword);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GetConnectionString(string redisHost, string redisPort, string redisPassword)
        {
            string RedisConnectionString = string.Format("{0}:{1},password={2}", redisHost, redisPort, redisPassword);
            return RedisConnectionString;

        }

        /// <summary>
        /// Get connection to Redis servers
        /// </summary>
        /// <returns></returns>
        protected ConnectionMultiplexer GetConnection()
        {
            if (_connection != null && _connection.IsConnected) return _connection;

            lock (_lock)
            {
                if (_connection != null && _connection.IsConnected) return _connection;

                if (_connection != null)
                {
                    //Connection disconnected. Disposing connection...
                    _connection.Dispose();
                }

                //Creating new instance of Redis Connection
                _connection = ConnectionMultiplexer.Connect(_connectionString.Value);
            }

            return _connection;
        }

        /// <summary>
        /// Create instance of RedisLockFactory
        /// </summary>
        /// <returns>RedisLockFactory</returns>
        protected RedLockFactory CreateRedisLockFactory()
        {
            //get password and value whether to use ssl from connection string
            var password = string.Empty;
            var useSsl = false;
            foreach (var option in GetConnectionString().Split(',').Where(option => option.Contains('=')))
            {
                switch (option.Substring(0, option.IndexOf('=')).Trim().ToLowerInvariant())
                {
                    case "password":
                        password = option.Substring(option.IndexOf('=') + 1).Trim();
                        break;
                    case "ssl":
                        bool.TryParse(option.Substring(option.IndexOf('=') + 1).Trim(), out useSsl);
                        break;
                }
            }

            //create RedisLockFactory for using Redlock distributed lock algorithm
            var endPoints = GetEndPoints().Select(endPoint => new RedLockEndPoint
            {
                EndPoint = endPoint,
                Password = password,
                Ssl = useSsl
            }).ToList();

            return new RedLockFactory( new RedLockConfiguration(endPoints));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Obtain an interactive connection to a database inside redis
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value</param>
        /// <returns>Redis cache database</returns>
        public IDatabase GetDatabase(int? db = null)
        {
            return GetConnection().GetDatabase(db ?? -1); //_settings.DefaultDb);
        }

        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        /// <param name="endPoint">The network endpoint</param>
        /// <returns>Redis server</returns>
        public IServer GetServer(EndPoint endPoint)
        {
            return GetConnection().GetServer(endPoint);
        }

        /// <summary>
        /// Gets all endpoints defined on the server
        /// </summary>
        /// <returns>Array of endpoints</returns>
        public EndPoint[] GetEndPoints()
        {
            return GetConnection().GetEndPoints();
        }

        /// <summary>
        /// Delete all the keys of the database
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value<</param>
        public void FlushDatabase(int? db = null)
        {
            var endPoints = GetEndPoints();

            foreach (var endPoint in endPoints)
            {
                GetServer(endPoint).FlushDatabase(db ?? -1); //_settings.DefaultDb);
            }
        }

        /// <summary>
        /// Perform some action with Redis distributed lock
        /// </summary>
        /// <param name="resource">The thing we are locking on</param>
        /// <param name="expirationTime">The time after which the lock will automatically be expired by Redis</param>
        /// <param name="action">Action to be performed with locking</param>
        /// <returns>True if lock was acquired and action was performed; otherwise false</returns>
        public bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action)
        {
            //use RedLock library
            using (var redisLock = _redisLockFactory.CreateLock(resource, expirationTime))
            {
                //ensure that lock is acquired
                if (!redisLock.IsAcquired)
                    return false;

                //perform action
                action();
                return true;
            }
        }

        /// <summary>
        /// Release all resources associated with this object
        /// </summary>
        public void Dispose()
        {
            //dispose ConnectionMultiplexer
            if (_connection != null)
                _connection.Dispose();

            //dispose RedisLockFactory
            if (_redisLockFactory != null)
                _redisLockFactory.Dispose();
        }

        #endregion
    }
}
