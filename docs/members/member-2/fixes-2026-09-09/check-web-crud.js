async (livePage) => {
  const page = await livePage.context().newPage();
  const id = '10000000-0000-4000-8000-000000000001';
  let item = {id, itemId:'20000000-0000-4000-8000-000000000001', objective:'Case without retained history', preferredRoutes:['Reuse'], currency:'LKR', maximumPickupCost:null, deadline:null, status:'Draft', revision:1, version:1};
  let refs = [], failReferences = true;
  const operations = [], errors = [];
  page.on('dialog', dialog => dialog.accept());
  page.on('pageerror', error => errors.push(error.message));
  await page.route('http://localhost:5080/**', async route => {
    const request = route.request(), method = request.method(), path = request.url().split('?')[0].replace('http://localhost:5080','');
    const output = (data,status=200) => route.fulfill({status,contentType:'application/json',body:JSON.stringify(data)});
    if (method !== 'GET') operations.push(method+' '+path);
    if (path === '/api/recovery/access') return output({isHuman:true,canManageValueReferences:true});
    if (path === '/api/value-references' && method === 'GET') return failReferences ? output({title:'Reference source temporarily unavailable.'},503) : output({items:refs,page:1,totalPages:1,totalCount:refs.length});
    if (path === '/api/value-references' && method === 'POST') {
      const reference = {...request.postDataJSON(),id:'30000000-0000-4000-8000-00000000000'+(refs.length+1),version:1,isVerified:false}; refs.push(reference); return output(reference,201);
    }
    if (path.startsWith('/api/value-references/')) {
      const reference = refs.find(x => path.includes(x.id));
      if (method === 'PUT') Object.assign(reference,request.postDataJSON(),{version:reference.version+1});
      if (path.endsWith('/verify')) Object.assign(reference,{isVerified:true,version:reference.version+1});
      if (method === 'DELETE') { refs = refs.filter(x=>x!==reference); return route.fulfill({status:204}); }
      return output(reference);
    }
    if (path.endsWith('/options') || path.endsWith('/proposals')) return output([]);
    if (path.endsWith('/cancel')) { item = {...item,status:'Cancelled',version:item.version+1}; return output(item); }
    if (method === 'DELETE') { item = null; return route.fulfill({status:204}); }
    return output(path === '/api/recovery-cases' ? {items:item?[item]:[],page:1,totalPages:1,totalCount:item?1:0} : item);
  });
  await page.goto('http://localhost:5173/recovery');
  await page.getByRole('button',{name:'Value references',exact:true}).click();
  await page.getByRole('button',{name:'Retry loading references'}).waitFor();
  const failureIsVisible = await page.getByRole('alert').isVisible();
  failReferences = false;
  await page.getByRole('button',{name:'Retry loading references'}).click();
  async function create(name) {
    await page.getByLabel('Category identifier',{exact:true}).fill('40000000-0000-4000-8000-000000000001');
    await page.getByLabel('Source name',{exact:true}).fill(name);
    await page.getByLabel('Low value',{exact:true}).fill('100');
    await page.getByLabel('High value',{exact:true}).fill('120');
    await page.getByRole('button',{name:'Save reference',exact:true}).click();
    await page.locator('.reference-row').filter({hasText:name}).waitFor();
  }
  await create('Reference to verify');
  await page.getByRole('button',{name:'Edit',exact:true}).click();
  await page.getByLabel('Source name',{exact:true}).fill('Updated verified evidence');
  await page.getByRole('button',{name:'Save reference',exact:true}).click();
  await page.locator('.reference-row').filter({hasText:'Updated verified evidence'}).waitFor();
  await page.getByRole('button',{name:'Verify',exact:true}).click();
  await page.getByText('Reference verified.',{exact:true}).waitFor();
  const immutable = await page.getByRole('button',{name:'Edit',exact:true}).count() === 0;
  await create('Reference to delete');
  await page.getByRole('button',{name:'Delete',exact:true}).click();
  await page.getByText('Reference deleted.',{exact:true}).waitFor();
  await page.getByRole('button',{name:'Cases',exact:true}).click();
  await page.getByRole('button',{name:/Open planning/}).click();
  await page.getByRole('button',{name:'Cancel case',exact:true}).click();
  await page.getByText('Case cancelled.',{exact:true}).waitFor();
  await page.getByRole('button',{name:'Delete case',exact:true}).click();
  await page.getByText('No recovery cases yet',{exact:true}).waitFor();
  const result = {failureIsVisible,immutable,remainingReferences:refs.length,caseDeleted:item===null,operations,errors};
  await page.close();
  if (!failureIsVisible || !immutable || refs.length !== 1 || item !== null || errors.length) throw new Error(JSON.stringify(result));
  return result;
}
