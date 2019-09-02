@echo off
set SolutionDir=%1
set ConfigurationName=%2
set ProjectDir=%3
set Version=%4
if "%ConfigurationName%"=="Release" nuget.exe push -Source "TeleoptiNugets" -ApiKey AzureDevOps %ProjectDir%bin\%ConfigurationName%\Hangfire.Configuration.%Version%.nupkg
