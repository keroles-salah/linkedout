import urllib.request, urllib.parse, http.cookiejar, sys, re
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
o = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())
d = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
r = urllib.request.Request('http://localhost:5078/Account/Login', data=d, method='POST')
r.add_header('Content-Type','application/x-www-form-urlencoded')
o.open(r)

pages = {
    '/': 'Feed',
    '/Profile': 'Profile',
    '/Groups': 'Groups',
    '/Companies': 'Companies',
    '/Circles': 'Circles',
    '/Opportunities': 'Jobs',
    '/Search?q=AI': 'Search',
    '/Groups/1': 'Group Detail',
    '/Companies/1': 'Company Detail',
    '/Profile/2': 'Other Profile',
    '/Connections': 'Connections',
    '/Profile/Edit': 'Edit Profile'
}

issues = []

for path, name in pages.items():
    try:
        resp = o.open('http://localhost:5078' + path)
        html = resp.read().decode('utf-8', 'ignore')
        
        # Check for common issues
        checks = {
            'Missing closing div': html.count('<div') - html.count('</div'),
            'Unclosed tags': html.count('<') - html.count('>') - html.count('</'),
            'CSHTML artifacts': '@@' in html or '@{' in html or '}' == html.strip()[:1],
            'Has layout': 'Linkdout' in html and '</html>' in html,
            'Has RTL': 'dir="rtl"' in html,
            'Has CSS': '#C27B4F' in html or 'terracotta' in html.lower(),
            'No CS errors': 'error CS' not in html and 'CS0' not in html,
            'Valid HTML close': '</html>' in html,
        }
        
        problems = []
        for check, val in checks.items():
            if isinstance(val, bool) and not val:
                problems.append(check)
            elif isinstance(val, int) and val != 0:
                problems.append(f'{check} (diff={val})')
        
        if problems:
            status = f'ISSUES: {", ".join(problems)}'
        else:
            status = 'OK'
            
        print(f'{status:<60} {name:<20} ({len(html)} chars)')
        
    except Exception as e:
        print(f'ERROR {getattr(e, "code", "?")} {" " * 50} {name}')
        issues.append(f'{name}: HTTP {getattr(e, "code", "?")}')

print(f'\nTotal pages checked: {len(pages)}, Issues found: {len([i for i in issues])}')
