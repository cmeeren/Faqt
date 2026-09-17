---
name: release-faqt
description: Prepare and publish Faqt releases. Use when updating release notes or package versions, creating release commits and tags, packing release artifacts, pushing release tags, watching tag CI/NuGet publication, or creating GitHub Releases.
---

# Release Faqt

## Overview

Use this skill to release Faqt through the repo's normal tag-driven NuGet flow, then create a GitHub Release after the tag CI has successfully pushed the package to NuGet. Keep the release change small and user-facing: version fields, release notes, README and documentation updates when behavior changed, verification, commit, tag, push, CI/NuGet confirmation, and GitHub Release publication. Follow the user's existing authorization boundaries for commits, pushes, and publication; preparation alone does not authorize them.

## Workflow

1. Require a clean working directory before starting a release. If `git status --short` reports changes unrelated to the authorized release work, stop and ask how to resolve them. Do not discard or stash user changes automatically. This precondition does not block an explicitly requested documentation edit or continuation of release preparation already underway.

2. Confirm the release shape before editing:
   - Bump `Version` in `src/Faqt/Faqt.fsproj`.
   - Keep `PackageReleaseNotes` pointing at the versioned GitHub Release URL:

````xml
<PackageReleaseNotes>https://github.com/cmeeren/Faqt/releases/tag/v/$(Version)</PackageReleaseNotes>
````

   - Use `v/<Version>` for the release tag. The CI workflow publishes packages only for tags under `refs/tags/v/`.

3. Prepare the release content:
   - Update code and docs only as needed for the release.
   - Update `RELEASE_NOTES.md` with user-facing behavior, migration impact, and usage guidance. Add entries under `### Unreleased`, creating that section if necessary.
   - Put actionable breaking changes first, using a `#### Breaking changes` or `#### Upgrading from <previous major>.x` subsection. Explain what affected users need to change. Group correctness fixes that restore expected behavior; do not catalog niche edge cases unless they have meaningful migration consequences.
   - Use other categorizing subsections only when grouping materially improves readability; for small releases, prefer plain bullets under the version heading.
   - Within each list, order entries by user impact and breadth. Put documentation/package-metadata-only entries last unless they are the main release purpose.
   - Keep `README.md`, `DOCUMENTATION.md`, and `RELEASE_NOTES.md` public and user-facing. Keep implementation-only rationale out of them.
   - Before tagging, rename `### Unreleased` to `### <Version> (<YYYY-MM-DD>)`, using the release date.
   - Inspect the versioned notes before committing and tagging. Include only intended public notes, with no stale `Unreleased` heading or private details.
   - Do not create or reuse the GitHub Release notes temp file yet; refresh it from the final tagged content after successful NuGet publication.

4. Verify locally, mirroring both source modes used by CI:

````powershell
dotnet tool restore
dotnet fantomas --check .
node --test .agents/skills/release-faqt/scripts/wait-for-nuget.test.js
dotnet test Faqt.slnx -c Release -p:DeterministicSourcePaths=false -p:EmbedAllSources=false
dotnet test Faqt.slnx -c Release -p:DeterministicSourcePaths=true -p:EmbedAllSources=true
````

   Require each command to succeed before continuing. CI also tests on Linux, Windows, and macOS; local success does not replace the tag's CI results. Use the SDK selected by `global.json` and install the runtime targeted by the test projects if it is unavailable locally.

5. Pack locally and inspect the package:

````powershell
$packageDir = Join-Path $env:TEMP ("faqt-release-" + [guid]::NewGuid().ToString("N"))
dotnet pack -c Release src/Faqt/Faqt.fsproj --output $packageDir
if ($LASTEXITCODE -ne 0) { throw "Package creation failed" }
````

   Inspect `Faqt.<Version>.nupkg`: confirm the version, versioned release-notes URL, packaged README and icon, and assemblies for every target framework declared in the project. Check that README links work outside GitHub. Local pack proves that the release URL is embedded; it cannot resolve until the matching GitHub Release exists. If preparing a major release, inspect important known consumers for affected contracts and run representative compatibility checks when practical.

