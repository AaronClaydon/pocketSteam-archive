function LoginPage() {
	$(document).ready(function(){
		clearInterval(heartbeatTimer); //Incase for some reason it didn't end (IT HAPPENS)
		$("#loginButton").click(function() { 
			DisableTextBoxes();
			$("#loginMessage").removeClass();
			$("#loginMessage").addClass("loginNotice");
			$("#loginMessage").text('Logging in to your Steam account...');

			dataString = 'userName=' + $("#userName").val() + "&passWord=" + $("#passWord").val() + "&steamGuardKey=" + $("#steamGuardKey").val();

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
	for (friendID in friends) {
		var friend = friends[friendID];

		//alert(friend.N + " - " + friend.St);
	}
}