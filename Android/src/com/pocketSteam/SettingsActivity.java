package com.pocketSteam;

import android.os.Bundle;
import android.preference.PreferenceActivity;

public class SettingsActivity extends PreferenceActivity {
	@Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);  
        setTitle(getString(R.string.app_name) + " / Settings");

        addPreferencesFromResource(R.layout.settings);
    }
}
