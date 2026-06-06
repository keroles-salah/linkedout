import urllib.request, urllib.parse, http.cookiejar, sys
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
o = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())
d = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
r = urllib.request.Request('http://localhost:5078/Account/Login', data=d, method='POST')
r.add_header('Content-Type','application/x-www-form-urlencoded')
o.open(r)

# Check search page content
resp = o.open('http://localhost:5078/Search?q=AI')
html = resp.read().decode('utf-8', 'ignore')

# Find all heading-like text between search results
import re
# Find section headers
sections = re.findall(r'<div style="font-weight:600;margin:1[^"]*">([^<]+)</div>', html)
print('Search section headers:')
for s in sections: print(f'  "{s}"')

# Check post dates
resp = o.open('http://localhost:5078/')
html = resp.read().decode('utf-8', 'ignore')
dates = re.findall(r'(\d{2}\s\w{3}\s\d{4})', html)
print(f'\nPost dates found: {len(dates)}')
for d in dates[:5]: print(f'  {d}')

# Check profile strength
resp = o.open('http://localhost:5078/Profile')
html = resp.read().decode('utf-8', 'ignore')
strength = re.findall(r'(\d+)%', html)
print(f'\nProfile strength: {strength}')

# Check for empty/zero stats that look bad
zeros = re.findall(r'>0</div><div class="lbl">', html)
print(f'Zero stats on profile: {len(zeros)}')
