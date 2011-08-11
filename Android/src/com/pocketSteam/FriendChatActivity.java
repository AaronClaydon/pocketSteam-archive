package com.pocketSteam;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.ImageView;

public class FriendChatActivity extends Activity {
	
	static String SteamID;
	static Context context;
	static Activity activity;
	
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		activity = this;
		context = getApplicationContext();
		
		setContentView(R.layout.chat);
		Bundle extras = getIntent().getExtras();

		SteamID = extras.getString("SteamID");
		for(SteamUserData friend : User.friends) {
			if(friend.SteamID.equals(SteamID)) {
				TextView tv = (TextView)findViewById(R.id.ChatSteamName);
				tv.setText(friend.SteamName);
				setTitle(getString(R.string.app_name) + " / " + friend.SteamName);
				
				tv = (TextView)findViewById(R.id.ChatState);
				tv.setText(friend.State);
				
				ImageView avatar = (ImageView)findViewById(R.id.ChatAvatar);
				avatar.setImageDrawable(friend.Avatar);
				
				avatar.setMaxHeight(64);
				avatar.setMaxWidth(64);
				avatar.setMinimumHeight(64);
				avatar.setMinimumWidth(64);
			}
		}
		
		User.chatOpen = true;
	}
	
	@Override
	public void onStart() {
		super.onStart();
		User.chatOpen = true;
	}
	@Override
	public void onStop() {
		super.onStart();
		User.chatOpen = false;
	}
	
	public static void Refresh() {
		for(SteamUserData friend : User.friends) {
			if(friend.SteamID.equals(SteamID)) {
				TextView tv = (TextView)activity.findViewById(R.id.ChatSteamName);
				tv.setText(friend.SteamName);
				activity.setTitle(context.getString(R.string.app_name) + " / " + friend.SteamName);
				
				tv = (TextView)activity.findViewById(R.id.ChatState);
				tv.setText(friend.State);
				
				ImageView avatar = (ImageView)activity.findViewById(R.id.ChatAvatar);
				avatar.setImageDrawable(friend.Avatar);
				
				avatar.setMaxHeight(32);
				avatar.setMaxWidth(32);
				avatar.setMinimumHeight(32);
				avatar.setMinimumWidth(32);
			}
		}
	}
	
	public void SendMessage(View view) {
		EditText msg = (EditText) findViewById(R.id.ChatMessage);
		String message = msg.getText().toString();
		int messageType = 2;
		
		if(!message.equals("")) //&& !Pattern.matches("/^\\s+$/", message)
		try {
			String[] splitMessage = message.split(" ");
			if(splitMessage[0].equals("/me")) {
				messageType = 3;
				message = message.replaceFirst("/me ", "");
			}
			
			String reply = API.Contact("/AjaxCommand/" + API.SessionToken + "/" + messageType, "messageTo=" + SteamID + "&messageText=" + message);
			if(!reply.equals("OK")) {
				new AlertDialog.Builder(FriendChatActivity.this)
                .setTitle(R.string.app_name)
                .setMessage(R.string.ConnectionExpired)
                .create().show();
			}
		} catch (Exception e) { }
		msg.setText("");
	}
}
