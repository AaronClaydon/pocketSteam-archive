function loginPage() {
	$(document).ready(function(){
		$(".loginForm").submit(function() { 
			$("#userName").attr('disabled', 'true');
			$("#passWord").attr('disabled', 'true');
			$("#loginButton").parent().find('.ui-btn-text').text('Please wait');
			$("#loginButton").attr('disabled', 'true');
			
			$("#loginMessage").text('Logging in to your Steam account...');
			$("#loginMessage").removeClass();
			$("#loginMessage").addClass("loginNotice");

			return false;
		})
	});
}