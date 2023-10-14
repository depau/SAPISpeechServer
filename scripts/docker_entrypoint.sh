#!/bin/bash
set -euo pipefail

source auto_xvfb

"$@" 2>&1 | grep -v fixme
