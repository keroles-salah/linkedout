
// Dark mode
(function(){
  var saved=localStorage.getItem('linkdout-theme');
  if(saved==='dark'||(!saved&&window.matchMedia('(prefers-color-scheme:dark)').matches)){
    document.documentElement.setAttribute('data-theme','dark');
    var btn=document.querySelector('.theme-toggle');
    if(btn)btn.textContent='☀️';
  }
})();
function toggleTheme(){
  var isDark=document.documentElement.getAttribute('data-theme')==='dark';
  if(isDark){
    document.documentElement.removeAttribute('data-theme');
    localStorage.setItem('linkdout-theme','light');
    document.querySelector('.theme-toggle').textContent='🌙';
  }else{
    document.documentElement.setAttribute('data-theme','dark');
    localStorage.setItem('linkdout-theme','dark');
    document.querySelector('.theme-toggle').textContent='☀️';
  }
}
// Mobile menu
function toggleMobileMenu(){
  var nav=document.querySelector('.nav-links');
  var ham=document.querySelector('.hamburger');
  nav.classList.toggle('mobile-open');
  ham.classList.toggle('active');
}
// Close mobile menu on link click
document.querySelectorAll('.nav-link').forEach(function(l){
  l.addEventListener('click',function(){
    document.querySelector('.nav-links').classList.remove('mobile-open');
    document.querySelector('.hamburger').classList.remove('active');
  });
});

// Notification count + bell
fetch('/api/notifications/count').then(r=>r.json()).then(d=>{
  if(d.count>0){
    var bell=document.getElementById('notif-link');
    if(bell){bell.querySelector('span').textContent='طلبات ('+d.count+')';bell.style.color='var(--primary)';}
    var badge=document.getElementById('notif-badge');
    if(badge){badge.style.display='flex';badge.textContent=d.count;}
  }
});

// Notification dropdown
function toggleNotifDropdown(e){
  e.stopPropagation();
  var dd=document.getElementById('notif-dropdown');
  if(dd.style.display==='flex'){dd.style.display='none';return}
  dd.style.display='flex';
  fetch('/api/notifications').then(r=>r.json()).then(d=>{
    var list=document.getElementById('notif-list');
    if(d.notifications&&d.notifications.length>0){
      list.innerHTML=d.notifications.slice(0,8).map(function(n){
        var icon=n.type==='like'?'❤️':n.type==='comment'?'💬':'🤝';
        var link=n.type==='connection'?'/Connections':'/Profile/'+n.userId;
        return '<a href="'+link+'" style="display:flex;align-items:flex-start;gap:0.7rem;padding:0.6rem;border-radius:10px;transition:var(--transition);font-size:0.82rem;text-decoration:none;color:var(--text)" onmouseover="this.style.background='var(--bg)'" onmouseout="this.style.background='transparent'"><span style="font-size:1.2rem">'+icon+'</span><div style="flex:1"><div style="font-size:0.8rem">'+n.text+'</div><div style="font-size:0.65rem;color:var(--text-muted)">'+n.time+'</div></div></a>';
      }).join('');
    }else{list.innerHTML='<div style="text-align:center;padding:1rem;color:var(--text-muted);font-size:0.82rem">لا توجد إشعارات جديدة</div>'}
  });
}
document.addEventListener('click',function(e){
  var dd=document.getElementById('notif-dropdown');
  if(dd&&dd.style.display==='flex'&&!e.target.closest('#notif-dropdown')&&!e.target.closest('.notif-bell')){dd.style.display='none'}
});

function showToast(m){var t=document.getElementById('toast');t.textContent=m;t.classList.add('show');setTimeout(function(){t.classList.remove('show')},2500)}

