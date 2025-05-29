#!/bin/bash
EXPECTED_SERVICE="$1"
MILESTONE="${PR_MILESTONE}"

echo "Expected service: $EXPECTED_SERVICE"
echo "Milestone: $MILESTONE"

if [[ -z "$MILESTONE" ]]; then
  echo "❌ Missing milestone"
  exit 1
fi

if [[ ! "$MILESTONE" =~ ^${EXPECTED_SERVICE}-V[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "❌ Milestone must be formatted like ${EXPECTED_SERVICE}-Vx.y.z"
  exit 1
fi

echo "✅ Milestone validated"
