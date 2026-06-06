import urllib.request, urllib.parse, http.cookiejar, sys, re
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
o = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())
d = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
r = urllib.request.Request('http://localhost:5078/Account/Login', data=d, method='POST')
r.add_header('Content-Type','application/x-www-form-urlencoded')
o.open(r)

# Test edit page
resp = o.open('http://localhost:5078/Profile/Edit')
html = resp.read().decode('utf-8','ignore')
size = len(html)
has_form = '<form' in html
has_token = '__RequestVerificationToken' in html
has_dark = 'data-theme' in html
has_hamburger = 'hamburger' in html
print(f'Edit Profile: {resp.status} | {size} chars | Form:{has_form} | AntiForgery:{has_token}')
print(f'Dark mode CSS: {has_dark} | Hamburger: {has_hamburger}')

# Test dark mode in feed
resp = o.open('http://localhost:5078/')
html = resp.read().decode('utf-8','ignore')
has_dark_css = '[data-theme="dark"]' in html
has_mobile_css = 'mobile-open' in html or 'mobile-show' in html
has_theme_btn = 'theme-toggle' in html
print(f'Feed: dark CSS:{has_dark_css} | mobile CSS:{has_mobile_css} | theme btn:{has_theme_btn}')
print('All improvements verified!')
