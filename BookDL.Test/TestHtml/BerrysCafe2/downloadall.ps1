$base = "https://www.berrys-cafe.jp/book/n1615134"
$dst="www.berrys-cafe.jp.book.n1615134"
$start = 1

$url = $base
$out = "$dst.html"
$res = Invoke-WebRequest -Uri $url -ErrorAction Stop
$res.Content | Out-File -FilePath $out -Encoding UTF8
Write-Host "Saved: $out"

$i = $start
while ($true) {
    $url = "$base/$i"
    $out = "$dst.$i.html"

    Write-Host "GET $url"

    # Invoke-WebRequest を使う（curl エイリアスより確実）
    try {
        $res = Invoke-WebRequest -Uri $url -ErrorAction Stop
    }
    catch {
        # 404 なら終了
        if ($_.Exception.Response.StatusCode.value__ -eq 404) {
            Write-Host "404 Not Found → 終了"
            break
        }

        # その他のエラーは通知して続行
        Write-Host "Error: $($_.Exception.Message)"
        break
    }

    # 正常なら保存
    $res.Content | Out-File -FilePath $out -Encoding UTF8
    Write-Host "Saved: $out"

    $i++
}
