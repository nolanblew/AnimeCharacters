param(
    [string] $OutputPath = "$PSScriptRoot\..\AnimeCharacters\wwwroot\data\extensions\genshin-impact-characters.json",
    [switch] $ResolveAniListIds
)

$ErrorActionPreference = "Stop"

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
            $result = Invoke-RestMethod -Uri "https://graphql.anilist.co" -Method Post -ContentType "application/json" -Headers @{ "User-Agent" = "AnimeCharactersDataUpdater/1.0" } -Body $body
        }
        catch {
            Start-Sleep -Milliseconds 1000
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

        Start-Sleep -Milliseconds 650
    }

    return $null
}

$voiceActorUri = "https://genshin-impact.fandom.com/api.php?action=parse&page=Voice_Actor&prop=text&format=json&formatversion=2"
$voiceActorResponse = Invoke-RestMethod -Uri $voiceActorUri -Headers @{ "User-Agent" = "AnimeCharactersDataUpdater/1.0" }
$html = $voiceActorResponse.parse.text

$tableMatch = [regex]::Match($html, "<table[^>]*wikitable[\s\S]*?</table>")
if (!$tableMatch.Success) {
    throw "Could not find the playable character voice actor table."
}

$characters = New-Object System.Collections.Generic.List[object]
$rows = [regex]::Matches($tableMatch.Value, "<tr[\s\S]*?</tr>") | Select-Object -Skip 1

foreach ($row in $rows) {
    $cells = [regex]::Matches($row.Value, "<td[^>]*>[\s\S]*?</td>")
    if ($cells.Count -lt 4) {
        continue
    }

    $characterLink = [regex]::Match($cells[0].Value, "<a href=`"(?<href>/wiki/[^`"]+)`" title=`"(?<title>[^`"]+)`"[^>]*>[\s\S]*?</a>")
    if (!$characterLink.Success) {
        continue
    }

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
        Name = [System.Net.WebUtility]::HtmlDecode($characterLink.Groups["title"].Value)
        ImageUrl = $null
        WikiUrl = "https://genshin-impact.fandom.com$($characterLink.Groups["href"].Value)"
        JapaneseVoiceActors = $japaneseVoiceActors
    })
}

$thumbnailByTitle = @{}
$characterTitles = $characters | ForEach-Object { $_.Name }

for ($i = 0; $i -lt $characterTitles.Count; $i += 40) {
    $chunk = $characterTitles[$i..([Math]::Min($i + 39, $characterTitles.Count - 1))]
    $titles = [System.Uri]::EscapeDataString(($chunk -join "|"))
    $imageUri = "https://genshin-impact.fandom.com/api.php?action=query&titles=$titles&prop=pageimages&piprop=thumbnail&pithumbsize=256&format=json&formatversion=2"
    $imageResponse = Invoke-RestMethod -Uri $imageUri -Headers @{ "User-Agent" = "AnimeCharactersDataUpdater/1.0" }

    foreach ($page in $imageResponse.query.pages) {
        if ($page.thumbnail.source) {
            $thumbnailByTitle[$page.title] = $page.thumbnail.source
        }
    }
}

foreach ($character in $characters) {
    if ($thumbnailByTitle.ContainsKey($character.Name)) {
        $character.ImageUrl = $thumbnailByTitle[$character.Name]
    }
}

if ($ResolveAniListIds) {
    $voiceActorCache = @{}

    foreach ($character in $characters) {
        foreach ($voiceActor in $character.JapaneseVoiceActors) {
            $cacheKey = if ($voiceActor.NativeName) { $voiceActor.NativeName } else { $voiceActor.Name }

            if (!$voiceActorCache.ContainsKey($cacheKey)) {
                $voiceActorCache[$cacheKey] = Resolve-AniListStaffId $voiceActor
            }

            $voiceActor.AniListStaffId = $voiceActorCache[$cacheKey]
        }
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if (!(Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$characters |
    Sort-Object { $_.Name } |
    ConvertTo-Json -Depth 12 |
    Set-Content -Path $OutputPath -Encoding utf8

Write-Output "Wrote $($characters.Count) Genshin Impact characters to $OutputPath"
