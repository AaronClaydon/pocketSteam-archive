package com.pocketSteam;
import java.util.ArrayList;

import android.app.AlertDialog;
import android.app.ListActivity;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.AdapterView.OnItemLongClickListener;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

public class FriendsListActivity extends ListActivity {
	
	static FriendsAdapter adapter;
	static Intent friendChatIntent;
	
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setTitle(getString(R.string.app_name) + " / Friends");
		setContentView(R.layout.friends);

		OnItemClickListener clickListener = new OnItemClickListener() {
			@Override
			public void onItemClick(AdapterView<?> parent, View arg1, int position, long arg3) {
				SteamUserData friend = adapter.friends.get(position);
				
				if(!friend.State.equals("Offline")) {
					friendChatIntent = new Intent(FriendsListActivity.this, com.pocketSteam.FriendChatActivity.class);
					friendChatIntent.putExtra("SteamID", friend.SteamID);
					
			        startActivity(friendChatIntent);
				}
			}
		};
		
		OnItemLongClickListener longClickListener = new OnItemLongClickListener() {
			@Override
			public boolean onItemLongClick(AdapterView<?> parent, View arg1, int position, long arg3) {
				final SteamUserData friend = adapter.friends.get(position);
				
				final String[] items;
				if(friend.State.equals("Offline")) {
					final String[] itemsT = { "Chat Log", "Community Profile" };
					items = itemsT;
				} else {
					final String[] itemsT = { "Chat", "Community Profile" };
					items = itemsT;
				}
				
				new AlertDialog.Builder(FriendsListActivity.this)
				.setTitle(friend.SteamName)
				.setItems(items, new DialogInterface.OnClickListener() {
				    public void onClick(DialogInterface dialog, int item) {
				        if(items[item].equals("Chat") || items[item].equals("Chat Log")) {
				        	friendChatIntent = new Intent(FriendsListActivity.this, com.pocketSteam.FriendChatActivity.class);
							friendChatIntent.putExtra("SteamID", friend.SteamID);
							
					        startActivity(friendChatIntent);
				        } else if(items[item].equals("Community Profile")) {
				        	Toast.makeText(getApplicationContext(), "Not yet implemented", Toast.LENGTH_SHORT).show();
				        }
				    }
				}).create().show();

				return true;
			}
		};
		
		adapter = new FriendsAdapter(this, R.layout.friend, User.friends);
		setListAdapter(adapter);
		getListView().setOnItemClickListener(clickListener);
		getListView().setOnItemLongClickListener(longClickListener);
		User.friendsListOpen = true;
	}
	
	@Override
	public void onStart() {
		super.onStart();
		User.friendsListOpen = true;
		adapter.notifyDataSetChanged();
	}
	@Override
	public void onStop() {
		super.onStart();
		User.friendsListOpen = false;
	}
	
	class FriendsAdapter extends ArrayAdapter<SteamUserData> {

        public ArrayList<SteamUserData> friends;

        public FriendsAdapter(Context context, int textViewResourceId, ArrayList<SteamUserData> friends) {
                super(context, textViewResourceId, friends);
                this.friends = friends;
        }
        
        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
                View v = convertView;
                if (v == null) {
                    LayoutInflater vi = (LayoutInflater)getSystemService(Context.LAYOUT_INFLATER_SERVICE);
                    v = vi.inflate(R.layout.friend, null);
                }
                SteamUserData friend = friends.get(position);
                if (friend != null) {
                        TextView userNameView = (TextView) v.findViewById(R.id.steamName);
                        TextView stateView = (TextView) v.findViewById(R.id.state);
                        
                        userNameView.setText(friend.SteamName);
                        stateView.setText(friend.State);
                        
                        SharedPreferences settings = PreferenceManager.getDefaultSharedPreferences(getBaseContext());
                        
                        if(friend.Avatar != null && settings.getBoolean("displayAvatar", true)) {
                        	ImageView image = (ImageView)v.findViewById(R.id.steamAvatar);
                        	image.setImageDrawable(friend.Avatar);
                        	
                        	image.setMinimumWidth(32);
                        	image.setMinimumHeight(32);
                        	image.setMaxWidth(32);
                        	image.setMaxHeight(32);
                        }
                }
                return v;
        }
	}
}
