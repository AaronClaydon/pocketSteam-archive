<?php
	$this->load->helper('url');
?>

<!DOCTYPE html> 
<html> 
    <head> 
    <title>Login</title> 
    
    <meta name="viewport" content="width=device-width, initial-scale=1"> 

    <link rel="stylesheet" href="http://code.jquery.com/mobile/1.0b3/jquery.mobile-1.0b3.min.css" />
    <link rel="stylesheet" href="content/mobile.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.6.3.min.js"></script>
    <script type="text/javascript" src="http://code.jquery.com/mobile/1.0b3/jquery.mobile-1.0b3.min.js"></script>

    <script type="text/javascript" src="content/mobile.js"></script>
    <script type="text/javascript">
        loginPage();
    </script>

    <meta content='True' name='HandheldFriendly' />
	<meta content='width=device-width; initial-scale=1.0; maximum-scale=1.0; user-scalable=0;' name='viewport' />
	<meta name="viewport" content="width=device-width" />

</head> 
<body> 

<div data-role="page">

    <div data-role="header">
        <a href="index.html" data-icon="grid">Desktop</a>
        <h1>Login</h1>
        <a href="<?= site_url('main/faq') ?>" data-rel="dialog" data-icon="info">FAQ</a>
    </div>

    <div data-role="content">
        <div class="loginForm">
            <div id="loginMessage" class="">Please enter your Steam username and password below</div>
            <form action="<?= site_url('main/login') ?>" method="post" id="loginForm">
                <div data-role="fieldcontain">
                    <label for="name">Username:</label><br />
                    <input type="text" name="userName" id="userName" value="azzytest"/>
                </div> 
                
                <div data-role="fieldcontain">
                    <label for="name">Password:</label><br />
                    <input type="password" name="passWord" id="passWord" value="test123"/>
                </div>  

                <div data-role="fieldcontain" class="hidden">
                    <label for="name">Steam Guard Key:</label><br />
                    <input type="text" name="steamGuardKey" id="steamGuardKey" value=""/>
                </div>  

                <button type="button" data-theme="b" id="loginButton">Login to Steam</button>
            </form>
        </div>
    </div>
</div>

</body>
</html>