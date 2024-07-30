"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:5296/game").build();

let cardInput = null;

// taken from https://stackoverflow.com/a/72350987
const until = (predFn, timeout) => {
    let timeoutElapsed = false;
    setTimeout(() => timeoutElapsed = true, timeout);
    const poll = (done) => (predFn() | timeoutElapsed ? done() : setTimeout(() => poll(done), 500));
    return new Promise(poll);
};

// Disable the send button until connection is established.
document.getElementById("status").textContent = "Not connected";

connection.on("WriteMessage", function (message) {
    document.getElementById("info").textContent = message;
});

connection.on("LobbyUsersUpdate", function (playerNames) {
    document.getElementById("lobby").textContent = playerNames;
});

connection.on("StartGame", function (handData) {
    document.getElementById("lastPlayedCardContainer").hidden = false;
    document.getElementById("hand").hidden = false;

    const handInfoElement = document.getElementById("handInfo");
    handInfoElement.innerHTML = "";

    for (const card of handData) {
        const cardElement = document.createElement("li");
        cardElement.textContent = card;
        handInfoElement.appendChild(cardElement);
    }
});

connection.on("PlayedCardUpdate", function (card) {
    document.getElementById("lastPlayedCard").textContent = card;
});

connection.on("HandUpdate", function (handData) {
    const handInfoElement = document.getElementById("handInfo");
    handInfoElement.innerHTML = "";

    for (const card of handData) {
        const cardElement = document.createElement("li");
        cardElement.textContent = card;
        handInfoElement.appendChild(cardElement);
    }
});

connection.on("RequestCard", async function () {
    document.getElementById("clientInfo").textContent = "Waiting for card input....";
    document.getElementById("card").hidden = false;

    await until(() => cardInput != null, 5000);

    document.getElementById("clientInfo").textContent = "";
    document.getElementById("card").hidden = true;

    let input = cardInput;
    cardInput = null;
    return input;
});

connection.start().then(function () {
    document.getElementById("status").textContent = "Connected!";
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("createLobbyButton").addEventListener("click", function (event) {
    const lobbyName = document.getElementById("textInput1").value;
    const playerName = document.getElementById("textInput2").value;

    connection.invoke("CreateLobby", lobbyName, playerName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("joinLobby").addEventListener("click", function (event) {
    const lobbyName = document.getElementById("textInput1").value;
    const playerName = document.getElementById("textInput2").value;

    connection.invoke("JoinLobby", lobbyName, playerName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("test").addEventListener("click", function (event) {
    connection.invoke("Test").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("startGame").addEventListener("click", function (event) {
    connection.invoke("StartGame").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("submitCardButton").addEventListener("click", function (event) {
    cardInput = document.getElementById("cardInput").value;
});
