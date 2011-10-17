<?php if ( ! defined('BASEPATH')) exit('No direct script access allowed');

class Admin extends CI_Controller {

	function __construct() {
		define("ADMIN_PASSCODE", '8167');
		parent::__construct();
	}

	private function authed() {
    	if($this->session->userdata('ps_admin_authcode') == ADMIN_PASSCODE) {
    		return true;
	    } else {
    		$this->load->view('Admin/login');
    		return false;
    	}
	}

	public function index() {
		$authed = $this->authed();

		if($authed) {
			echo 'ADMIN';
		}
	}

	public function login() {
		if($_POST['password'] == ADMIN_PASSCODE) {
			$this->session->set_userdata('ps_admin_authcode', $_POST['password']);
			echo 'Done';
		} else {
			echo 'Invalid';
		}
	}

	public function logout() {
		$this->session->set_userdata('ps_admin_authcode', '');
		echo 'Logged out';
	}
}
?>