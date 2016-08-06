using System;
using Newtonsoft.Json.Linq;
using TwitchLib;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace TwitchAutomator
{
    class Program
    {

        private static List<string> _games;

        static void Main(string[] args)
        {
            // Usual credits+Intros
            Console.WriteLine("We rely on a list of games to detect your game. Any issues please contact me.");
            Console.WriteLine("Credits: Created by Sean @ SeanOdonnell.co.uk Feel Free to check out other projects I've created + Full Credits.");
            Console.WriteLine("Version 2.0 Now with less discord!");
            // Get details.
            getGame();
            
        }
        static void getGame()
        {
            _games = new List<string>(File.ReadAllLines(@"./gameslist.dat")); // Our games list
            try
            {
                foreach (Process p in System.Diagnostics.Process.GetProcesses()) // For each process we have running
                {
                    if (p.MainWindowTitle != null) // Exclude ones with no main window (Likely system processes)
                    {
                        if (_games.Contains(p.MainWindowTitle)) // If the title is within our games list, that user is likely playing that game.
                        {
                            updateTwitch(p.MainWindowTitle);

                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }

            Console.WriteLine("We couldn't find any games. Retrying in 5 seconds.");
            System.Threading.Thread.Sleep(5000);
        }

        static void updateTwitch(string game)
        {
            string token;
            Console.WriteLine("Fetching Twitch Details");
            Console.WriteLine("A page will open up requesting authorisation to your twitch account, this is so we can update your details.");
            System.Diagnostics.Process.Start("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=9xcf5l6r7kipl9d0mkmnjltoo8y3wv9&scope=channel_editor&redirect_uri=http://twitch.seanodonnell.co.uk");
            Console.WriteLine("Please copy and paste in the token you got from the website you just visited, this is your OAuth key and must not be shared with anyone.");
            token = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Console Cleared to avoid Token Leaks");
            Console.WriteLine("Please input your Twitch Channel Name");

            if (string.IsNullOrEmpty(Properties.Settings.Default.twitchname))
            {
                Properties.Settings.Default.twitchname = Console.ReadLine();
                Properties.Settings.Default.Save();
            }
            string cGame = checkGame(game);
            string cTitle = checkTitle(game);
            if (cTitle != "unchanged")
            {
                TwitchApi.UpdateStreamTitle(cTitle, Properties.Settings.Default.twitchname, token);
            }
            TwitchApi.UpdateStreamGame(cGame, Properties.Settings.Default.twitchname, token);

            Console.WriteLine("I have updated the game to " + cGame);
            refresh(game, token, Properties.Settings.Default.twitchname);

        }
        static void refresh(string game,string token, string channelname)
        {
            if(Properties.Settings.Default.refreshtime == 0) //If it's not been set then then.... we can try set it.
            {
                Console.WriteLine("How often do you want us to refresh? (Time in minutes)");
                int minutes = Convert.ToInt32(Console.ReadLine());
                int milliseconds = minutes * 60000;
                Properties.Settings.Default.refreshtime = milliseconds;
                Properties.Settings.Default.Save();
            }
            Console.WriteLine("We will refresh every " + Properties.Settings.Default.refreshtime / 60000 + " Minutes" );

            System.Threading.Thread.Sleep(Properties.Settings.Default.refreshtime);
            getGame();

        }

        static void refreshTwitch(string game, string sid, string mid, string token, string channelname)
        {
            string cGame = checkGame(game);
            string cTitle = checkTitle(game);
            if(cTitle != "unchanged")
            {
                TwitchApi.UpdateStreamTitle(cTitle, channelname, token);
            }
            TwitchApi.UpdateStreamGame(cGame, channelname, token);
            Console.WriteLine("I have updated the game to " + cGame);
        }

        // Will compare the game we have detected against the list of games in the games.txt file
        static string checkGame(string game)
        {
            // Local Variable so we don't mess with the other one.
            string cGame = game;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "games.txt"); // Get our path to the games list. (JSON)
            JObject result = JObject.Parse(File.ReadAllText(path));
            // If the result is in the list. Then we will return the correct title.
            if(result["games"][game] != null)
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
        static string checkTitle(string game)
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
            else
            {
                Console.WriteLine("Couldn't find game. We will not modify your title.");
                return "unchanged";
            }
        }

    }
}
