package com.pocketSteam;

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteDatabase.CursorFactory;
import android.database.sqlite.SQLiteOpenHelper;

public class Database extends SQLiteOpenHelper {
	
	private static String DatabaseName = "pocketSteam.db";
	
	public Database(Context context) {
		super(context, DatabaseName, null, 1);
	}

	@Override
	public void onCreate(SQLiteDatabase db) {
		db.execSQL("CREATE TABLE Avatars (id INTEGER PRIMARY KEY AUTOINCREMENT, SteamID TEXT, Avatar TEXT);");
	}

	@Override
	public void onUpgrade(SQLiteDatabase db, int arg1, int arg2) {
		db.execSQL("DROP TABLE Avatars");
		onCreate(db);
	}
}
