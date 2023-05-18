
param (
    [Parameter(Mandatory = $true)] [string] $mockresourcegroup,  
    [Parameter(Mandatory = $true)] [string] $mockwebappname,
    [Parameter(Mandatory = $true)] [string] $ftrunning
)

Write-Output "Set Mock Service Configuration in appsetting..."
az webapp config appsettings set -g $mockresourcegroup -n $mockwebappname --settings IsFTRunning=$ftrunning
az webapp restart --name $mockwebappname --resource-group $mockresourcegroup