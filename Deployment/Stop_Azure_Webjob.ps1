param (
    [Parameter(Mandatory = $true)] [string] $resourcegroup,  
    [Parameter(Mandatory = $true)] [string] $appname

)

Write-Output "Stopping event aggregator webjob"
Stop-AzWebAppContinuousWebJob -ResourceGroupName $(resourcegroup) -AppName $(appname) -Name EventAggregationWebJob
az webapp restart --name $appname --resource-group $resourcegroup
