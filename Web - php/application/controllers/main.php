<?php if ( ! defined('BASEPATH')) exit('No direct script access allowed');

class Main extends CI_Controller {

	function BasicView($name) 
	{
		$this->load->library('user_agent');
		//$isMobile = $this->agent->is_mobile();
		$isMobile = true; //DEBUG!

		if($isMobile) {
			$this->load->view('Mobile/header');
			$this->load->view('Mobile/' . $name);
		}
		else {
			$this->load->view('Desktop/header');
			$this->load->view('Desktop/' . $name);
		}
	}

	function ParseView($name, $template)
	{
		$this->load->library('user_agent');
		$this->load->library('parser');
		//$isMobile = $this->agent->is_mobile();
		$isMobile = true; //DEBUG!

		if($isMobile) {
			$this->load->view('Mobile/header');
			$this->parser->parse('Mobile/' . $name, $template);
		}
		else {
			$this->load->view('Desktop/header');
			$this->parser->parse('Desktop/' . $name, $template);
		}
	}

	public function index()
	{
		$this->load->model('configModel');
		$globalConfig = $this->configModel->getConfig();

		if($globalConfig['Login-Enabled'] == "True") {
			$this->BasicView('login');
		} else {
			$template = array(
					'title' => 'Offline',
					'message' => $globalConfig['Offline-Message']
				);
			$this->ParseView('base', $template);
		}
	}

	public function logout($reply = "no")
	{
		$this->load->model('databaseModel');
		$sessionData = $this->databaseModel->getSession($this->session->userdata('ps_sessionToken'), $this->session->userdata('ps_passKey'));

		if(isset($sessionData->SMCSPort)) {
			if($sessionData->PassKey != $this->session->userdata('ps_passKey')) {
				$this->session->set_userdata('ps_sessionToken', '');
				$this->session->set_userdata('ps_passKey', '');
				
				echo 'Invalid';

				die();
			}

			if($reply == "yes") {
				$this->load->model('tcpModel');
				$this->session->set_userdata('ps_sessionToken', '');
				$this->session->set_userdata('ps_passKey', '');

				$message = '{"Type":1,"CommandValue":""}';
				echo $this->tcpModel->sendServer($sessionData->SMCSPort, $message);

				$template = array(
				            'title' => 'Logged out',
				            'message' => 'You have successfuly been logged out of the Steam network.'
				            );
				$this->ParseView('base', $template);
			}else {
				$this->BasicView('logout');
			}
		}
		else {
			$this->session->set_userdata('ps_sessionToken', '');
			$this->session->set_userdata('ps_passKey', '');
			
			echo 'Expired';
		}
	}

	public function faq() {
		$this->BasicView('faq');
	}

	public function login() {
		$this->load->model('configModel');
		$globalConfig = $this->configModel->getConfig();

		if($globalConfig['Login-Enabled'] != "True") {
			die('Disabled');
		}

		$userName = @$_POST['userName'] or die('MissingField');
		$passWord = @$_POST['passWord'] or die('MissingField');

		$steamGuardKey = "";
		if(isset($_POST['steamGuardKey'])) {
			$steamGuardKey = $_POST['steamGuardKey'];
		}
		//$userName = "azzytest"; //Lets use these values for the moment
		//$passWord = "testing123";

		$sessionToken = uniqid();
		$passKey = uniqid();

		//Contact CSMCS to start SMCS and to get port
		$input = $sessionToken . "\n";
		$input .= $userName . "\n";
		$input .= $passWord . "\n";
		$input .= $steamGuardKey . "";

		//Get the CSMCS port from global
		$csmcsPort = $globalConfig['CSMCS-Port'];

		$this->load->model('tcpModel');
		$CSMCSoutput = $this->tcpModel->sendServer($csmcsPort, $input);
		$CSMCSoutputArray = explode("\n", $CSMCSoutput);

		if($CSMCSoutputArray[0] == "Port") {
			$smcsPort = $CSMCSoutputArray[1];
		}
		else {
			echo 'pocketSteamOffline';
			return; //Stop it from continuing with the script if inproper reply
		}
 
 		//Add session to database
		$this->load->model('databaseModel');
		$this->databaseModel->addSession(array(
			$sessionToken,
			$_SERVER['REMOTE_ADDR'],
			time(),
			time(),
			$passKey,
			1,
			$smcsPort));

		//Now lets verify the login with SMCS
		$SMCSoutput = $this->tcpModel->sendServer($smcsPort, "RepeatSteamReply");

		if($SMCSoutput == "Success") {
			echo 'Success:' . $sessionToken . ':' . $passKey;
			$this->session->set_userdata('ps_sessionToken', $sessionToken);
			$this->session->set_userdata('ps_passKey', $passKey);
		}
		else{
			echo $SMCSoutput;
		}
	}

