import config from './config.js'; // Import the config file

function uuidv4() {
	return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
	  (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
	);
}

var playerId = uuidv4(); // Generate a UUID for playerId
var playerName = "unknown";
 

function roundNum(num) {
    return Math.round(num * 100) / 100;
}

async function fetchPlayerName(playerId) {
    const response = await fetch(`${config.baseUrl}/api/Lobby/GetPlayerName?id=${playerId}`);
    if (response.ok) {
        const name = await response.text();
		console.log(name);
        return name;
    } else {
        throw new Error('Player not found');
    }
}

async function savePlayerName(playerId, playerName) {
    const response = await fetch(`${config.baseUrl}/api/Lobby/SetPlayerName?id=${playerId}&name=${playerName}`, {
		method: "POST"
	});

    if (response.ok) {
        return;
    } else {
		console.log(response);
        throw new Error('Player not found');
    }
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

	async GetPlayerName(playerId) {
		try {
			const name = await fetchPlayerName(playerId);
			console.log(name);
			return name;
		} catch (error) {
			console.error(error);
			return "unknown";
		}
	}

	async SetPlayerName(playerId, playerName) {
		try {
			await savePlayerName(playerId, playerName);
			return;
		} catch (error) {
			console.error(error);
			return "unknown";
		}
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