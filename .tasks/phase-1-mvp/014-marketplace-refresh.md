# Task: Implement Marketplace Refresh

## What To Do
Add manual refresh functionality to fetch the latest marketplace data, with proper error handling and last-sync timestamp display.

## How To Do
1. Add refresh UI to MarketplaceView:
   - Refresh button (icon + text)
   - Last synced timestamp display
   - Loading indicator during refresh
2. Update `MarketplaceViewModel`:
   ```csharp
   public ICommand RefreshCommand { get; }
   public DateTime? LastSyncedAt { get; set; }
   public bool IsRefreshing { get; set; }
   public string RefreshError { get; set; }
   ```
3. Implement refresh flow:
   - Disable refresh button during operation
   - Call MarketplaceService.RefreshAsync()
   - Update plugin list on success
   - Show error message on failure
   - Update LastSyncedAt timestamp
4. Error handling:
   - Network errors (no internet, timeout)
   - API errors (rate limit, 404)
   - Parse errors (invalid JSON)
   - Display user-friendly error messages
5. Format last synced time:
   - Relative time ("5 minutes ago")
   - Fallback to absolute time
6. Auto-refresh on first load if cache is stale (>24 hours)

## Acceptance Criteria
- [x] Refresh button visible in marketplace view
- [x] Last synced timestamp displayed
- [x] Refresh button disabled during refresh operation
- [x] Loading indicator shown during refresh
- [x] Plugin list updates after successful refresh
- [x] Error message displayed on refresh failure
- [x] Network errors handled gracefully
- [x] API rate limit errors handled with appropriate message
- [x] Parse errors handled gracefully
- [x] Last synced time formatted in user-friendly way
