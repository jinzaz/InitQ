﻿using InitQ.Model;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InitQ.Cache
{
    public class RedisCacheService : ICacheService
    {
        int Default_Timeout = 60 * 10 * 10;//默认超时时间（单位秒）
        JsonSerializerSettings jsonConfig = new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        ConnectionMultiplexer connectionMultiplexer;
        IDatabase database;
        ISubscriber sub;

        class CacheObject<T>
        {
            public int ExpireTime { get; set; }
            public bool ForceOutofDate { get; set; }
            public T Value { get; set; }
        }

        public RedisCacheService(ConfigurationOptions options)
        {
            //设置线程池最小连接数
            ThreadPool.SetMinThreads(200, 200);

            connectionMultiplexer = ConnectionMultiplexer.Connect(options);
            database = connectionMultiplexer.GetDatabase();
            //通道广播
            sub = connectionMultiplexer.GetSubscriber();
        }

        /// <summary>
        /// desc    redis连接对象初始化缓存服务
        /// ps      通过传递过来的对象，直接支持哨兵模式
        /// author  hyz
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public RedisCacheService(ConnectionMultiplexer connection, int dbIndex)
        {
            //设置线程池最小连接数
            ThreadPool.SetMinThreads(200, 200);

            connectionMultiplexer = connection;// ConnectionMultiplexer.Connect(options);

            database = connectionMultiplexer.GetDatabase(dbIndex);
            //通道广播
            sub = connectionMultiplexer.GetSubscriber();
        }

        public RedisCacheService(string configuration, int dbIndex)
        {
            //设置线程池最小连接数
            ThreadPool.SetMinThreads(200, 200);

            connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);

            database = connectionMultiplexer.GetDatabase(dbIndex);
            //通道广播
            sub = connectionMultiplexer.GetSubscriber();
        }

        public ConnectionMultiplexer GetRedis()
        {
            return connectionMultiplexer;
        }

        public IDatabase GetDatabase()
        {
            return database;
        }

        public ISubscriber GetSubscriber()
        {
            return sub;
        }

        /// <summary>
        /// 连接超时设置
        /// </summary>
        public int TimeOut
        {
            get
            {
                return Default_Timeout;
            }
            set
            {
                Default_Timeout = value;
            }
        }

        public string Get(string key)
        {
            return database.StringGet(key);
        }

        public async Task<string> GetAsync(string key)
        {
            return await database.StringGetAsync(key);
        }

        public T Get<T>(string key)
        {
            var cacheValue = database.StringGet(key);
            if (string.IsNullOrEmpty(cacheValue)) return default(T);
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var cacheValue = await database.StringGetAsync(key);
            if (string.IsNullOrEmpty(cacheValue)) return default(T);
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }

        public bool Set(string key, object data)
        {
            return database.StringSet(key, JsonConvert.SerializeObject(data));
        }

        public Task<bool> SetAsync(string key, object data)
        {
            return database.StringSetAsync(key, JsonConvert.SerializeObject(data));
        }

        public bool Set(string key, object data, int cacheTime)
        {
            var timeSpan = TimeSpan.FromSeconds(cacheTime);
            return database.StringSet(key, JsonConvert.SerializeObject(data), timeSpan);
        }

        public async Task<bool> SetAsync(string key, object data, int cacheTime)
        {
            var timeSpan = TimeSpan.FromSeconds(cacheTime);
            return await database.StringSetAsync(key, JsonConvert.SerializeObject(data), timeSpan);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        public bool Remove(string key)
        {
            return database.KeyDelete(key, CommandFlags.HighPriority);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await database.KeyDeleteAsync(key, CommandFlags.HighPriority);
        }
        /// <summary>
        /// 判断key是否存在
        /// </summary>
        public bool Exists(string key)
        {
            return database.KeyExists(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await database.KeyExistsAsync(key);
        }
        /// <summary>
        /// 模糊查询key的集合
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] ScriptEvaluateKeys(string key)
        {
            var redisResult = database.ScriptEvaluate(LuaScript.Prepare(
                //Redis的keys模糊查询：
                " local res = redis.call('KEYS', @keypattern) " +
                " return res "), new { @keypattern = key });

            string[] preSult = (string[])redisResult;//将返回的结果集转为数组
            return preSult;
        }

        public long ListLeftPush(string key, string value)
        {
            var res = database.ListLeftPush(key, value);
            return res;
        }

        public long ListLeftPush<T>(string key, T value) where T : class
        {
            var res = database.ListLeftPush(key, JsonConvert.SerializeObject(value));
            return res;
        }

        public async Task<long> ListLeftPushAsync(string key, string value)
        {
            var res = await database.ListLeftPushAsync(key, value);
            return res;
        }

        public async Task<long> ListLeftPushAsync<T>(string key, T value) where T : class
        {
            var res = await database.ListLeftPushAsync(key, JsonConvert.SerializeObject(value));
            return res;
        }

        public long ListRightPush(string key, string value)
        {
            var res = database.ListRightPush(key, value);
            return res;
        }

        public long ListRightPush<T>(string key, T value) where T : class
        {
            var res = database.ListRightPush(key, JsonConvert.SerializeObject(value));
            return res;
        }

        public async Task<long> ListRightPushAsync(string key, string value)
        {
            var res = await database.ListRightPushAsync(key, value);
            return res;
        }

        public async Task<long> ListRightPushAsync<T>(string key, T value) where T : class
        {
            var res = await database.ListRightPushAsync(key, JsonConvert.SerializeObject(value));
            return res;
        }

        public T ListLeftPop<T>(string key) where T : class
        {
            var cacheValue = database.ListLeftPop(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }
        public async Task<T> ListLeftPopAsync<T>(string key) where T : class
        {
            var cacheValue = await database.ListLeftPopAsync(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }


        public T ListRightPop<T>(string key) where T : class
        {
            var cacheValue = database.ListRightPop(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }
        public async Task<T> ListRightPopAsync<T>(string key) where T : class
        {
            var cacheValue = await database.ListRightPopAsync(key);
            if (string.IsNullOrEmpty(cacheValue)) return null;
            var res = JsonConvert.DeserializeObject<T>(cacheValue);
            return res;
        }


        public string ListLeftPop(string key)
        {
            var cacheValue = database.ListLeftPop(key);
            return cacheValue;
        }
        public async Task<string> ListLeftPopAsync(string key)
        {
            var cacheValue = await database.ListLeftPopAsync(key);
            return cacheValue;
        }


        public string ListRightPop(string key)
        {
            var cacheValue = database.ListRightPop(key);
            return cacheValue;
        }
        public async Task<string> ListRightPopAsync(string key)
        {
            var cacheValue = await database.ListRightPopAsync(key);
            return cacheValue;
        }



        public long ListLength(string key)
        {
            return database.ListLength(key);
        }
        public async Task<long> ListLengthAsync(string key)
        {
            return await database.ListLengthAsync(key);
        }


        public long Publish(string key, string msg)
        {
            var count = database.Publish(key, msg);
            return count;
        }
        public async Task<long> PublishAsync(string key, string msg)
        {
            var count = await database.PublishAsync(key, msg);
            return count;
        }


        public void Subscribe(string key, Action<RedisChannel, RedisValue> action)
        {
            sub.Subscribe(key, action);
        }

        public async Task SubscribeAsync(string key, Action<RedisChannel, RedisValue> action)
        {
            await sub.SubscribeAsync(key, action);
        }


        public async Task<bool> SortedSetAddAsync(string key, string msg, double score)
        {
            var bl = await database.SortedSetAddAsync(key, msg, score);
            return bl;
        }


        public async Task<string[]> SortedSetRangeByScoreAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending)
        {
            var arry = await database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order);
            return arry.ToStringArray();
        }


        public async Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, double start, double stop)
        {
            var bl = await database.SortedSetRemoveRangeByScoreAsync(key, start, stop);
            return bl;
        }

        public async Task<bool> SortedSetAddAsync(string key, string msg, DateTime time)
        {
            var score = (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            var bl = await database.SortedSetAddAsync(key, msg, score);
            return bl;
        }


        public async Task<string[]> SortedSetRangeByScoreAsync(string key, DateTime? startTime, DateTime? stopTime, Exclude exclude = Exclude.None, Order order = Order.Ascending)
        {
            var start = double.NegativeInfinity;
            var stop = double.PositiveInfinity;
            if (startTime.HasValue)
            {
                start = (startTime.Value.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
            if (stopTime.HasValue)
            {
                stop = (stopTime.Value.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
            var arry = await database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order);
            return arry.ToStringArray();
        }


        public async Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, DateTime? startTime, DateTime? stopTime)
        {
            var start = double.NegativeInfinity;
            var stop = double.PositiveInfinity;
            if (startTime.HasValue)
            {
                start = (startTime.Value.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
            if (stopTime.HasValue)
            {
                stop = (stopTime.Value.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
            var bl = await database.SortedSetRemoveRangeByScoreAsync(key, start, stop);
            return bl;
        }

        public bool Set(string key, object data, TimeSpan cacheTime)
        {
            return database.StringSet(key, JsonConvert.SerializeObject(data), cacheTime);
        }

        public async Task<bool> SetAsync(string key, object data, TimeSpan cacheTime)
        {
            return await database.StringSetAsync(key, JsonConvert.SerializeObject(data), cacheTime);
        }


        public long Increment(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var res = database.StringIncrement(key, value, flags);
            database.KeyExpire(key, cacheTime);
            return res;
        }

        public async Task<long> IncrementAsync(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var res = await database.StringIncrementAsync(key, value, flags);
            await database.KeyExpireAsync(key, cacheTime);
            return res;
        }



        public long Decrement(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var res = database.StringDecrement(key, value, flags);
            database.KeyExpire(key, cacheTime);
            return res;
        }

        public async Task<long> DecrementAsync(string key, TimeSpan cacheTime, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var res = await database.StringDecrementAsync(key, value, flags);
            await database.KeyExpireAsync(key, cacheTime);
            return res;
        }

        public bool KeyExpire(string key, TimeSpan cacheTime, CommandFlags flags = CommandFlags.None)
        {
            var res = database.KeyExpire(key, cacheTime, flags);
            return res;
        }

        public async Task<bool> KeyExpireAsync(string key, TimeSpan cacheTime, CommandFlags flags = CommandFlags.None)
        {
            var res = await database.KeyExpireAsync(key, cacheTime, flags);
            return res;
        }
    }
}
