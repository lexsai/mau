"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:5296/game").build();

//Disable the send button until connection is established.
document.getElementById("status").textContent = "Not connected";

connection.on("WriteMessage", function (message) {
    document.getElementById("status").textContent = message;
});

connection.start().then(function () {
    document.getElementById("status").textContent = "Connected!";
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("createLobbyButton").addEventListener("click", function (event) {
    let lobbyName = document.getElementById("textInput1").value;
    let playerName = document.getElementById("textInput2").value;
    connection.invoke("CreateLobby", lobbyName, playerName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("joinLobby").addEventListener("click", function (event) {
    let lobbyName = document.getElementById("textInput1").value;
    let playerName = document.getElementById("textInput2").value;
    connection.invoke("JoinLobby", lobbyName, playerName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
