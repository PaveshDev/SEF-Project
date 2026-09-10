async (livePage) => {
 const page=await livePage.context().newPage();
 const dir='C:/Users/paves/Downloads/PRo/docs/members/member-2/fixes-2026-09-09/screenshots/';
 const id='10000000-0000-4000-8000-000000000001';
 let item={id,itemId:'20000000-0000-4000-8000-000000000001',revision:1,version:1,objective:'Find a useful next home for an office chair',preferredRoutes:['Reuse','Donate'],currency:'LKR',maximumPickupCost:1500,deadline:'2026-10-10T10:00:00Z',status:'Draft'};
 const estimate={valueLow:4000,valueHigh:6000,repairCost:0,pickupCost:0,netValue:4000,shortfall:0,totalCost:0,currency:'LKR'};
 let options=[],proposal=null,empty=false;
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
     options=[{id:'50000000-0000-4000-8000-000000000001',caseRevision:item.revision,version:1,route:'Reuse',requiresPartner:false,requiresPickup:false,status:'Validated',estimate,evidence:[{referenceId:'30000000-0000-4000-8000-000000000001',version:1,sourceName:'Sample furniture market evidence',observedAt:'2026-09-01T10:00:00Z'}],nonFinancialBenefits:['Extends useful life'],integration:{match:null,pickup:null}},
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
     banner.style.cssText='position:fixed;top:0;left:0;right:0;z-index:99999;pointer-events:none;background:#ffe9a8;color:#282014;font:600 13px Arial;padding:9px;text-align:center;border-bottom:2px solid #8b6b12';
     document.body.prepend(banner);
   });
 });

 const checks=[], captures=[];
 async function reveal(locator, direction=1) {
   for(let i=0;i<12;i++) {
     if(await locator.count())return;
     await page.mouse.move(900,700); await page.mouse.wheel(0,500*direction);
     await page.evaluate(()=>new Promise(resolve=>requestAnimationFrame(()=>requestAnimationFrame(resolve))));
   }
   await locator.waitFor({timeout:3000});
 }
 async function type(locator,text) {
   await reveal(locator); await locator.click({force:true}); await locator.press('ControlOrMeta+A'); await locator.pressSequentially(text);
 }

 async function capture(name) {
   for(const size of [{name:'desktop',width:1440,height:1000},{name:'tablet',width:768,height:1024},{name:'mobile',width:390,height:844}]) {
     await page.setViewportSize(size);
     await page.evaluate(()=>new Promise(resolve=>requestAnimationFrame(()=>requestAnimationFrame(resolve))));
     await page.screenshot({path:dir+'flutter-'+size.name+'-'+name+'.png'});
     captures.push('flutter-'+size.name+'-'+name+'.png');
   }
   await page.setViewportSize({width:1440,height:1000});
 }
 await page.goto('http://localhost:5177/#/recovery');
 await page.locator('flt-semantics-placeholder').evaluate(e=>e.click());
 await page.getByRole('group',{name:/Find a useful next home/}).waitFor();
 await capture('cases');
 await page.getByRole('group',{name:/Find a useful next home/}).evaluate(e=>e.click());
 await page.getByRole('button',{name:'Edit inputs',exact:true}).waitFor();
 await page.getByRole('button',{name:'Edit inputs',exact:true}).click();
 await type(page.getByRole('textbox',{name:'Objective',exact:true}),'Updated Flutter recovery objective');
 await capture('edit');
 await page.getByRole('button',{name:'Save case',exact:true}).click();
 await page.getByRole('button',{name:'Edit inputs',exact:true}).waitFor();
 checks.push({test:'Flutter editing preserves both routes',pass:item.preferredRoutes.join(',')==='Reuse,Donate'&&item.objective==='Updated Flutter recovery objective'});
 await page.getByRole('button',{name:'Plan',exact:true}).click();
 await page.getByRole('button',{name:'Select Reuse',exact:true}).waitFor();
 await capture('options');
 await page.getByRole('button',{name:'Select Reuse',exact:true}).click();
 await type(page.getByRole('textbox',{name:/Explanation/}),'Verified reuse with current assessment and evidence');
 await page.getByRole('button',{name:'Create proposal',exact:true}).click();
 await reveal(page.getByRole('button',{name:'Approve',exact:true}));
 await capture('proposal');
 await reveal(page.getByRole('button',{name:'All cases',exact:true}),-1);
 await page.getByRole('button',{name:'All cases',exact:true}).evaluate(e=>e.click());
 await page.getByRole('button',{name:'All cases',exact:true}).waitFor({state:'hidden'});
 await page.getByRole('group',{name:/Updated Flutter recovery objective/}).waitFor();
 await page.getByRole('group',{name:/Updated Flutter recovery objective/}).click({force:true});
 await page.getByRole('button',{name:'All cases',exact:true}).waitFor();
 await reveal(page.getByRole('button',{name:'Approve',exact:true}));
 checks.push({test:'Flutter reopens proposal from server history',pass:true});
 await capture('reopened-proposal');
 await reveal(page.getByRole('button',{name:'All cases',exact:true}),-1);
 await page.getByRole('button',{name:'All cases',exact:true}).evaluate(e=>e.click());
 await page.getByRole('button',{name:'All cases',exact:true}).waitFor({state:'hidden'});
 await page.getByRole('button',{name:'Value references',exact:true}).click();
 await page.getByRole('button',{name:'Add reference',exact:true}).waitFor();
 await capture('references');
 await page.getByRole('button',{name:'Add reference',exact:true}).click();
 await page.getByRole('button',{name:'Save reference',exact:true}).waitFor();
 await capture('reference-form');
 await page.getByRole('button',{name:'Save reference',exact:true}).click();
 await capture('reference-validation');
 if(checks.some(c=>!c.pass)||errors.length)throw new Error(JSON.stringify({checks,errors}));
 await page.close();
 return {checks,captures,errors};
}
