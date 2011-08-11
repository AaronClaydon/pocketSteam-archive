package com.pocketSteam;

import java.io.File;
import java.io.FileOutputStream;
import java.lang.reflect.Type;
import java.net.URL;
import java.net.URLConnection;
import java.util.ArrayList;

import com.pocketSteam.gson.Gson;
import com.pocketSteam.gson.reflect.TypeToken;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.ContentValues;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Bitmap;
import android.graphics.Bitmap.CompressFormat;
import android.graphics.BitmapFactory;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;
import android.view.View;
import android.widget.Button;

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
	public void ChatLogsButton(View view) {

	}
	public void SettingsButton(View view) {

	}
	public void DisconnectButton(View view) {
		moveTaskToBack(true);
        //TODO: Disconnect!
        API.connected = false;
        try {
			API.Contact("/AjaxCommand/" + API.SessionToken + "/1", "");
		} catch (Exception e) { }
    	this.finish();
	}
	/*
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
	*/
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
				Button enableButtons;
				enableButtons = (Button)findViewById(R.id.buttonFriends);
				
				if(connectingDialog == null && result.Status == 1) {
					connectingDialog = ProgressDialog.show(MenuActivity.this,    
		        						getString(R.string.PleaseWait), getString(R.string.GettingData), true);
				} else if(connectingDialog != null && result.Status != 1 && User.Data != null) {
					connectingDialog.cancel();
					connectingDialog = null;
					
					enableButtons.setEnabled(true);
					enableButtons = (Button)findViewById(R.id.buttonSettings);
					enableButtons.setEnabled(true);
					enableButtons = (Button)findViewById(R.id.buttonDisconnect);
					enableButtons.setEnabled(true);
					enableButtons = (Button)findViewById(R.id.buttonChatLogs);
					enableButtons.setEnabled(true);
					
					API.Started = true;
				}
				if(API.Started = true && !enableButtons.isEnabled() && result.Status != 1 && User.Data != null) {
					enableButtons.setEnabled(true);
					enableButtons = (Button)findViewById(R.id.buttonSettings);
					enableButtons.setEnabled(true);
					enableButtons = (Button)findViewById(R.id.buttonDisconnect);
					enableButtons.setEnabled(true);
					enableButtons = (Button)findViewById(R.id.buttonChatLogs);
					enableButtons.setEnabled(true);
				}
			}
			
			for(Message msg : result.Messages) {
				if(msg.Type == 1) {
					SteamUserData userData = gson.fromJson(msg.MessageValue, SteamUserData.class);
					
					User.Data = userData;
				}
				else if(msg.Type == 4) {
					Type collectionType = new TypeToken<ArrayList<SteamUserData>>(){}.getType();
					ArrayList<SteamUserData> friends = gson.fromJson(msg.MessageValue, collectionType);
					
					User.friends = friends;
					
					Runnable avatarRunnable = new Runnable() {
						@Override
						public void run() {
							for(SteamUserData friend : User.friends) {
								try {
									int position = User.friends.indexOf(friend);
						        	
									if(android.os.Environment.getExternalStorageState().equals(android.os.Environment.MEDIA_MOUNTED)) {
							        	//Database shenanigans!
							        	String[] avatarNameSplit = friend.AvatarURL.split("/");							        	
							        	String avatarName = avatarNameSplit[avatarNameSplit.length-1];
							        	
							        	Database dbHelper = new Database(MenuActivity.this);
							        	SQLiteDatabase db = dbHelper.getWritableDatabase();
							        	Cursor cursor = db.rawQuery("SELECT * FROM Avatars WHERE SteamID='" + friend.SteamID + "'", null);
							        	Boolean exists = cursor.moveToPosition(0);
							        	if(!exists) {
							        		friend.Avatar = API.DownloadImage(friend.AvatarURL);
							        		
							        		ContentValues cv = new ContentValues();
								        	cv.put("SteamID", friend.SteamID);
								        	cv.put("Avatar", avatarName);
								        	db.insert("Avatars", "SteamID", cv);
								        	
								        	String filepath = Environment.getExternalStorageDirectory().getAbsolutePath() + "/pocketSteam/AvatarCache/"; 
								        	File directoryCreate = new File(filepath);
								        	directoryCreate.mkdirs(); //Create the directories for the cache
								        	
								        	filepath += friend.SteamID.replaceAll(":", "_");
								        	FileOutputStream fos = null;
								        	fos = new FileOutputStream(filepath); 
								        	((BitmapDrawable)friend.Avatar).getBitmap().compress(CompressFormat.PNG, 75, fos);

							        	}
							        	if(!cursor.getString(2).equals(avatarName) && !avatarName.equals("fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg")) {
							        		db.delete("Avatars", "SteamID='" + friend.SteamID + "'", null);
							        		
							        		friend.Avatar = API.DownloadImage(friend.AvatarURL);
							        		
							        		ContentValues cv = new ContentValues();
								        	cv.put("SteamID", friend.SteamID);
								        	cv.put("Avatar", avatarName);
								        	db.insert("Avatars", "SteamID", cv);
								        	
								        	String filepath = Environment.getExternalStorageDirectory().getAbsolutePath() + "/pocketSteam/AvatarCache/"; 
								        	File directoryCreate = new File(filepath);
								        	directoryCreate.mkdirs(); //Create the directories for the cache
								        	
								        	filepath += friend.SteamID.replaceAll(":", "_");
								        	FileOutputStream fos = null;
								        	fos = new FileOutputStream(filepath); 
								        	((BitmapDrawable)friend.Avatar).getBitmap().compress(CompressFormat.PNG, 75, fos);
							        		
							        	} else {
							        		String filepath = Environment.getExternalStorageDirectory().getAbsolutePath() + "/pocketSteam/AvatarCache/";
							        		filepath += cursor.getString(1).replaceAll(":", "_");
							        		
							        		Bitmap bitmap = BitmapFactory.decodeFile(filepath);
							        		if(bitmap == null) {
							        			db.delete("Avatars", "SteamID='" + friend.SteamID + "'", null);
							        		} else {
							        			friend.Avatar = new BitmapDrawable(bitmap);
							        		}
							        	}
							        				        	
							        	User.friends.set(position, friend);
							        	cursor.close();
							        	db.close();
							        	dbHelper.close();
									}
						        	
								} catch(Exception ex) { }
							}
							if(User.friendsListOpen) {
								runOnUiThread(new Runnable() {
									@Override
									public void run() {
										FriendsListActivity.adapter.notifyDataSetChanged();
									} });
							}
						}
					};
					Thread avatarThread = new Thread(avatarRunnable);
					avatarThread.start();
					
					if(User.friendsListOpen) {
						/*
						FriendsListActivity.adapter.clear();
						for(SteamUserData friend : User.friends) {
							FriendsListActivity.adapter.add(friend);
						}
						*/
						
						FriendsListActivity.adapter.friends = User.friends;
						FriendsListActivity.adapter.notifyDataSetChanged();
					}
					if(User.chatOpen) {
						FriendChatActivity.Refresh();
					}
				}
			}
		}
	}
	/*
	private class DownloadImageTask extends AsyncTask<ArrayList<SteamUserData>, Void, HashMap<SteamUserData, Bitmap>> {
	     protected HashMap<SteamUserData, Bitmap> doInBackground(ArrayList<SteamUserData> friends) throws Exception {
	         for(SteamUserData friend : User.friends) {
	        	 URL url = new URL(friend.AvatarURL);
	        	 URLConnection connection = url.openConnection();
	        	 connection.setUseCaches(true);
	        	 Object response = connection.getContent();
	        	 if (response instanceof Bitmap) {
	        	 	Bitmap bitmap = (Bitmap)response;
	        	 	friend.Avatar = bitmap;
	        	 }
	         }
	     }

		@Override
		protected HashMap<SteamUserData, Bitmap> doInBackground(
				ArrayList<SteamUserData>... arg0) {
			// TODO Auto-generated method stub
			return null;
		}
	 }
	 */
}
