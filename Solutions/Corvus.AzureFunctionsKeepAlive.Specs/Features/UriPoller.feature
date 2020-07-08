@perScenarioContainer
Feature: UriPoller

Scenario: Poll an unauthenticated endpoint
	When I poll the endpoint called 'Google' at Url 'https://www.google.co.uk'
	Then the operation should complete successfully

Scenario: Polling an endpoint that doesn't exist doesn't throw an exception
	When I poll the endpoint called 'Non-existent' at Url 'https://postman-echo.com/status/404'
	Then the operation should complete successfully
