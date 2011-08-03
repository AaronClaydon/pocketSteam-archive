package com.pocketSteam;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.List;

import com.pocketSteam.gson.Gson;
import com.pocketSteam.gson.reflect.TypeToken;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

public class MenuActivity extends Activity {	
	
	AlertDialog connectingDialog = null;
	
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		setTitle(getString(R.string.app_name) + " / Menu");
		setContentView(R.layout.menu);
		Thread serverHandlerThread = new Thread(new Runnable() {
			@Override
			public void run() {
				try {
					while(API.connected) {
						new BackgroundTask().execute();
						
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

	}
	
	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
	    if (keyCode == KeyEvent.KEYCODE_BACK) {
	        moveTaskToBack(true);
	        //TODO: Disconnect!
	        API.connected = false;
	        try {
				API.Contact("/AjaxCommand/" + API.SessionToken + "/1", "");
			} catch (Exception e) { }
	    	this.finish();
	        return true;
	    }
	    return super.onKeyDown(keyCode, event);
	}
	
	private class BackgroundTask extends AsyncTask<Void, Void, String> {

		@Override
		protected String doInBackground(Void... arg0) {
			try {
				String reply = API.Contact("/AjaxReply/" + API.SessionToken, "");
				
				return reply;
			} catch(Exception ex) { }
			return null;
		}
		protected void onPostExecute(String rawResult) {
			Gson gson = new Gson();
			JsonReply result;
			try {
				result = gson.fromJson(rawResult, JsonReply.class);
			} catch(Exception ex) { return; }
			
			if(result.equals("Your session has expired")) {
				API.connected = false;
				new AlertDialog.Builder(MenuActivity.this)
                .setTitle(R.string.app_name)
                .setMessage(R.string.ConnectionExpired)
                .create().show();
			}
			
			if(result != null) {
				if(connectingDialog == null && result.Status == 1) {
					connectingDialog = ProgressDialog.show(MenuActivity.this,    
		        						getString(R.string.PleaseWait), getString(R.string.GettingData), true);
				} else if(connectingDialog != null && result.Status != 1) {
					connectingDialog.cancel();
					connectingDialog = null;
					
					Button enableButtons;
					enableButtons = (Button)findViewById(R.id.buttonFriends);
					enableButtons.setEnabled(true);
					enableButtons = (Button)findViewById(R.id.buttonSettings);
					enableButtons.setEnabled(true);
				}
			}
			
			for(Message msg : result.Messages) {
				if(msg.Type == 4) {
					Type collectionType = new TypeToken<ArrayList<SteamFriend>>(){}.getType();
					ArrayList<SteamFriend> friends = gson.fromJson(msg.MessageValue, collectionType);
					
					User.friends = friends;
					if(User.friendsListOpen) {
						FriendsListActivity.adapter.clear();
						for(SteamFriend friend : User.friends) {
							FriendsListActivity.adapter.add(friend);
						}
					}
				}
			}
		}
	}
}