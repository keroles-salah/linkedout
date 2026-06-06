const CACHE='linkdout-v2';
const ASSETS=['/','/css/linkdout.css','/manifest.json'];

self.addEventListener('install',e=>{
  e.waitUntil(caches.open(CACHE).then(c=>c.addAll(ASSETS)).catch(()=>{}));
  self.skipWaiting();
});

self.addEventListener('activate',e=>{
  e.waitUntil(caches.keys().then(keys=>Promise.all(keys.filter(k=>k!==CACHE).map(k=>caches.delete(k)))));
  self.clients.claim();
});

self.addEventListener('fetch',e=>{
  if(e.request.method!=='GET')return;
  if(e.request.url.includes('/api/'))return;
  e.respondWith(
    caches.match(e.request).then(cached=>{
      var fetched=fetch(e.request).then(response=>{
        if(response.ok){
          var clone=response.clone();
          caches.open(CACHE).then(c=>c.put(e.request,clone));
        }
        return response;
      });
      return cached||fetched;
    })
  );
});
