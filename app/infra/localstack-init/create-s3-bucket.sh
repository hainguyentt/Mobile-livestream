#!/bin/bash
# Create S3 bucket for user photos in LocalStack
echo "Creating S3 bucket: livestream-photos"
awslocal s3 mb s3://livestream-photos --region ap-northeast-1

# Set CORS policy for direct browser uploads
awslocal s3api put-bucket-cors --bucket livestream-photos --cors-configuration '{
  "CORSRules": [
    {
      "AllowedHeaders": ["*"],
      "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
      "AllowedOrigins": ["http://localhost:3000", "http://localhost:3001"],
      "ExposeHeaders": ["ETag"],
      "MaxAgeSeconds": 3000
    }
  ]
}'

echo "S3 bucket created successfully."
