param(
    [string] $OutputPath = "$PSScriptRoot\..\AnimeCharacters\wwwroot\data\extensions\genshin-impact-characters.json",
    [switch] $ResolveAniListIds,
    [int] $RequestDelayMilliseconds = 700
)

$ErrorActionPreference = "Stop"
$script:LastApiRequestAt = $null

function Invoke-JsonApiRequest {
    param(
        [string] $Uri,
        [string] $Method = "Get",
        [string] $Body = $null,
        [string] $ContentType = "application/json",
        [string] $Activity = "Updating Genshin Impact data",
        [string] $Status = "Requesting API data"
    )

    if ($script:LastApiRequestAt -ne $null) {
        $elapsedMilliseconds = ((Get-Date) - $script:LastApiRequestAt).TotalMilliseconds
        $remainingDelay = $RequestDelayMilliseconds - $elapsedMilliseconds

        if ($remainingDelay -gt 0) {
            Start-Sleep -Milliseconds ([int] [Math]::Ceiling($remainingDelay))
        }
    }

    Write-Progress -Activity $Activity -Status $Status

    $headers = @{
        "User-Agent" = "AnimeCharactersDataUpdater/1.0 (https://www.animecharacters.app/)"
    }

    try {
        if ([string]::IsNullOrEmpty($Body)) {
            return Invoke-RestMethod -Uri $Uri -Method $Method -Headers $headers
        }

        return Invoke-RestMethod -Uri $Uri -Method $Method -ContentType $ContentType -Headers $headers -Body $Body
    }
    finally {
        $script:LastApiRequestAt = Get-Date
    }
}

function Convert-HtmlFragmentToText {
    param([string] $Html)

    if ([string]::IsNullOrWhiteSpace($Html)) {
        return $null
    }

    $withoutReferences = [regex]::Replace($Html, "<sup[\s\S]*?</sup>", "")
    $withSpaces = [regex]::Replace($withoutReferences, "</?(br|p)[^>]*>", " ")
    $withoutTags = [regex]::Replace($withSpaces, "<[^>]+>", "")
    $decoded = [System.Net.WebUtility]::HtmlDecode($withoutTags)

    return [regex]::Replace($decoded, "\s+", " ").Trim()
}

function Convert-WikiImageUrl {
    param([string] $Url)

    if ([string]::IsNullOrWhiteSpace($Url)) {
        return $null
    }

    $decoded = [System.Net.WebUtility]::HtmlDecode($Url)
    return ($decoded -split "\?")[0]
}

function Get-NormalizedName {
    param([string] $Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $null
    }

    $formD = $Name.Normalize([System.Text.NormalizationForm]::FormD)
    $builder = [System.Text.StringBuilder]::new()

    foreach ($char in $formD.ToCharArray()) {
        if ([System.Globalization.CharUnicodeInfo]::GetUnicodeCategory($char) -ne [System.Globalization.UnicodeCategory]::NonSpacingMark) {
            [void] $builder.Append($char)
        }
    }

    $plain = $builder.ToString().Normalize([System.Text.NormalizationForm]::FormC).ToLowerInvariant()
    return ([regex]::Replace($plain, "[^\p{L}\p{N}]+", " ")).Trim()
}

function Test-NameMatch {
    param(
        [string] $Expected,
        [string] $Candidate
    )

    $expectedNormalized = Get-NormalizedName $Expected
    $candidateNormalized = Get-NormalizedName $Candidate

    if ([string]::IsNullOrWhiteSpace($expectedNormalized) -or [string]::IsNullOrWhiteSpace($candidateNormalized)) {
        return $false
    }

    if ($expectedNormalized -eq $candidateNormalized) {
        return $true
    }

    $parts = $expectedNormalized.Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)
    if ($parts.Length -eq 2) {
        return "$($parts[1]) $($parts[0])" -eq $candidateNormalized
    }

    return $false
}

