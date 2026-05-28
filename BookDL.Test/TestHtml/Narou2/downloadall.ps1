$usrUrl = "https://mypage.syosetu.com/2685431/"
$usrOut = "mypage.syosetu.com.2685431.html"
$bookUrlBase = "https://ncode.syosetu.com/n3131ks/"
$bookOutBase = "ncode.syosetu.com.n3131ks"
$start = 1


function Save-WebPage {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Url,
        [Parameter(Mandatory=$true)]
        [string]$OutputPath
    )
    $response = Invoke-WebRequest -Uri $Url -ErrorAction Stop
    $response.Content | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Host "Saved: $OutputPath (StatusCode: $($response.StatusCode))"
    return [PSCustomObject]@{
        Url = $Url
        Name = $OutputPath
    }
}


$data = [System.Collections.Generic.List[object]]::new()
try {
    $r = Save-WebPage -Url $usrUrl -OutputPath $usrOut
    $data.Add($r)

    $r = Save-WebPage -Url $bookUrlBase -OutputPath "$bookOutBase.html"
    $data.Add($r)

    $i = $start
    while ($true) {
        $url = "$bookUrlBase$i/"
        $out = "$bookOutBase.$i.html"
        $r = Save-WebPage -Url $url -OutputPath $out
        $data.Add($r)
        $i++
    }
}
catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 404) {
        Write-Host "404 Not Found → 終了"
    }
    else{
        Write-Host "Error: $($_.Exception.Message)"
    }
}

$data | ConvertTo-Json -Depth 5 | Out-File -FilePath manifest.json -Encoding utf8NoBOM
