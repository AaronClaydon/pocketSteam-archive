package com.pocketSteam;
import com.pocketSteam.R;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.*;
import android.widget.*;

import java.io.*;

public class LoginActivity extends Activity {
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
    	super.onCreate(savedInstanceState);
        setContentView(R.layout.login);
        setTitle(getString(R.string.app_name) + " / Login");

        EditText userName = (EditText)this.findViewById(R.id.userName);
    	EditText passWord = (EditText)this.findViewById(R.id.passWord);
    	CheckBox rememberMeBox = (CheckBox)this.findViewById(R.id.rememberMe);
    	
    	try {
	    	FileInputStream fileStream = openFileInput("pocketSteam_LastUsername");
	    	InputStreamReader inputreader = new InputStreamReader(fileStream);
	        BufferedReader buffreader = new BufferedReader(inputreader);
	        
	        userName.setText(buffreader.readLine());
	        
	    	fileStream.close();
    	}
    	catch(Exception ex) { }
    	
    	try {
	    	FileInputStream fileStream = openFileInput("pocketSteam_SavedPassword");
	    	InputStreamReader inputreader = new InputStreamReader(fileStream);
	        BufferedReader buffreader = new BufferedReader(inputreader);
	        
	        String pass = buffreader.readLine();
	        if(!pass.equals("")) {
	        	passWord.setText(pass);
	        	rememberMeBox.setChecked(true);
	        }
	        
	    	fileStream.close();
    	}
    	catch(Exception ex) { }
    }
       
    public void LoginButton(View view) {
    	EditText userName = (EditText)this.findViewById(R.id.userName);
    	EditText passWord = (EditText)this.findViewById(R.id.passWord);
    	Button loginButton = (Button)this.findViewById(R.id.loginButton);
    	CheckBox rememberMeBox = (CheckBox)this.findViewById(R.id.rememberMe);
    	
    	userName.setEnabled(false);
    	passWord.setEnabled(false);
    	loginButton.setEnabled(false);
    	rememberMeBox.setEnabled(false);
    	
    	new LoginTask().execute(userName.getText().toString(), passWord.getText().toString());
    }
    
    private class LoginTask extends AsyncTask<String, Void, String> {
    	AlertDialog connectingDialog;
    	String userName = null;
    	String passWord = null;
    	
        protected String doInBackground(String... loginDetails) {
        	String reply = null;
        	
        	userName = loginDetails[0];
        	passWord = loginDetails[1];
        	
			try {
				//reply = "Success:rofl:what";
				reply = API.Contact("/Home/Login/", "userName=" + loginDetails[0] + "&passWord=" + loginDetails[1] + "&steamGuardAccessKey=");
			} catch (Exception e) {
				return null;
			}

            return reply;
        }
        
        protected void onPreExecute() {
        	connectingDialog = ProgressDialog.show(LoginActivity.this,    
        			getString(R.string.PleaseWait), getString(R.string.Connecting), true);
               /*     
        	connectingDialog = new AlertDialog.Builder(LoginActivity.this)
            .setTitle(R.string.app_name)
            .setMessage(R.string.Connecting)
            .create();
        	*/
        	connectingDialog.show();
        }
        
        private void UnlockMenu() {
        	EditText userName = (EditText)findViewById(R.id.userName);
        	EditText passWord = (EditText)findViewById(R.id.passWord);
        	Button loginButton = (Button)findViewById(R.id.loginButton);
        	CheckBox rememberMeBox = (CheckBox)findViewById(R.id.rememberMe);
        	
        	userName.setEnabled(true);
        	passWord.setEnabled(true);
        	loginButton.setEnabled(true);
        	rememberMeBox.setEnabled(true);
        }

        protected void onPostExecute(String result) {
        	connectingDialog.cancel();
        	
        	if(result == null) {
        		UnlockMenu();
        		new AlertDialog.Builder(LoginActivity.this)
                .setTitle(R.string.app_name)
                .setMessage(R.string.CannotConnect)
                .create().show();
        		
        		return;
        	}
        	String[] resultArray = result.split(":");
        	if(resultArray[0].equals("Invalid")) {
        		UnlockMenu();
        		new AlertDialog.Builder(LoginActivity.this)
                .setTitle(R.string.app_name)
                .setMessage(R.string.InvalidDetails)
                .create().show();
        	} else if(resultArray[0].equals("SteamGuard")) {
        		UnlockMenu();
        		new AlertDialog.Builder(LoginActivity.this)
                .setTitle(R.string.app_name)
                .setMessage("TEMP - SteamGuard not supported")
                .create().show();
        	} else if(resultArray[0].equals("Success")) {
        		UnlockMenu();
        		User.userName = userName;
        		API.SessionToken = resultArray[1];
        		API.PassKey = resultArray[2];
        		
        		try {
	        		FileOutputStream fos = openFileOutput("pocketSteam_LastUsername", Context.MODE_PRIVATE);
	    			fos.write(userName.getBytes());
	    			fos.close();
        		}
        		catch(Exception ex) {}
        		
        		CheckBox rememberPass = (CheckBox)findViewById(R.id.rememberMe);
        		if(rememberPass.isChecked()) {
        			try {
	        			FileOutputStream fos = openFileOutput("pocketSteam_SavedPassword", Context.MODE_PRIVATE);
	        			fos.write(passWord.getBytes());
	        			fos.close();
        			}
        			catch(Exception ex) { }
        		}
        		else
        		{
        			try {
	        			FileOutputStream fos = openFileOutput("pocketSteam_SavedPassword", Context.MODE_PRIVATE);
	        			fos.write("".getBytes());
	        			fos.close();
        			}
        			catch(Exception ex) { }
        		}
        		Intent menuIntent = new Intent(LoginActivity.this, com.pocketSteam.MenuActivity.class);
        		startActivity(menuIntent);
        		LoginActivity.this.finish();
        	} else {
        		UnlockMenu();
        		new AlertDialog.Builder(LoginActivity.this)
                .setTitle(R.string.app_name)
                .setMessage(R.string.CannotConnect)
                .create().show();
        	}
        }
    }
}