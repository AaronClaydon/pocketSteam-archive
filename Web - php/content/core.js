var loginUrl = "index.php/main/login";
var displayUrl = "index.php/main/display";
var loadDataUrl = "/pocketsteam/index.php/main/heartbeat";
var logoutUrl = "/pocketsteam/index.php/main/logout";
var sendCommandUrl = "/pocketsteam/index.php/main/sendmessage";

var heartbeatTimer;
var heartbeatInterval = 1500;

var userData;
var friends;
var status = 0;

function DisplayPage() {
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
    //message = message || "";
    //message = '{"To":"STEAM_0:1:20189445","Message":"Hi"}'; //HOLY MOTHER TESTING
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