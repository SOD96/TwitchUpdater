using System;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TwitchAutomator
{
    class Youtube
    {
        /*
        This code is here incase Google get their finger out their ass and actually allow me to change what game you are playing on a youtube video.
        
        // This stage begins the Auth Process with Google to get a token to allow us to modify videos.
        static void YTAuth(string game)
        {

            Console.WriteLine("A Web page should have opened. Please login and paste the token in.");
            Process.Start("https://accounts.google.com/o/oauth2/auth?client_id=918058421522-836deusm52p691tjnjrj71to8qvn02km.apps.googleusercontent.com&scope=https://www.googleapis.com/auth/youtube.force-ssl&response_type=code&access_type=offline&redirect_uri=urn:ietf:wg:oauth:2.0:oob");
            string token = Console.ReadLine();
            using (WebClient client = new WebClient())
            {
                NameValueCollection values = new NameValueCollection();
                values["grant_type"] = "authorization_code"; // What we need.
                values["code"] = token; // The token we just got from google to get an actual token.
                values["client_id"] = "918058421522-836deusm52p691tjnjrj71to8qvn02km.apps.googleusercontent.com"; // ID of the app.
                values["client_secret"] = "IqeajCFEyVI6XJtMto9X0Ucl"; // This is required to make requests. It's meant to be secret. Oh well.
                values["redirect_uri"] = "urn:ietf:wg:oauth:2.0:oob"; //Redirect back to google.
                var response = client.UploadValues("https://accounts.google.com/o/oauth2/token", values); // Post the information over to google
                var responseString = Encoding.Default.GetString(response); // Read it into a string.

                JObject jo = JObject.Parse(responseString); //Parse the JSON we get back
                string access_token = jo["access_token"].ToString(); // Used to retrieve the information we want.
                string refresh_token = jo["refresh_token"].ToString(); //Used to get another access token
                Console.WriteLine(access_token);
                // Set application saves. We need to keep our refresh and access token to either get a new token, or save another video again.
                Properties.Settings.Default.googleaccess = access_token;
                Properties.Settings.Default.googlerefresh = refresh_token;
                Properties.Settings.Default.Save();
                getYTVideo(game); //Send our token off to fetch the youtube video that the user wants to edit.

            }

        }

        // Get details on the current Youtube Video incase the user doesn't want to overwrite any information.
        static void getYTVideo(string game)
        {
            // Lets get our Youtube Video Information
            Console.WriteLine("Please enter your videos Unique ID");
            string lid = Console.ReadLine();

            using (WebClient client = new WebClient())
            {
                var responseString = client.DownloadString("https://www.googleapis.com/youtube/v3/videos?part=snippet&id=" + lid + "&access_token=" + Properties.Settings.Default.googleaccess);
                JObject jo = JObject.Parse(responseString);

                string title = jo["items"][0]["snippet"]["title"].ToString(); //Title of the video
                string description = jo["items"][0]["snippet"]["description"].ToString(); // Description of the video.
                string tags = jo["items"][0]["snippet"]["tags"].ToString(); // tags of the video. (NOT TO BE CHANGED! EVERRR!!!)
                string categoryid = jo["items"][0]["snippet"]["categoryId"].ToString(); // category of the video. (NOT TO BE CHANGED! EVERRR!!!)

                updateYTVideo(title, description, Properties.Settings.Default.googleaccess, lid, tags, categoryid, game);
                // I know we are passing a lot of shit. Google for some reason sets everything to null if you don't....
            }
        }

        static void updateYTVideo(string title, string description, string token, string id, string tags, string categoryId, string game)
        {
            //string cTitle = checkTitle(game);
            //string cGame = checkGame(game);

            using (WebClient client = new WebClient())
            {
                try
                {
                    Data data = new Data();
                    data.id = id;

                    if (cTitle != "unchanged")
                    {
                        data.snippet.Add("title", cTitle); // The title the user has put in games.txt
                    }
                    else
                    {
                        data.snippet.Add("title", title);
                    }
                    data.snippet.Add("description", description); // We will not change this. No point.
                    data.snippet.Add("tags", tags); // Tags of that video. Never to be changed.
                    data.snippet.Add("categoryId", categoryId); // Category of the video. Never to be changed.

                    string json = JsonConvert.SerializeObject(data, new KeyValuePairConverter());
                    Console.WriteLine(json);
                    Console.ReadLine();
                    client.Headers.Add("Authorization", "Bearer " + token);
                    client.Headers.Remove("Content-Type");
                    client.Headers.Add("Content-Type", "application/json");
                    //var response = client.UploadValues("https://www.googleapis.com/youtube/v3/videos", adsad);
                    var r = client.UploadString("https://www.googleapis.com/youtube/v3/videos?part=snippet&client_id=918058421522-836deusm52p691tjnjrj71to8qvn02km.apps.googleusercontent.com", "PUT", json);
                }
                catch (WebException e)
                {
                    Console.WriteLine(e + " Something went wrong. You most likely messed up on the video id.");
                }

            }
        }
        */
    }
}
