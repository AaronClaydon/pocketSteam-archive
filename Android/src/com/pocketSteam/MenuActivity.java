package com.pocketSteam;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;

public class MenuActivity extends Activity {
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		setTitle(getString(R.string.app_name) + " / Menu");
		setContentView(R.layout.menu);
	}
	
	public void FriendsButton(View view) {
		Intent friendsIntent = new Intent(MenuActivity.this, com.pocketSteam.FriendsListActivity.class);
        startActivity(friendsIntent);
	}
	/*
	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
	    if (keyCode == KeyEvent.KEYCODE_BACK) {
	        moveTaskToBack(true);
	        //TODO: Disconnect!
	    	this.finish();
	        return true;
	    }
	    return super.onKeyDown(keyCode, event);
	}
	*/
}
