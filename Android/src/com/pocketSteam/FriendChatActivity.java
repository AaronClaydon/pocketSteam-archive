package com.pocketSteam;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.ContentValues;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.text.Html;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.EditText;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.ImageView;
import android.widget.Button;
import android.widget.Toast;

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
				
				//Disable textbox and button if offline
				if(friend.State.equals("Offline")) {
					EditText textBox = (EditText)findViewById(R.id.ChatMessage);
					textBox.setEnabled(false);
					
					Button sendButton = (Button)findViewById(R.id.ChatSendButton);
					sendButton.setEnabled(false);
				}
			}
		}
		
		User.chatOpen = true;
		LoadChatWindow();
		
		ImageView avatar = (ImageView)findViewById(R.id.ChatAvatar);
		avatar.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View arg0) {
				final String[] items = { "Clear Chat History" };
				
				new AlertDialog.Builder(getApplicationContext())
				.setItems(items, new DialogInterface.OnClickListener() {
				    public void onClick(DialogInterface dialog, int item) {
				        if(items[item].equals("Clear Chat History")) {
				        	Database dbHelper = new Database(getApplicationContext());
				        	SQLiteDatabase db = dbHelper.getWritableDatabase();
				        	db.rawQuery("DELETE FROM Messages WHERE SteamID='" + SteamID + "'", null);
				        	
				        	FriendChatActivity.LoadChatWindow();
				        	
				        	Toast.makeText(getApplicationContext(), "History cleared", Toast.LENGTH_SHORT).show();
				        } else if(items[item].equals("Community Profile")) {
				        }
				    }
				}).create().show();
			}});
		
		//TextView chat = (TextView)activity.findViewById(R.id.ChatMessages);
		//chat.setMovementMethod(new ScrollingMovementMethod());
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
    		
    		ScrollView scroll = (ScrollView)activity.findViewById(R.id.ChatLogScroll);
    		scroll.fullScroll(View.FOCUS_DOWN);
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
				
				//Disable textbox and button if offline
				EditText textBox = (EditText)activity.findViewById(R.id.ChatMessage);
				Button sendButton = (Button)activity.findViewById(R.id.ChatSendButton);
				if(friend.State.equals("Offline")) {
					textBox.setEnabled(false);
					sendButton.setEnabled(false);
				} else {
					textBox.setEnabled(true);
					sendButton.setEnabled(true);
				}
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
			
        	final int messageType2 = messageType;
        	final String message2 = message;
        	new Thread(new Runnable() {
				@Override
				public void run() {
					try {
						API.Contact("/AjaxCommand/" + API.SessionToken + "/" + messageType2, "messageTo=" + SteamID + "&messageText=" + message2);
					} catch (Exception e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
				} }).start();

		} catch (Exception e) { }
		msg.setText("");
		LoadChatWindow();
	}
}
