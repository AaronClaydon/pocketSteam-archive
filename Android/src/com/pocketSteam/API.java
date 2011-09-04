package com.pocketSteam;

import java.io.*;
import java.net.*;

import javax.net.ssl.HttpsURLConnection;

import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;

import android.graphics.drawable.Drawable;

public class API {
	static String APIServer = "https://pocketsteam.com";
	static int VERSION = 34;
	
	static String SessionToken = null;
	static String PassKey = null;
	static Boolean connected = false;
	static Boolean Started = false;
	
	 public static String Contact(String targetURL, String urlParameters) throws Exception
	  {
	    URL url;
	    HttpsURLConnection connection = null;  
	      //Create connection
	      url = new URL(APIServer + targetURL);
	      connection = (HttpsURLConnection)url.openConnection();
	      connection.setRequestMethod("POST");
	      connection.setRequestProperty("Content-Type", 
	           "application/x-www-form-urlencoded");
				
	      connection.setRequestProperty("Content-Length", "" + 
	               Integer.toString(urlParameters.getBytes().length));
	      connection.setRequestProperty("Content-Language", "en-US");  
	      connection.addRequestProperty("Cookie", "passKey=" + PassKey);
				
	      connection.setUseCaches (false);
	      connection.setDoInput(true);
	      connection.setDoOutput(true);

	      //Send request
	      DataOutputStream wr = new DataOutputStream (
	                  connection.getOutputStream ());
	      wr.writeBytes (urlParameters);
	      wr.flush ();
	      wr.close ();

	      //Get Response	
	      InputStream is = connection.getInputStream();
	      BufferedReader rd = new BufferedReader(new InputStreamReader(is));
	      String line;
	      StringBuffer response = new StringBuffer(); 
	      while((line = rd.readLine()) != null) {
	        response.append(line);
	      }
	      rd.close();
	      connection.disconnect();
	      return response.toString();
	  }
	 
	 public static Drawable DownloadImage(String targetURL) throws Exception
	 {
		 URI URL = new URI(targetURL);
		 DefaultHttpClient httpClient = new DefaultHttpClient();
	     HttpGet request = new HttpGet(URL);
	     HttpResponse response = httpClient.execute(request);
	     InputStream stream = response.getEntity().getContent();
	     
	     Drawable drawable = Drawable.createFromStream(stream, "src");
	     return drawable;
	 }
}