// ===== LIKE =====
document.addEventListener('click',function(e){
  var btn=e.target.closest('.post-action');
  if(!btn)return;
  // Like button
  if(btn.querySelector('path[d*="20.84"]')){
    e.preventDefault();
    var postEl=btn.closest('.post, .card');
    var postId=postEl?postEl.dataset.postId:null;
    if(!postId){showToast('خطأ: لا يمكن تحديد المنشور');return;}
    fetch('/api/interactions/like/'+postId,{method:'POST',headers:{'Content-Type':'application/json'}})
    .then(r=>r.json()).then(d=>{
      var svg=btn.querySelector('svg path');
      if(d.liked){btn.classList.add('liked');svg.setAttribute('fill','currentColor');}
      else{btn.classList.remove('liked');svg.setAttribute('fill','none');}
      btn.querySelector('span').textContent=d.likeCount;
    }).catch(()=>showToast('خطأ في الاتصال'));
  }
  // Comment button — toggle comments
  if(btn.querySelector('path[d*="21 15"]')&&!btn.dataset.bound){
    btn.dataset.bound='1';
    btn.addEventListener('click',function(ev){
      ev.preventDefault();
      var postEl=btn.closest('.post, .card');
      var postId=postEl?postEl.dataset.postId:null;
      if(!postId)return;
      var existing=postEl.querySelector('.comments-section');
      if(existing){existing.remove();return;}
      var sec=document.createElement('div');sec.className='comments-section';
      sec.style.cssText='padding:0.8rem 1rem;border-top:1px solid rgba(45,37,32,0.06);max-height:300px;overflow-y:auto';
      // Input
      var inp=document.createElement('div');inp.style.cssText='display:flex;gap:0.5rem;margin-bottom:0.8rem';
      inp.innerHTML='<input type="text" placeholder="اكتب تعليقاً..." style="flex:1;padding:0.5rem 0.8rem;border-radius:12px;border:1.5px solid rgba(45,37,32,0.1);font-family:inherit;font-size:0.85rem;background:var(--bg)"><button style="padding:0.5rem 1rem;border-radius:12px;background:var(--primary);color:#fff;border:none;font-family:inherit;cursor:pointer;font-size:0.85rem">أرسل</button>';
      sec.appendChild(inp);
      var list=document.createElement('div');list.className='comments-list';sec.appendChild(list);
      postEl.appendChild(sec);
      // Load comments
      fetch('/api/interactions/comments/'+postId).then(r=>r.json()).then(comments=>{
        comments.forEach(c=>{var d=document.createElement('div');d.style.cssText='padding:0.4rem 0;font-size:0.85rem';d.innerHTML='<b>'+c.authorName+'</b> '+c.body;list.prepend(d);});
      });
      // Submit comment
      inp.querySelector('button').addEventListener('click',function(){
        var val=inp.querySelector('input').value.trim();if(!val)return;
        fetch('/api/interactions/comment/'+postId,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({body:val})})
        .then(r=>r.json()).then(d=>{var el=document.createElement('div');el.style.cssText='padding:0.4rem 0;font-size:0.85rem';el.innerHTML='<b>'+d.author+'</b> '+d.body;list.prepend(el);inp.querySelector('input').value='';btn.querySelector('span').textContent=d.id?''+(parseInt(btn.querySelector('span').textContent||0)+1):btn.querySelector('span').textContent;});
      });
    });
  }
  // Share button
  if(btn.querySelector('circle[cx="18"]')&&btn.querySelector('line')){
    e.preventDefault();
    var postEl=btn.closest('.post, .card');
    var postId=postEl?postEl.dataset.postId:null;
    if(!postId)return;
    fetch('/api/interactions/share/'+postId,{method:'POST'}).then(r=>r.json()).then(d=>{
      btn.querySelector('span').textContent=d.shareCount;showToast('تمت المشاركة!');
    }).catch(()=>showToast('خطأ'));
  }
});

