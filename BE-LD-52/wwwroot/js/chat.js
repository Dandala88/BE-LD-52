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
    connection.invoke("InitializeGrid", 10, 10, key).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});