# Table of contents

<img src="https://raw.githubusercontent.com/cmeeren/Faqt/main/logo/faqt-logo-docs.png" width="300" align="right" />

<!-- TOC -->

* [Before you create an issue](#before-you-create-an-issue)
* [Suggesting new assertions](#suggesting-new-assertions)
* [Pull requests](#pull-requests)
  * [Required unit tests for assertions](#required-unit-tests-for-assertions)
  * [Pull request workflow](#pull-request-workflow)
  * [Rebasing when handling updates from `upstream/main`](#rebasing-when-handling-updates-from-upstreammain)
* [Deployment checklist](#deployment-checklist)

<!-- TOC -->

Requests and contributions are welcome! This project is maintained on a voluntary basis, so please respect the
maintainers' time by spending time on your issues and pull requests before posting them. Remember that it's up to _you_
to make a strong case to convince the the maintainers of the merits of a feature or bug.

# Before you create an issue

- Please search existing issues (both open and closed) before opening a new one.
- Provide as much relevant info as possible.
- If it's a bug report, provide a minimal repro. See
  StackOverflow's [How to create a Minimal, Reproducible Example](https://stackoverflow.com/help/minimal-reproducible-example).
  Make code more readable by placing it in Markdown code blocks with the language `f#` or `fsharp` to get syntax
  highlighting:
  ``````md
  ```f#
  let f a b = a + b
  ```
  ``````
- Be clear and specific.
- Preview and read your issue before posting, and rewrite it if necessary. We all write issues at times without having a
  clear idea up front of what we want to communicate. Just don't _post_ a jumbled mess; rewrite it.

# Suggesting new assertions

Please open an issue to request new assertions. The following are not requirements, but will increase the chances of it
being accepted:

- The assertion is sufficiently general and useful in a variety of contexts. For example, `BeEven`/`BeOdd` is more
  likely to be accepted than `ContainsItemWithThreeCharacters`.
- The assertion does not require new NuGet dependencies.
- The assertion does not conflict with existing assertions, either by causing confusion for users or by causing overload
  resolution problems for F#.
- The request includes the following:
  - A motivating use-case, which should be as short as possible while still convincing any current or future maintainer
    that the assertion is worth keeping.
  - The desired behavior for all edge cases (null, empty, etc.).
  - Any other related assertions that could be added (for example, if suggesting `BeEven`, it would make sense to
    include `BeOdd`, too).
  - The current way of accomplishing this using existing assertions, if there is a way.

# Pull requests

To make everyone's experience as enjoyable as possible, please
read [Don't "Push" Your Pull Requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) and please keep
the following things in mind:

- Unless it's a trivial fix, consider opening an issue first to discuss it with the maintainers.
- For all pull requests, please follow the workflow described below.

## Required unit tests for assertions

When writing tests for your proposed assertions, please take inspiration from tests of similar assertions. For
consistency, the following tests should be used for all assertions:

* One test to verify chaining.
* One test per success condition, including edge cases like `null` subject. The tests could be one or more
  parametrized `Theory` tests. If there is a single success condition and no edge cases, it can be omitted since it is
  fully covered by the chaining test described above.
* At least two tests per call to `Fail` to verify failure output with and without `because`. Additional tests to capture
  all variations of output format, if relevant.
* One test per failure condition, including edge cases like `null` subject. The tests could be one or more
  parametrized `Theory` tests. If a `Fact` or a whole `Theory` (not just a test case in a `Theory`) is fully covered by
  an output test described above, it can be omitted.
* After tests are written, verify that the tests actually invoke the correct assertion method/overload (easy to miss if
  copy-pasting).

## Pull request workflow

1. Fork Faqt on GitHub
2. Clone your fork locally
3. Add the upstream repo: `git remote add upstream git@github.com:cmeeren/Faqt.git`
4. Create a local branch: `git checkout -b myBranch`
5. Work on your feature
6. Rebase if required (see below)
7. Push the branch to GitHub: `git push origin myBranch`
8. Send a Pull Request on GitHub

You should **never** work on a clone of `main`, and you should **never** send a pull request from `main` - always from a
branch. The reasons for this are detailed below.

## Rebasing when handling updates from `upstream/main`

While you're working on your branch, it is possible that Faqt's main branch may be updated. If this happens, you should:

1. [Stash](https://git-scm.com/book/en/v2/Git-Tools-Stashing-and-Cleaning) any un-committed changes (or use your IDE's
   similar functionality, such as shelving the changes in Rider)
2. `git checkout main`
3. `git pull upstream main`
4. `git rebase main myBranch`
5. `git push origin main` (optional; this this makes sure your fork's `main` is up to date)
6. `git checkout myBranch`
7. Unstash/unshelve anything you stashed/shelved in step 1

This ensures that your history is "clean", with one branch off from `main` containing your changes in a straight line.
Failing to do this ends up with several messy merges in your history, which is preferable to avoid in order to keep the
project history understandable. This is the reason why you should always work in a branch and you should never be
working in, or sending pull requests from, `main`.

If you have pushed your branch to GitHub and you need to rebase like this (whether or not you have already created a
pull request), you need to use `git push -f` to force rewrite the remote branch.

# Deployment checklist

For maintainers.

* Make necessary changes to the code.
* Update the changelog.
* Update the version and release notes in the fsproj files.
* Commit and tag the commit in the format `v/x.y.z` (this is what triggers deployment).
* Push the changes and the tag to the repo. If the build succeeds, the package is automatically published to NuGet.
