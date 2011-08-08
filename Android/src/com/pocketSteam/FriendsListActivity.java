package com.pocketSteam;
import java.util.ArrayList;

import android.app.ListActivity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class FriendsListActivity extends ListActivity {
	
	static FriendsAdapter adapter;
	
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setTitle(getString(R.string.app_name) + " / Friends");
		setContentView(R.layout.friends);

		OnItemClickListener clickListener = new OnItemClickListener() {
			@Override
			/*
			public void onItemClick(AdapterView<?> parent, View arg1, int position,
					long arg3) {
				SteamUserData friend = (SteamUserData)parent.getItemAtPosition(position);
				Toast.makeText(getApplicationContext(), "Clicked on: " + friend.SteamName, Toast.LENGTH_SHORT).show();
				try {
					API.Contact("/AjaxCommand/" + API.SessionToken + "/2", "messageTo=" + friend.SteamID + "&messageText=LOL I CLICKED ON U");
				} catch (Exception e) { }
			}
			*/
			public void onItemClick(AdapterView<?> parent, View arg1, int position, long arg3) {
				SteamUserData friend = (SteamUserData)parent.getItemAtPosition(position);
				
				Intent friendChatIntent = new Intent(FriendsListActivity.this, com.pocketSteam.FriendChatActivity.class);
				friendChatIntent.putExtra("SteamID", friend.SteamID);
				friendChatIntent.putExtra("SteamName", friend.SteamName);
				friendChatIntent.putExtra("SteamState", friend.State);
				friendChatIntent.putExtra("SteamAvatar", friend.AvatarURL);
				
		        startActivity(friendChatIntent);
			}
		};
		adapter = new FriendsAdapter(this, R.layout.friend, User.friends);
		setListAdapter(adapter);
		getListView().setOnItemClickListener(clickListener);
	}
	
	@Override
	public void onStart() {
		super.onStart();
		User.friendsListOpen = true;
	}
	@Override
	public void onStop() {
		super.onStart();
		User.friendsListOpen = false;
	}
	
	class FriendsAdapter extends ArrayAdapter<SteamUserData> {

        private ArrayList<SteamUserData> friends;

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
                }
                return v;
        }
	}
}
