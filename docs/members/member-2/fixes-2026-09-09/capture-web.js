async (livePage) => {
 const page=await livePage.context().newPage();
 const dir='C:/Users/paves/Downloads/PRo/docs/members/member-2/fixes-2026-09-09/screenshots/';
 const id='10000000-0000-4000-8000-000000000001';
 let item={id,itemId:'20000000-0000-4000-8000-000000000001',revision:1,version:1,objective:'Find a useful next home for an office chair',preferredRoutes:['Reuse','Donate'],currency:'LKR',maximumPickupCost:1500,deadline:'2026-10-10T10:00:00Z',status:'Draft'};
 const estimate={valueLow:4000,valueHigh:6000,repairCost:0,pickupCost:0,netValue:4000,shortfall:0,totalCost:0,currency:'LKR'};
 let options=[],proposal=null,empty=true;
 const requests=[],errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 const output=(route,data,status=200)=>route.fulfill({status,contentType:'application/json',body:JSON.stringify(data)});
 await page.route('http://localhost:5080/**',async route=>{
   const req=route.request(),path='/'+req.url().split('/').slice(3).join('/').split('?')[0],method=req.method();
   requests.push({method,path});
   if(method==='GET'&&path==='/api/recovery/access')return output(route,{isHuman:true,canManageValueReferences:true});
   if(method==='GET'&&path.endsWith('/proposals'))return output(route,proposal?[proposal]:[]);
   if(method==='GET'&&path==='/api/recovery-cases')return output(route,{items:empty?[]:[item],page:1,pageSize:12,totalCount:empty?0:1,totalPages:empty?0:1});
   if(method==='GET'&&path==='/api/value-references')return output(route,{items:[{id:'30000000-0000-4000-8000-000000000001',categoryId:'40000000-0000-4000-8000-000000000001',condition:'Good',route:'Reuse',valueLow:4000,valueHigh:6000,currency:'LKR',sourceName:'Sample furniture market evidence',observedAt:'2026-09-01T10:00:00Z',isVerified:true,version:1}],page:1,pageSize:12,totalCount:1,totalPages:1});
   if(method==='GET'&&path==='/api/recovery-cases/'+id)return output(route,item);
   if(method==='GET'&&path.endsWith('/options'))return output(route,options);
   if(method==='PUT'&&path==='/api/recovery-cases/'+id){item={...item,...req.postDataJSON().inputs,version:item.version+1,revision:item.revision+1};return output(route,item);}
   if(method==='POST'&&(path.endsWith('/plan')||path.endsWith('/replan'))){
     item={...item,version:item.version+1,status:'Planning'};
     options=[{id:'50000000-0000-4000-8000-000000000001',caseRevision:item.revision,version:1,route:'Reuse',requiresPartner:false,requiresPickup:false,status:'Validated',estimate,evidence:[{sourceName:'Sample furniture market evidence',observedAt:'2026-09-01T10:00:00Z'}],nonFinancialBenefits:['Extends useful life'],integration:{match:null,pickup:null}},
     {id:'50000000-0000-4000-8000-000000000002',caseRevision:item.revision,version:1,route:'Donate',requiresPartner:true,requiresPickup:true,status:'Draft',estimate:null,evidence:[],nonFinancialBenefits:[],integration:{match:null,pickup:null}}];
     return output(route,{case:item,options,unavailableInputs:['Donate: accepted eligible match unavailable.']});
   }
   if(method==='POST'&&path.endsWith('/proposals')){
     const data=req.postDataJSON();
     proposal={id:'60000000-0000-4000-8000-000000000001',caseId:id,caseRevision:item.revision,revision:1,version:1,optionId:data.optionId,matchId:null,pickupPlanId:null,explanation:data.explanation,expiresAt:data.expiresAt,status:'AwaitingApproval',estimate};
     item={...item,version:item.version+1,status:'AwaitingApproval'};
     return output(route,proposal,201);
   }
   if(method==='POST'&&path.endsWith('/decisions')){proposal={...proposal,version:proposal.version+1,status:req.postDataJSON().decision};item={...item,version:item.version+1,status:proposal.status};return output(route,proposal);}
   if(method==='GET'&&path.startsWith('/api/recovery-proposals/'))return output(route,proposal);
   return output(route,{title:'Unimplemented screenshot fixture',status:501},501);
 });
 await page.addInitScript(()=>{
   addEventListener('DOMContentLoaded',()=>{
     const banner=document.createElement('div');banner.textContent='UI PREVIEW — simulated API data; no database or Gemini execution';
     banner.style.cssText='position:relative;background:#ffe9a8;color:#282014;font:600 13px Arial;padding:9px;text-align:center;border-bottom:2px solid #8b6b12';
     document.body.prepend(banner);
   });
 });
 const sizes=[{name:'desktop',width:1440,height:1000},{name:'tablet',width:768,height:1024},{name:'mobile',width:390,height:844}];
 const captures=[],checks=[];
 async function capture(slug){
  for(const size of sizes){
   await page.setViewportSize(size);
   await page.screenshot({path:dir+size.name+'-'+slug+'-preview.png',fullPage:true});
   captures.push(size.name+'-'+slug+'-preview.png');
   checks.push({view:slug,size:size.name,...await page.evaluate(()=>({viewport:innerWidth,width:document.documentElement.scrollWidth}))});
  }
  await page.setViewportSize({width:1440,height:1000});
 }
 await page.goto('http://localhost:5173/recovery');
 await page.getByText('No recovery cases yet',{exact:true}).waitFor();
 await capture('empty');
 empty=false;await page.reload();
 await page.getByRole('button',{name:'Open planning'}).waitFor();
 await capture('cases');
 await page.getByRole('button',{name:'Value references',exact:true}).click();
 await page.getByText('Sample furniture market evidence',{exact:true}).waitFor();
 await capture('references');
 await page.getByRole('button',{name:'Cases',exact:true}).click();
 await page.getByRole('button',{name:'Open planning'}).click();
 await page.getByRole('button',{name:'Plan case',exact:true}).waitFor();
 await capture('workflow-initial');
 await page.getByRole('button',{name:'Edit inputs',exact:true}).click();
 await capture('edit-case');
 await page.getByLabel('Objective', {exact:true}).fill('Recover the chair with its complete route preferences');
 await page.getByRole('button',{name:'Save case',exact:true}).click();
 await page.getByRole('dialog').waitFor({state:'hidden'});
 checks.push({test:'objective-only edit preserves both preferred routes',pass:item.preferredRoutes.join(',')==='Reuse,Donate'});
 await page.getByRole('button',{name:'Plan case',exact:true}).click();
 await page.getByText('Planning completed. Select an eligible option to prepare a proposal.',{exact:true}).waitFor();
 checks.push({test:'unvalidated option selection disabled',pass:await page.getByRole('button',{name:'Select Donate',exact:true}).isDisabled()});
 await capture('options');
 await page.getByRole('button',{name:'Select Reuse',exact:true}).click();
 await page.getByLabel('Proposal expiry',{exact:false}).fill('2026-10-01T10:00');
 await page.getByLabel('Explanation',{exact:true}).fill('Reuse is supported by the confirmed condition and the sample value reference.');
 await capture('proposal-create');
 await page.getByRole('button',{name:'Create proposal',exact:true}).click();
 await page.getByText('Proposal created and loaded for review.',{exact:true}).waitFor();
 await capture('proposal-review');
 await page.getByRole('combobox',{name:'Decision',exact:true}).selectOption('RevisionRequested');
 checks.push({test:'revision request requires comment',pass:await page.getByRole('button',{name:'Submit decision',exact:true}).isDisabled()});
 await page.getByRole('combobox',{name:'Decision',exact:true}).selectOption('Approved');
 await page.getByRole('button',{name:'Submit decision',exact:true}).click();
 await page.getByText('Decision submitted. Proposal, case and options refreshed.',{exact:true}).waitFor();
 await capture('decision-success');
 const before=requests.length;
 await page.getByRole('button',{name:'All cases',exact:false}).click();
 await page.getByRole('button',{name:'Open planning'}).click();
 await page.getByLabel('Proposal history',{exact:true}).waitFor();
 checks.push({test:'existing proposal survives leaving/reopening',pass:(await page.getByLabel('Proposal history',{exact:true}).inputValue())===proposal.id,reopenRequests:requests.slice(before)});
 await capture('reopen-proposal');
 await page.getByRole('button',{name:'All cases',exact:false}).click();
 await page.getByRole('button',{name:'New recovery case',exact:false}).click();
 await page.getByRole('dialog').waitFor();
 await page.getByLabel('Item identifier',{exact:true}).fill('invalid');
 await page.getByLabel('Objective',{exact:true}).fill('   ');
 await page.getByLabel('Currency',{exact:true}).fill('12!');
 await page.getByRole('button',{name:'Save case',exact:true}).click();
 checks.push({test:'invalid identifiers objective and currency rejected',pass:await page.locator('[aria-invalid="true"]').count()===3});
 await capture('validation');
 const focusInside=await page.evaluate(()=>document.querySelector('dialog').contains(document.activeElement));
 for(let i=0;i<20;i++)await page.keyboard.press('Tab');
 checks.push({test:'dialog contains keyboard focus',pass:focusInside&&await page.evaluate(()=>document.querySelector('dialog').contains(document.activeElement))});
 await page.keyboard.press('Escape');
 checks.push({test:'escape closes dialog and restores focus',pass:await page.getByRole('dialog').count()===0&&await page.getByRole('button',{name:'New recovery case',exact:false}).evaluate(e=>e===document.activeElement)});
 await page.getByRole('button',{name:'Agent monitor',exact:true}).click();
 await page.getByText('Agent integration unavailable.',{exact:true}).waitFor();
 await capture('agent-unavailable');
 await page.close();
 if(checks.some(x=>x.pass===false||x.width>x.viewport))throw new Error(JSON.stringify(checks));
 return {captures,checks,errors,requestCount:requests.length};
}
