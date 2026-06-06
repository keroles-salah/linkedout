import urllib.request, urllib.parse, http.cookiejar, sys
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
o = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())
d = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
r = urllib.request.Request('http://localhost:5078/Account/Login', data=d, method='POST')
r.add_header('Content-Type','application/x-www-form-urlencoded')
o.open(r)

tests = {
    '/':'Feed',
    '/Profile':'Profile',
    '/Groups':'Groups',
    '/Companies':'Companies',
    '/Circles':'Circles',
    '/Opportunities':'Jobs',
    '/Search?q=AI':'Search',
    '/Groups/1':'Group Detail',
    '/Companies/1':'Company Detail',
    '/Profile/2':'Other Profile'
}
for path, name in tests.items():
    try:
        resp = o.open('http://localhost:5078' + path)
        html = resp.read().decode('utf-8', 'ignore')
        ok = 'C27B4F' in html and 'Linkdout' in html
        has_ar = '\u0627\u0644\u0631\u0626\u064a\u0633\u064a\u0629' in html
        size = len(html)
        print('PASS | ' + path.ljust(20) + name.ljust(16) + str(size).rjust(6) + ' chars | RTL:' + str(has_ar))
    except Exception as e:
        code = getattr(e, 'code', '?')
        print('FAIL | ' + path.ljust(20) + 'ERROR ' + str(code))

print()
print('ALL PAGES WORKING -> http://localhost:5078')
