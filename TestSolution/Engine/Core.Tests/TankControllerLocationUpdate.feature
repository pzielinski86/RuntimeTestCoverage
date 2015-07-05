Feature: TankControllerLocationUpdate
	TankController is API for any game host. 
	When Tank.Move or Tank.Rotate are called by a bot then TankController.Update should handle it 
	and refresh a game scene.

Scenario: Tank rotation
	Given a tank bot
	When I call a rotate method
	And I call a TankController.Update 2 times with direction (0,0,1)
	Then the tank rotation should be updated

Scenario: Tank movement without obstacles
	Given a tank bot
	And path without obstacles
	When I call a move method
	And I call a TankController.Update 3 times with direction (0.5,0.6,0.7)
	Then the tank position should be updated

Scenario: Tank movement with obstacles
	Given a tank bot
	And path with obstacles
	When I call a move method
	And I call a TankController.Update 30 times with direction (0,0,1)
	Then the tank position should not be updated

Scenario: Tank rotation and movement
	Given a tank bot
	And path without obstacles
	When I call a rotate method
	And I call a move method
	And I call a TankController.Update until the tank is fully rotated
	Then the tank should be first rotated
	And position should not be changed at this time
	When I call a TankController.Update again
	Then the tank should be moved into the requested position

Scenario: Tank out of terrain
	Given a tank bot
	When I call a move method
	And tank is out of terrain
	And I call a TankController.Update 2 times with direction (0,0,4)
	Then the tank should not move
