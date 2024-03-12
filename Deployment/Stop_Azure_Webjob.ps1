param (
    [Parameter(Mandatory = $true)] [string] $resourcegroup,  
    [Parameter(Mandatory = $true)] [string] $appname

)

Write-Output "Stopping event aggregator webjob"
az webapp webjob continuous stop --name $appname --resource-group $resourcegroup --webjob-name EventAggregationWebJob
az webapp restart --name $appname --resource-group $resourcegroup