function Convert-JapaneseVoiceActorLink {
    param([string] $LinkHtml)

    $text = Convert-HtmlFragmentToText $LinkHtml

    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    $name = $text
    $nativeName = $null

    $match = [regex]::Match($text, "^(?<name>.*?)\s*\((?<native>[^()]*)\)")
    if ($match.Success) {
        $name = $match.Groups["name"].Value.Trim()
        $nativeName = $match.Groups["native"].Value.Trim()
    }

    [ordered]@{
        Name = $name
        NativeName = $nativeName
        AniListStaffId = $null
    }
}

function Get-CharacterIconUrls {
    $characterListUri = "https://genshin-impact.fandom.com/api.php?action=parse&page=Character/List&prop=text&format=json&formatversion=2"
    $characterListResponse = Invoke-JsonApiRequest -Uri $characterListUri -Status "Fetching Character/List icon data from Fandom"
    $html = $characterListResponse.parse.text
    $iconUrls = @{}

    $rows = [regex]::Matches($html, "<tr\b[\s\S]*?</tr>")
    $rowCount = [Math]::Max($rows.Count, 1)
    $rowIndex = 0

    foreach ($row in $rows) {
        $rowIndex++
        Write-Progress -Activity "Updating Genshin Impact data" -Status "Parsing Fandom character icons" -PercentComplete (($rowIndex / $rowCount) * 100)

        $nameMatch = [regex]::Match($row.Value, "<td\b[^>]*\bdata-name=`"(?<name>[^`"]+)`"")
        if (!$nameMatch.Success) {
            continue
        }

        $imageMatch = [regex]::Match($row.Value, "<img\b(?=[^>]*\bdata-image-key=`"[^`"]+_Icon\.png`")[^>]*\bdata-src=`"(?<src>[^`"]+)`"")
        if (!$imageMatch.Success) {
            $imageMatch = [regex]::Match($row.Value, "<img\b[^>]*\bdata-src=`"(?<src>[^`"]+)`"")
        }

        if (!$imageMatch.Success) {
            continue
        }

        $name = [System.Net.WebUtility]::HtmlDecode($nameMatch.Groups["name"].Value)
        if (!$iconUrls.ContainsKey($name)) {
            $iconUrls[$name] = Convert-WikiImageUrl $imageMatch.Groups["src"].Value
        }
    }

    return $iconUrls
}

function Resolve-AniListStaffId {
    param([hashtable] $VoiceActor)

    $queries = @($VoiceActor.NativeName, $VoiceActor.Name) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

    foreach ($queryValue in $queries) {
        $body = @{
            query = @"
query searchStaff(`$search: String) {
  Page(page: 1, perPage: 5) {
    staff(search: `$search, sort: SEARCH_MATCH) {
      id
      languageV2
      name {
        full
        native
      }
    }
  }
}
"@
            variables = @{
                search = $queryValue
            }
        } | ConvertTo-Json -Depth 8

        try {
            $result = Invoke-JsonApiRequest -Uri "https://graphql.anilist.co" -Method Post -Body $body -Status "Resolving AniList staff: $queryValue"
        }
        catch {
            continue
        }

        foreach ($staff in $result.data.Page.staff) {
            if ($staff.languageV2 -ne "JAPANESE") {
                continue
            }

            if ((Test-NameMatch $VoiceActor.Name $staff.name.full) -or (Test-NameMatch $VoiceActor.NativeName $staff.name.native)) {
                return [int] $staff.id
            }
        }
    }

    return $null
}

$iconUrlsByCharacterName = Get-CharacterIconUrls

$voiceActorUri = "https://genshin-impact.fandom.com/api.php?action=parse&page=Voice_Actor&prop=text&format=json&formatversion=2"
$voiceActorResponse = Invoke-JsonApiRequest -Uri $voiceActorUri -Status "Fetching Voice Actor table from Fandom"
$html = $voiceActorResponse.parse.text

$tableMatch = [regex]::Match($html, "<table[^>]*wikitable[\s\S]*?</table>")
if (!$tableMatch.Success) {
    throw "Could not find the playable character voice actor table."
}

$characters = New-Object System.Collections.Generic.List[object]
$rows = [regex]::Matches($tableMatch.Value, "<tr[\s\S]*?</tr>") | Select-Object -Skip 1
$rowCount = [Math]::Max($rows.Count, 1)
$rowIndex = 0

foreach ($row in $rows) {
    $rowIndex++
    Write-Progress -Activity "Updating Genshin Impact data" -Status "Parsing voice actor rows" -PercentComplete (($rowIndex / $rowCount) * 100)

    $cells = [regex]::Matches($row.Value, "<td[^>]*>[\s\S]*?</td>")
    if ($cells.Count -lt 4) {
        continue
    }

    $characterLink = [regex]::Match($cells[0].Value, "<a href=`"(?<href>/wiki/[^`"]+)`" title=`"(?<title>[^`"]+)`"[^>]*>[\s\S]*?</a>")
    if (!$characterLink.Success) {
        continue
    }

    $characterName = [System.Net.WebUtility]::HtmlDecode($characterLink.Groups["title"].Value)
    $japaneseCell = [regex]::Replace($cells[3].Value, "<sup[\s\S]*?</sup>", "")
    $voiceActorLinks = [regex]::Matches($japaneseCell, "<a [^>]*>[\s\S]*?</a>")
    $japaneseVoiceActors = New-Object System.Collections.Generic.List[object]

    foreach ($voiceActorLink in $voiceActorLinks) {
        $voiceActor = Convert-JapaneseVoiceActorLink $voiceActorLink.Value
        if ($null -ne $voiceActor -and -not [string]::IsNullOrWhiteSpace($voiceActor.Name)) {
            [void] $japaneseVoiceActors.Add($voiceActor)
        }
    }

    if ($japaneseVoiceActors.Count -eq 0) {
        continue
    }

    [void] $characters.Add([ordered]@{
        Name = $characterName
        ImageUrl = $iconUrlsByCharacterName[$characterName]
        WikiUrl = "https://genshin-impact.fandom.com$($characterLink.Groups["href"].Value)"
        JapaneseVoiceActors = $japaneseVoiceActors
    })
}

if ($ResolveAniListIds) {
    $voiceActorCache = @{}
    $voiceActorWorkItems = @($characters | ForEach-Object { $_.JapaneseVoiceActors } | ForEach-Object { $_ })
    $totalVoiceActors = [Math]::Max($voiceActorWorkItems.Count, 1)
    $voiceActorIndex = 0

    foreach ($character in $characters) {
        foreach ($voiceActor in $character.JapaneseVoiceActors) {
            $voiceActorIndex++
            $cacheKey = if ($voiceActor.NativeName) { $voiceActor.NativeName } else { $voiceActor.Name }
            Write-Progress -Activity "Resolving AniList staff IDs" -Status "$voiceActorIndex/$totalVoiceActors $($voiceActor.Name)" -PercentComplete (($voiceActorIndex / $totalVoiceActors) * 100)

            if (!$voiceActorCache.ContainsKey($cacheKey)) {
                $voiceActorCache[$cacheKey] = Resolve-AniListStaffId $voiceActor
            }

            $voiceActor.AniListStaffId = $voiceActorCache[$cacheKey]
        }
    }
}

$missingImageCount = @($characters | Where-Object { [string]::IsNullOrWhiteSpace($_.ImageUrl) }).Count
$outputDirectory = Split-Path -Parent $OutputPath
if (!(Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$characters |
    Sort-Object { $_.Name } |
    ConvertTo-Json -Depth 12 |
    Set-Content -Path $OutputPath -Encoding utf8

Write-Progress -Activity "Updating Genshin Impact data" -Completed
Write-Output "Wrote $($characters.Count) Genshin Impact characters to $OutputPath"
Write-Output "Missing character icon URLs: $missingImageCount"
