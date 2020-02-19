var socket = [];

var startSocket = function () {
    socket = new WebSocket("ws://" + location.host + "/api/firstwebsocket");
    socket.addEventListener('message', function (event) {
        readmessage(event);
    });
};

var sendmessage = function (message) {
    socket.send(message);
};

var readmessage = function (event) {
   //event.data;
};