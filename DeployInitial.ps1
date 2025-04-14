# Set variables
$resourceGroup    = "DistributedWordCompletion_wus_rg"
$location         = "westus"
$acrName          = "distributedwordcompletionacr"
$containerAppEnv  = "distributedwordcompletion-acr-env-wus"
$workspaceName    = "workspace-istributedordompletionwusrgXtQ"
$containerApps    = @(
    @{ name = "prefixtree-service-a"; dockerfile = "./PrefixTreeServiceA/Dockerfile"; port = 8080 },
    @{ name = "prefixtree-service-b"; dockerfile = "./PrefixTreeServiceB/Dockerfile"; port = 8080 },
    @{ name = "word-ingestor";       dockerfile = "./IngestorService/Dockerfile"; port = 8080 }
)

# Get subscription ID
$subscriptionId = az account show --query id -o tsv

# (Resource group, ACR, Container Apps Environment creation are commented out to be less destructive)
# az group create --name $resourceGroup --location $location
# az acr create --resource-group $resourceGroup --name $acrName --sku Basic --admin-enabled false
# az containerapp env create --name $containerAppEnv --resource-group $resourceGroup --location $location --logs-workspace-id $workspaceId --logs-workspace-key $workspaceKey

# Log in to ACR
az acr login --name $acrName

# Get Log Analytics workspace info
$workspaceId  = az monitor log-analytics workspace show --resource-group $resourceGroup --workspace-name $workspaceName --query customerId -o tsv
$workspaceKey = az monitor log-analytics workspace get-shared-keys --resource-group $resourceGroup --workspace-name $workspaceName --query primarySharedKey -o tsv

# Loop through each container app entry
foreach ($app in $containerApps) {
    $timestamp = Get-Date -Format 'yyyyMMddHHmmss'
    $imageTag = "$acrName.azurecr.io/$($app.name):$timestamp"

    Write-Host "Building Docker image for $($app.name) with tag $imageTag"
    # Build the Docker image; use --pull or --no-cache as needed
    docker build --pull -t $imageTag -f $app.dockerfile .
    docker push $imageTag

    # Check if the container app already exists
    $existingJson = az containerapp show --name $($app.name) --resource-group $resourceGroup 2>$null
    if ($?) {
        # Convert the JSON output to an object
        $existing = $existingJson | ConvertFrom-Json
        # The deployed image is usually under properties.template.containers[0].image 
        $currentImage = $existing.properties.template.containers[0].image

        if ($currentImage -eq $imageTag) {
            Write-Host "No update required for $($app.name) since the image tag is unchanged."
        }
        else {
            Write-Host "Updating container app $($app.name) to new image $imageTag"
            az containerapp update --name $($app.name) --resource-group $resourceGroup --image $imageTag
        }
    }
    else {
        Write-Host "Creating container app $($app.name) with image $imageTag"
        az containerapp create --name $($app.name) --resource-group $resourceGroup `
            --environment $containerAppEnv --image $imageTag --target-port $($app.port) `
            --ingress external  `
            --registry-server "$acrName.azurecr.io" --system-assigned
    }

    # Retrieve the container app's principal ID and assign AcrPull role
    $principalId = az containerapp show --name $($app.name) --resource-group $resourceGroup --query "identity.principalId" -o tsv
    az role assignment create --assignee $principalId --role "AcrPull" --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.ContainerRegistry/registries/$acrName"
}