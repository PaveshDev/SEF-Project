# Start from one shared baseline

1. The leader reviews this local skeleton, commits it, and pushes it to `main` in the real team repository.
2. Everyone clones that same repository. Do not separately initialize four repositories or use `--allow-unrelated-histories`.
3. Each member starts from the same updated `main` baseline using the assigned branch below.
4. Commit only your work and submit pull requests. Shared-file changes belong in a separately coordinated integration change.
5. Before opening or updating a PR, commit or stash unfinished work, fetch `origin`, and merge `origin/main` into your feature branch.
6. Resolve actual conflicts deliberately, run relevant checks, restore and resolve any stashed changes, and request review. Never use blanket ours/theirs resolution or force pushes.

| Member | Branch |
| --- | --- |
| 1 | `feature/member-1-items` |
| 2 | `feature/member-2-recovery` |
| 3 | `feature/member-3-partners` |
| 4 | `feature/member-4-collections` |

Member example (replace the URL and branch for your role):

```sh
git clone <REAL_REPOSITORY_URL> waste-to-value
cd waste-to-value
git switch main
git pull --ff-only origin main
git switch -c feature/member-1-items
```

Before each PR update, with your work committed or stashed:

```sh
git fetch origin
git merge origin/main
# Resolve conflicts deliberately, stage resolved files, and complete the merge.
# Restore any stashed changes, resolve them, then run relevant checks.
git push -u origin feature/member-1-items
```

## Leader baseline commands

The workspace was empty and was not inside a Git repository at initial inspection. The delivered local repository uses `main`, with no commits or remote created by the scaffold task. Run these from the project root after review; replace the remote placeholder. Use an empty remote repository so it shares this baseline history. If the real remote already has history, inspect and reconcile it deliberately first.

```sh
git status --short --branch
git diff
git ls-files --others --exclude-standard
# Read the untracked files: git diff alone does not display them.
git add -- .editorconfig .gitattributes .gitignore .nvmrc global.json AGENTS.md README.md .github frontend backend mobile agents database docs
git diff --cached --check
git diff --cached --stat
git diff --cached
git commit -m "chore: scaffold Waste-to-Value project"
git remote add origin "<REAL_REPOSITORY_URL>"
git remote -v
git push -u origin main
```

If Git requires an identity, set your real identity locally in this repository (`git config user.name "<YOUR_NAME>"` and `git config user.email "<YOUR_EMAIL>"`). Do not change another person's identity or global Git settings. No remote repository, push, deployment, branch protection, or required-review setting has been created by this task.
