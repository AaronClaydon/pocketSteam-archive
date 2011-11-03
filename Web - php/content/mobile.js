function LoginPage() {
	$(document).ready(function(){
		clearInterval(heartbeatTimer); //Incase for some reason it didn't end (IT HAPPENS)
		$("#loginButton").click(function() { 
			DisableTextBoxes();
			$("#loginMessage").removeClass();
			$("#loginMessage").addClass("loginNotice");
			$("#loginMessage").text('Logging in to your Steam account...');

			dataString = 'platform=Web&userName=' + $("#userName").val() + "&passWord=" + $("#passWord").val() + "&steamGuardKey=" + $("#steamGuardKey").val();

			$.ajax({
                type: "POST",
                data: dataString,
                url: loginUrl,

                success: function (data) {
	                var splitData = data.split(':');
	                
	                EnableTextBoxes();
	                
	                if(splitData[0] == "Success") {
	                	window.location = displayUrl;

	                } else if(splitData[0] == "pocketSteamOffline") {
	                	$("#loginMessage").removeClass();
	                	$("#loginMessage").addClass("loginError");
						$("#loginMessage").text('The pocketSteam server is currently not responding.');
					} else if(splitData[0] == "Invalid") {
	                	$("#loginMessage").removeClass();
	                	$("#loginMessage").addClass("loginError");
						$("#loginMessage").text('Invalid username/password was provided.');
					} else if(splitData[0] == "SteamGuard") {
						$("#loginMessage").removeClass();
	                	$("#loginMessage").addClass("loginError");
						$("#loginMessage").text('Please enter the Steam Guard key sent to your email account');
						$(".hidden").css('display', 'inline');
	                } else {
	                	alert('Unknown reply: '+ data);
	                }
                },
                error: function (data) {
                    alert('HTTP Error');
                }
            });


			return false;
		});

		function DisableTextBoxes() {
			$.mobile.showPageLoadingMsg();
			$("#userName").attr('disabled', 'disabled');
			$("#passWord").attr('disabled', 'disabled');
			$("#steamGuardKey").attr('disabled', 'disabled');
			$("#loginButton").parent().find('.ui-btn-text').text('Please wait');
			$("#loginButton").attr('disabled', 'disabled');
		}

		function EnableTextBoxes() {
			$.mobile.hidePageLoadingMsg();
			$("#userName").removeAttr('disabled');
			$("#passWord").removeAttr('disabled');
			$("#steamGuardKey").removeAttr('disabled');
			$("#loginButton").parent().find('.ui-btn-text').text('Login to Steam');
			$("#loginButton").removeAttr('disabled');
		}
	});
}

function ParseData(data) {
	if(data == "Expired") {
		$("#reply").html('Session Expired!');
		return;
	}

	var json = jQuery.parseJSON(data);
	
	for (message in json.M) {
    	var msgType = json.M[message].T;
    	var msg = jQuery.parseJSON(json.M[message].M);

    	if(msgType == 1) {
    		userData = msg;
    		UpdateInfo();
    	} else if(msgType == 2) {
    		alert(msg.N + ' said: ' + msg.M);
    	} else if(msgType == 3) {
    		//EMOTE MESSAGES!
    	} else if(msgType == 4) {
    		friends = msg;
    		UpdateFriends();
    	} else if (msgType == 5) {
    		$("#globalMessages").append('<div class="info">' + msg.GM + '</div>');
    	}
    }
}

function UpdateInfo() {
	$("#reply").html('<div class="friend"><img src="' + userData['A'] + '" alt="Avatar" class="steam_online"> ' + userData['N'] + ' - <span>' + userData['St'] + '</span></div>');
}

function UpdateFriends() {
	if ($("div[data-url='Friends']").length > 0) {
		var friendsHTML = FormatFriends();
		$("div[data-url='Friends'] .displayContent").html(friendsHTML);
	}
}

function FormatFriends() {
	var friendsList = '';
	
	for (friendID in friends) {
		var friend = friends[friendID];

		var avatarState = "offline";
		if (friend['StID'] == 1) {
            avatarState = "ingame";
	    } else if (friend['StID'] == 2 ||friend['StID'] == 3 || friend['StID'] == 4 || friend['StID'] == 5) {
			avatarState = "online";
        }

		friendsList = friendsList + '<div class="friend" onclick="StatePage();"><img src="' + friend['A'] + '" alt="Avatar" class="steam_' + avatarState + '"> ' + friend['N'] + ' - <span>' + friend['St'] + '</span></div>';
	}

	return friendsList;
}

function FriendsPage() {
	var html = '' + FormatFriends();

	ChangePage('Friends', html);
}

function StatePage() {
	var html = 'Current state: <b id="stateName">' + userData['St'] + '</b><div data-role="controlgroup"><a href="javascript: ChangeState(1);" data-role="button">Online</a><a href="javascript: ChangeState(3);" data-role="button">Away</a><a href="javascript: ChangeState(2);" data-role="button">Busy</a><a href="javascript: ChangeState(4);" data-role="button">Snooze</a><a href="javascript: ChangeState(0);" data-role="button">Offline</a></div>';
	ChangePage('Change State', html);
}

function LogoutPage() {
	var html = 'Are you sure you want to logout? <a href="javascript: Logout();" data-role="button">Yes</a><a href="index.html" data-role="button" data-rel="back">No</a>';
	ChangePage('Logout', html, 'd');
}

function ChangeState(state) {
    SendCommand(4, state);
    var stateName = "Online";

    if(state == 0) {
    	stateName = "Offline";
    } else if(state == 2) {
    	stateName = "Busy";
    } else if(state == 3) {
    	stateName = "Away";
    } else if(state == 4) {
    	stateName = "Snooze";
    }
    $("#stateName").text(stateName);
}

function ChangePage(title, html, theme) {
	theme = theme || "a";
	if ($("div[data-url='" + title + "']").length > 0){
		$.mobile.changePage( "#" + title );
	} else {
		var newPage = $('<div data-role="page" data-url="' + title + '"><div data-role="header" data-theme="' + theme + '"><a href="#" data-rel="back" data-role="button">Info</a><h1>' + title + '</h1></div><div data-role="content"><div class="displayContent">' + html + '</div></div></div>');
		newPage.appendTo( $.mobile.pageContainer );
		$.mobile.changePage( newPage );
	}
}