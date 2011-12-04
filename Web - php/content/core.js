var loginUrl = "main/login";
var displayUrl = "main/display";
var loadDataUrl = "/pocketsteam/main/heartbeat";
var logoutUrl = "/pocketsteam/main/logout";
var sendCommandUrl = "/pocketsteam/main/sendmessage";

var heartbeatTimer;
var heartbeatInterval = 1500;

var userData;
var friends = [];
var friendMessages = [];
var status = 0;

function StartHeartBeat() {
	heartbeatTimer = setInterval(function () {
            LoadData();
        }, heartbeatInterval);
    LoadData();
}

function LoadData() {
	$.ajax({
        type: "POST",
        url: loadDataUrl,
        success: function (data) {
        	//alert('MRONING!: ' + data);
        	ParseData(data);
        },
        error: function () {
        	alert('Uncaught Http Error, report this!');
        }
    });
}

function SendCommand(type, message) {
    dataString = 'type=' + type + "&message=" + message;

    $.ajax({
        type: "POST",
        data: dataString,
        url: sendCommandUrl,
        success: function (data) {
            if (data != "OK") {
                alert('Server replied: ' + data);
            }
        },
        error: function () {
            alert('Uncaught Http Error, report this!');
        }
    });
}

function SendChatMessage(type, message, to) {
    var messageArray = {};
    messageArray["To"] = to;
    messageArray["Message"] = message;
    var messageJson = JSON.stringify(messageArray);
    SendCommand(type, messageJson);
}

function Redirect(redirectLocation) {
	$.mobile.changePage( redirectLocation, {
		transition: "slide",
		reloadPage: true
	});	
}

function Logout() {
	clearInterval(heartbeatTimer);
	Redirect(logoutUrl);
}