	public function Display() {
		$this->load->model('databaseModel');
		$sessionData = $this->databaseModel->getSession($this->session->userdata('ps_sessionToken'), $this->session->userdata('ps_passKey'));

		if(isset($sessionData->SMCSPort)) {
			if($sessionData->PassKey != $this->session->userdata('ps_passKey')) {
				$this->session->set_userdata('ps_sessionToken', '');
				$this->session->set_userdata('ps_passKey', '');
				
				$template = array(
				            'title' => 'Invalid session access',
				            'message' => 'The session key and pass key do not match, please try and log back in.'
				            );
				$this->ParseView('base', $template);

				die();
			}

			//echo "SessionToken: " . $this->session->userdata('ps_sessionToken') . "<br />";
			//echo "PassKey: " . $this->session->userdata('ps_passKey') . "<br />";

			$this->parseView('display', array('username' => 'DERP A USERNAME'));

			// $this->load->model('tcpModel');
			// $value = '{\"To\":\"STEAM_0:1:20189445\",\"Message\":\"Hi\"}';
			// $message = '{"Type":2,"CommandValue":"' . $value . '"}';
			// echo $this->tcpModel->sendServer($sessionData->SMCSPort, $message);
		}
		else {
			$this->session->set_userdata('ps_sessionToken', '');
			$this->session->set_userdata('ps_passKey', '');
			
			$template = array(
			            'title' => 'Session Expired',
			            'message' => 'The session you are trying to access has expired, please log back into pocketSteam.'
			            );
			$this->ParseView('base', $template);
		}
	}

	//AJAX STUFF
	public function heartbeat()
	{
		$sessionToken = $this->session->userdata('ps_sessionToken');
		$this->load->model('databaseModel');
		$sessionData = $this->databaseModel->getSession($sessionToken, $this->session->userdata('ps_passKey'));

		if(isset($sessionData->SMCSPort)) {
			if($sessionData->PassKey != $this->session->userdata('ps_passKey')) {
				$this->session->set_userdata('ps_sessionToken', '');
				$this->session->set_userdata('ps_passKey', '');
				
				echo 'Expired';

				die();
			}
			
			//Update the last updated part of the table
			$data = array(
               'LastHeartbeat' => time(),
            );

			$this->db->where('SessionToken', $sessionToken);
			$this->db->update('sessions', $data); 

			//Get all the messages from the db
			$this->db->select('id,Type,MessageValue')->from('messages')->where('SessionToken', $sessionToken);

			$query = $this->db->get();
			$data = array('S' => $sessionData->Status, 'M' => array());
			$ids = array();

			//Put data from DB into json array!
			foreach ($query->result() as $row)
			{
			    $data['M'][] = array('T' => $row->Type, 'M' => $row->MessageValue);
			    $ids[] = $row->id;
			}
			$this->db->where_in('id', $ids)->delete('messages');

			$json = json_encode($data);

			echo $json;
		}
		else {
			$this->session->set_userdata('ps_sessionToken', '');
			$this->session->set_userdata('ps_passKey', '');
			
			echo 'Expired';
		}
	}
}