extends Node2D;

func _physics_process(delta):
	$Label.text = "x: " + $player.position.x + " y: " + $player.position.y;
	$Label2.text = "rotation: " + $player/tankTurret.rotation;
