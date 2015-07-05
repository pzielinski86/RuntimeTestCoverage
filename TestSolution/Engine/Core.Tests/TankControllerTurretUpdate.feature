Feature: TankControllerTurretUpdate
	TankController is API for any game host. 
	When any tank methodsare called by a bot then TankController.Update should handle it 
	and refresh a game scene.

Scenario: Tank turret rotation
	Given a tank bot
	When I call a rotate method on turret
	And I call a TankController.Update 2 times with direction (0,0,1)
	Then the tank turret rotation should be updated

Scenario: Cannon tilt
	Given a tank bot
	When I call a tilt method on cannon
	And I call a TankController.Update 2 times with direction (0,0,1)
	Then the tank cannon position should be updated.

Scenario: Bullet fire
	Given a tank bot
	When I call TankController.Update with the world information
	And I call a fire method on cannon	
	Then a new bullet should be added into collection
	And the bullet start position should be equal to the cannon fire point
	And the bullet direction should be equal to the cannon direction
