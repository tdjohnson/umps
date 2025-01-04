import config from './config.js'; // Import the config file
import { v4 as uuidv4 } from 'uuid'; // Import the uuid library

var playerId = uuidv4(); // Generate a UUID for playerId
var playerName = "unknown";

function roundNum(num) {
    return Math.round(num * 100) / 100;
}

export class UMPS {
	constructor(url = config.defaultUrl) {
		this.hub = new signalR.HubConnectionBuilder().withUrl(url).configureLogging(signalR.LogLevel.Information).build();
		this.hub.start()
			.then(function () {
				console.log("Connected to UMPS");
				console.log(`PlayerID = ${playerId}`);
			}).catch(function (err) {
				console.error("Error connecting to UMPS: ", err);
			})
	}

	GetPlayerId() {
		return playerId;
	}

	GetPlayerName() {
		return playerName;
	}
	on(eventName, callback) {
		this.hub.on(eventName, callback);
	}

	send(eventName, data) {
		this.hub.invoke(eventName, data);
	}

	SendData (pos,dir) {
		this.hub.invoke("SendData", {
			id: playerId.toString(),
			x: roundNum(pos.x),
			y: roundNum(pos.y),
			z: roundNum(pos.z),
			xd: roundNum(dir.x),
			yd: roundNum(dir.y),
			zd: roundNum(dir.z)
		});
	}

}