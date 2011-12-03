<div data-role="page">
    
    <script type="text/javascript">
        DisplayPage();
    </script>

    <div data-role="header">
        <a href="javascript: FriendsPage();" data-role="button">Friends</a>
        <h1>Info</h1>
    </div>

    <div data-role="content">
        <div class="displayContent">
            <div id="globalMessages"></div>
            <div id="reply">Loading data....</div>

            <div data-role="controlgroup">
                <a href="javascript: SendChatMessage(2, 'This is a test message', 'STEAM_0:1:20189445');" data-role="button">Test button</a>
                <a href="javascript: StatePage();" data-role="button">Change state</a>
                <a href="javascript: alert('Not implemented');" data-role="button">View community profile</a>
                <a href="javascript: alert('Not implemented');" data-role="button">Settings</a>
                <a href="javascript: LogoutPage();" data-role="button">Logout</a>
            </div>
        </div>
    </div>
</div>

</body>
</html>