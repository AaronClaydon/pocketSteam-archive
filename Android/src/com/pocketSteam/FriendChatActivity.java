package com.pocketSteam;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.text.Html;
import android.text.method.ScrollingMovementMethod;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.ImageView;
import android.widget.ScrollView;;

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
		LoadChatWindow();
		
		TextView chat = (TextView)activity.findViewById(R.id.ChatMessages);
		chat.setMovementMethod(new ScrollingMovementMethod());
	}
	
	public static void LoadChatWindow() {
		TextView chat = (TextView)activity.findViewById(R.id.ChatMessages);
		 
		Database dbHelper = new Database(activity);
    	SQLiteDatabase db = dbHelper.getWritableDatabase();
    	Cursor cursor = db.rawQuery("SELECT * FROM Messages WHERE SteamID='" + SteamID + "'", null);
    	Boolean exists = cursor.moveToFirst();
    	
    	if(exists) {
    		String messages = "";
    		
    		while(!cursor.isAfterLast()) {
    			messages += Html.fromHtml(cursor.getString(3)) + "\n";
    			cursor.moveToNext();
    		}
    		
    		chat.setText(messages);
    		
    		//ScrollView scroll = (ScrollView)activity.findViewById(R.id.ChatMessagesScroll);
    		//scroll.fullScroll(View.FOCUS_DOWN);
    	}
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
		message = message.replaceAll("&", "and");
		int messageType = 2;
		
		if(!message.equals("")) //&& !Pattern.matches("/^\\s+$/", message)
		try {
			String[] splitMessage = message.split(" ");
			if(splitMessage[0].equals("/me")) {
				messageType = 3;
				message = message.replaceFirst("/me ", "");
			}
			
			Database dbHelper = new Database(FriendChatActivity.this);
        	SQLiteDatabase db = dbHelper.getWritableDatabase();
			ContentValues cv = new ContentValues();
        	cv.put("SteamID", SteamID);
        	cv.put("Type", messageType);
        	if(messageType == 2) {
        		cv.put("Message", User.Data.SteamName + ": " + message);
        	} else {
        		cv.put("Message", User.Data.SteamName + " " + message);
        	}
        	db.insert("Messages", "SteamID", cv);
			
			String reply = API.Contact("/AjaxCommand/" + API.SessionToken + "/" + messageType, "messageTo=" + SteamID + "&messageText=" + message);
			if(!reply.equals("OK")) {
				new AlertDialog.Builder(FriendChatActivity.this)
                .setTitle(R.string.app_name)
                .setMessage(R.string.ConnectionExpired)
                .create().show();
			}
		} catch (Exception e) { }
		msg.setText("");
		LoadChatWindow();
	}
}
