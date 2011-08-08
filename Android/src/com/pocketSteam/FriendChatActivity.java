package com.pocketSteam;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;

public class FriendChatActivity extends Activity {
	
	String SteamID;
	
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.chat);
		
		Bundle extras = getIntent().getExtras();
		SteamID = extras.getString("SteamID");
		
		TextView tv = (TextView)findViewById(R.id.ChatSteamName);
		tv.setText(extras.getString("SteamName"));
		
		tv = (TextView)findViewById(R.id.ChatState);
		tv.setText(extras.getString("SteamState"));
	}
	
	public void SendMessage(View view) {
		EditText msg = (EditText) findViewById(R.id.ChatMessage);
		String message = msg.getText().toString();
		
		if(!message.equals("")) //&& !Pattern.matches("/^\\s+$/", message)
		try {
			API.Contact("/AjaxCommand/" + API.SessionToken + "/2", "messageTo=" + SteamID + "&messageText=" + message);
		} catch (Exception e) { }
		msg.setText("");
	}
}
