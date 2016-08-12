using System;
using Newtonsoft.Json.Linq;
using TwitchLib;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using TwitchAutomator.Properties;

namespace TwitchAutomator
{

    class Program
    {
        private static List<string> _games;
        private static string _token;

        static void Main(string[] args)
        {
            VersionChecks();
            GetOauthToken();

            // Our games list 
            _games = new List<string>(File.ReadAllLines(@"./gameslist.dat"));          

            while (true)
            {
                FindGameWindow();
            }
        }


        static void GetOauthToken()
        {
            Console.WriteLine("Fetching Twitch Details");
            Console.WriteLine("A page will open up requesting authorisation to your twitch account, this is so we can update your details.");
            Process.Start("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=9xcf5l6r7kipl9d0mkmnjltoo8y3wv9&scope=channel_editor&redirect_uri=http://twitch.seanodonnell.co.uk");
            Console.WriteLine("Please copy and paste in the token you got from the website you just visited, this is your OAuth key and must not be shared with anyone.");
            _token = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Console Cleared to avoid Token Leaks");


        }

        /*
        static void twitchGameUpdate(string game, string twitchname)
        {
            string cGame = checkGame(game);
            string cTitle = checkTitle(game);
            if (cTitle != "unchanged")
            {
                TwitchApi.UpdateStreamTitle(cTitle, twitchname, _token);
            }
            TwitchApi.UpdateStreamGame(cGame, twitchname, _token);

            Console.WriteLine("I have updated the game to " + cGame);

        }
        */


        static void FindGameWindow()
        {
            bool gameFound = false;

            foreach (Process p in Process.GetProcesses()) // For each process we have running
            {
                if (_games.Contains(p.MainWindowTitle)) // If the title is within our games list, that user is likely playing that game.
                {
                    gameFound = true;                  
                    SetGame(p.MainWindowTitle);
                    Thread.Sleep(Settings.Default.refreshtime);
                    break;
                }
            }

            if (!gameFound)
            {
                Console.WriteLine("We couldn't find any games. Retrying in 5 seconds.");
                Thread.Sleep(5000);
            }

        }

        static void SetGame(string game)
        {

            Console.WriteLine("Please input your Twitch Channel Name"); // This is being changed to a constant ask due to people changing twitch channels. Temp fix until the UI is done.
            string twitchname = Console.ReadLine();

            UpdateTwitch(game,twitchname);

            if (Settings.Default.refreshtime == 0) //If it's not been set then then.... we can try set it.
            {
                Console.WriteLine("How often do you want us to refresh? (Time in minutes)");
                int minutes = Convert.ToInt32(Console.ReadLine());
                int milliseconds = minutes * 60000;
                Settings.Default.refreshtime = milliseconds;
                Settings.Default.Save();
            }
            Console.WriteLine("We will refresh every " + Settings.Default.refreshtime / 60000 + " Minutes");

        }


        static void UpdateTwitch(string game, string twitchname)
        {
            string cGame = CheckGame(game);
            string cTitle = CheckTitle(game);
            if (cTitle != "unchanged")
            {
                TwitchApi.UpdateStreamTitle(cTitle, twitchname, _token);
            }
            TwitchApi.UpdateStreamGame(cGame, twitchname, _token);

            Console.WriteLine("I have updated the game to " + cGame);

        }

        // Will compare the game we have detected against the list of games in the games.txt file
        static string CheckGame(string game)
        {
            // Local Variable so we don't mess with the other one.
            string cGame = game;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "games.txt"); // Get our path to the games list. (JSON)
            JObject result = JObject.Parse(File.ReadAllText(path));
            // If the result is in the list. Then we will return the correct title.
            if (result["games"][game] != null)
            {
                Console.WriteLine("I have corrected the game to a custom one");
                cGame = result["games"][game]["name"].ToString();
            }
            else
            {
                Console.WriteLine("Couldn't find game. Restoring to Default Title");
            }
            return cGame;
        }

        // Will compare the game we have detected to the list of titles that should be set in the games.txt file.
        static string CheckTitle(string game)
        {
            string cTitle;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "games.txt"); // Get our path to the games list. (JSON)

            JObject result = JObject.Parse(File.ReadAllText(path));

            // If the result is in the list. Then we will return the correct title.
            if (result["games"][game] != null)
            {
                Console.WriteLine("I have corrected the title to a custom one");
                cTitle = result["games"][game]["title"].ToString();
                return cTitle;
            }

            Console.WriteLine("Couldn't find game. We will not modify your title.");
            return "unchanged";
        }

        static void VersionChecks()
        {
            // Version Checks
            var path = Directory.GetCurrentDirectory() + "/TwitchAutomator.exe"; // Path to the EXE
            var file = File.OpenRead(path); // Opens file
            var appmd5 = MD5.Create().ComputeHash(file); // Creates the MD5 Of the file
            var cMD5 = BitConverter.ToString(appmd5).Replace("-", "").ToLower(); // Converts our computed hash to a string and replaces the - with nothing and puts it lowercase for easier comparing.

            // Call my website to see if the version is the same... 
            var wc = new WebClient();
            string uMD5 = wc.DownloadString("http://seanodonnell.co.uk/twitch/versioncheck.txt"); // Download the md5 from the site.
            if (uMD5 != cMD5)
            {
                Console.WriteLine("This is an out of date version, please redownload the bot!"); // Tell user to redownload
            }
        }


    }
}
