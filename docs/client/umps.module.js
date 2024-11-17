var playerId = Math.floor(Math.random() * 100).toString();

function roundNum(num) {
    return Math.round(num * 100) / 100;
}


export class UMPS {
	constructor() {
		this.hub = new signalR.HubConnectionBuilder().withUrl("http://umps.tdj23.com:8080/controlhub").configureLogging(signalR.LogLevel.Information).build();
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