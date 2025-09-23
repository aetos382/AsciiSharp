param(
  [ValidateSet('tck')]
  [string[]] $Containers = @('tck'))

Push-Location $PSScriptRoot

$env:DOCKER_BUILDKIT = 1
$env:BUILDX_GIT_LABELS = 'true'
$env:BUILDX_GIT_LABELS = 'full'

try {
  if ($Containers -contains 'tck') {
    docker buildx bake tck
  }
}
finally {
  Pop-Location
}
