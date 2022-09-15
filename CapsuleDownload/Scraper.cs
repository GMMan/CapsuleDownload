using CapsuleDownload.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace CapsuleDownload
{
    public class Scraper
    {
        Client client;

        public string BasePath { get; private set; }
        public string Username { get; set; }
        public List<Game> Library { get; set; }

        public Scraper(Client client, string basePath)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            BasePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public async Task<bool> CheckLogin()
        {
            if (string.IsNullOrEmpty(Username)) return false;
            try
            {
                await client.GetLibrary(Username);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ScrapeAccount()
        {
            string json = await client.GetLibrary(Username);
            File.WriteAllText(Path.Combine(BasePath, "library.json"), json);

            Library = JsonSerializer.Deserialize<List<Game>>(json);
            foreach (var game in Library)
            {
                string gameDir = Path.Combine(BasePath, game.Id);
                Directory.CreateDirectory(gameDir);

                json = await client.GetGame(Username, game.Id);
                File.WriteAllText(Path.Combine(gameDir, "item.json"), json);
                // Not using anything from this, so not deserializing

                json = await client.GetGameInfo(Username, game.Id);
                File.WriteAllText(Path.Combine(gameDir, "info.json"), json);
                game.Info = JsonSerializer.Deserialize<GameInfo>(json);

                json = await client.GetDownload(Username, game.Id);
                File.WriteAllText(Path.Combine(gameDir, "download.json"), json);
                game.Download = JsonSerializer.Deserialize<Download>(json);

                json = await client.GetBox(Username, game.Id);
                File.WriteAllText(Path.Combine(gameDir, "box.json"), json);
                game.Box = JsonSerializer.Deserialize<Box>(json);

                // Download images
                if (game.Info.Banner != null)
                {
                    await DownloadToFile(game.Info.Banner, Path.Combine(gameDir, Path.GetFileName(game.Info.Banner)));
                }

                if (game.Info.BoxArt != null)
                {
                    await DownloadToFile(game.Info.BoxArt, Path.Combine(gameDir, Path.GetFileName(game.Info.BoxArt)));
                }
            }
        }

        async Task DownloadToFile(string url, string savePath)
        {
            using (var webStream = await client.GetStream(url))
            using (var fs = File.Create(savePath))
            {
                await webStream.CopyToAsync(fs);
            }
        }

        public void Load()
        {
            try
            {
                string json = File.ReadAllText(Path.Combine(BasePath, "library.json"));
                Library = JsonSerializer.Deserialize<List<Game>>(json);

                foreach (var game in Library)
                {
                    string gameDir = Path.Combine(BasePath, game.Id);

                    json = File.ReadAllText(Path.Combine(gameDir, "info.json"));
                    game.Info = JsonSerializer.Deserialize<GameInfo>(json);

                    json = File.ReadAllText(Path.Combine(gameDir, "download.json"));
                    game.Download = JsonSerializer.Deserialize<Download>(json);

                    json = File.ReadAllText(Path.Combine(gameDir, "box.json"));
                    game.Box = JsonSerializer.Deserialize<Box>(json);
                }
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("Please make sure you have dumped your library first.");
                throw;
            }
        }

        public List<string> GenerateDownloadUrls()
        {
            if (Library == null) throw new InvalidOperationException("Library not loaded.");

            List<string> list = new List<string>();
            foreach (var game in Library)
            {
                string downloadDir = Path.Combine(BasePath, game.Id, "big_file");
                Directory.CreateDirectory(downloadDir);

                var parts = game.Download.GenerateDownloadUrls();
                foreach (var url in parts)
                {
                    list.Add(url);
                    list.Add($"\tdir={downloadDir}");
                }
            }

            return list;
        }

        public void DecryptAllGames(bool force)
        {
            if (Library == null) throw new InvalidOperationException("Library not loaded.");

            foreach (var game in Library)
            {
                try
                {
                    Console.WriteLine($"Decrypting {game.Name}");
                    DecryptGame(game, force);
                }
                catch (FileNotFoundException)
                {
                    Console.Error.WriteLine($"Game {game.Name} missing files.");
                }
            }
        }

        public void DecryptGame(Game game, bool force = false)
        {
            string gameDir = Path.Combine(BasePath, game.Id);
            string downloadDir = Path.Combine(gameDir, "big_file");

            var download = game.Download;
            var crypto = download.Crypto["big_file"];
            string decryptedPath = Path.Combine(gameDir, "big_file_decrypted.zip");
            if (File.Exists(decryptedPath) && !force)
            {
                Console.WriteLine("Decrypted file exists, skipping.");
                return;
            }
            var decryptor = CreateDecryptor(crypto.KeyBytes, crypto.IvBytes);
            using (var destStream = File.Create(decryptedPath))
            {
                if (download.IsUsingParts)
                {
                    var numParts = download.GetPartsCount();
                    for (int i = 0; i < numParts; ++i)
                    {
                        Console.WriteLine($"Decrypting part {i + 1} of {numParts}");
                        using (var srcStream = File.OpenRead(Path.Combine(downloadDir, Download.GetPartName(i))))
                        {
                            DecryptFile(decryptor, srcStream, destStream);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Decrypting full file");
                    using (var srcStream = File.OpenRead(Path.Combine(downloadDir, "fullgame")))
                    {
                        DecryptFile(decryptor, srcStream, destStream);
                    }
                }
            }
        }

        static IBlockCipher CreateDecryptor(byte[] key, byte[] iv)
        {
            CfbBlockCipher cfb = new CfbBlockCipher(new BlowfishEngine(), iv.Length * 8);
            cfb.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
            return cfb;
        }

        static void DecryptFile(IBlockCipher decryptor, Stream src, Stream dest)
        {
            var blockSize = decryptor.GetBlockSize();
            var buffer = new byte[(4096 + blockSize - 1) / blockSize * blockSize];
            while (src.Position < src.Length)
            {
                int expected = Math.Min(buffer.Length, (int)(src.Length - src.Position));
                int read = src.Read(buffer, 0, expected);
                if (read != expected) throw new IOException("Partial read");
                // Zero pad partial block
                for (int i = read; i < buffer.Length; ++i)
                {
                    buffer[i] = 0;
                }
                for (int i = 0; i < buffer.Length; i += blockSize)
                {
                    decryptor.ProcessBlock(buffer, i, buffer, i);
                }
                dest.Write(buffer, 0, read);
            }
        }
    }
}
