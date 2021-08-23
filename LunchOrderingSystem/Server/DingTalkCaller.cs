using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace LunchOrderingSystem.Server
{
    public class DingTalkCaller
    {
        public HttpClient Client { get; }

        public const string CONTENT_TYPE_JSON = "application/json";

        private ILogger<DingTalkCaller> _logger;
        private readonly DingTalkConfigs _dingTalkConfigs;

        public DingTalkCaller(HttpClient client, ILogger<DingTalkCaller> logger, IOptions<DingTalkConfigs> dingTalkConfigs)
        {
            client.Timeout = new TimeSpan(0, 0, 30);
            Client = client;
            _logger = logger;
            _dingTalkConfigs = dingTalkConfigs.Value;
        }

        public async Task<bool> SendTextMsgAsync(string content, bool isAtAll)
        {
            var data = new
            {
                isAtAll = isAtAll,
                text = new
                {
                    content = content
                },
                msgtype = "text",
            };

            await CommonPostAsync(data);

            return true;
        }

        private async Task CommonPostAsync(object postData)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var requestUri = $"https://oapi.dingtalk.com/robot/send?access_token={_dingTalkConfigs.Token}&timestamp={timestamp}&sign={GetSign(timestamp)}";
            using (HttpContent httpContent = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8))
            {
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(CONTENT_TYPE_JSON);
                using (var response = Client.PostAsync(requestUri, httpContent).Result)
                {
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
            }
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        private string GetSign(string timestamp)
        {
            var mac = new HMACSHA256();
            string secret = _dingTalkConfigs.Secret;
            var stringToSign = $"{timestamp}\n{secret}";
            mac.Key = Encoding.UTF8.GetBytes(secret);
            var signData = mac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            string sign = HttpUtility.UrlEncode(Convert.ToBase64String(signData));
            return sign;
        }
    }
}
