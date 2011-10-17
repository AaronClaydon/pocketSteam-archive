<?php
class tcpModel extends CI_Model {
	function sendServer($port, $input) {
		$fp = @fsockopen("localhost", $port, $errno, $errstr, 5) or die('pocketSteamOffline');
		
		fwrite($fp, $input);

		$output = "";

		while (!feof($fp)) {
	        $output .= fgets($fp, 128);
	    }
	    /*
	    $outputArray = explode("\n", $output);
	    echo "CSMCS SAID: " . $output . "<br />";

	    print_r($outputArray);

	    if($outputArray[0] == "Port") 
	    {
	    	$fpSmcs = fsockopen("localhost", $outputArray[1], $errno, $errstr, 5);
	    	fwrite($fpSmcs, "HI!");

	    	$smcsReply = "";
	    	while (!feof($fpSmcs)) {
		        $smcsReply .= fgets($fpSmcs, 128);
		    }

	    	echo "SMCS SAID: " . $smcsReply;
	    } 
	    else 
	    {
	    	echo "ERROR";
	    	return;
	    }
	    */
	    fclose($fp);

	    return $output;
	}
}
?>