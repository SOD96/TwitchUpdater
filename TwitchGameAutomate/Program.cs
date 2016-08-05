using System;
using Newtonsoft.Json.Linq;
using TwitchLib;
using System.IO;
using System.Net;

namespace TwitchGameAutomate
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
            string serverid; // The unique serverID for discord
            string memberid; // The unique member ID for users within the server   
            Console.WriteLine("Please enter your Discord ServerID (Server Settings > Widget > Enable Widget > Server ID)");
            serverid = Console.ReadLine();
            Console.WriteLine("Please enter your Discord Tag. (Example#2571) ");
            memberid = Console.ReadLine();

            if(string.IsNullOrWhiteSpace(serverid) || string.IsNullOrWhiteSpace(memberid))
            {
                Console.WriteLine("You did not enter any values, please try again");
                detailsInput();
            }

            getGame(serverid, memberid);


        }
        // Will query Discord to detect the game that someone is playing.
        static void getGame(string sid,string mid)
        {
            //Variables
            string game;
            string discordtag;
            Console.WriteLine("The following details I have are: Your server ID is: " + sid + "Your memberID is " + mid);
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
            System.Diagnostics.Process.Start("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=9xcf5l6r7kipl9d0mkmnjltoo8y3wv9&redirect_uri=http://twitch.seanodonnell.co.uk&scope=user_read+channel_editor");
            Console.WriteLine("It will then open up a page showing your authorisation token in the URL, Please paste this in now. It looks like the following:");
            Console.WriteLine("http://localhost/#access_token=92837aansm238571haj&scope=user_read+channel_editor+channel_read");
            Console.WriteLine("We only need the value AFTER the Access_Token= Part. Do not copy the & which appears after it.");
            token = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Console Cleared to avoid Token Leaks");
            Console.WriteLine("Please input your Twitch Channel Name");
            string channelname = Console.ReadLine();
            string cGame = checkGame(game);
            string cTitle = checkTitle(game);
            if (cTitle != "unchanged")
            {
                TwitchApi.UpdateStreamTitle(cTitle, channelname, token);
            }
            TwitchApi.UpdateStreamGame(cGame, channelname, token);

            Console.WriteLine("I have updated the game to " + cGame);
            refresh(sid,mid,token,channelname);

        }
        static void refresh(string sid,string mid,string token, string channelname)
        {
            Console.WriteLine("Refresh in 5 minutes");
            System.Threading.Thread.Sleep(300000);
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
    }
}
