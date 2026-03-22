#!/bin/bash
# Verify sender email address in LocalStack SES
echo "Verifying SES email identity: noreply@livestream.local"
awslocal ses verify-email-identity \
  --email-address noreply@livestream.local \
  --region ap-northeast-1

echo "SES email identity verified."
