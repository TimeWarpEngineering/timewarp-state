#!/usr/bin/env bash
# Purpose
#   GitHub Actions gate before `Run CI Pipeline`: when a passing `dev ci` run already covered this exact tree,
#   restore its packages into artifacts/packages and set fast_path=true so the workflow skips the pipeline.
#
# Design
#   Reads timewarp-ganda 660's unsigned note on refs/notes/ganda-ci (same contract as
#   `ganda repo ci-note status --require ...`). ganda is not installed on the runner (private repo, no
#   release binary), and compiling the dev CLI here would spend the minutes this gate exists to save, so the
#   check is git + jq. Every miss prints why and exits 0: the full pipeline then runs as before.
#   Lookup is by HEAD^{tree} (RFC resolution 4): a merge commit or refs/pull/N/merge whose tree equals the
#   tree `dev ci` ran on hits; any other tree misses.
#   Drift (RFC resolution 5): the note's sdk must equal `dotnet --version` after Setup .NET, and its rid must
#   equal this runner's rid and start with linux-. playwright and chromium are recorded for the reader.
#   Packages (RFC resolution 3): refs/ci-packages/<tree> holds the *.nupkg files; the set of names and every
#   sha256 must match the note exactly, or the gate clears artifacts/packages and misses.
#   REQUIRED_STAGES matches CiNoteInput.StageIds in tools/dev-cli/endpoints/workflow/ci-note-input.cs.
#   Bash, not a runfile: a runfile compile on a cold runner costs more than the whole fast path.
#   why: task 150, no-signing override (Steven 2026-11-17)

set -uo pipefail

REQUIRED_STAGES=(assert-version-ssot build test e2e verify-samples)
NOTES_REF=refs/notes/ganda-ci
PACKAGES_DIR=artifacts/packages
OUTPUT=${GITHUB_OUTPUT:-/dev/stdout}

miss() {
  rm -rf "$PACKAGES_DIR"
  echo "CI fast path: miss ($1). Running the full pipeline."
  exit 0
}

tree=$(git rev-parse 'HEAD^{tree}') || miss "cannot read HEAD's tree"
echo "CI fast path: tree $tree"

git fetch --no-tags --quiet origin "+$NOTES_REF:$NOTES_REF" 2>/dev/null || miss "no $NOTES_REF on origin"
note=$(git notes --ref="$NOTES_REF" show "$tree" 2>/dev/null) || miss "no CI note for tree $tree"
jq -e 'type == "object"' >/dev/null 2>&1 <<<"$note" || miss "the CI note is not a JSON object"
echo "CI fast path: note $note"

[ "$(jq -r '.tree // ""' <<<"$note")" = "$tree" ] || miss "the note names another tree"

for stage in "${REQUIRED_STAGES[@]}"; do
  value=$(jq -r --arg s "$stage" '.stages[$s] // "missing"' <<<"$note")
  [ "$value" = pass ] || miss "stage $stage is $value"
done

runner_sdk=$(dotnet --version 2>/dev/null) || miss "dotnet --version failed"
note_sdk=$(jq -r '.tools.sdk // ""' <<<"$note")
[ "$note_sdk" = "$runner_sdk" ] || miss "SDK drift: note $note_sdk, runner $runner_sdk"

case "$(uname -m)" in
  x86_64 | amd64) arch=x64 ;;
  aarch64 | arm64) arch=arm64 ;;
  *) arch=$(uname -m) ;;
esac
runner_rid="linux-$arch"
note_rid=$(jq -r '.tools.rid // ""' <<<"$note")
case "$note_rid" in linux-*) ;; *) miss "the note's rid '$note_rid' is not linux" ;; esac
[ "$note_rid" = "$runner_rid" ] || miss "RID drift: note $note_rid, runner $runner_rid"

[ "$(jq -r '.packages | length' <<<"$note")" -gt 0 ] 2>/dev/null || miss "the note lists no packages"
jq -e 'all(.packages[]; (.name | test("^[A-Za-z0-9._-]+\\.nupkg$")) and (.sha256 | test("^[0-9a-f]{64}$")))' \
  >/dev/null <<<"$note" || miss "the note has a malformed package entry"

packages_ref="refs/ci-packages/$tree"
git fetch --no-tags --quiet origin "+$packages_ref:$packages_ref" 2>/dev/null || miss "no $packages_ref on origin"

rm -rf "$PACKAGES_DIR"
mkdir -p "$PACKAGES_DIR"
git archive --format=tar "$packages_ref" | tar -x -C "$PACKAGES_DIR" || miss "cannot unpack $packages_ref"

listed=$(jq -r '.packages[].name' <<<"$note" | LC_ALL=C sort)
present=$(cd "$PACKAGES_DIR" && find . -mindepth 1 -printf '%P\n' | LC_ALL=C sort)
[ "$listed" = "$present" ] || miss "the package ref does not hold exactly the packages the note lists"

while read -r name sha; do
  actual=$(sha256sum "$PACKAGES_DIR/$name" | cut -d' ' -f1)
  [ "$actual" = "$sha" ] || miss "sha256 mismatch for $name"
done < <(jq -r '.packages[] | "\(.name) \(.sha256)"' <<<"$note")

echo "CI fast path: hit. dev ci passed ${REQUIRED_STAGES[*]} on this tree; $(wc -l <<<"$listed") package(s) restored."
echo "fast_path=true" >>"$OUTPUT"
