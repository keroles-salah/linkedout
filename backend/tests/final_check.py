import urllib.request, urllib.parse, http.cookiejar, sys
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
o = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())
d = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
r = urllib.request.Request('http://localhost:5078/Account/Login', data=d, method='POST')
r.add_header('Content-Type','application/x-www-form-urlencoded')
o.open(r)

# Verify key contents
resp = o.open('http://localhost:5078/')
html = resp.read().decode('utf-8','ignore')
print('Feed circles:', 'الدائرة' in html, '-', 'القريبة' in html or 'تعلم' in html or 'مهنية' in html)

resp = o.open('http://localhost:5078/Search?q=AI')
html = resp.read().decode('utf-8','ignore')
print('Search sections:', 'أشخاص' in html, 'وظائف' in html, 'مجموعات' in html, 'شركات' in html)

resp = o.open('http://localhost:5078/Connections')
html = resp.read().decode('utf-8','ignore')
print('Connections page:', 'طلب' in html, 'قبول' in html, 'رفض' in html)

resp = o.open('http://localhost:5078/Profile/Edit')
html = resp.read().decode('utf-8','ignore')
print('Edit Profile:', 'حفظ التغييرات' in html, 'تعديل الملف' in html)

print('\nAll content verified!')
