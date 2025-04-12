# Set variables
$resourceGroup = "DistributedWordCompletion_wus_rg"
$location = "westus"
$acrName = "distributedwordcompletionacr"
$containerAppEnv = "distributedwordcompletion-acr-env-wus"
$workspaceName = "workspace-istributedordompletionwusrgXtQ"
$containerApps = @(
#    @{ name = "prefixtree-service-a"; dockerfile = "./PrefixTreeServiceA/Dockerfile"; port = 8080 },
#    @{ name = "prefixtree-service-b"; dockerfile = "./PrefixTreeServiceB/Dockerfile"; port = 8080 },
    @{ name = "word-ingestor"; dockerfile = "./IngestorService/Dockerfile"; port = 8080 }
)

# Get subscription ID
$subscriptionId = az account show --query id -o tsv

# Create the resource group
#az group create --name $resourceGroup --location $location

# Create ACR
#az acr create --resource-group $resourceGroup --name $acrName --sku Basic --admin-enabled false

# Log in to ACR
az acr login --name $acrName



# Get Log Analytics workspace info
$workspaceId = az monitor log-analytics workspace show --resource-group $resourceGroup --workspace-name $workspaceName --query customerId -o tsv
$workspaceKey = az monitor log-analytics workspace get-shared-keys --resource-group $resourceGroup --workspace-name $workspaceName --query primarySharedKey -o tsv

# Create Container Apps Environment (no identity here)
#az containerapp env create --name $containerAppEnv --resource-group $resourceGroup --location $location --logs-workspace-id $workspaceId --logs-workspace-key $workspaceKey


# Build and push Docker images
foreach ($app in $containerApps) {
    $timestamp = Get-Date -Format 'yyyyMMddHHmmss'
    $imageTag = "$acrName.azurecr.io/$($app.name):$timestamp"

    # Build the Docker image; use --pull or --no-cache if needed to force updates
    docker build --pull -t $imageTag -f $app.dockerfile .
    docker push $imageTag

    # Check if the container app exists, then update or create accordingly
    $existing = az containerapp show --name $($app.name) --resource-group $resourceGroup 2>$null
    if ($?) {
        az containerapp update --name $($app.name) --resource-group $resourceGroup --image $imageTag
    }
    else {
        az containerapp create --name $($app.name) --resource-group $resourceGroup `
            --environment $containerAppEnv --image $imageTag --target-port $($app.port) `
            --ingress external --registry-server "$acrName.azurecr.io" --system-assigned
    }

    # Get principal ID and assign AcrPull role (if needed)
    $principalId = az containerapp show --name $($app.name) --resource-group $resourceGroup --query "identity.principalId" -o tsv
    az role assignment create --assignee $principalId --role "AcrPull" --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.ContainerRegistry/registries/$acrName"
}