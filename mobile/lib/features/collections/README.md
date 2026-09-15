# Collections

Member 4's collector/owner UI preview is registered at `/collections` through `collectionsRoutes`. The central router already includes this list; shared navigation remains unchanged.

The screen includes a sample overview, searchable pickup list, preferred availability, pickup details, static proposal review preferences, milestones, local failure notes, and a one-time-code input preview. Collector/owner selection only changes presentation; it is not authentication. Data is fictional and changes are held in memory. Review, status-selection, and handover input state is discarded when the pickup detail screen closes. Availability and failure notes last until the collections screen is disposed.

QR scanning, maps, photo capture/upload, code verification, backend calls, agents, and cross-client synchronization are not implemented. Use the shared Dio client for future ASP.NET requests. See [delivery notes](../../../../docs/members/member-4/ui-preview.md) and [ownership](../../../../docs/ownership.md).
