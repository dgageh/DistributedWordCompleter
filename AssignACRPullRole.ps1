$subscriptionId = az account show --query id -o tsv


# Get the managed identity of the container app environment
$principalId = az containerapp env show `
  --name $containerAppEnvPrimary `
  --resource-group $resourceGroup `
  --query "identity.principalId" -o tsv

# Assign 'AcrPull' role on the ACR
az role assignment create `
  --assignee $principalId `
  --role "AcrPull" `
  --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.ContainerRegistry/registries/$acrName"
