package com.pocketSteam;

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

public class Database extends SQLiteOpenHelper {
	
	private static String DatabaseName = "pocketSteam.db";
	
	public Database(Context context) {
		super(context, DatabaseName, null, 1);
	}

	@Override
	public void onCreate(SQLiteDatabase db) {
		db.execSQL("CREATE TABLE Avatars (id INTEGER PRIMARY KEY AUTOINCREMENT, SteamID TEXT, Avatar TEXT);");
		db.execSQL("CREATE TABLE Settings (id INTEGER PRIMARY KEY AUTOINCREMENT, Setting TEXT, Value TEXT);");
		db.execSQL("CREATE TABLE Messages (id INTEGER PRIMARY KEY AUTOINCREMENT, SteamID TEXT, Type INTEGER, Message TEXT, DateCreated TIMESTAMP);");
	}

	@Override
	public void onUpgrade(SQLiteDatabase db, int arg1, int arg2) {
		db.execSQL("DROP TABLE Avatars");
		db.execSQL("DROP TABLE Settings");
		db.execSQL("DROP TABLE Messages");
		onCreate(db);
	}
}
