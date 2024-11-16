import * as SIGNALR from './signalr.js';

var playerId = Math.floor(Math.random() * 100).toString();
 
var hub = new SIGNALR.HubConnectionBuilder().withUrl("http://188.245.62.68:8080/controlhub").configureLogging(SIGNALR.LogLevel.Information).build();

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