// ===== POST COMPOSER =====
document.addEventListener('click',function(e){
  var comp=e.target.closest('.composer');
  if(!comp)return;
  if(e.target.tagName==='INPUT'||e.target.tagName==='TEXTAREA'||e.target.tagName==='BUTTON')return;
  var existing=document.querySelector('.composer-modal');
  if(existing){existing.remove();return;}
  var modal=document.createElement('div');modal.className='composer-modal';
  modal.style.cssText='position:fixed;inset:0;z-index:200;background:rgba(0,0,0,0.4);display:flex;align-items:center;justify-content:center;padding:2rem';
  modal.innerHTML=`
    <div style="background:var(--bg-card);border-radius:20px;padding:2rem;width:100%;max-width:560px;box-shadow:var(--shadow-lg)">
      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:1.2rem">
        <h3 style="font-family:Amiri,serif;font-size:1.2rem">منشور جديد</h3>
        <button onclick="this.closest('.composer-modal').remove()" style="background:none;border:none;font-size:1.5rem;cursor:pointer;color:var(--text-muted)">&times;</button>
      </div>
      <textarea id="composer-body" placeholder="شارك إنجاز، فكرة، أو فرصة..." style="width:100%;min-height:120px;padding:0.8rem;border-radius:12px;border:1.5px solid rgba(45,37,32,0.1);font-family:inherit;font-size:0.9rem;resize:vertical;background:var(--bg)"></textarea>
      <input id="composer-tags" type="text" placeholder="هاشتاقات (مفصولة بفاصلة)" style="width:100%;padding:0.6rem 0.8rem;border-radius:12px;border:1.5px solid rgba(45,37,32,0.1);font-family:inherit;font-size:0.85rem;margin-top:0.6rem;background:var(--bg)">
      <input id="composer-images" type="text" placeholder="رابط صورة (اختياري)" style="width:100%;padding:0.6rem 0.8rem;border-radius:12px;border:1.5px solid rgba(45,37,32,0.1);font-family:inherit;font-size:0.85rem;margin-top:0.6rem;background:var(--bg)">
      <button id="composer-submit" style="width:100%;padding:0.7rem;border-radius:14px;background:var(--primary);color:#fff;border:none;font-family:Amiri,serif;font-size:1rem;font-weight:600;cursor:pointer;margin-top:0.8rem;transition:var(--transition)">انشر</button>
    </div>`;
  document.body.appendChild(modal);
  modal.addEventListener('click',function(ev){if(ev.target===modal)modal.remove();});
  document.getElementById('composer-submit').addEventListener('click',function(){
    var body=document.getElementById('composer-body').value.trim();
    if(!body){showToast('اكتب شيئاً أولاً');return;}
    var tagsVal=document.getElementById('composer-tags').value.trim();
    var tags=tagsVal?tagsVal.split(',').map(t=>t.trim()).filter(t=>t):null;
    var imagesVal=document.getElementById('composer-images').value.trim();
    var images=imagesVal?imagesVal.split(',').map(t=>t.trim()).filter(t=>t):null;
    fetch('/api/interactions/posts',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({body:body,tags:tags,images:images})})
    .then(r=>{if(r.ok)return r.json();throw new Error('Error');})
    .then(()=>{modal.remove();showToast('تم النشر! ✓');setTimeout(()=>location.reload(),800);})
    .catch(()=>showToast('خطأ في النشر'));
  });
});

// ===== CONNECT BUTTON =====
document.addEventListener('click',function(e){
  var btn=e.target.closest('.btn-connect');
  if(!btn)return;
  e.preventDefault();
  var uid=btn.dataset.userId;
  if(!uid)return;
  btn.disabled=true;btn.textContent='جاري الإرسال...';
  fetch('/api/interactions/connect/'+uid,{method:'POST'})
  .then(r=>r.json()).then(d=>{
    if(d.error){showToast(d.error);btn.disabled=false;btn.textContent='تواصل';}
    else{btn.textContent='طلب مرسل ✓';btn.style.background='var(--emerald-bg)';btn.style.color='var(--emerald)';btn.style.borderColor='var(--emerald)';showToast('تم إرسال الطلب!');}
  }).catch(()=>{showToast('خطأ');btn.disabled=false;btn.textContent='تواصل';});
});

// ===== JOIN GROUP =====
document.addEventListener('click',function(e){
  var btn=e.target.closest('.btn-join-group');
  if(!btn)return;
  e.preventDefault();
  var gid=btn.dataset.groupId;
  if(!gid)return;
  fetch('/api/interactions/groups/join/'+gid,{method:'POST'})
  .then(r=>r.json()).then(d=>{
    if(d.isMember){btn.textContent='مغادر المجموعة';btn.className='btn btn-outline btn-join-group';showToast('انضممت للمجموعة! ✓');}
    else{btn.textContent='انضمام';btn.className='btn btn-primary btn-join-group';showToast('غادرت المجموعة');}
  }).catch(()=>showToast('خطأ'));
});

