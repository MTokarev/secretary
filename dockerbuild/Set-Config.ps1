# The app uses config file to construct links (for example to locate backend from the frontend).
# To make it easy to change the binding address during docker build this function was created.

param(
  [Parameter(Mandatory)]
  [string]$siteFqdn,

  [Parameter(Mandatory)]
  [string]$configFilePath,

  [Parameter]
  [string]$appSettingsFilePath
)

function Set-Config() {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$siteFqdn,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$configFilePath,

    [Parameter]
    [ValidateNotNullOrEmpty()]
    [string]$appSettingsFilePath,


    [int]$jsonConvertionDepth = 10
  )

  if (!Test-Path $configFilePath) {
    throw "Unable to find file '$configFilePath'."
  }

  # Check if appSetting provided, then update the CORS
  if (![string]::IsNullOrEmpty($appSettingsFilePath)) {
    if (!Test-Path $appSettingsFilePath) {
      throw "Unable to find file '$appSettingsFilePath'."
    }
    $appSettingsRaw = Get-Content $appSettingsFilePath
    $appSettingsObject = $appSettingsRaw | ConvertFrom-Json -Depth $jsonConvertionDepth
    $appSettingsObject.CorsUris += $siteFqdn
    Write-Output "CORS policy has been updated with '$siteFqdn'."
    $appSettingsToWrite = $appSettingsObject | ConvertTo-Json -Depth $jsonConvertionDepth
  
    Set-Content -Path $appSettingsFilePath -Value $appSettingsToWrite -Force -Confirm:$false
  }

  $contentRaw = Get-Content $configFilePath
  $contentObject = $contentRaw | ConvertFrom-Json -Depth $jsonConvertionDepth
  $contentObject.siteConfig.siteAddress = $siteFqdn
  $contentObject.urls.base = $siteFqdn
  Write-Output "siteAddress and base url have been set to '$siteFqdn'."
  $contenToWriteRaw = $contentObject | ConvertTo-Json -Depth $jsonConvertionDepth

  Set-Content -Path $configFilePath -Value $contenToWriteRaw -Force -Confirm:$false


}

Set-Config -siteFqdn $siteFqdn -configFilePath $configFilePath