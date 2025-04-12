# 1. Set up your variables
$resourceGroup = "DistributedWordCompletion_wus_rg"
$locationPrimary = "westus"
$acrName = "distributedwordcompletionacr"
$containerAppEnv = "distributedwordcompletion-acr-env-wus"

# Define the container app names and ports
$containerApps = @(
    @{ name = "prefixtree-service-a"; port = 8080; },
    @{ name = "prefixtree-service-b"; port = 8080; },
    @{ name = "word-ingestor"; port = 8080; }
)

# 2. Build and Push Docker Images to Azure Container Registry
# Loop through each container app and build/push its image
foreach ($app in $containerApps) {
    $imageTag = "$acrName.azurecr.io/$($app.name)"
    Write-Host "Building image for $($app.name)..."
    docker build -t $imageTag -f ./$($app.name)/Dockerfile .
    Write-Host "Pushing image for $($app.name)..."
    docker push $imageTag
}

# 3. Update Container Apps with the latest images
foreach ($app in $containerApps) {
    $imageTag = "$acrName.azurecr.io/$($app.name)"
    Write-Host "Updating container app $($app.name) with the image $imageTag..."
    az containerapp update --name $app.name --resource-group $resourceGroup --image $imageTag
}

Write-Host "Deployment Complete!"
