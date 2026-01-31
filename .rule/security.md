# Security Requirements

- GitHub tokens stored in OS credential manager (never plain text)
- HTTPS-only for GitHub API calls
- Validate all GitHub repository URLs
- Verify plugin integrity via Git commit hashes
- Transaction-based installations with rollback on failure
