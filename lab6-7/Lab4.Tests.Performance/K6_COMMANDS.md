# K6 Тести - Команди Запуску

Цей документ містить усі команди для запуску k6 smoke тестів для Lab4 Student API.

## Передумови

### Встановлення K6

#### На Windows (PowerShell)

```powershell
# Завантажити k6
$ProgressPreference = 'SilentlyContinue'
Invoke-WebRequest -Uri https://github.com/grafana/k6/releases/download/v0.52.0/k6-v0.52.0-windows-amd64.zip -OutFile k6.zip

# Розпакувати архів
Expand-Archive -Path k6.zip -DestinationPath .

# Перевірити версію
.\k6-v0.52.0-windows-amd64\k6.exe --version
```

#### На macOS/Linux

```bash
# За допомогою Homebrew (macOS)
brew install k6

# За допомогою apt (Linux)
sudo apt-get install k6

# Або завантажити з GitHub
curl -L https://github.com/grafana/k6/releases/download/v0.52.0/k6-v0.52.0-linux-amd64.tar.gz | tar xz
```

## Запуск API Сервера

### Запуск з .NET 10 (локально)

```powershell
cd "C:\Users\nikit\OneDrive\Документы\GitHub\lab6-7\lab6-7"

# Запуск API на http://localhost:5000
.\dotnet10\dotnet.exe run --project Lab4.Data --urls "http://localhost:5000"
```

### Запуск у фоновому режимі (PowerShell)

```powershell
Start-Process -NoNewWindow -FilePath .\dotnet10\dotnet.exe -ArgumentList "run --project Lab4.Data --urls http://localhost:5000"
```

## Запуск K6 Тестів

### 1. Базовий smoke тест

```powershell
cd Lab4.Tests.Performance

# На Windows з встановленим k6
"C:\Users\nikit\OneDrive\Документы\GitHub\lab6-7\lab6-7\k6-v0.52.0-windows-amd64\k6.exe" run scripts/smoke-test.js

# Або якщо k6 у PATH
k6 run scripts/smoke-test.js
```

### 2. Load тест (поступове збільшення навантаження)

```powershell
# Базовий load тест
k6 run -e BASE_URL=http://localhost:5000 scripts/load-test.js

# Load тест з експортом результатів
k6 run -e BASE_URL=http://localhost:5000 --out json=load-results.json scripts/load-test.js
```

### 3. Stress тест (граничне навантаження)

```powershell
# Базовий stress тест
k6 run -e BASE_URL=http://localhost:5000 scripts/stress-test.js

# Stress тест з експортом результатів
k6 run -e BASE_URL=http://localhost:5000 --out json=stress-results.json scripts/stress-test.js
```

### 3. Тест з експортом результатів у JSON

```powershell
k6 run -e BASE_URL=http://localhost:5000 --out json=results.json scripts/smoke-test.js
```

### 4. Тест з детальним виводом

```powershell
k6 run -v -e BASE_URL=http://localhost:5000 scripts/smoke-test.js
```

### 5. Тест з користувацькими параметрами

```powershell
# Різні сценарії навантаження
k6 run --vus 10 --duration 1m -e BASE_URL=http://localhost:5000 scripts/smoke-test.js
```

## Команди з NPM

### Встановлення залежностей (якщо потрібно)

```bash
npm install
```

### Запуск через NPM скрипти

```bash
# Smoke тест
npm run smoke

# Smoke тест з користувацькою URL
npm run smoke:staged
```

## Повна послідовність запуску

### Повний цикл на Windows

```powershell
# 1. Перейти в директорію проекту
cd "C:\Users\nikit\OneDrive\Документы\GitHub\lab6-7\lab6-7"

# 2. Запустити API сервер (в окремому терміналі або фоново)
.\dotnet10\dotnet.exe run --project Lab4.Data --urls "http://localhost:5000"

# 3. Чекати 5-10 секунд для запуску API

# 4. В новому терміналі запустити тести
cd Lab4.Tests.Performance
& "..\..\k6-v0.52.0-windows-amd64\k6.exe" run -e BASE_URL=http://localhost:5000 scripts/smoke-test.js
```

### Скрипт-помічник (PowerShell)

Створити файл `run-k6-tests.ps1`:

```powershell
param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$K6Path = ".\k6-v0.52.0-windows-amd64\k6.exe",
    [switch]$StartApi = $false
)

if ($StartApi) {
    Write-Host "Запуск API сервера..."
    Start-Process -NoNewWindow -FilePath .\dotnet10\dotnet.exe -ArgumentList "run --project Lab4.Data --urls http://localhost:5000"
    Write-Host "API запущений. Очікування 10 секунд..."
    Start-Sleep -Seconds 10
}

Write-Host "Запуск k6 тестів з BASE_URL=$BaseUrl"
cd Lab4.Tests.Performance
& $K6Path run -e BASE_URL=$BaseUrl scripts/smoke-test.js
```