// ===== JOB APPLY =====
document.addEventListener('click',function(e){
  var btn=e.target.closest('.btn-apply');
  if(!btn)return;
  e.preventDefault();
  showToast('تم تسجيل تقديمك! سنتواصل معك قريباً ✓');
  btn.textContent='تم التقديم ✓';btn.disabled=true;
  btn.style.background='var(--emerald-bg)';btn.style.color='var(--emerald)';btn.style.borderColor='var(--emerald)';
});


// ═══ USER HOVER CARD ═══
var hoverCard=document.createElement('div');hoverCard.className='user-hover-card';
hoverCard.innerHTML='<div class="cover"></div><div class="avatar"></div><div class="info"></div>';
document.body.appendChild(hoverCard);
var hoverTimer=null;
document.addEventListener('mouseover',function(e){
  var el=e.target.closest('[data-user-id]');
  if(!el||!el.dataset.userId)return;
  var uid=el.dataset.userId;
  clearTimeout(hoverTimer);
  hoverTimer=setTimeout(function(){
    fetch('/api/users/'+uid).then(function(r){return r.json()}).then(function(u){
      hoverCard.querySelector('.avatar').style.cssText='width:52px;height:52px;border-radius:50%;border:3px solid var(--bg);margin-top:-28px;margin-bottom:0.5rem;background:linear-gradient(135deg,var(--primary),var(--accent));display:flex;align-items:center;justify-content:center;color:#fff;font-weight:700;font-size:1.2rem';
      hoverCard.querySelector('.avatar').textContent=u.fullName?u.fullName[0]:'?';
      hoverCard.querySelector('.info').innerHTML='<div style="font-weight:600;font-size:0.9rem">'+(u.fullName||'')+'</div><div style="font-size:0.72rem;color:var(--text-secondary);margin:0.2rem 0 0.4rem">'+(u.headline||'')+'</div><div style="display:flex;gap:0.8rem;font-size:0.68rem;color:var(--text-muted)"><span>👥 '+(u.connectionCount||0)+'</span><span>⚡ '+(u.xp||0)+' XP</span></div>';
      var rect=el.getBoundingClientRect();
      hoverCard.style.left=Math.min(rect.left,window.innerWidth-300)+'px';
      hoverCard.style.top=Math.min(rect.bottom+8,window.innerHeight-200)+'px';
      hoverCard.classList.add('visible');
    }).catch(function(){});
  },400);
});
document.addEventListener('mouseout',function(e){
  var el=e.target.closest('[data-user-id]');
  if(el){clearTimeout(hoverTimer);hoverCard.classList.remove('visible')}
});

// ═══ SCROLL REVEAL ═══
var revealObserver=new IntersectionObserver(function(entries){
  entries.forEach(function(e){if(e.isIntersecting){e.target.classList.add('revealed');revealObserver.unobserve(e.target)}});
},{threshold:0.1});
document.querySelectorAll('.scroll-reveal').forEach(function(el){revealObserver.observe(el)});

// ═══ IMAGE LIGHTBOX ═══
var lbOverlay=document.createElement('div');lbOverlay.className='lightbox-overlay';
lbOverlay.innerHTML='<img src="" alt=""><button class="lightbox-close">&times;</button>';
document.body.appendChild(lbOverlay);
lbOverlay.querySelector('.lightbox-close').addEventListener('click',function(){lbOverlay.classList.remove('active')});
lbOverlay.addEventListener('click',function(e){if(e.target===lbOverlay)lbOverlay.classList.remove('active')});
document.addEventListener('keydown',function(e){if(e.key==='Escape')lbOverlay.classList.remove('active')});
document.addEventListener('click',function(e){
  var img=e.target.closest('.post img,.card img');if(!img||!img.src)return;
  e.preventDefault();lbOverlay.querySelector('img').src=img.src;lbOverlay.classList.add('active');
});


