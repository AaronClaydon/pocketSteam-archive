package com.pocketSteam;

import java.util.ArrayList;

public class User {
	static String userName;
	static String steamID;
	static String steamName;
	static ArrayList<SteamFriend> friends = new ArrayList<SteamFriend>();
	
	static Boolean friendsListOpen = false;
}