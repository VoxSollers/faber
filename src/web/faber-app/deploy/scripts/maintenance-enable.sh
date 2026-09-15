#!/bin/bash
echo "Enabling maintenance mode..."
touch /usr/share/nginx/html/maintenance/.maintenance_on
if nginx -s reload; then
    echo "✅ Maintenance mode enabled successfully"
    echo "🔧 Website is now in maintenance mode"
    echo "📝 Maintenance page: /maintenance/maintenance.html"
else
    echo "❌ Failed to reload nginx configuration"
    exit 1
fi