// ═══ SHARE MODAL ═══
var currentShareUrl='';
function openShareModal(postId){
  currentShareUrl=window.location.origin+'/posts/'+postId;
  document.getElementById('shareLinkInput').value=currentShareUrl;
  document.getElementById('shareModalOverlay').classList.add('active');
}
document.getElementById('shareModalOverlay').addEventListener('click',function(e){if(e.target===this)this.classList.remove('active')});
function copyShareLink(){var i=document.getElementById('shareLinkInput');i.select();document.execCommand('copy');showToast('تم نسخ الرابط!')}
function shareToWhatsApp(){window.open('https://wa.me/?text='+encodeURIComponent(currentShareUrl),'_blank')}
function shareToTwitter(){window.open('https://twitter.com/intent/tweet?url='+encodeURIComponent(currentShareUrl),'_blank')}

// ═══ REACTION PICKER ═══
document.addEventListener('click',function(e){
  var shareBtn=e.target.closest('[data-share-id]');
  if(shareBtn){e.preventDefault();openShareModal(shareBtn.dataset.shareId);return}
  var reactBtn=e.target.closest('[data-post-react]');
  if(!reactBtn)return;
  e.preventDefault();
  var pid=reactBtn.dataset.postReact;
  var existing=document.querySelector('.reaction-picker');
  if(existing){existing.remove();return}
  var picker=document.createElement('div');picker.className='reaction-picker';
  var reactions=[{emoji:'👍',label:'إعجاب',type:'like'},{emoji:'❤️',label:'حب',type:'love'},{emoji:'💡',label:'مفيد',type:'insightful'},{emoji:'🔥',label:'رائع',type:'awesome'}];
  reactions.forEach(function(r){
    var btn=document.createElement('button');btn.className='reaction-btn';btn.textContent=r.emoji;btn.title=r.label;
    btn.addEventListener('click',function(ev){ev.stopPropagation();
      fetch('/api/interactions/react/'+pid,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({type:r.type})})
      .then(function(res){return res.json()}).then(function(d){
        reactBtn.querySelector('span').textContent=d.count;
        reactBtn.querySelector('.reaction-label').textContent=d.type==='like'?'إعجاب':d.type==='love'?'حب':d.type==='insightful'?'مفيد':'رائع';
        picker.remove();showToast(r.emoji+' '+r.label);
      });
    });
    picker.appendChild(btn);
  });
  var rect=reactBtn.getBoundingClientRect();
  picker.style.position='fixed';picker.style.bottom=(window.innerHeight-rect.top+8)+'px';picker.style.left=rect.left+'px';
  document.body.appendChild(picker);
  setTimeout(function(){picker.classList.add('visible')},10);
  setTimeout(function(){if(picker.parentNode)picker.remove()},5000);
});



// ═══ POST VIEW TRACKING ═══
document.querySelectorAll('.post[data-post-id]').forEach(function(el){
  var pid=el.dataset.postId;
  var obs=new IntersectionObserver(function(entries){
    entries.forEach(function(e){if(e.isIntersecting){fetch('/api/interactions/view/'+pid,{method:'POST'});obs.unobserve(el)}});
  },{threshold:0.5});
  obs.observe(el);
});

// ═══ RELATIVE TIME ═══
function relativeTime(dateStr){
  var d=new Date(dateStr),now=new Date(),diff=Math.floor((now-d)/1000);
  if(diff<60)return 'الآن';
  if(diff<3600)return 'منذ '+Math.floor(diff/60)+' دقيقة';
  if(diff<86400)return 'منذ '+Math.floor(diff/3600)+' ساعة';
  if(diff<604800)return 'منذ '+Math.floor(diff/86400)+' يوم';
  return d.toLocaleDateString('ar-EG');
}

