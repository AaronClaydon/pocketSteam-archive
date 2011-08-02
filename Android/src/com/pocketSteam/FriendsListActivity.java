package com.pocketSteam;
import java.util.ArrayList;

import android.app.ListActivity;
import android.content.Context;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.TextView;
import android.widget.Toast;

public class FriendsListActivity extends ListActivity {
	
	@Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setTitle(getString(R.string.app_name) + " / Friends");
		setContentView(R.layout.friends);
		/*
		OnItemClickListener clickListener = new OnItemClickListener() {
			@Override
			public void onItemClick(AdapterView<?> parent, View arg1, int position,
					long arg3) {
				Toast.makeText(getApplicationContext(), "Clicked on: " + parent.getItemAtPosition(position).toString(), Toast.LENGTH_SHORT).show();
			}
		};	*/	
		ArrayList<SteamFriend> friends = new ArrayList<SteamFriend>();
		
		SteamFriend friendAzzy = new SteamFriend();
		friendAzzy.SteamName = "Azzy";
		friendAzzy.State = "Online";
		friends.add(friendAzzy);
		
		SteamFriend friendTrip = new SteamFriend();
		friendTrip.SteamName = "Trippeh";
		friendTrip.State = "Offline";
		friends.add(friendTrip);
		
		SteamFriend friendQ = new SteamFriend();
		friendQ.SteamName = "Qwerty";
		friendQ.State = "Playing: Minecraft";
		friends.add(friendQ);
		
		FriendsAdapter adapter = new FriendsAdapter(this, R.layout.friend, friends);
		setListAdapter(adapter);
		//getListView().setOnItemClickListener(clickListener);
	}
	
	public class SteamFriend {
		public String SteamName;
		public String State;
	}
	
	private class FriendsAdapter extends ArrayAdapter<SteamFriend> {

        private ArrayList<SteamFriend> friends;

        public FriendsAdapter(Context context, int textViewResourceId, ArrayList<SteamFriend> friends) {
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
                SteamFriend friend = friends.get(position);
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
