using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CapsuleDownload
{
    public class Client
    {
        const string BASE_CAPSULE_ADDR = "https://capsule.greenmangaming.com";
        const string BASE_STOREFRONT_ADDR = "https://www.greenmangaming.com";

        HttpClient client;
        HttpClientHandler handler;

        public Client()
        {
            handler = new HttpClientHandler();
            client = new HttpClient(handler);
        }

        public string SessionToken
        {
            get
            {
                var jar = handler.CookieContainer.GetCookies(new Uri(BASE_CAPSULE_ADDR));
                var cookie = jar["login"];
                if (cookie == null)
                {
                    // Try to grab from login specifically
                    jar = handler.CookieContainer.GetCookies(new Uri($"{BASE_CAPSULE_ADDR}/login"));
                    cookie = jar["login"];
                }
                return cookie?.Value;
            }
            set
            {
                var jar = handler.CookieContainer.GetCookies(new Uri(BASE_CAPSULE_ADDR));
                jar.Clear();
                jar = handler.CookieContainer.GetCookies(new Uri($"{BASE_CAPSULE_ADDR}/login"));
                jar.Clear();
                if (value != null)
                {
                    handler.CookieContainer.Add(new Cookie("login", value, "/", new Uri(BASE_CAPSULE_ADDR).Host));
                }
            }
        }

        public async Task<string> Login(string username, string password)
        {
            var builder = new UriBuilder($"{BASE_CAPSULE_ADDR}/login");
            var postParams = new Dictionary<string, string>();
            postParams.Add("username", username);
            postParams.Add("password", password);

            var req = new HttpRequestMessage(HttpMethod.Post, builder.Uri)
            {
                Content = new FormUrlEncodedContent(postParams)
            };
            var resp = await client.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            // Try to fix cookies
            SessionToken = SessionToken;
            return await resp.Content.ReadAsStringAsync();
        }

        public async Task<string> GetLibrary(string username)
        {
            var builder = new UriBuilder($"{BASE_CAPSULE_ADDR}/user/{username}/games");
            return await client.GetStringAsync(builder.Uri);
        }

        public async Task<string> GetGame(string username, string id)
        {
            var builder = new UriBuilder($"{BASE_CAPSULE_ADDR}/user/{username}/games/{id}");
            return await client.GetStringAsync(builder.Uri);
        }

        public async Task<string> GetDownload(string username, string id)
        {
            var builder = new UriBuilder($"{BASE_CAPSULE_ADDR}/user/{username}/games/{id}/download");
            return await client.GetStringAsync(builder.Uri);
        }

        public async Task<string> GetBox(string username, string id)
        {
            var builder = new UriBuilder($"{BASE_CAPSULE_ADDR}/user/{username}/games/{id}/box");
            return await client.GetStringAsync(builder.Uri);
        }

        public async Task<string> GetGameInfo(string username, string id)
        {
            var builder = new UriBuilder($"{BASE_STOREFRONT_ADDR}/api/game/{id}");
            return await client.GetStringAsync(builder.Uri);
        }

        public async Task<Stream> GetStream(string url)
        {
            return await client.GetStreamAsync(url);
        }
    }
}
