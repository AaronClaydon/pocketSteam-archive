<?php
class configModel extends CI_Model {
	function getConfig() {
		$this->load->helper('file');

		$configLocation = $this->config->item('globalConfigLocation');
		$configSplit = $this->config->item('globalConfigSplit');
		$rawConfigString = read_file($configLocation);

		$lineArray = explode($configSplit, $rawConfigString);
		$config = array();

		foreach($lineArray as $line) {
			$data = explode(": ", $line);
			$config[$data[0]] = $data[1];
		}

		return $config;
	}
}
?>