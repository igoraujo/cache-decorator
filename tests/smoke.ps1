$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http

$projectRoot = Split-Path -Parent $PSScriptRoot
$app = Join-Path $projectRoot 'bin\Release\net10.0\cache-decorator.dll'
if (-not (Test-Path $app)) {
    throw 'Build the solution in Release before running this script.'
}

function Assert-True([bool] $condition, [string] $message) {
    if (-not $condition) { throw $message }
}

function Test-Environment([string] $environment) {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $port = $listener.LocalEndpoint.Port
    $listener.Stop()
    $baseUrl = "http://127.0.0.1:$port"

    $start = [System.Diagnostics.ProcessStartInfo]::new()
    $start.FileName = 'dotnet'
    $start.Arguments = "`"$app`" --urls $baseUrl"
    $start.WorkingDirectory = $projectRoot
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.EnvironmentVariables['ASPNETCORE_ENVIRONMENT'] = $environment
    $client = [System.Net.Http.HttpClient]::new()
    $client.Timeout = [TimeSpan]::FromSeconds(3)
    $process = [System.Diagnostics.Process]::Start($start)

    try {
        $ready = $false
        $lastError = $null
        for ($i = 0; $i -lt 60; $i++) {
            if ($process.HasExited) { throw "Application exited with code $($process.ExitCode)." }
            try {
                $response = $client.GetAsync("$baseUrl/WeatherForecast").GetAwaiter().GetResult()
                $ready = $true
                break
            }
            catch {
                $lastError = $_
                Start-Sleep -Milliseconds 500
            }
        }
        if (-not $ready) { throw "Application did not start: $lastError" }

        Assert-True ($response.StatusCode -eq 200) "$environment`: WeatherForecast failed."
        $forecasts = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
        Assert-True ($forecasts.Count -eq 5) "$environment`: expected five forecasts."
        foreach ($forecast in $forecasts) {
            Assert-True ($null -ne $forecast.date -and $null -ne $forecast.temperatureC -and
                $null -ne $forecast.temperatureF -and $null -ne $forecast.summary) "$environment`: unexpected forecast shape."
        }
        $response.Dispose()

        foreach ($entry in @(@(0, '00000000000'), @(1, '11111111111'), @(2, '22222222222'))) {
            foreach ($attempt in 1..2) {
                $response = $client.GetAsync("$baseUrl/Cliente/buscarcpf/$($entry[0])").GetAwaiter().GetResult()
                $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                Assert-True ($response.StatusCode -eq 200 -and $body -eq $entry[1]) "$environment`: CPF $($entry[0]) failed."
                $response.Dispose()
            }
        }

        $response = $client.GetAsync("$baseUrl/Cliente/buscarcpf/99").GetAwaiter().GetResult()
        Assert-True ($response.StatusCode -eq 204) "$environment`: unknown CPF must remain empty."
        $response.Dispose()
        $response = $client.GetAsync("$baseUrl/Cliente/buscarcpf/not-a-number").GetAwaiter().GetResult()
        Assert-True ($response.StatusCode -eq 400) "$environment`: invalid ID must return 400."
        $response.Dispose()

        $response = $client.GetAsync("$baseUrl/swagger/v1/swagger.json").GetAwaiter().GetResult()
        if ($environment -eq 'Development') {
            Assert-True ($response.StatusCode -eq 200) 'Development: Swagger document unavailable.'
            $document = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
            Assert-True ($document.openapi -like '3.0.*') 'Development: expected an OpenAPI 3.0 document.'
            Assert-True ($null -ne $document.paths.'/WeatherForecast' -and
                $null -ne $document.paths.'/Cliente/buscarcpf/{id}') 'Development: Swagger routes missing.'
            $response.Dispose()
            $response = $client.GetAsync("$baseUrl/swagger/index.html").GetAwaiter().GetResult()
            Assert-True ($response.StatusCode -eq 200) 'Development: Swagger UI unavailable.'
        }
        else {
            Assert-True ($response.StatusCode -eq 404) 'Production: Swagger must remain disabled.'
        }
        $response.Dispose()

        if ($environment -eq 'Production') {
            $samples = @()
            for ($i = 0; $i -lt 100; $i++) {
                $timer = [System.Diagnostics.Stopwatch]::StartNew()
                $response = $client.GetAsync("$baseUrl/Cliente/buscarcpf/1").GetAwaiter().GetResult()
                $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                $timer.Stop()
                Assert-True ($response.StatusCode -eq 200 -and $body -eq '11111111111') 'Latency sample failed.'
                $samples += $timer.Elapsed.TotalMilliseconds
                $response.Dispose()
            }
            $sorted = @($samples | Sort-Object)
            $average = ($samples | Measure-Object -Average).Average
            Write-Host ('Production: 100 sequential cached requests, mean {0:N2} ms, p95 {1:N2} ms.' -f $average, $sorted[94])
        }
        Write-Host "$environment`: smoke tests passed."
    }
    finally {
        $client.Dispose()
        if (-not $process.HasExited) {
            $process.Kill()
            $process.WaitForExit()
        }
        $process.Dispose()
    }
}

Test-Environment 'Development'
Test-Environment 'Production'
