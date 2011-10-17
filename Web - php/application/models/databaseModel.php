<?php
class databaseModel extends CI_Model {
	function addSession($data) {
		$sql = "INSERT INTO sessions (SessionToken, IPAddress, DateCreated, LastHeartbeat, PassKey, Status, SMCSPort) 
		VALUES (?, ?, ?, ?, ?, ?, ?);";
		$query = $this->db->query($sql, $data);
	}
}
?>