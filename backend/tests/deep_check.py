import urllib.request, urllib.parse, http.cookiejar, sys
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
o = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())
d = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
r = urllib.request.Request('http://localhost:5078/Account/Login', data=d, method='POST')
r.add_header('Content-Type','application/x-www-form-urlencoded')
o.open(r)

# Look for actual rendering issues in the feed
resp = o.open('http://localhost:5078/')
html = resp.read().decode('utf-8', 'ignore')

# Check circles section
circle_area = html.find('دوائري')
if circle_area > 0:
    snippet = html[circle_area:circle_area+600]
    print('=== CIRCLES SECTION ===')
    print(snippet[:400])
    print()

# Check post cards structure
print('=== POST CARDS ===')
post_count = html.count('data-post-id')
print(f'Posts found: {post_count}')
print(f'Like buttons: {html.count("post-action")}')
print(f'Comment sections: {html.count("comment")}')
print()

# Check for any CSHTML that leaked through
if '@{' in html or '@@' in html or '@using' in html or '@model' in html:
    print('WARNING: Razor syntax found in rendered HTML!')
    idx = html.find('@{')
    if idx > 0: print(html[max(0,idx-50):idx+100])
else:
    print('No Razor leaks in rendered HTML')

# Check for common formatting issues
encoding_issues = []
for pattern in ['Ã', 'Â', 'Ã˜', 'Ã¹', 'Ø§']:
    count = html.count(pattern)
    if count > 0:
        encoding_issues.append(f'{pattern}: {count} times')
if encoding_issues:
    print(f'\nENCODING ISSUES: {encoding_issues}')
else:
    print('No encoding issues')

# Check dark mode CSS
if '[data-theme="dark"]' in html:
    print('Dark mode CSS: present')
else:
    print('WARNING: Dark mode CSS missing!')
