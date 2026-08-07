#!/usr/bin/env bash
set -euo pipefail

# Usage:
#   ./scripts/bootstrap-github-labels.sh OWNER/REPO
#
# Creates/updates the recommended labels only.
# Milestones and issues are intentionally left as a reviewed manual import
# from docs/github/ to avoid creating a large backlog in the wrong repository.

REPO="${1:-}"

if [[ -z "$REPO" ]]; then
  echo "Usage: $0 OWNER/REPO" >&2
  exit 2
fi

command -v gh >/dev/null 2>&1 || {
  echo "GitHub CLI 'gh' is required." >&2
  exit 3
}

gh auth status >/dev/null

create_label() {
  local name="$1"
  local color="$2"
  local description="$3"
  gh label create "$name" --repo "$REPO" --color "$color" --description "$description" --force
}

create_label "type:bug" "d73a4a" "Defect in implemented behaviour"
create_label "type:feature" "0e8a16" "New user-visible capability"
create_label "type:architecture" "5319e7" "Architecture or ADR-impacting change"
create_label "type:documentation" "0075ca" "Documentation-only work"
create_label "type:test" "1d76db" "Test infrastructure or coverage"
create_label "type:refactor" "c5def5" "Behaviour-preserving restructuring"
create_label "type:chore" "ededed" "Repository/build/maintenance work"
create_label "type:security" "b60205" "Security hardening; vulnerabilities still reported privately"
create_label "area:ui" "fbca04" "Windows Forms and UX"
create_label "area:domain" "bfdadc" "Domain model and invariants"
create_label "area:persistence" "c2e0c6" "SQLite, EF Core and migrations"
create_label "area:import-export" "fef2c0" "Exchange formats"
create_label "area:backup-recovery" "f9d0c4" "Backup, restore and crash recovery"
create_label "area:references" "d4c5f9" "Repository/chat/document/link references"
create_label "area:build-release" "bfd4f2" "CI, packaging and release"
create_label "priority:p0" "b60205" "Release blocker or data-loss risk"
create_label "priority:p1" "d93f0b" "Must be addressed in current milestone"
create_label "priority:p2" "fbca04" "Normal planned priority"
create_label "priority:p3" "cfd3d7" "Nice-to-have or later"
create_label "status:blocked" "000000" "Cannot progress"
create_label "status:needs-decision" "e99695" "Explicit product/architecture decision required"
create_label "status:ready" "0e8a16" "Defined well enough for implementation"

echo "Recommended labels created/updated in $REPO"
