#!/bin/bash
set -euo pipefail

/usr/bin/Xvfb "$DISPLAY" -screen "$XVFB_SCREEN" "$XVFB_RESOLUTION" -ac -nolisten tcp &
xvfb_pid=$!

# Wait for Xvfb to be ready
while ! xdpyinfo -display "$DISPLAY" >/dev/null 2>&1; do
  echo "$(date) Waiting for Xvfb..."
  sleep 0.5
done

trap 'kill $xvfb_pid 2>/dev/null 2>&1' EXIT
"$@"
