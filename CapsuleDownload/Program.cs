using CapsuleDownload.Models;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CapsuleDownload
{
    class Program
    {
        static Client client;
        static Scraper scraper;

        static async Task<int> Main(string[] args)
        {
            var forceOption = new Option<bool>("--force", "Force decryption even if decrypted file already exists.");
            var baseDirArgument = new Argument<DirectoryInfo>("baseDir", "Directory to save all data in.");
            var gameIdArgument = new Argument<string>("gameId", () => null, "Specific game ID to operate on.");

            var rootCommand = new RootCommand("GMG Capsule Scraper");

            var dumpCommand = new Command("dump", "Dump library.")
            {
                baseDirArgument
            };
            rootCommand.AddCommand(dumpCommand);

            var generateLinksCommand = new Command("generate-links", "Generate download links for aria2.")
            {
                baseDirArgument
            };
            rootCommand.AddCommand(generateLinksCommand);

            var decryptCommand = new Command("decrypt", "Decrypt downloaded files, either for one game or all.")
            {
                forceOption,
                baseDirArgument,
                gameIdArgument
            };
            rootCommand.AddCommand(decryptCommand);

            dumpCommand.SetHandler(async (baseDir) =>
            {
                await Setup(baseDir.FullName, true);
                await scraper.ScrapeAccount();
            }, baseDirArgument);

            generateLinksCommand.SetHandler(async (baseDir) =>
            {
                await Setup(baseDir.FullName, false);
                scraper.Load();
                string savePath = Path.Combine(scraper.BasePath, "downloads.txt");
                File.WriteAllLines(savePath, scraper.GenerateDownloadUrls());
                Console.WriteLine($"Downloads list saved to {savePath}, please use aria2c to download.");
            }, baseDirArgument);

            decryptCommand.SetHandler(async (force, baseDir, gameId) =>
            {
                await Setup(baseDir.FullName, false);
                scraper.Load();
                if (gameId == null)
                {
                    scraper.DecryptAllGames(force);
                }
                else
                {
                    var game = scraper.Library.Find(x => x.Id == gameId);
                    if (game == null)
                    {
                        throw new ArgumentException("Incorrect game ID.", nameof(gameId));
                    }
                    scraper.DecryptGame(game, true);
                }
            }, forceOption, baseDirArgument, gameIdArgument);

            // Override default exception handling, see https://github.com/dotnet/command-line-api/issues/796#issuecomment-999734612
            var parser = new CommandLineBuilder(rootCommand).UseDefaults().UseExceptionHandler((e, context) =>
            {
                if (e is AggregateException aggEx)
                {
                    aggEx.Handle(inner =>
                    {
                        Console.WriteLine($"Something went wrong: {inner}");
                        return true;
                    });
                }
                else
                {
                    Console.WriteLine($"Something went wrong: {e}");
                }
            }, 1)
            .Build();

            return await parser.InvokeAsync(args);
        }

        static async Task Setup(string basePath, bool authRequired)
        {
            string credPath = Path.Combine(basePath, "token.json");
            TokenStore tokens = null;
            if (File.Exists(credPath))
            {
                try
                {
                    var json = File.ReadAllText(credPath);
                    tokens = JsonSerializer.Deserialize<TokenStore>(json);
                }
                catch (JsonException)
                {
                    Console.Error.WriteLine("Error deserializing token, clearing it.");
                    File.Delete(credPath);
                }
            }

            client = new Client();
            scraper = new Scraper(client, basePath);
            if (tokens != null)
            {
                client.SessionToken = tokens.Token;
                scraper.Username = tokens.Username;
                if (!await scraper.CheckLogin())
                {
                    Console.Error.WriteLine("Token invalid.");
                    tokens = null;
                }
            }
            Directory.CreateDirectory(basePath);
            if (authRequired)
            {
                while (tokens == null)
                {
                    Console.WriteLine("Please log in.");
                    Console.Write("Email address: ");
                    string email = Console.ReadLine().Trim();
                    Console.Write("Password: ");
                    string password = ReadPassword();
                    try
                    {
                        var json = await client.Login(email, password);
                        User user = JsonSerializer.Deserialize<User>(json);
                        tokens = new TokenStore
                        {
                            Token = client.SessionToken,
                            Username = user.Username
                        };
                        scraper.Username = user.Username;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to log in, please try again. ({ex.Message})");
                    }
                }

                File.WriteAllText(credPath, JsonSerializer.Serialize(tokens));
            }
        }

        static string ReadPassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
                }
                else
                {
                    sb.Append(key.KeyChar);
                }
            }
            return sb.ToString();
        }
    }
}
