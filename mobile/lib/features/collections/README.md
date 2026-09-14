# Collections

Member 4's collector/owner UI preview is registered at `/collections` through `collectionsRoutes`. The central router already includes this list; shared navigation remains unchanged.

The screen includes a sample overview, searchable pickup list, preferred availability, pickup details, static proposal review preferences, milestones, local failure notes, and a one-time-code input preview. Collector/owner selection only changes presentation; it is not authentication. Data is fictional and changes are held in memory. Review, status-selection, and handover input state is discarded when the pickup detail screen closes. Availability and failure notes last until the collections screen is disposed.

QR scanning, maps, photo capture/upload, code verification, backend calls, agents, and cross-client synchronization are not implemented. Use the shared Dio client for future ASP.NET requests. See [delivery notes](../../../../docs/members/member-4/ui-preview.md) and [ownership](../../../../docs/ownership.md).

## Integrated handover identity requirement

The separate `PickupDetailScreen` and `HandoverScreen` now have API adapters, but
the merged app has no shared authenticated session or verified-user provider.
Handover submission is therefore disabled by default. The collector/owner UI
selector, demo data, item owner, and collector assignment must never supply the
acting user ID. No development actor ID is used.

Shared integration must supply `actorProvider` from the current authenticated
session when constructing these screens. The callback must resolve the current
session's verified user ID at submission time and return null after logout or
session expiry. `requireCollectionsActorId` rejects missing, malformed, and
all-zero GUIDs before verification or proof submission. This injectable callback
is an integration seam, not a new authentication provider; routes intentionally
leave it unset until shared authentication exists.

The same session must configure the API adapter's authenticated transport through
the shared network integration. Supplying a GUID alone is not authentication.
ASP.NET must authenticate the request, authorize the pickup operation, and bind
the actor to its verified principal rather than trust the client-provided ID.
No backend authorization or authentication implementation is changed here.
