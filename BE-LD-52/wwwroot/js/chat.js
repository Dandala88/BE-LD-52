"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (full) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${full.userId} says ${full.type}`;
});

connection.on("ReceiveUser", function (user) {
    var username = document.getElementById("username");
    var userid = document.getElementById("userid");
    username.innerText = user.name;
    userid.innerText = user.id;
});

connection.on("ReceiveCell", function (cell) {
    console.log(cell.id)
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    var full = {
        UserId : user,
        Type : message
    }
    console.log(full);
    connection.invoke("SendMessage", full).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("getUserInfo").addEventListener("click", function (event) {
    var userId = crypto.randomUUID();
    var name = document.getElementById("userInput").value;
    if (!name)
        userId = "ageneratedid"
    var newUser = {
        Id: userId,
        Name: name
    }
    connection.invoke("GetUser", newUser).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("initGrid").addEventListener("click", function (event) {
    var key = document.getElementById("initkey").value;
    var x = document.getElementById("columns").value;
    var y = document.getElementById("rows").value;
    connection.invoke("InitializeGrid", parseInt(x), parseInt(y), key).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("updateCell").addEventListener("click", function (event) {
    var newState = document.getElementById("newState").value;
    var x = document.getElementById("newX").value;
    var y = document.getElementById("newY").value;

    connection.invoke("UpdateCell", x, y, "till", 2000).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("getGrid").addEventListener("click", function (event) {
    connection.invoke("GetGrid").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("timer").addEventListener("click", function (event) {
    connection.invoke("UpdateCell","asdfds", 0, 0, "till").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});