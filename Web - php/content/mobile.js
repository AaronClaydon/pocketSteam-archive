function loginPage() {
	$(document).ready(function(){
		$("#loginButton").click(function() { 
			DisableTextBoxes();
			$("#loginMessage").removeClass();
			$("#loginMessage").addClass("loginNotice");
			$("#loginMessage").text('Logging in to your Steam account...');

			dataString = 'userName=' + $("#userName").val() + "&passWord=" + $("#passWord").val() + "&steamGuardKey=" + $("#steamGuardKey").val();

			$.ajax({
                type: "POST",
                data: dataString,
                url: "index.php/main/login",

                success: function (data) {
	                var splitData = data.split(':');
	                
	                EnableTextBoxes();
	                
	                if(splitData[0] == "Success") {
	                	var gotoLocation = "index.php/main/display";
	                	window.location.replace(gotoLocation);

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
			$.mobile.pageLoading();  
			$("#userName").attr('disabled', 'disabled');
			$("#passWord").attr('disabled', 'disabled');
			$("#steamGuardKey").attr('disabled', 'disabled');
			$("#loginButton").parent().find('.ui-btn-text').text('Please wait');
			$("#loginButton").attr('disabled', 'disabled');
			
		}

		function EnableTextBoxes() {
			$.mobile.pageLoading(true);
			$("#userName").removeAttr('disabled');
			$("#passWord").removeAttr('disabled');
			$("#steamGuardKey").removeAttr('disabled');
			$("#loginButton").parent().find('.ui-btn-text').text('Login to Steam');
			$("#loginButton").removeAttr('disabled');
		}
	});
}