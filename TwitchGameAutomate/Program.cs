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
    //  A simple program to automate the task of changing your game every time you stream. Must be in a discord server in order to work.
    class Program
    {

        static void Main(string[] args)
        {
            // Usual credits+Intros
            Console.WriteLine("Warning: This program uses the Discord API to fetch the game you are currently playing, Naming inaccuracies are Discords fault. Please use the game.txt file to resolve these issues.");
            Console.WriteLine("Credits: Created by Sean @ SeanOdonnell.co.uk Feel Free to check out other projects I've created + Full Credits.");
            Console.WriteLine("Version 1.0");
            // Get details.
            detailsInput();
        }

        static void detailsInput()
        {
            // Variables
            if(string.IsNullOrEmpty(Properties.Settings.Default.serverid) || string.IsNullOrEmpty(Properties.Settings.Default.discordtag))
            {
                Console.WriteLine("Please enter your Discord ServerID (Server Settings > Widget > Enable Widget > Server ID)");
                Properties.Settings.Default.serverid = Console.ReadLine();
                Console.WriteLine("Please enter your Discord Tag. (Example#2571) ");
                Properties.Settings.Default.discordtag = Console.ReadLine();
                Properties.Settings.Default.Save();
                if (string.IsNullOrWhiteSpace(Properties.Settings.Default.serverid) || string.IsNullOrWhiteSpace(Properties.Settings.Default.discordtag))
                {
                    Console.WriteLine("You did not enter any values, please try again");
                    detailsInput();
                }
            }
            Console.WriteLine("Scanning for Games Every 5 Seconds");
            System.Threading.Thread.Sleep(5000);

            getGame(Properties.Settings.Default.serverid, Properties.Settings.Default.discordtag);


        }
        // Will query Discord to detect the game that someone is playing.
        static void getGame(string sid,string mid)
        {
            //Variables
            string game;
            string discordtag;
            //Console.WriteLine("The following details I have are: Your server ID is: " + sid + "Your memberID is " + mid); - No longer required tbh
            System.Net.WebClient wc = new System.Net.WebClient();

            try
            {
                var json = wc.DownloadString("https://discordapp.com/api/servers/" + sid + "/widget.json");

                JObject result = JObject.Parse(json);
                //result["members"][mid]
                foreach(var m in result["members"])
                {
                    discordtag = m["username"] + "#" + m["discriminator"];
                    if(discordtag == mid)
                    {
                        if(m["game"] != null)
                        {
                            if (m["game"]["name"] != null)
                            {
                                game = m["game"]["name"].ToString();
                                Console.WriteLine("I have detected: " + game);
                                updateTwitch(game, sid, mid); //Send the user to the twitch update.
                            }
                            else
                            {
                                Console.WriteLine("I cannot detect any games currently being played. Please try again.");
                                Console.WriteLine("Press Enter to Continue...");
                                Console.ReadLine();
                                detailsInput();

                            }
                        }
                        else
                        {
                            Console.WriteLine("Cannot Detect Any Games");
                            detailsInput();
                        }


                    }
                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e + "We errored out trying to connect, this is either due to an incorrect server or widgets being disabled or something else.");
                detailsInput();
            }
        }
        static void updateTwitch(string game, string sid, string mid)
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
            refresh(sid,mid,token, Properties.Settings.Default.twitchname);

        }
        static void refresh(string sid,string mid,string token, string channelname)
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
            refreshGame(sid, mid, token, channelname);

        }

        static void refreshGame(string sid, string mid, string token, string channelname)
        {
            //Variables
            string game;
            string discordtag;
            System.Net.WebClient wc = new System.Net.WebClient();
            // https://discordapp.com/api/servers/210487055686434816/widget.json
            try
            {
                var json = wc.DownloadString("https://discordapp.com/api/servers/" + sid + "/widget.json");
                JObject result = JObject.Parse(json);
                //result["members"][mid]
                foreach (var m in result["members"])
                {
                    discordtag = m["username"] + "#" + m["discriminator"];
                    if (discordtag == mid)
                    {
                        game = m["game"]["name"].ToString();
                        Console.WriteLine("I have detected: " + game);
                        refreshTwitch(game, sid, mid,token, channelname); //Send the user to the twitch update.
                    }
                }
            }
            catch
            {

            }
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
            refresh(sid, mid, token, channelname);
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
                Console.WriteLine("Couldn't find game. Resorting to default Discord Title");
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

        static string getGame()
        {
            string stuff = "null";
            var processes = Process.GetProcesses();
            List<string> gameList = new List<string>();
            foreach (var process in processes)
            {
                gameList.Add(process.ProcessName.ToLower());
            }
            foreach(var game in gameList)
            {

            }
            return stuff;
        }


    }
}
