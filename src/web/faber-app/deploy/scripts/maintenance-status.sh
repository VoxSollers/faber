#!/bin/bash
if [ -f /usr/share/nginx/html/maintenance/.maintenance_on ]; then
    echo "🔧 Maintenance mode: ENABLED"
    echo "📅 Since: $(stat -c %y /usr/share/nginx/html/maintenance/.maintenance_on 2>/dev/null || echo 'Unknown')"
    echo "📝 Maintenance page: /maintenance/maintenance.html"
else
    echo "🌐 Maintenance mode: DISABLED"
    echo "✅ Website is live"
fi
