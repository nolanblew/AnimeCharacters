param(
    [switch] $ResolveAniListIds,
    [int] $RequestDelayMilliseconds = 700,
    [switch] $RefreshImages
)

$ErrorActionPreference = "Stop"

# Register each extension updater here so local and CI refreshes stay aligned.
& "$PSScriptRoot\Update-GenshinImpactData.ps1" @PSBoundParameters
