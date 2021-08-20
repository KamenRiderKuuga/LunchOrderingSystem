using LunchOrderingSystem.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LunchOrderingSystem.Server
{
    /// <summary>
    /// 用于初始化一些静态的配置类
    /// </summary>
    public class LazyStaticResources
    {
        /// <summary>
        /// 默认反序列化配置
        /// </summary>
        private static JsonSerializerOptions _deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        private static Lazy<List<MenuItem>> _menuInfo = new Lazy<List<MenuItem>>(() =>
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "menuList.json");
            return JsonSerializer.Deserialize<List<MenuItem>>(File.ReadAllText(configPath), _deserializeOptions);
        });

        public static List<MenuItem> MenuInfoData { get => _menuInfo.Value; }
    }
}
