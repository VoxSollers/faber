#!/bin/bash
echo "Disabling maintenance mode..."
rm -f /usr/share/nginx/html/maintenance/.maintenance_on
if nginx -s reload; then
    echo "✅ Maintenance mode disabled successfully"
    echo "🌐 Website is now live"
else
    echo "❌ Failed to reload nginx configuration"
    exit 1
fi
