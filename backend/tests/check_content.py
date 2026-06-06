import urllib.request, urllib.parse, http.cookiejar, sys
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
o = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())
d = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
r = urllib.request.Request('http://localhost:5078/Account/Login', data=d, method='POST')
r.add_header('Content-Type','application/x-www-form-urlencoded')
o.open(r)

# Check specific pages for content issues
checks = {
    '/': ['post-body', 'profile-card', 'card-header', '"دائرة"', 'فرص متاحة'],
    '/Profile': ['profile-header', 'exp-item', 'skill-tag', 'قوة الملف', 'نبذة عني'],
    '/Groups': ['groups-grid', 'group-card', 'group-cover', 'مجموعة'],
    '/Groups/1': ['btn-join-group', 'post-body', 'أعضاء'],
    '/Companies': ['pages-grid', 'page-card', 'وظائف مفتوحة'],
    '/Companies/1': ['btn-apply', 'الوظائف المفتوحة'],
    '/Circles': ['circle-dot', 'دائرة', 'ملخص'],
    '/Opportunities': ['btn-apply', 'فرصة'],
    '/Search?q=AI': ['نتائج', 'أشخاص', 'أوظائف', 'مجموعات', 'شركات'],
    '/Connections': ['طلب', 'قبول', 'رفض'],
    '/Profile/Edit': ['form', 'تعديل', 'حفظ التغييرات'],
}

for path, keywords in checks.items():
    try:
        resp = o.open('http://localhost:5078' + path)
        html = resp.read().decode('utf-8', 'ignore')
        missing = [k for k in keywords if k not in html]
        if missing:
            print(f'MISSING in {path}: {missing}')
        else:
            print(f'OK: {path}')
    except Exception as e:
        print(f'ERROR {path}: {getattr(e, "code", "?")}')

# Check for script errors
resp = o.open('http://localhost:5078/')
html = resp.read().decode('utf-8', 'ignore')
# Check JS syntax issues
script_start = html.rfind('<script>')
if script_start > 0:
    script_content = html[script_start:]
    # Check for obvious JS issues
    issues = []
    if 'function(' in script_content and script_content.count('{') != script_content.count('}'):
        issues.append('JS brace mismatch')
    print(f'JS section: {len(script_content)} chars, issues: {issues if issues else "none"}')
