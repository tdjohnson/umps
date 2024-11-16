import * as SIGNALR from './signalr.js';

// Create a script element
var script = document.createElement('script');
script.src = "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.js;
// Append the script element to the head of the document
document.head.appendChild(script);

var playerId = Math.floor(Math.random() * 100).toString();
 
var hub = new SIGNALR.HubConnectionBuilder().withUrl("http://188.245.62.68:8080/controlhub").configureLogging(signalR.LogLevel.Information).build();
var prePos = -1;


hub.start()
    .then(function () {
        console.log("Connected to UMPS");
 	}).catch(function (err) {
    console.error("Error connecting to UMPS: ", err);
	});

hub.on("ReceiveData", function (player) {
		if(player.id == playerId) return;
		botBody.position.set(player.x,player.y,player.z);
		console.log(`player ${player.id} x=${player.x},y=${player.y},z=${player.z}`);
	});