// ═══ SEARCH AUTOCOMPLETE ═══
var searchTimeout=null;
document.addEventListener('input',function(e){
  var inp=e.target.closest('input[name="q"]');
  if(!inp)return;
  clearTimeout(searchTimeout);
  var val=inp.value.trim();
  if(val.length<2)return;
  searchTimeout=setTimeout(function(){
    var existing=document.querySelector('.search-autocomplete');
    if(existing)existing.remove();
    fetch('/api/search/autocomplete?q='+encodeURIComponent(val)).then(r=>r.json()).then(d=>{
      if(!d||d.length===0)return;
      var dd=document.createElement('div');dd.className='search-autocomplete';
      dd.style.cssText='position:absolute;top:100%;left:0;right:0;background:var(--bg-card);backdrop-filter:blur(16px);border:1px solid rgba(124,58,237,0.1);border-radius:var(--radius);box-shadow:var(--shadow-xl);z-index:150;margin-top:4px;overflow:hidden';
      d.slice(0,6).forEach(function(r){
        var a=document.createElement('a');a.href=r.url;a.style.cssText='display:flex;align-items:center;gap:0.6rem;padding:0.6rem 0.8rem;font-size:0.82rem;color:var(--text);transition:var(--transition);text-decoration:none';
        a.onmouseover=function(){this.style.background='var(--bg)'};a.onmouseout=function(){this.style.background='transparent'};
        a.innerHTML='<span style="font-size:1.1rem">'+r.icon+'</span><span>'+r.name+'</span><span style="margin-right:auto;font-size:0.7rem;color:var(--text-muted)">'+r.type+'</span>';
        dd.appendChild(a);
      });
      inp.parentNode.appendChild(dd);
    });
  },250);
});
document.addEventListener('click',function(e){if(!e.target.closest('input[name="q"]')){var dd=document.querySelector('.search-autocomplete');if(dd)dd.remove()}});

// ═══ BOOKMARK ═══
var bmIds=[];
fetch('/api/interactions/bookmarks').then(r=>r.json()).then(d=>{bmIds=d;document.querySelectorAll('.post-action[data-bookmark-id]').forEach(el=>{if(bmIds.includes(parseInt(el.dataset.bookmarkId)))el.classList.add('bookmarked')})});
document.addEventListener('click',function(e){
  var btn=e.target.closest('.post-action[data-bookmark-id]');
  if(!btn)return; e.preventDefault();
  var pid=btn.dataset.bookmarkId;
  fetch('/api/interactions/bookmark/'+pid,{method:'POST'}).then(r=>r.json()).then(d=>{
    if(d.bookmarked){btn.classList.add('bookmarked');showToast('تم الحفظ!')}
    else{btn.classList.remove('bookmarked');showToast('تم الإزالة')}
  });
});

// Share button handler
document.addEventListener('click',function(e){
  var btn=e.target.closest('.post-action[data-share-id]');
  if(btn){e.preventDefault();openShareModal(btn.dataset.shareId);}
});
// ═══ INFINITE SCROLL ═══
(function(){
  var loading=false,page=1,hasMore="true";
  if(!hasMore)return;
  var sentinel=document.createElement('div');sentinel.className='feed-loader active';sentinel.innerHTML='<div class="spinner"></div>';
  var main=document.querySelector('main');if(!main)return;
  main.appendChild(sentinel);
  var obs=new IntersectionObserver(function(entries){
    entries.forEach(function(e){
      if(e.isIntersecting&&!loading&&hasMore){
        loading=true;page++;
        fetch('/?page='+page,{headers:{'X-Requested-With':'XMLHttpRequest'}})
        .then(function(r){return r.text()})
        .then(function(html){
          var parser=new DOMParser();var doc=parser.parseFromString(html,'text/html');
          var newPosts=doc.querySelectorAll('.post');
          if(newPosts.length===0){hasMore=false;sentinel.remove();return}
          newPosts.forEach(function(p){main.insertBefore(p,sentinel);revealObserver.observe(p)});
          var newHasMore=doc.querySelector('[data-has-more]');
          if(!newHasMore||newHasMore.dataset.hasMore==='false'){hasMore=false;sentinel.remove()}
        }).catch(function(){hasMore=false;sentinel.innerHTML='<p style="color:var(--text-muted)">خطأ في التحميل</p>'})
        .finally(function(){loading=false});
      }
    });
  },{threshold:0.1});
  obs.observe(sentinel);
})();