Запуск скрипту:

```powershell
# Без запуску API
.\run-k6-tests.ps1

# З запуском API
.\run-k6-tests.ps1 -StartApi

# З користувацькою URL
.\run-k6-tests.ps1 -BaseUrl "http://localhost:3000"
```

## Інтерпретація результатів

### Основні метрики

- **http_req_duration**: Час відповіді запиту (P50, P90, P95, P99)
- **http_req_failed**: Кількість невдалих запитів
- **http_reqs**: Загальна кількість запитів
- **vus**: Кількість віртуальних користувачів

### Приклад успішного результату

```
✓ all data is valid JSON
✓ response status code is 200 or 201
✓ no request failed
✓ response time < 500ms for GET
✓ response time < 1000ms for POST

checks.........................: 100.00% ✓ 50 ✗ 0
data_received..................: 2.5 kB ✓ 0 ✗ 0
data_sent.......................: 4.1 kB ✓ 0 ✗ 0
http_req_blocked...............: avg=0.25ms min=0ms med=0ms max=1ms p(90)=0ms p(95)=0ms p(99)=1ms
http_req_connecting............: avg=0.02ms min=0ms med=0ms max=0ms p(90)=0ms p(95)=0ms p(99)=0ms
http_req_duration..............: avg=2.54ms min=0.84ms med=2.01ms max=5.74ms p(90)=4.9ms p(95)=5.41ms p(99)=5.74ms
http_req_failed................: 0.00% ✓ 0 ✗ 60
http_req_receiving.............: avg=0.62ms min=0.17ms med=0.52ms max=1.24ms p(90)=1.05ms p(95)=1.16ms p(99)=1.24ms
http_req_sending...............: avg=0.19ms min=0.08ms med=0.16ms max=0.34ms p(90)=0.28ms p(95)=0.3ms p(99)=0.34ms
http_req_tls_handshaking.......: avg=0ms min=0ms med=0ms max=0ms p(90)=0ms p(95)=0ms p(99)=0ms
http_req_waiting...............: avg=1.73ms min=0.56ms med=1.28ms max=3.78ms p(90)=3.48ms p(95)=3.64ms p(99)=3.78ms
http_requests..................: 60 1.98/s
iteration_duration.............: avg=1.04s min=1.01s med=1.03s max=1.08s
iterations......................: 30 0.99/s
vus............................: 1 min=1 max=1
vus_max.........................: 1 min=1 max=1
```

## Розв'язання проблем

### K6 не знайден

```powershell
# Переконатися, що ви в правильній директорії та використовуєте повний шлях
"C:\Users\nikit\OneDrive\Документы\GitHub\lab6-7\lab6-7\k6-v0.52.0-windows-amd64\k6.exe" --version

# Або додати до PATH
$env:Path += ";C:\Users\nikit\OneDrive\Документы\GitHub\lab6-7\lab6-7\k6-v0.52.0-windows-amd64"
```

### API сервер недоступний

```powershell
# Перевірити, чи працює API
Test-NetConnection localhost -Port 5000

# Запустити API з іншою URL
.\dotnet10\dotnet.exe run --project Lab4.Data --urls "http://localhost:3000"
```

### Помилка при запуску тестів

```powershell
# Запустити з детальним виводом помилок
k6 run -v scripts/smoke-test.js

# Перевірити синтаксис файлу тесту
k6 inspect scripts/smoke-test.js
```

## Додаткові параметри K6

### Контроль навантаження

```powershell
# VUs (Virtual Users) та тривалість
k6 run --vus 5 --duration 30s scripts/smoke-test.js

# Поступове збільшення навантаження (ramp-up)
k6 run --stage 10s:10 --stage 30s:50 --stage 10s:0 scripts/smoke-test.js
```

### Експорт результатів

```powershell
# JSON формат
k6 run --out json=results.json scripts/smoke-test.js

# CSV формат
k6 run --out csv=results.csv scripts/smoke-test.js

# Мультиплексний вихід
k6 run --out json=results.json --out csv=results.csv scripts/smoke-test.js
```

### Налагодження та логування

```powershell
# Максимально детальний вивід
k6 run -vvv scripts/smoke-test.js

# Запис логів у файл
k6 run scripts/smoke-test.js > test-output.log 2>&1

# Не показувати прогрес
k6 run --quiet scripts/smoke-test.js
```

## Цікаві посилання

- [K6 Документація](https://k6.io/docs/)
- [K6 GitHub](https://github.com/grafana/k6)
- [K6 Cloud](https://cloud.k6.io/)
