<div data-role="page">

    <div data-role="header">
        <a href="index.html" data-icon="grid">Desktop</a>
        <h1>Login</h1>
        <a href="<?= site_url('main/faq') ?>" data-rel="dialog" data-icon="info">FAQ</a>
    </div>

    <script type="text/javascript">
        LoginPage();
    </script>

    <div data-role="content">
        <div class="loginForm">
            <div id="loginMessage" class="">Please enter your Steam username and password below</div>
            <form action="<?= site_url('main/login') ?>" method="post" id="loginForm">
                <div data-role="fieldcontain">
                    <label for="name">Username:</label><br />
                    <input type="text" name="userName" id="userName" />
                </div> 
                
                <div data-role="fieldcontain">
                    <label for="name">Password:</label><br />
                    <input type="password" name="passWord" id="passWord" />
                </div>  

                <div data-role="fieldcontain" class="hidden">
                    <label for="name">Steam Guard Key:</label><br />
                    <input type="text" name="steamGuardKey" id="steamGuardKey" value=""/>
                </div>  

                <button type="submit" data-theme="b" id="loginButton">Login to Steam</button>
            </form>
        </div>
    </div>

    <div data-role="footer" data-position="fixed"> 
        <h4>&copy; <a href="http://www.azzy.co/">Azzy</a> &amp; Hosted by <a href="http://www.pwned.com/">Pwned.com</a></h4> 
    </div>
</div>

</body>
</html>