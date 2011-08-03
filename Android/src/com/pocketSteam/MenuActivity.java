package com.pocketSteam;

import com.pocketSteam.gson.Gson;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.widget.TextView;

public class MenuActivity extends Activity {	
	
	Boolean connected = true;
	
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		setTitle(getString(R.string.app_name) + " / Menu");
		setContentView(R.layout.menu);
		Thread serverHandlerThread = new Thread(new Runnable() {
			@Override
			public void run() {
				try {
					while(connected) {
						String reply = API.Contact("/AjaxReply/" + API.SessionToken, "");
						Gson gson = new Gson();
						JsonReply obj = gson.fromJson(reply, JsonReply.class);
						
						//setTitle(getString(R.string.app_name) + " / Menu [" + obj.Status + "]");
						
						Thread.sleep(1250);
					}
				}
				catch(Exception ex) { MenuActivity.this.finish(); }
			} }, "Communication Thread");
		serverHandlerThread.start();
	}
	
	public void FriendsButton(View view) {
		Intent friendsIntent = new Intent(MenuActivity.this, com.pocketSteam.FriendsListActivity.class);
        startActivity(friendsIntent);
	}
	
	public void SettingsButton(View view) {
		new AlertDialog.Builder(MenuActivity.this)
		.setTitle("DEBUG")
		.setMessage("Sent").create().show();
	}
	
	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
	    if (keyCode == KeyEvent.KEYCODE_BACK) {
	        moveTaskToBack(true);
	        //TODO: Disconnect!
	        connected = false;
	        try {
				API.Contact("/AjaxCommand/" + API.SessionToken + "/1", "");
			} catch (Exception e) { }
	    	this.finish();
	        return true;
	    }
	    return super.onKeyDown(keyCode, event);
	}
}
