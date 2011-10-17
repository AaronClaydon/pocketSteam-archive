<?php if ( ! defined('BASEPATH')) exit('No direct script access allowed');

class Main extends CI_Controller {

	public function index()
	{
		$this->load->library('user_agent');
		//$isMobile = $this->agent->is_mobile();
		$isMobile = true; //DEBUG!

		if($isMobile) {
			$this->load->view('Mobile/login');
		}
		else {
			$this->load->view('Desktop/login');
		}
	}

	public function faq() {
		$this->load->view('Mobile/faq');
	}

	public function login() {
		$userName = @$_POST['userName'] or die('MissingField');
		$passWord = @$_POST['passWord'] or die('MissingField');

		$steamGuardKey = "";
		if(isset($_POST['steamGuardKey'])) {
			$steamGuardKey = $_POST['steamGuardKey'];
		}
		//$userName = "azzytest"; //Lets use these values for the moment
		//$passWord = "test123";

		$sessionToken = uniqid();
		$passKey = uniqid();

		//Contact CSMCS to start SMCS and to get port
		$input = $sessionToken . "\n";
		$input .= $userName . "\n";
		$input .= $passWord . "\n";
		$input .= $steamGuardKey . "";

		$this->load->model('tcpModel');
		$CSMCSoutput = $this->tcpModel->sendServer(8165, $input);
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
		echo "SessionToken" . $this->session->userdata('ps_sessionToken') . "<br />";
		echo "PassKey" . $this->session->userdata('ps_passKey') . "<br />";
			
		$this->load->model('tcpModel');
		$value = '{\"To\":\"STEAM_0:1:20189445\",\"Message\":\"Hi\"}';
		$message = '{"Type":2,"CommandValue":"' . $value . '"}';
		echo $this->tcpModel->sendServer(5001, $message);
	}
}