<?php
class databaseModel extends CI_Model {

	function __construct()
    {
		$this->load->model('configModel');
		$globalConfig = $this->configModel->getConfig();

		$db['hostname'] = 'localhost';
		$db['username'] = 'pocketsteam';
		$db['password'] = $globalConfig['DB-Password'];
		$db['database'] = 'pocketsteam';
		$db['dbdriver'] = 'mysql';
        $this->load->database($db);

        parent::__construct();
    }

	function addSession($data) {
		$sql = "INSERT INTO sessions (SessionToken, IPAddress, DateCreated, LastHeartbeat, PassKey, Status, SMCSPort) 
		VALUES (?, ?, ?, ?, ?, ?, ?);";
		$query = $this->db->query($sql, $data);
	}

	function addLog($data) {
		$sql = "INSERT INTO logs (Username, Platform, TimeStarted) 
		VALUES (?, ?, ?);";
		$query = $this->db->query($sql, $data);
	}

	function getSession($sessionToken, $passKey) {
		$this->db->select('SessionToken,IPAddress,DateCreated,LastHeartbeat,PassKey,Status,SMCSPort');
		$this->db->from('sessions');
		$this->db->where('SessionToken', $sessionToken);
		$this->db->where('PassKey', $passKey);

		$query = $this->db->get()->row();

		return $query;
	}
}
?>