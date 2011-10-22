<?php if ( ! defined('BASEPATH')) exit('No direct script access allowed');

class Admin extends CI_Controller {

	var $adminPassword;

	function __construct() {
		parent::__construct();

		$this->load->model('configModel');
		$config = $this->configModel->getConfig();
		$this->adminPassword = $config['Admin-Password'];
	}

	function authed() {
    	if($this->session->userdata('ps_admin_authcode') == $this->adminPassword) {
    		return true;
	    } else {
    		$this->load->view('Admin/login');
    		return false;
    	}
	}

	public function index() {
		$authed = $this->authed();

		if($authed) {
			$this->load->view('Admin/index');
		}
	}

	public function login() {
		if($_POST['password'] == $this->adminPassword) {
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