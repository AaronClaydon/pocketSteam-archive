<?php
class tcpModel extends CI_Model {
	function sendServer($port, $input) {
		$fp = @fsockopen("localhost", $port, $errno, $errstr, 5);

		if($errno != 0) {
			return 'pocketSteamOffline';
		}

		fwrite($fp, $input);

		$output = "";

		while (!feof($fp)) {
	        $output .= fgets($fp, 128);
	    }

	    fclose($fp);

	    return $output;
	}
}
?>