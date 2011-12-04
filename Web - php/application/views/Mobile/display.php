<div data-role="page">
    
    <script type="text/javascript">
        DisplayPage();
    </script>

    <div data-role="header">
        <a href="javascript: FriendsPage();" data-role="button">Friends</a>
        <h1>Info</h1>
        <a href="<?= site_url('main/faq') ?>" data-rel="dialog" data-icon="alert"><span class="ui-li-count ui-btn-up-c ui-btn-corner-all">2</span></a>
    </div>

    <div data-role="content">
        <div class="displayContent">
            <div id="globalMessages"></div>
            <div id="reply">Loading data....</div>

            <div data-role="controlgroup">
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