name: review
description: Review uncommitted C# changes
context: fork
agent: Explore

Changes
-> !git diff HEAD

Review the changes above as a senior .NET dev. Look for:
* async mistakes: missing await, .Result, async void
* EF Core: N+1 queries, missing AsNoTracking on reads
* missing CancellationToken on I/O calls
* secrets or connection strings in code

Return max 10 findings: file, line, problem, fix.
