#!/usr/bin/env pwsh
# Setup LocalStack resources after docker-compose up
# Run this once after starting the dev environment: .\app\infra\setup-localstack.ps1

$docker = "C:\Program Files\Docker\Docker\resources\bin\docker.exe"
$container = "livestream_localstack"

Write-Host "Waiting for LocalStack to be healthy..."
$maxRetries = 10
for ($i = 0; $i -lt $maxRetries; $i++) {
    $status = & $docker inspect --format "{{.State.Health.Status}}" $container 2>&1
    if ($status -eq "healthy") { break }
    Write-Host "  Not ready yet ($status), retrying in 3s..."
    Start-Sleep -Seconds 3
}

Write-Host "Creating S3 bucket: livestream-photos"
& $docker exec $container awslocal s3 mb s3://livestream-photos --region ap-northeast-1

Write-Host "Setting S3 CORS policy..."
$cors = '{"CORSRules":[{"AllowedHeaders":["*"],"AllowedMethods":["GET","PUT","POST","DELETE"],"AllowedOrigins":["http://localhost:3000","http://localhost:3001"],"ExposeHeaders":["ETag"],"MaxAgeSeconds":3000}]}'
& $docker exec $container awslocal s3api put-bucket-cors --bucket livestream-photos --cors-configuration $cors

Write-Host "Verifying SES email identity..."
& $docker exec $container awslocal ses verify-email-identity --email-address noreply@livestream.local --region ap-northeast-1

Write-Host "LocalStack setup complete."
& $docker exec $container awslocal s3 ls