6. Commit and tag when authorized:
   - Use `git-commit` to execute the authorized commit; it routes message authoring to `git-commit-message`.
   - Use `v/<Version>` for releases, for example `v/6.0.0`. Confirm the tag points at the intended release commit and matches the package version.

7. Push the reviewed commit and tag after the user explicitly confirms that action. Successful tag CI publishes the package to NuGet, so pushing the tag is publication authorization, not merely a backup operation.

8. Watch the tag CI run and confirm NuGet publication before creating a GitHub Release:
   - Find the run for the pushed `v/<Version>` tag, not just the branch push run. Verify its `headSha` matches the release commit.

````powershell
$tag = "v/<Version>"
$repo = "cmeeren/Faqt"
$run = @(gh run list --repo $repo --workflow ci.yml --branch $tag --event push --limit 1 --json databaseId,headBranch,headSha,status,conclusion,url | ConvertFrom-Json)
if (-not $run) { throw "No CI run found for tag $tag" }
$runId = $run[0].databaseId
gh run watch $runId --repo $repo --exit-status
````

   - Confirm the tag run succeeded and that the `Push` step performed a real NuGet upload. The workflow uses `--skip-duplicate`, so a successful step can mean the package already existed. Inspect the fresh run logs; if the push was skipped as a duplicate, logs are unavailable, or publication is unclear, stop and inspect/report before creating a GitHub Release.
   - Verify the exact package version is visible on NuGet with [scripts/wait-for-nuget.js](scripts/wait-for-nuget.js). Run from the repository root with Node.js 22 or newer. The helper checks every 30 seconds for up to ten minutes, bounds each request to 15 seconds, and exits nonzero on timeout or an unexpected HTTP response. It retries HTTP 404, 429, server errors, and network failures; it does not publish anything. Wait using bounded tool waits rather than starting additional polling commands.

````powershell
node .agents/skills/release-faqt/scripts/wait-for-nuget.js "<Version>"
if ($LASTEXITCODE -ne 0) { throw "NuGet availability check failed; do not create the GitHub Release" }
````

9. Refresh and inspect the GitHub Release notes immediately before creating the release:
   - Copy the exact release section from the tagged `RELEASE_NOTES.md` to a file under the user's temp directory, for example `$env:TEMP\faqt-release-v<Version>.md`.
   - Read the tagged file with `git show "v/<Version>:RELEASE_NOTES.md"` so the notes match the commit that was packaged and published.
   - Inspect the temporary file. Include only the released version's notes, not the whole changelog or a stale `Unreleased` heading. Make documentation links absolute so they work from the GitHub Release page, using the release tag for version-specific documentation.

10. Create the GitHub Release only after successful NuGet publication, notes inspection, and authorization for that action:

````powershell
$tag = "v/<Version>"
$repo = "cmeeren/Faqt"
if (gh release view $tag --repo $repo 2>$null) { throw "GitHub Release already exists for $tag" }
gh release create $tag --repo $repo --verify-tag --title "Faqt <Version>" --notes-file "$env:TEMP\faqt-release-v<Version>.md"
````

   If the existing-release check finds a release, stop instead of creating a duplicate. If the check fails for authentication or connectivity reasons, resolve that uncertainty before creating one. For a prerelease version, also pass `--prerelease`.

11. Verify the created release and remove temporary notes and package files when practical:

````powershell
$tag = "v/<Version>"
$repo = "cmeeren/Faqt"
gh release view $tag --repo $repo --json tagName,name,url
````

   Confirm the release URL targets the same tag as the `PackageReleaseNotes` URL embedded in the package metadata.

## Guardrails

- Do not create the GitHub Release before the tag CI has successfully pushed to NuGet and the exact version is available.
- Use `gh release create --verify-tag` so GitHub does not auto-create or retarget the release tag.
- Use `--repo cmeeren/Faqt` for `gh` release and run commands; do not rely on implicit repository context.
- Do not create duplicate releases or move an existing release tag.
- Refresh release notes from tagged content after NuGet publication; do not reuse an older temp file.
- Do not treat the versioned `PackageReleaseNotes` URL as fully verified until the matching GitHub Release exists.
