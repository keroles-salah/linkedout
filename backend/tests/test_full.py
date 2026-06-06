import sys, urllib.request, urllib.parse, http.cookiejar, re, json
sys.stdout.reconfigure(encoding='utf-8')

cj = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj))

# Step 1: MVC Login
r = opener.open('http://localhost:5078/Account/Login')
body = r.read().decode()
token_match = re.search(r'__RequestVerificationToken[^>]+value="([^"]+)"', body)
token = token_match.group(1) if token_match else None

data = urllib.parse.urlencode({
    '__RequestVerificationToken': token,
    'Email': 'keroles@linkdout.com',
    'Password': 'Test1234'
}).encode()
r = opener.open(urllib.request.Request('http://localhost:5078/Account/Login', data))
print(f'Login: {r.status} -> {r.url}')

# Step 2: Test all pages
pages = [
    ('/', 'Feed'),
    ('/Profile', 'Profile'),
    ('/Leaderboard', 'Leaderboard'),
    ('/Jobs/Match', 'Job Match'),
    ('/Groups', 'Groups'),
    ('/Companies', 'Companies'),
    ('/Circles', 'Circles'),
    ('/Opportunities', 'Jobs'),
    ('/Search?q=AI', 'Search'),
]

for url, name in pages:
    try:
        r = opener.open(f'http://localhost:5078{url}')
        body = r.read().decode()
        status = '✅' if r.status == 200 else f'❌ {r.status}'
        
        # Quick content checks
        checks = []
        if 'XP' in body: checks.append('XP')
        if 'leaderboard' in body.lower() or 'متصدرين' in body or 'لوحة' in body: checks.append('leader')
        if name == 'Leaderboard':
            if '🥇' in body: checks.append('gold')
            elif 'XP' in body: checks.append('entries')
        if name == 'Job Match':
            if 'تطابق' in body: checks.append('match')
        if name == 'Profile':
            if 'مستوى' in body or 'Level' in body: checks.append('level')
            if 'الشارات' in body: checks.append('badges')
        
        print(f'{status} | {name:15s} | {len(body):>6} chars | {",".join(checks) if checks else "-"}')
    except Exception as e:
        print(f'❌ | {name:15s} | {e}')
