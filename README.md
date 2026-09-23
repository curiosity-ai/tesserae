# pr-assets

Screenshots referenced from pull request descriptions, one folder per PR number.

This branch is an orphan: it shares no history with `master` and is never merged.
It exists because GitHub's API has no image-upload endpoint, so a PR body written
by tooling can only embed an image that lives in the repository - and source
branches are the wrong place for binaries.

Reference a file as:

    https://github.com/curiosity-ai/tesserae/blob/pr-assets/<pr>/<file>.png?raw=true

Use the `blob/...?raw=true` form rather than `raw.githubusercontent.com`: on a
private repository the blob URL renders for anyone with repository access, while
the raw host needs a token.

A folder can be deleted once its PR is merged or closed and nobody needs to read
the description any more.
