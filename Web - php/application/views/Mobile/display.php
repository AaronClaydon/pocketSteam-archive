<div data-role="page">
    
    <script type="text/javascript">
        DisplayPage();
    </script>
    
    <div data-role="header">
        <a href="<?= site_url('main/faq') ?>" data-rel="dialog" data-icon="info">Friends</a>
        <h1>Info</h1>
    </div>

    <div data-role="content">
        <div class="displayContent">
            <div id="reply">Loading data....</div>

            <div data-role="controlgroup">
            <a href="index.html" data-role="button">Change state</a>
            <a href="index.html" data-role="button">View community profile</a>
            <a href="<?= site_url('main/logout') ?>" data-role="button">Logout</a>
            </div>
        </div>
    </div>
</div>

</body>
</html>