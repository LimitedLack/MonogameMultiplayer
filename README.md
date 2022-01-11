# MonogameMultiplayer
### Proof-of-concept 2 player client + host, running through TCP with the Monogame framework in C#.

## Info
Proof-of-concept should be emphasized here, from my testing most of this technically works, just not very well. LAN connections are smooth enough, but there is quite literally 0 interpolation, so connecting over WAN networks will probably cause a ton of visible jitter.
Everything is also only run through one network stream, and the method to send the position between clients is super hacky, so expect there to be weird glitches that can be caused by wrong or missing text in the stream.

## Running
After downloading, run the Monogame.exe file to open a selection for either a client or a server.

### Server
After selecting the server option (option #2), follow the instructions to run the server. For example, "192.168.1.6" for the IPv4, and "12345" for the port. You should see a message that says "Game server started, waiting for connections." if the server successfully started.

### Client
Once the server is up, rerun the Monogame.exe and choose the client option (option #1). Enter the ip and the port used to set up the server.
You should get three options:
1) Login as Player 1
2) Login as Player 2
3) Login as Viewer

Selecting option #1 or #2 will connect to the server as that player. If another player is already connected as that acting player, it will kick them from the server and replace them with you.
Logging in as a viewer allows you to see the other two players moving, but will not let you interact with anything else. Keep in mind that the viewer option is extremely buggy, and will really only let one person connect as a viewer.
