#TwitchAutomator

This tool is used to automatically detect what game you are playing and adjust your stream title + Game Played based on what you are playing.

It features a list of games which can be added to called gameslist.dat (Please note that not all games in the world will be in here, please add to this if you can)

games.txt features a JSON document containing game names and what you can adjust your stream to automatically go to.

The way the app works is...

Finds Game > Checks game against JSON List > If game is in JSON list it will autocorrect stream to those settings > If not in that list it will NOT change the stream title but it will change the game.

If the app cannot find any games or can't find anything to change it will not update.

At the moment it only works for twitch but I have the intention to role this out for youtube aswell providing I can understand their API, Help is welcome and I will accept pull requests!

#Credits

Sean - http://seanodonnell.co.uk / @TGN_Sean (Creator of app)  
TwitchLib - https://github.com/swiftyspiffy/TwitchLib  
NewtonsoftJSon - http://www.newtonsoft.com/json  

#Licence

This app makes use of the MIT Licence.