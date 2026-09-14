async (livePage) => {
  const page = await livePage.context().newPage();
  const keys = [];
  const payload = { categoryId: '40000000-0000-4000-8000-000000000001', condition: 'Good', route: 'Reuse', valueLow: 100, valueHigh: 120, currency: 'LKR', sourceName: 'Retry test evidence', observedAt: '2026-09-01T10:00:00Z' };
  await page.route('http://localhost:5080/**', async route => {
    const request = route.request();
    if (request.method() === 'POST') {
      keys.push(request.headers()['idempotency-key']);
      if (keys.length === 1) return route.abort('failed');
      return route.fulfill({status: 201, contentType: 'application/json', body: JSON.stringify({...payload, id: '30000000-0000-4000-8000-000000000001', version: 1})});
    }
    return route.fulfill({status: 200, contentType: 'application/json', body: JSON.stringify({items: [], page: 1, totalPages: 0, totalCount: 0})});
  });
  await page.goto('http://localhost:5173/recovery');
  const first = await page.evaluate(async payload => {
    const api = await import('/src/modules/recovery/services/recoveryApi.js');
    try { await api.createReference(payload); return false; } catch { return true; }
  }, payload);
  await page.reload();
  await page.evaluate(async payload => {
    const api = await import('/src/modules/recovery/services/recoveryApi.js');
    await api.createReference(payload);
    await api.createReference(payload);
  }, payload);
  const result = { lostResponseRejected: first, sameKeyAfterReload: keys[0] === keys[1], freshKeyAfterSuccess: keys[1] !== keys[2], keysPresent: keys.every(key => typeof key === 'string' && key.length > 20) };
  await page.close();
  if (Object.values(result).some(value => !value)) throw new Error(JSON.stringify(result));
  return result;
}
